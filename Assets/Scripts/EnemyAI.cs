using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

// Para o Script funcionar, a cena deve ter um NavMesh criado e feito o bake com NavMeshSurfacee agentType Enemy 
// e o inimigo deve ter um componente NavMeshAgent anexado.

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{

    [SerializeField] private float distanceToEngage = 15f;
    [SerializeField] private float distanceToDisengage = 20f;
    [SerializeField] private float distanceToAttack = 4f;
    [SerializeField] private float attackSpeed = .3f;
    private bool isAttacking = false;
    private Transform playerTransform;
    private NavMeshAgent navMeshAgent;
    private Rigidbody rb;
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Dead,
    }

    private State currentState;

    void Start()
    {
        currentState = State.Idle;
        playerTransform = GameObject.FindWithTag("Player").transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }


    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.Chase:
                HandleChaseState();
                break;
            case State.Attack:
                HandleAttackState();
                break;
            case State.Dead:
                HandleDeadState();
                break;
        }

        DebugDrawCircle(transform.position, distanceToEngage, Color.yellow);  // Engage
        DebugDrawCircle(transform.position, distanceToAttack, Color.red);      // Attack
        DebugDrawCircle(transform.position, distanceToDisengage, Color.blue);    // Disengage
        //Debug.Log(Vector3.Distance(transform.position, playerTransform.position));
    }

    private void HandleIdleState()
    {
        // Logic for Idle state
        //Debug.Log("Enemy is idle.");
        navMeshAgent.isStopped = true;
        if (Vector3.Distance(transform.position, playerTransform.position) < distanceToEngage && !isAttacking)
        {

            currentState = State.Chase;
        }
    }
    private void HandleChaseState()
    {
        // Logic for Chase state
        //Debug.Log("Enemy is chasing the player.");
        navMeshAgent.SetDestination(playerTransform.position);
        navMeshAgent.isStopped = false;

        // Trocar para Attack se estiver perto o suficiente
        if (Vector3.Distance(transform.position, playerTransform.position) < distanceToAttack)
        {
            currentState = State.Attack;
        }
        // Trocar para Idle se estiver longe o bastante
        else if (Vector3.Distance(transform.position, playerTransform.position) > distanceToDisengage)
        {
            currentState = State.Idle;
        }
    }
    private void HandleAttackState()
    {
        // Logic for Attack state
        // Debug.Log("Enemy is attacking the player.");
        navMeshAgent.isStopped = true;
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(attackSpeed);
        //TODO Insirir aqui a lógica de ataque (ex: reduzir vida do jogador)
        StartCoroutine(AttackCooldown(wait));
        currentState = State.Idle; // Seta para Idle após o ataque para evitar multiplos ataques seguidos
        isAttacking = true;
    }

    private void HandleDeadState()
    {
        // Logic for Dead state
        Debug.Log("Enemy is dead.");
        navMeshAgent.isStopped = true;
        rb.constraints = RigidbodyConstraints.None;

    }
    
        private IEnumerator AttackCooldown(WaitForSecondsRealtime wait)
    {
        Debug.Log("Enemy is cooling down after attack.");
        yield return wait;
        Debug.Log("Enemy finished cooldown.");
        isAttacking = false;
    }

    private void DebugDrawCircle(Vector3 center, float radius, Color color, int segments = 50)
    {   // Draw a circle in the XZ plane for visualization
        float angleStep = 360f / segments;
        Vector3 previousPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            Debug.DrawLine(previousPoint, newPoint, color);
            previousPoint = newPoint;
        }
    }
}