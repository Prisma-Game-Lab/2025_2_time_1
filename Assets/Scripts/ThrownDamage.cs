using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class ThrownDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float minImpactVelocity = 3f;

    [Header("VFX")]
    [SerializeField] private VisualEffect bloodEffect;
    [SerializeField] private float vfxLifetime = 2f;

    private Rigidbody rb;
    private bool hasHit = false;
    private HoldableObject holdable;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        holdable = GetComponent<HoldableObject>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        if (holdable != null && IsBeingHeld()) return;
        if (rb.velocity.magnitude < minImpactVelocity) return;

        IDamageable damageable =
            collision.collider.GetComponentInParent<IDamageable>();

        if (damageable == null) return;
        if (collision.collider.GetComponentInParent<PlayerMovement>() != null) return;

        // --------------------
        // DANO
        // --------------------
        damageable.GetHit(damage);
        hasHit = true;

        // --------------------
        // VFX DE SANGUE
        // --------------------
        if (bloodEffect != null)
        {
            ContactPoint contact = collision.contacts[0];

            Vector3 spawnPoint = contact.point + contact.normal * 0.1f;
            Quaternion rotation = Quaternion.LookRotation(contact.normal);

            VisualEffect vfx = Instantiate(bloodEffect, spawnPoint, rotation);

            if (vfx.HasInt("BloodCount"))
                vfx.SetInt("BloodCount", 20);

            vfx.Play();
            Destroy(vfx.gameObject, vfxLifetime);
        }
    }

    private bool IsBeingHeld()
    {
        return rb != null && rb.useGravity == false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (rb.velocity.magnitude < 0.1f)
            hasHit = false;
    }
}
