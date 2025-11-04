using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoldableObject : MonoBehaviour
{
    private Rigidbody rb;
    private Transform holdParent;
    private bool isHeld = false;
    public float holdDistance = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PickUp(Camera cam)
    {
        if (cam == null) return;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) return;
        }

        isHeld = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        holdParent = cam.transform;
    }

    public void Drop()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) return;
        }

        isHeld = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        holdParent = null;
    }

    void FixedUpdate()
    {
        if (isHeld && holdParent != null)
        {
            Vector3 targetPos = holdParent.position + holdParent.forward * holdDistance;
            rb.MovePosition(Vector3.Lerp(rb.position, targetPos, Time.fixedDeltaTime * 10f));
        }
    }
}
