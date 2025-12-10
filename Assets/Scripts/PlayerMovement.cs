using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour, IDamageable
{
    public static PlayerMovement Instance { get; private set; }

    [Header("Configurações de Movimento")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintMultiplier = 1.8f;
    [SerializeField] float rotationSmoothTime = 0.05f;
    [SerializeField] Camera playerCamera;

    [Header("Configurações de Pulo")]
    [SerializeField] float jumpForce = 6f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.25f;
    [SerializeField] LayerMask groundMask = ~0;

    [Header("Input Actions")]
    [SerializeField] InputAction moveAction;
    [SerializeField] InputAction lookAction;
    [SerializeField] InputAction jumpAction;
    [SerializeField] InputAction attackAction;        // botão esquerdo
    [SerializeField] InputAction heavyAttackAction;   // botão direito

    [Header("Attack Stats")]
    [SerializeField] int attackDamage = 10;
    [SerializeField] int heavyAttackDamage = 25;
    [SerializeField] float attackCooldown = 0.5f;
    [SerializeField] float heavyAttackCooldown = 1.2f;
    [SerializeField] float lightAttackForce = 20f;
    [SerializeField] float heavyAttackForce = 50f;
    [SerializeField] float lightAttackDelay = 0.15f;
    [SerializeField] float range = 8f;

    [Header("Efeitos Visuais")]
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] ParticleSystem heavyAttackEffect;
    [SerializeField] ParticleSystem bloodEffect;
    [SerializeField] float cameraImpactBack = 0.3f;
    [SerializeField] float cameraImpactSpeed = 4f;

    [Header("Status")]
    [SerializeField] int health = 100;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float pitch;
    private float yaw;
    private bool isGrounded;
    private float cooldownTimer = 0;
    private float lastJumpAttemptTime = -1f;
    private float colRadius;
    private float jumpCheckCooldown = 0.5f;
    private float jumpCheckTimer = 0f;

    private HoldableObject heldObject;
    private bool isAttacking = false;
    private PlayerSounds playerSounds;
    private float deathUpwardForce = 10f;
    private float deathBackwardForce = 15f;
    private float deathTorqueForce = 5f;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true;

        moveAction?.Enable();
        lookAction?.Enable();
        jumpAction?.Enable();
        attackAction?.Enable();
        heavyAttackAction?.Enable();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        yaw = transform.eulerAngles.y;
        pitch = playerCamera != null ? playerCamera.transform.localEulerAngles.x : 0f;
        if (pitch > 180f) pitch -= 360f;

        CapsuleCollider col = GetComponentInChildren<CapsuleCollider>();
        colRadius = col != null ? col.radius : 0.5f;

        playerSounds = GetComponent<PlayerSounds>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            return;
        }

        if (moveAction != null) moveInput = moveAction.ReadValue<Vector2>();
        if (lookAction != null) lookInput = lookAction.ReadValue<Vector2>();

        float currentSensitivity = GameManager.Instance.MouseSensitivity;
        yaw += lookInput.x * currentSensitivity * Time.deltaTime;
        pitch -= lookInput.y * currentSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        bool jumpPressed = (jumpAction != null && jumpAction.triggered) ||
                           (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);

        if (jumpPressed && Time.time - lastJumpAttemptTime > 0.2f)
            lastJumpAttemptTime = Time.time;

        

        // ataque leve
        if (!isAttacking && attackAction != null && attackAction.triggered)
            StartCoroutine(HandleAttack(lightAttackForce, lightAttackDelay, attackCooldown, false));

        // ataque pesado (sincronizado com câmera)
        if (!isAttacking && heavyAttackAction != null && heavyAttackAction.triggered)
            StartCoroutine(HandleHeavyAttack());

        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Slerp(
                playerCamera.transform.localRotation,
                Quaternion.Euler(pitch, 0f, 0f),
                rotationSmoothTime * 50f
            );
        }

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        isGrounded = GroundCheck();

        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCheckTimer = jumpCheckCooldown;
        }

        // --- Interação com E ---
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (playerCamera == null) return;

            if (heldObject == null)
            {
                Vector3 rayDirection = playerCamera.transform.forward;
                Vector3 rayOrigin = playerCamera.transform.position + rayDirection * 2f + Vector3.up * 0.5f;
                rayOrigin += rayDirection * 1.5f;

                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection, 3f, ~0, QueryTriggerInteraction.Ignore);
                if (hits == null || hits.Length == 0) return;

                RaycastHit? validHit = hits
                    .Where(h => h.collider != null && h.collider.transform.root != transform)
                    .OrderBy(h => h.distance)
                    .FirstOrDefault();

                if (!validHit.HasValue || validHit.Value.collider == null)
                    return;

                RaycastHit hit = validHit.Value;

                HoldableObject holdable = hit.collider.GetComponent<HoldableObject>() ??
                                          hit.collider.GetComponentInParent<HoldableObject>();

                if (holdable == null) return;

                holdable.PickUp(playerCamera);
                heldObject = holdable;
            }
            else
            {
                heldObject.Drop();
                heldObject = null;
            }
        }
        // Controle de Audio de Passos
        if (isGrounded && moveInput.magnitude > 0.1f)
        {
            if (playerSounds != null)
            {
                playerSounds.PlayWalkSound();
            }
        }
        else
        {
            if (playerSounds != null)
            {
                playerSounds.StopWalkSound();
            }
        }
    }

    private IEnumerator HandleAttack(float force, float delay, float cooldown, bool heavy)
    {
        if (cooldownTimer > 0) yield break;
        isAttacking = true;

        yield return new WaitForSeconds(delay);

        ApplyAttack(force, heavy, attackDamage);
        cooldownTimer = cooldown;

        isAttacking = false;
    }

    private IEnumerator HandleHeavyAttack()
    {
        if (cooldownTimer > 0) yield break;
        isAttacking = true;

        yield return StartCoroutine(CameraImpact(() =>
        {
            ApplyAttack(heavyAttackForce, true, heavyAttackDamage);
        }));

        cooldownTimer = heavyAttackCooldown;
        isAttacking = false;
    }

    private void ApplyAttack(float force, bool heavy, int damage)
    {
        if (heldObject != null)
        {
            Rigidbody thrownRb = heldObject.GetComponent<Rigidbody>();
            heldObject.Drop();

            if (thrownRb != null)
            {
                thrownRb.AddForce(playerCamera.transform.forward * force, ForceMode.VelocityChange);
                thrownRb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            }

            heldObject = null;
        }
        else
        {
            Ray attackRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(attackRay, out RaycastHit hit, range))
            {
                if (hit.rigidbody != null)
                    hit.rigidbody.velocity = playerCamera.transform.forward * force;

                //Verifica se o objeto é um "Inimigo"
                if (hit.collider.CompareTag("Enemy"))
                {
                    //Sangue
                    if (bloodEffect != null)
                    {
                        Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    }

                    IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                    if (damageable != null)
                    {   
                        damageable.GetHit(damage);
                    }
                }
                else
                {
                    if (heavy && heavyAttackEffect != null)
                        Instantiate(heavyAttackEffect, hit.point, Quaternion.identity);
                    else if (hitEffect != null)
                        Instantiate(hitEffect, hit.point, Quaternion.identity);
                }
            }
        }
    }

    private IEnumerator CameraImpact(System.Action onReturnStart)
    {
        Vector3 startPos = playerCamera.transform.localPosition;
        Vector3 backPos = startPos - Vector3.forward * cameraImpactBack;
        float t = 0f;

        // fase de recuo (carregando o golpe)
        while (t < 1f)
        {
            t += Time.deltaTime * cameraImpactSpeed;
            playerCamera.transform.localPosition = Vector3.Lerp(startPos, backPos, t);
            yield return null;
        }

        // início da volta (ataque sai aqui)
        onReturnStart?.Invoke();

        // fase de retorno
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraImpactSpeed;
            playerCamera.transform.localPosition = Vector3.Lerp(backPos, startPos, t);
            yield return null;
        }

        playerCamera.transform.localPosition = startPos;
    }

    private bool GroundCheck()
    {
        // Ground check
        if (groundCheck != null)
        {
            Vector3 checkPos = groundCheck.position + new Vector3(0, 0.1f, 0);
            isGrounded = Physics.Raycast(checkPos, -groundCheck.transform.up, groundDistance);
            if (isGrounded) return true;
            int points = 4;
            for (int i = 0; i < points; i++)
            {
                float angle = (360f / points) * i * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * colRadius;
                if (Physics.Raycast(checkPos + offset, -groundCheck.transform.up, groundDistance))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        isGrounded = GroundCheck();

        float currentSpeed = moveSpeed;
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            currentSpeed *= sprintMultiplier;

        

        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        float vSpeed = rb.velocity.y; ;
        //print(jumpCheckTimer);
        jumpCheckTimer -= Time.deltaTime;
        if (jumpCheckTimer < 0) { jumpCheckTimer = 0; }
        if (isGrounded)
        {
            if (jumpCheckTimer <= 0f) {
                vSpeed = 0f;
            }
        }

        rb.velocity = move * currentSpeed + new Vector3(0, vSpeed, 0);

        float extraGravityMultiplier = 2f;
        rb.AddForce(Physics.gravity * (extraGravityMultiplier - 1f), ForceMode.Acceleration);
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
        attackAction?.Disable();
        heavyAttackAction?.Disable();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    public void GetHit(int damage)
    {
        Debug.Log("Player got hit for " + damage + " damage.");
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Player has died.");
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(Vector3.up * deathUpwardForce, ForceMode.Impulse);
        // TODO: Fazer ele ser lançado para tras ????
        // Aplica torque aleatório para girar
        Vector3 randomTorque = new Vector3(
            Random.Range(-deathTorqueForce, deathTorqueForce),
            Random.Range(-deathTorqueForce, deathTorqueForce),
            Random.Range(-deathTorqueForce, deathTorqueForce)
        );
        rb.AddTorque(randomTorque, ForceMode.Impulse);
        OnDisable();
        //TODO: Implement death behavior (e.g., respawn, game over screen)
    }
}