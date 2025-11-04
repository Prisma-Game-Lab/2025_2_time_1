using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
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

    private void Attack(bool on)
    {
        if (cooldownTimer <= 0)
        {
            Ray attackRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(attackRay, out RaycastHit hit, range))
            {
                if (hit.rigidbody != null)
                    hit.rigidbody.velocity = playerCamera.transform.forward * attackPhysicsForce;
            }
            cooldownTimer = attackCooldown;
        }
    }

    void Start()
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
        if (col != null)
            colRadius = col.radius;
        else
            colRadius = 0.5f;
    }

    void Update()
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

        if (attackAction != null && attackAction.IsPressed()) Attack(true);
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Slerp(
                playerCamera.transform.localRotation,
                Quaternion.Euler(pitch, 0f, 0f),
                rotationSmoothTime * 50f
            );
        }

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Ground check aprimorado com raycasts em círculo
        if (groundCheck != null)
        {
            Vector3 checkPos = groundCheck.position + new Vector3(0, 0.1f, 0);
            isGrounded = Physics.Raycast(checkPos, -groundCheck.transform.up, groundDistance, groundMask);
            if (!isGrounded)
            {
                int points = 4;
                for (int i = 0; i < points; i++)
                {
                    float angle = (360f / points) * i * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * colRadius;
                    if (Physics.Raycast(checkPos + offset, -groundCheck.transform.up, groundDistance, groundMask))
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
            if (playerCamera == null)
                return;

            if (heldObject == null)
            {
                Vector3 rayOrigin = playerCamera.transform.position;
                Vector3 rayDirection = playerCamera.transform.forward;

                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 3f, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider != null && hit.collider.transform.root != transform)
                    {
                        HoldableObject holdable = hit.collider.GetComponent<HoldableObject>() ??
                                                  hit.collider.GetComponentInParent<HoldableObject>();

                        if (holdable != null)
                        {
                            holdable.PickUp(playerCamera);
                            heldObject = holdable;
                        }
                    }
                }
            }
            else
            {
                heldObject.Drop();
                heldObject = null;
            }
        }
    }

    void FixedUpdate()
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
