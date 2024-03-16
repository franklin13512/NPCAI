using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public enum AnimalState//Two state for animals
{
    Idle,
    Moving,
}
//The script needs a NavMeshAgent
[RequireComponent(typeof(NavMeshAgent))]
public class Animal : MonoBehaviour
{
    [Header("Wandering")]
    public float WanderDistance = 50f;
    public float WanderSpeed = 5f;
    public float WanderTime = 6f;

    [Header("Idle")]
    public float IdleTime = 5f;

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
        }
    }

    protected Vector3 GetRandomPositionForNav(Vector3 OriginalPos, float Distance)
    {
        Vector3 RandomDirection = Random.insideUnitSphere * Distance;
        RandomDirection += OriginalPos;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(RandomDirection, out hit, Distance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return GetRandomPositionForNav(OriginalPos, Distance);
        }
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

        while (NMA.remainingDistance > NMA.stoppingDistance)
        {
            if(Time.time - StartTime >= WanderTime)
            {
                NMA.ResetPath();
                SetState(AnimalState.Idle);
                yield break;
            }

            yield return null;
        }
        SetState(AnimalState.Idle);
    }

    protected void SetState(AnimalState NewState)
    {
        if(CurrentState == NewState) return;

        CurrentState = NewState;
        ChangingState(NewState);
        Debug.Log("1");
    }
    
    protected void ChangingState(AnimalState NewState)
    {
        UpdatedState();
        Debug.Log("2");
    }
}
