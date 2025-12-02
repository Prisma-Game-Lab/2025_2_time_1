using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Collider que bloqueia a passagem")]
    public Collider doorCollider;

    public void OpenDoor()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = false; // Desativa a barreira
        }
    }

    public void CloseDoor()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = true; // Ativa a barreira
        }
    }
}
