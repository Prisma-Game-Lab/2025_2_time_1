using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
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
    [SerializeField] InputAction attackAction;

    [Header("Attack Stats")]
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attackCooldown = 0.5f;
    [SerializeField] float attackPhysicsForce = 20f;
    [SerializeField] float attackDelayBeforeThrow = 0.15f; // tempo antes de lançar
    [SerializeField] float range = 8f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float pitch;
    private float yaw;
    private bool isGrounded;
    private float cooldownTimer = 0;
    private float lastJumpAttemptTime = -1f;
    private float colRadius;

    private HoldableObject heldObject;
    private bool isAttacking = false;

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

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        yaw = transform.eulerAngles.y;
        pitch = playerCamera != null ? playerCamera.transform.localEulerAngles.x : 0f;
        if (pitch > 180f) pitch -= 360f;

        CapsuleCollider col = GetComponentInChildren<CapsuleCollider>();
        colRadius = col != null ? col.radius : 0.5f;
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

        // ataque
        if (!isAttacking && attackAction != null && attackAction.triggered)
            StartCoroutine(HandleAttack());

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

        // Ground check
        if (groundCheck != null)
        {
            Vector3 checkPos = groundCheck.position + new Vector3(0, 0.1f, 0);
            isGrounded = Physics.Raycast(checkPos, -groundCheck.transform.up, groundDistance);
            if (!isGrounded)
            {
                int points = 4;
                for (int i = 0; i < points; i++)
                {
                    float angle = (360f / points) * i * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * colRadius;
                    if (Physics.Raycast(checkPos + offset, -groundCheck.transform.up, groundDistance))
                    {
                        isGrounded = true;
                        break;
                    }
                }
            }
        }

        bool jumpPressed = (jumpAction != null && jumpAction.triggered) ||
                           (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);

        if (jumpPressed && Time.time - lastJumpAttemptTime > 0.2f)
            lastJumpAttemptTime = Time.time;

        if (jumpPressed && isGrounded)
        {
            Vector3 v = rb.velocity;
            v.y = 0f;
            rb.velocity = v;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
    }

    private IEnumerator HandleAttack()
    {
        if (cooldownTimer > 0) yield break;

        isAttacking = true;

        // mini delay simulando o "puxar o braço"
        yield return new WaitForSeconds(attackDelayBeforeThrow);

        // Se estiver segurando algo, arremessa
        if (heldObject != null)
        {
            Rigidbody thrownRb = heldObject.GetComponent<Rigidbody>();
            heldObject.Drop();

            if (thrownRb != null)
            {
                // força pra frente
                thrownRb.AddForce(playerCamera.transform.forward * attackPhysicsForce, ForceMode.VelocityChange);
                // rotação aleatória pra efeito natural
                thrownRb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            }

            heldObject = null;
        }
        else
        {
            // ataque normal com raycast
            Ray attackRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(attackRay, out RaycastHit hit, range))
            {
                if (hit.rigidbody != null)
                    hit.rigidbody.velocity = playerCamera.transform.forward * attackPhysicsForce;
            }
        }

        cooldownTimer = attackCooldown;
        isAttacking = false;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        float currentSpeed = moveSpeed;
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            currentSpeed *= sprintMultiplier;

        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        rb.velocity = move * currentSpeed + new Vector3(0, rb.velocity.y, 0);
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
        attackAction?.Disable();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
