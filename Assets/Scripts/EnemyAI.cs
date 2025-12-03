using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
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
    private Collider col;

    private enum State { Idle, Chase, Attack, Dead }
    private State currentState;

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
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // deixa a movimentação normal
        rb.isKinematic = true;
    }

    void Update()
    {
        if (isDead) return;

        switch (currentState)
        {
            case State.Idle: HandleIdleState(); break;
            case State.Chase: HandleChaseState(); break;
            case State.Attack: HandleAttackState(); break;
        }
    }

    private void HandleIdleState()
    {
        if (IsAgentReady())
            navMeshAgent.isStopped = true;

        if (playerTransform == null) return;

        if (Vector3.Distance(transform.position, playerTransform.position) < distanceToEngage)
            currentState = State.Chase;
    }

    private void HandleChaseState()
    {
        if (playerTransform == null)
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
        if (isAttacking || playerTransform == null) return;

        if (IsAgentReady())
            navMeshAgent.isStopped = true;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        Attack(attackDamage);

        yield return new WaitForSeconds(attackSpeed);

        isAttacking = false;

        if (!isDead && playerTransform != null)
            currentState = State.Chase;
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

    void Attack(int damage)
    {
        if (playerDamageable != null)
            playerDamageable.GetHit(damage);
    }

    private bool IsAgentReady()
    {
        return navMeshAgent != null &&
               navMeshAgent.enabled &&
               navMeshAgent.isOnNavMesh;
    }
}
