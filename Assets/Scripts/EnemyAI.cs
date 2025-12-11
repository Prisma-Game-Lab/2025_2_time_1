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
    public static System.Action<EnemyAI> OnEnemyDied;

    [Header("Configurações")]
    [SerializeField] private float distanceToEngage = 15f;
    [SerializeField] private float distanceToDisengage = 20f;
    [SerializeField] private float distanceToAttack = 4f;
    [SerializeField] private float attackSpeed = .6f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] public int health = 30;

    [Header("Death Physics")]
    [SerializeField] private float deathUpwardForce = 10f;
    [SerializeField] private float deathTorqueForce = 5f;

    [Header("Strong Punch")]
    [SerializeField] private float strongPunchLaunchForce = 420f;
  

    private bool isAttacking = false;
    private bool isDead = false;

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
    private Collider col;

    private enum State { Idle, Chase, Attack, Dead }
    private State currentState;
    private float pathUpdateDeadline = 0;
    private float pathUpdateDelay = 0.2f;
    private bool isAttacking = false;

    void Start()
    {
        currentState = State.Idle;

        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
            playerDamageable = playerGO.GetComponent<IDamageable>();
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.stoppingDistance = distanceToAttack - 0.1f;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        animator = GetComponent<Animator>();
    
        col = GetComponent<Collider>();

        // deixa a movimentação normal
        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        switch (currentState)
        {
            case State.Idle: HandleIdleState(); break;
            case State.Chase: HandleChaseState(); break;
            case State.Attack: HandleAttackState(); break;
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
        if (IsAgentReady())
            navMeshAgent.isStopped = true;
        animator.SetFloat("speed", navMeshAgent.velocity.magnitude);
        if (playerTransform == null) return;

        if (Vector3.Distance(transform.position, playerTransform.position) < distanceToEngage)
            currentState = State.Chase;
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
        else if (Vector3.Distance(transform.position, playerTransform.position) > distanceToDisengage || playerTransform == null)
        {
            currentState = State.Idle;
            return;
        }

        if (IsAgentReady())
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(playerTransform.position);
        }

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist < distanceToAttack)
            currentState = State.Attack;
        else if (dist > distanceToDisengage)
            currentState = State.Idle;
    }

    private void HandleAttackState()
    {

        if (playerTransform == null) return;

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
        LookAtTarget();
        if (!isAttacking)
        {
            StartCoroutine(AttackRoutine());            
        }
    }
    
        private IEnumerator AttackCoroutine(WaitForSecondsRealtime wait)
    {
        animator.SetTrigger("attack");
        Attack(attackDamage);
        yield return wait;
        isAttacking = false;

    }

    // ==========================================================
    // DAMAGE NORMAL
    // ==========================================================

    public void GetHit(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
            Die();
    }

    // ==========================================================
    // STRONG PUNCH (VERSÃO QUE FUNCIONA)
    // ==========================================================

    public void TakeStrongPunch(Vector3 direction)
    {
        if (isDead) return;
        isDead = true;

        currentState = State.Dead;
        OnEnemyDied?.Invoke(this);

        // DESATIVA O NAVMESH
        if (navMeshAgent != null)
        {
            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = true;

            navMeshAgent.enabled = false;
        }

        
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // DIREÇÃO + LEVE UP
        Vector3 launchDir = (direction.normalized + Vector3.up * 0.7f).normalized;

       
        rb.AddForce(launchDir * strongPunchLaunchForce * 0.6f, ForceMode.VelocityChange);


        rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);

        // só desativa o collider depois que ele já voou longe
        StartCoroutine(DisableColliderLater());

        Destroy(gameObject, 4f);
    }

    private IEnumerator DisableColliderLater()
    {
        yield return new WaitForSeconds(2f);
        if (col != null)
            col.enabled = false;
    }

    // ==========================================================
    // MORTE NORMAL
    // ==========================================================

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = State.Dead;

        OnEnemyDied?.Invoke(this);

        if (navMeshAgent != null)
        {
            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = true;

            navMeshAgent.enabled = false;
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.AddForce(Vector3.up * deathUpwardForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * deathTorqueForce, ForceMode.Impulse);

        Destroy(gameObject, 3f);
    }

    public void Attack(int damage)
    {
        if (playerDamageable != null)
            playerDamageable.GetHit(damage);
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

    private bool IsAgentReady()
    {
        return navMeshAgent != null &&
               navMeshAgent.enabled &&
               navMeshAgent.isOnNavMesh;
    }
}
