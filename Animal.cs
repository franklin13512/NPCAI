using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public enum AnimalState//Two state for animals
{
    Idle,
    Moving,
    Chase,
}
//The script needs a NavMeshAgent
[RequireComponent(typeof(NavMeshAgent))]
//[RequireComponent(typeof(BoxCollider))]
public class Animal : MonoBehaviour
{
    [Header("Wandering")]
    [SerializeField] private float WanderDistance = 50f;
    [SerializeField] private float WanderSpeed = 5f;
    [SerializeField] private float WanderTime = 6f;

    [Header("Idle")]
    [SerializeField] private float IdleTime = 5f;

    [Header("Chase")]
    [SerializeField] private float RunSpeed = 8f;

    [Header("property")]
    [SerializeField] private int Health = 10;

    protected NavMeshAgent NMA;
    protected AnimalState CurrentState = AnimalState.Idle;

    private void Start()
    {
        InitialState();
    }

    protected void InitialState()
    {
        NMA = GetComponent<NavMeshAgent>();
        NMA.speed = WanderSpeed;

        CurrentState = AnimalState.Idle;
        UpdatedState();
    }

    protected void UpdatedState()
    {
        switch (CurrentState)
        {
            case AnimalState.Idle:
                HandleIdle();
                break;
            case AnimalState.Moving:
                HandleMoving();
                break;
            case AnimalState.Chase:
                HandleChase();
                break;
        }
    }

    protected Vector3 GetRandomPositionForNav(Vector3 OriginalPos, float Distance)
    {
        for(int i = 0;i < 5; i++)
        {
            Vector3 RandomDirection = Random.insideUnitSphere * Distance;
            RandomDirection += OriginalPos;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(RandomDirection, out hit, Distance, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return OriginalPos;
        
    }

    protected virtual void HandleChase()
    {
        StopAllCoroutines();
    }

    protected virtual void CheckChase()
    {

    }

    protected void HandleIdle()
    {
        StartCoroutine(WaitForMove());
    }

    private IEnumerator WaitForMove() 
    {
        float WaitTime = Random.Range(IdleTime / 2, IdleTime);
        yield return new WaitForSeconds(WaitTime);

        Vector3 RandomDestination = GetRandomPositionForNav(transform.position, WanderDistance);

        NMA.SetDestination(RandomDestination);
        Debug.Log("Set new destination");
        SetState(AnimalState.Moving);
    }

    protected void HandleMoving()
    {
        StartCoroutine(WaitForReachDestination());
    }

    private IEnumerator WaitForReachDestination()
    {
        float StartTime = Time.time;

        while (NMA.pathPending || NMA.remainingDistance > NMA.stoppingDistance && NMA.isActiveAndEnabled)
        {
            if(Time.time - StartTime >= WanderTime)
            {
                NMA.ResetPath();
                SetState(AnimalState.Idle);
                yield break;
            }

            CheckChase();

            yield return null;
        }
        SetState(AnimalState.Idle);
    }

    protected void SetState(AnimalState NewState)
    {
        if(CurrentState == NewState) return;

        CurrentState = NewState;
        ChangingState(NewState);
        //Debug.Log("1");
    }
    
    protected void ChangingState(AnimalState NewState)
    {
        if (NewState == AnimalState.Moving)
        {
            NMA.speed = WanderSpeed;
        }
        if(NewState == AnimalState.Chase) {
            NMA.speed = RunSpeed;
        }

        UpdatedState();
        //Debug.Log("2");
    }

    public void TakeDamage(int Damage)
    {
        Health -= Damage;

        if(Health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
