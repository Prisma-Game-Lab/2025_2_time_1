using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Para o Script funcionar, a cena deve ter um NavMesh criado e feito o bake com NavMeshSurfacee agentType Enemy 
// e o inimigo deve ter um componente NavMeshAgent anexado.

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour, IDamageable
{

    [SerializeField] private float distanceToEngage = 30f;
    [SerializeField] private float distanceToDisengage = 40f;
    [SerializeField] private float distanceToAttack = 8f;
    [SerializeField] private float attackSpeed = 2f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] public int health = 30;
    [SerializeField] private float deathUpwardForce = 10f;
    [SerializeField] private float deathTorqueForce = 5f;
    private Transform playerTransform;
    private IDamageable playerDamageable;
    private NavMeshAgent navMeshAgent;
    private Rigidbody rb;
    private Animator animator;
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Dead,
    }

    private State currentState;
    private float pathUpdateDeadline = 0;
    private float pathUpdateDelay = 0.2f;
    private bool isAttacking = false;

    void Start()
    {
        currentState = State.Idle;
        playerTransform = GameObject.FindWithTag("Player").transform;
        playerDamageable = GameObject.FindWithTag("Player").GetComponent<IDamageable>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.stoppingDistance = distanceToAttack - 0.1f;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        animator = GetComponent<Animator>();
    }


    void FixedUpdate()
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
        Debug.Log("Velocidade desejada" + navMeshAgent.desiredVelocity.magnitude);
        Debug.Log("Velocidade atual" + navMeshAgent.velocity.magnitude);

        DebugDrawCircle(transform.position, distanceToEngage, Color.yellow);  // Engage
        DebugDrawCircle(transform.position, distanceToAttack, Color.red);      // Attack
        DebugDrawCircle(transform.position, distanceToDisengage, Color.blue);    // Disengage
        //Debug.Log(Vector3.Distance(transform.position, playerTransform.position));
        //Debug.Log(animator.GetBool("isWalking"));
    }

    private void HandleIdleState()
    {
        // Logic for Idle state
        // Debug.Log("Enemy is idle.");
        navMeshAgent.isStopped = true;
        animator.SetFloat("speed", navMeshAgent.velocity.magnitude);
        // print("Distancia ao jogador: " + Vector3.Distance(transform.position, playerTransform.position));
        if (Vector3.Distance(transform.position, playerTransform.position) < distanceToEngage)
        {

            currentState = State.Chase;
            return;
        }
    }
    private void HandleChaseState()
    {
        // Logic for Chase state
        // Debug.Log("Enemy is chasing.");
        // navMeshAgent.SetDestination(playerTransform.position);
        UpdatePath();
        navMeshAgent.isStopped = false;
        animator.SetFloat("speed", navMeshAgent.velocity.magnitude);
        // Trocar para Attack se estiver perto o suficiente
        if (Vector3.Distance(transform.position, playerTransform.position) < distanceToAttack)
        {
            currentState = State.Attack;
            return;
        }
        // Trocar para Idle se estiver longe o bastante
        else if (Vector3.Distance(transform.position, playerTransform.position) > distanceToDisengage)
        {
            currentState = State.Idle;
            return;
        }
    }
    private void HandleAttackState()
    {

        animator.SetBool("isInRange",true);
        // Troca para Chase se o jogador estiver longe para o ataque
        if (Vector3.Distance(transform.position, playerTransform.position) > distanceToAttack)
        {
            currentState = State.Chase;
            animator.SetBool("isInRange",false);
            return;
        }
        // Logic for Attack state
        navMeshAgent.isStopped = true;
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(attackSpeed);
        LookAtTarget();
        if (!isAttacking)
        {
            Debug.Log("Enemy is attacking the player.");
            isAttacking = true;
            StartCoroutine(AttackCoroutine(wait));
        }
    }

    private void HandleDeadState()
    {
        // Logic for Dead state
        //Debug.Log("Enemy is dead.");
        navMeshAgent.isStopped = true;
        rb.constraints = RigidbodyConstraints.None;

        // Setando animação de idle (sem movimento)
        if (animator.GetBool("isWalking") == true)
        {
            animator.SetBool("isWalking", false);
        }

    }
    
        private IEnumerator AttackCoroutine(WaitForSecondsRealtime wait)
    {
        animator.SetTrigger("attack");
        Attack(attackDamage);
        yield return wait;
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

    public void GetHit(int damage)
    {
        //Debug.Log("Enemy got hit for " + damage + " damage.");
        if (currentState == State.Dead)
            return;
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        currentState = State.Dead;
        
        // Libera todas as constraints do Rigidbody
        rb.constraints = RigidbodyConstraints.None;
        
        // Desativa o NavMeshAgent para não interferir com a física
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        
        // Aplica força para cima
        rb.AddForce(Vector3.up * deathUpwardForce, ForceMode.Impulse);
        
        // Aplica torque aleatório para girar
        Vector3 randomTorque = new Vector3(
            Random.Range(-deathTorqueForce, deathTorqueForce),
            Random.Range(-deathTorqueForce, deathTorqueForce),
            Random.Range(-deathTorqueForce, deathTorqueForce)
        );
        rb.AddTorque(randomTorque, ForceMode.Impulse);
        
        // Destruir o objeto após alguns segundos
        Destroy(gameObject, 3f);
    }

    public void Attack(int damage)
    {
        if (playerDamageable != null)
        {
            playerDamageable.GetHit(damage);
        }
    }

    private void LookAtTarget()
    {
        Vector3 lookPos = playerTransform.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation,rotation,0.2f);

    }

    private void UpdatePath()
    {
        if (Time.time >= pathUpdateDeadline)
        {
            pathUpdateDeadline = Time.time + pathUpdateDelay;
            navMeshAgent.SetDestination(playerTransform.position);
        }
    }
}