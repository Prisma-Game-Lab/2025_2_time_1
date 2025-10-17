using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

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

    //Queda
    [SerializeField] float fallVelocity = 10f;
    [SerializeField] float fallVelocityMultiplier = 1.05f;
    [SerializeField] float fallVelocityIncrement = 0.05f;
    private float currentFallVelocity;
    private bool isGrounded = true;
    private float raycastDistance;
    private float playerHeight;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        targetAngle = this.gameObject.transform.rotation.eulerAngles;
        //TODO Mudar isso quando trocar o collider
        playerHeight = this.GetComponent<CapsuleCollider>().height;
        raycastDistance = playerHeight + 0.15f;
        currentFallVelocity = fallVelocityMultiplier;
    }

    private void FixedUpdate()
    {
        rb.velocity = (velocidade.x*transform.right + velocidade.y*transform.forward) * Time.fixedDeltaTime * 300 * speed;
        targetAngle += new Vector3(cameraVelocity.y * cameraspeed.y, cameraVelocity.x * cameraspeed.x, 0) * Time.fixedDeltaTime * 300;
        rb.MoveRotation(Quaternion.Euler(0,targetAngle.y,0));
        m_camera.transform.rotation = Quaternion.Euler(targetAngle);
        //Talvez castar esse raycast todo FixedUpdate seja pesado, testar depois
        checkIsGrounded();
        if (!isGrounded)
        {
            rb.velocity += Vector3.down * fallVelocity * currentFallVelocity * Time.fixedDeltaTime;
            currentFallVelocity += fallVelocityIncrement;
        }
        else
        {
            currentFallVelocity = fallVelocityMultiplier;
        }
        Debug.Log(isGrounded);
        Debug.DrawRay(transform.position, Vector3.down * raycastDistance, Color.red);
    }

    public void Move(InputAction.CallbackContext value)
    {
        velocidade = value.ReadValue<Vector2>();
    }

    public void MoveCamera(InputAction.CallbackContext value)
    {
        cameraVelocity = value.ReadValue<Vector2>();
    }

    private void checkIsGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}
