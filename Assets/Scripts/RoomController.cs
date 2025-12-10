using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Porta vinculada")]
    public DoorController door;

    private int enemiesAlive = 0;

    private void Start()
    {
        // Encontra todos os inimigos filhos desta sala
        EnemyAI[] enemies = GetComponentsInChildren<EnemyAI>();
        enemiesAlive = enemies.Length;

        // Começa ouvindo o evento
        EnemyAI.OnEnemyDied += OnEnemyDied;
    }

    private void OnDestroy()
    {
        EnemyAI.OnEnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied(EnemyAI enemy)
    {
        // Só conta mortes de inimigos dentro dessa sala
        if (enemy.transform.IsChildOf(transform))
        {
            enemiesAlive--;

            if (enemiesAlive <= 0)
            {
                door.OpenDoor();
            }
        }
    }
}
