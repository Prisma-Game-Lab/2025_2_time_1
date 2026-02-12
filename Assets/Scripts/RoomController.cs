using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour
{
    [Header("Porta vinculada")]
    public DoorController door;

    [Header("Quantos inimigos precisam morrer")]
    public int killsToOpen = 3;

    private int currentKills = 0;
    private bool doorOpened = false;

    private void OnEnable()
    {
        EnemyAI.OnEnemyDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        EnemyAI.OnEnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied(EnemyAI enemy)
    {
        if (doorOpened) return;

        currentKills++;

        Debug.Log($"Inimigos mortos: {currentKills}/{killsToOpen}");

        if (currentKills >= killsToOpen)
        {
            doorOpened = true;
            door.OpenDoor();
        }

    }
}
