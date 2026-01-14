using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Modelo da porta fechada")]
    public GameObject doorClosed;

    [Header("Modelo da porta aberta")]
    public GameObject doorOpen;

    private void Start()
    {
        CloseDoor();
    }

    public void OpenDoor()
    {
        if (doorClosed != null)
            doorClosed.SetActive(false);

        if (doorOpen != null)
            doorOpen.SetActive(true);
    }

    public void CloseDoor()
    {
        if (doorClosed != null)
            doorClosed.SetActive(true);

        if (doorOpen != null)
            doorOpen.SetActive(false);
    }
}
