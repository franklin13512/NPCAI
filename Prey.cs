using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Prey : Animal
{
    [Header("Prey Variables")]
    [SerializeField] private float DetectionRange = 10f;
    [SerializeField] private float EscapeDistance = 80f;

    private Predator CurrentPredator = null;

    public void AlertPrey(Predator predator)
    {
        SetState(AnimalState.Chase);
        CurrentPredator = predator;
        StartCoroutine(RunFromPredator());
    }

    private IEnumerator RunFromPredator()
    {
        while(CurrentPredator == null || Vector3.Distance(transform.position, CurrentPredator.transform.position) > DetectionRange){
            yield return null; 
        }

        while(CurrentPredator != null && Vector3.Distance(transform.position, CurrentPredator.transform.position) <= DetectionRange)
        {
            RunAwayFromPredator();

            yield return null;
        }

        if(!NMA.pathPending && NMA.remainingDistance > NMA.stoppingDistance)
        {
            yield return null;
        }

        SetState(AnimalState.Idle);
    }

    private void RunAwayFromPredator()
    {
        if(NMA != null && NMA.isActiveAndEnabled)
        {
            if(!NMA.pathPending && NMA.remainingDistance < NMA.stoppingDistance)
            {
                Vector3 RunDirection = transform.position - CurrentPredator.transform.position;
                Vector3 EscapeDestination = transform.position + RunDirection.normalized * (EscapeDistance * 2);
                NMA.SetDestination(GetRandomPositionForNav(EscapeDestination, EscapeDistance));

            }
        }
    }

    protected override void Die()
    {
        StopAllCoroutines();
        base.Die();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, DetectionRange);
    }
}

