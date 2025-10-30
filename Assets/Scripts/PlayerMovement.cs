using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configura��es de Movimento")]
    [SerializeField] float moveSpeed = 5f;
    //[SerializeField] float mouseSensitivity = 100f;
    [SerializeField] float rotationSmoothTime = 0.05f;
    [SerializeField] Camera playerCamera;

    [Header("Configura��es de Pulo")]
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

    private void attack(bool on)
    {
        if (cooldownTimer <= 0) {
            Ray attackRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(attackRay, out RaycastHit hit, range))
            {
                if (hit.rigidbody != null) hit.rigidbody.velocity = (playerCamera.transform.forward * attackPhysicsForce);
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

        if (moveAction != null) moveAction.Enable();
        if (lookAction != null) lookAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (attackAction != null) attackAction.Enable();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        yaw = transform.eulerAngles.y;
        pitch = playerCamera != null ? playerCamera.transform.localEulerAngles.x : 0f;
        if (pitch > 180f) pitch -= 360f;
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

        //Pega a sensibilidade do GameManager
        float currentSensitivity = GameManager.Instance.MouseSensitivity;
        yaw += lookInput.x * currentSensitivity * Time.deltaTime;
        Debug.Log("Pitch before clamp: " + pitch);
        Debug.Log("Look Input Y: " + lookInput.y);
        pitch -= lookInput.y * currentSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        Debug.Log("Pitch after clamp: " + pitch);

        if (attackAction != null) if (attackAction.IsPressed()) attack(true);
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Slerp(
                playerCamera.transform.localRotation,
                Quaternion.Euler(pitch, 0f, 0f),
                rotationSmoothTime * 50f
            );
        }

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);


        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }


        bool jumpPressed = false;

        if (jumpAction != null && jumpAction.triggered)
            jumpPressed = true;

        if (!jumpPressed && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;

        if (jumpPressed && Time.time - lastJumpAttemptTime > 0.2f)
        {
            Debug.Log($"Jump attempt � isGrounded: {isGrounded} (groundDistance={groundDistance})");
            lastJumpAttemptTime = Time.time;
        }

        if (jumpPressed && isGrounded)
        {
            Vector3 v = rb.velocity;
            v.y = 0f;
            rb.velocity = v;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (playerCamera != null)
            {
                Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 3f))
                {
                    ResettableObject resettable = hit.collider.GetComponent<ResettableObject>();
                    if (resettable != null)
                        resettable.ResetObject();
                }
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

        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        rb.velocity = move * moveSpeed + new Vector3(0, rb.velocity.y, 0);
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
        if (lookAction != null) lookAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
        if (attackAction != null) attackAction.Disable();
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