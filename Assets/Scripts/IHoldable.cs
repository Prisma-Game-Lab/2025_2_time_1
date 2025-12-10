using UnityEngine;

public interface IHoldable
{
    void PickUp(Camera cam);
    void Drop();
    Rigidbody GetRigidbody();
}
