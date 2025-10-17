using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float mouseSensitivity = 100f;
    [SerializeField] float rotationSmoothTime = 0.05f; 
    [SerializeField] Camera playerCamera;

    [Header("Input Actions")]
    [SerializeField] InputAction moveAction;
    [SerializeField] InputAction lookAction;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float pitch;
    private float yaw;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        moveAction.Enable();
        lookAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        yaw = transform.eulerAngles.y;
        pitch = playerCamera.transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();

       
        yaw += lookInput.x * mouseSensitivity * Time.deltaTime;
        pitch -= lookInput.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        
        playerCamera.transform.localRotation = Quaternion.Slerp(
            playerCamera.transform.localRotation,
            Quaternion.Euler(pitch, 0f, 0f),
            rotationSmoothTime * 50f
        );

        
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void FixedUpdate()
    {
        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        rb.velocity = move * moveSpeed + new Vector3(0, rb.velocity.y, 0);
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
    }
}
