using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Idle,
        Chase,
        Attack
    }

    private State currentState;

    void Start()
    {
        currentState = State.Idle;
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
        }
    }

    private void HandleIdleState()
    {
        // Logic for Idle state
        Debug.Log("Enemy is idle.");

        // Checa pra ver se a distância do jogador é menor que 10 unidades
        float distanceToEngage = 10f;
        if (Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) < distanceToEngage)
        {
            currentState = State.Chase;
        }
    }
    private void HandleChaseState()
    {
        // Logic for Chase state
        Debug.Log("Enemy is chasing the player.");

        // Checa pra ver se a distância do jogador é menor que 2 unidades
        float distanceToAttack = 2f;
        float distanceToDisengage = 15f;
        if (Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) < distanceToAttack)
        {
            currentState = State.Attack;
        }
        else if (Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) > distanceToDisengage)
        {
            currentState = State.Idle;
        }
    }
    private void HandleAttackState()
    {
        // Logic for Attack state
        Debug.Log("Enemy is attacking the player.");
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
        StartCoroutine(AttackCooldown(wait));
    }

    private IEnumerator AttackCooldown(WaitForSecondsRealtime wait)
    {
        Debug.Log("Enemy is cooling down after attack.");
        yield return wait;
        currentState = State.Chase;
    }
}