using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoldableObject : MonoBehaviour, IHoldable
{
    private Rigidbody rb;
    private Transform holdParent;
    private bool isHeld = false;

    [Header("Configurações de Pegar")]
    public float holdDistance = 2f;
    public float followSpeed = 15f;

    [Header("Ajustes Visuais")]
    public Vector3 holdOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void PickUp(Camera cam)
    {
        if (cam == null) return;
        if (rb == null) rb = GetComponent<Rigidbody>();

        isHeld = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        holdParent = cam.transform;
    }

    public void Drop()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        isHeld = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        holdParent = null;
    }

    void FixedUpdate()
    {
        if (isHeld && holdParent != null)
        {
            Vector3 targetPos =
                holdParent.position +
                holdParent.forward * holdDistance +
                holdParent.TransformDirection(holdOffset);

            rb.MovePosition(
                Vector3.Lerp(rb.position, targetPos, Time.fixedDeltaTime * followSpeed)
            );

            Quaternion targetRot =
                holdParent.rotation *
                Quaternion.Euler(rotationOffset);

            rb.MoveRotation(
                Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * followSpeed)
            );
        }
    }

    // Interface IHoldable
    public Rigidbody GetRigidbody()
    {
        return rb;
    }
}
