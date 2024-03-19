using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : Animal
{
    [Header("Predator Variables")]
    [SerializeField] private float DetectionRange = 20f;
    [SerializeField] private float ChaseTime = 10f;
    [SerializeField] private int BiteDamage = 3;
    [SerializeField] private float BiteCoolDown = 1f;

    private Prey CurrentChaseTarget;

    protected override void CheckChase()
    {
        if (CurrentChaseTarget)
        {
            return;
        }

        Collider[] colliders = new Collider[10];
        int NumOfTargets = Physics.OverlapSphereNonAlloc(transform.position, DetectionRange, colliders);

        for(int i = 0; i < NumOfTargets; i++)
        {
            Prey prey = colliders[i].GetComponent<Prey>();

            if (prey != null)
            {
                StarChase(prey);
                return;
            }
        }

        CurrentChaseTarget = null;
    }

    private void StarChase(Prey prey)
    {
        CurrentChaseTarget = prey;
        SetState(AnimalState.Chase);
    }

    protected override void HandleChase()
    {
        if(CurrentChaseTarget != null)
        {
            CurrentChaseTarget.AlertPrey(this);
            StartCoroutine(ChasePrey());
        }
        else
        {
            SetState(AnimalState.Idle);
        }

    }

    private IEnumerator ChasePrey()
    {
        float StartTime = Time.time;

        while(CurrentChaseTarget != null && Vector3.Distance(transform.position, CurrentChaseTarget.transform.position) > NMA.stoppingDistance)
        {
            if((Time.time - StartTime) > ChaseTime || CurrentChaseTarget == null)
            {
                StopChase();
                yield break;
            }

            SetState(AnimalState.Chase);
            NMA.SetDestination(CurrentChaseTarget.transform.position);

            yield return null;
        }

        if (CurrentChaseTarget)
        {
            CurrentChaseTarget.TakeDamage(BiteDamage);
        }

        yield return new WaitForSeconds(BiteCoolDown);

        CurrentChaseTarget = null;

        HandleChase();
        CheckChase();
    }

    private void StopChase()
    {
        NMA.ResetPath();
        CurrentChaseTarget = null;
        SetState(AnimalState.Idle);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, DetectionRange);
    }
}
