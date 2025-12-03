using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour, IDamageable
{
    // 🔥 EVENTO DE MORTE PARA O SISTEMA DE PORTAS
    public static System.Action<EnemyAI> OnEnemyDied;

    [SerializeField] private float distanceToEngage = 15f;
    [SerializeField] private float distanceToDisengage = 20f;
    [SerializeField] private float distanceToAttack = 4f;
    [SerializeField] private float attackSpeed = .6f; // tempo entre ataques
    [SerializeField] private int attackDamage = 10;
    [SerializeField] public int health = 30;

    [Header("Death Physics")]
    [SerializeField] private float deathUpwardForce = 10f;
    [SerializeField] private float deathTorqueForce = 5f;

    [Header("Strong Punch Death")]
    [SerializeField] private float strongPunchLaunchForce = 40f;

    private bool isAttacking = false;
    private bool isDead = false;

    private Transform playerTransform;
    private IDamageable playerDamageable;
    private NavMeshAgent navMeshAgent;
    private Rigidbody rb;
    private Collider col;

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

        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
            playerDamageable = playerGO.GetComponent<IDamageable>();
        }
        else
        {
            Debug.LogWarning("[EnemyAI] Player com tag 'Player' não encontrado na cena.");
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Pequena proteção: se o agent existir, desliga atualização de rotação se você rotacionar manualmente
        if (navMeshAgent != null)
        {
            navMeshAgent.updateRotation = true;
        }

        // Mantém rotações livres (mas trava eixos de rotação X/Z)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (isDead) return;

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

        DebugDrawCircle(transform.position, distanceToEngage, Color.yellow);
        DebugDrawCircle(transform.position, distanceToAttack, Color.red);
        DebugDrawCircle(transform.position, distanceToDisengage, Color.blue);
    }

    private void HandleIdleState()
    {
        // Pare o agente se ele estiver ativo no NavMesh
        if (IsAgentReady())
        {
            navMeshAgent.isStopped = true;
        }

        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist < distanceToEngage && !isAttacking)
        {
            currentState = State.Chase;
        }
    }

    private void HandleChaseState()
    {
        if (playerTransform == null)
        {
            currentState = State.Idle;
            return;
        }

        // Só definimos destino se o agente estiver pronto
        if (IsAgentReady())
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(playerTransform.position);
        }

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist < distanceToAttack)
        {
            currentState = State.Attack;
        }
        else if (dist > distanceToDisengage)
        {
            currentState = State.Idle;
        }
    }

    private void HandleAttackState()
    {
        // evita reentrância no ataque
        if (isAttacking || playerTransform == null) return;

        // se o agente estiver pronto, pare-o antes do ataque
        if (IsAgentReady())
        {
            navMeshAgent.isStopped = true;
        }

        // executa ataque controlado por coroutine
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Aplica dano imediatamente (ou toca animação/espera conforme design)
        Attack(attackDamage);

        // aguarda cooldown (usa WaitForSeconds normal; se quiser realtime, troca)
        float wait = Mathf.Max(0.01f, attackSpeed);
        float elapsed = 0f;
        while (elapsed < wait)
        {
            if (isDead) // se morrer no meio do cooldown, aborta
            {
                isAttacking = false;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;

        // volta pra chase se ainda houver player e não estiver morto
        if (!isDead && playerTransform != null)
            currentState = State.Chase;
        else if (!isDead)
            currentState = State.Idle;
    }

    private void HandleDeadState()
    {
        // quando morrer, só aplica física. navMeshAgent deve estar desativado
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            // só para garantir que não chamamos isStopped em um agente inválido
            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = true;

            navMeshAgent.enabled = false;
        }

        rb.constraints = RigidbodyConstraints.None;
    }

    private IEnumerator AttackCooldown(WaitForSecondsRealtime wait)
    {
        yield return wait;
        // não usado agora, mantido por compatibilidade
    }

    private void DebugDrawCircle(Vector3 center, float radius, Color color, int segments = 50)
    {
        float angleStep = 360f / segments;
        Vector3 prev = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float ang = i * angleStep * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(Mathf.Cos(ang) * radius, 0, Mathf.Sin(ang) * radius);
            Debug.DrawLine(prev, next, color);
            prev = next;
        }
    }

    // ------------------------------------------------------------
    //  DAMAGE RECEBIDO
    // ------------------------------------------------------------

    public void GetHit(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    // 🔥 CHAMADO PELO PLAYER PARA SOCO FORTE
    public void TakeStrongPunch(Vector3 direction)
    {
        if (isDead) return;

        isDead = true;
        currentState = State.Dead;

        // evento da porta/sala
        OnEnemyDied?.Invoke(this);

        // desativa navmesh com segurança
        if (navMeshAgent != null)
        {
            if (navMeshAgent.isOnNavMesh)
            {
                // tenta parar o agente antes de desabilitar
                navMeshAgent.isStopped = true;
            }
            navMeshAgent.enabled = false;
        }

        // libera física e lança o inimigo
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(direction * strongPunchLaunchForce, ForceMode.Impulse);

        // desativa colisão após o launch (evita prender em geometria)
        StartCoroutine(DisableColliderAfterDelay());

        Destroy(gameObject, 3f);
    }

    private IEnumerator DisableColliderAfterDelay()
    {
        yield return new WaitForSeconds(1.2f);
        if (col != null)
            col.enabled = false;
    }

    // ------------------------------------------------------------
    //  MORTE NORMAL
    // ------------------------------------------------------------

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = State.Dead;

        // evento
        OnEnemyDied?.Invoke(this);

        // desativa navmesh com segurança
        if (navMeshAgent != null)
        {
            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = true;

            navMeshAgent.enabled = false;
        }

        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(Vector3.up * deathUpwardForce, ForceMode.Impulse);

        Vector3 torque = new Vector3(
            Random.Range(-deathTorqueForce, deathTorqueForce),
            Random.Range(-deathTorqueForce, deathTorqueForce),
            Random.Range(-deathTorqueForce, deathTorqueForce)
        );
        rb.AddTorque(torque, ForceMode.Impulse);

        Destroy(gameObject, 3f);
    }

    void Attack(int damage)
    {
        if (playerDamageable != null)
            playerDamageable.GetHit(damage);
    }

    // ------------------- UTIL --------------------
    private bool IsAgentReady()
    {
        // Verifica se o agent existe, está habilitado e foi colocado em um NavMesh válido
        return navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh;
    }
}
