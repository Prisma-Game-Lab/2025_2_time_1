using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Vector2 cameraspeed;
    [SerializeField] Camera m_camera;
    [SerializeField] InputAction actions;
    Rigidbody rb;
    private Vector2 velocidade;
    private Vector2 cameraVelocity;
    private Vector3 targetAngle;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        targetAngle = this.gameObject.transform.rotation.eulerAngles;
    }

    private void FixedUpdate()
    {
        rb.velocity = (velocidade.x*transform.right + velocidade.y*transform.forward) * Time.fixedDeltaTime * 300 * speed;
        targetAngle += new Vector3(cameraVelocity.y * cameraspeed.y, cameraVelocity.x * cameraspeed.x, 0) * Time.fixedDeltaTime * 300;
        rb.MoveRotation(Quaternion.Euler(0,targetAngle.y,0));
        m_camera.transform.rotation = Quaternion.Euler(targetAngle);
    }

    public void Move(InputAction.CallbackContext value)
    {
        velocidade = value.ReadValue<Vector2>();
    }

    public void MoveCamera(InputAction.CallbackContext value)
    {
        cameraVelocity = value.ReadValue<Vector2>();
    }
}
