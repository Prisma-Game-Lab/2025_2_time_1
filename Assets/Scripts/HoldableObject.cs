using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoldableObject : MonoBehaviour
{
    private Rigidbody rb;
    private Transform holdParent;
    private bool isHeld = false;

    [Header("Configurações de Pegar")]
    public float holdDistance = 2f;
    public float followSpeed = 15f;

    [Header("Ajustes Visuais")]
    public Vector3 holdOffset = Vector3.zero;        // Ajuste de posição
    public Vector3 rotationOffset = Vector3.zero;    // Novo: ajuste de rotação (em graus)

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
            // Posição alvo com offset
            Vector3 targetPos = holdParent.position + holdParent.forward * holdDistance
                                + holdParent.TransformDirection(holdOffset);

            rb.MovePosition(Vector3.Lerp(rb.position, targetPos, Time.fixedDeltaTime * followSpeed));

            // Rotação alvo com offset
            Quaternion targetRot = holdParent.rotation * Quaternion.Euler(rotationOffset);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * followSpeed));
        }
    }
}
