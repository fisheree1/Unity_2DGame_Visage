using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMomPatrolState : IState
{
    private NPCMomP manager;
    private NPCMomParameter parameter;
    private int patrolPosition;
    private float timer=0f;
    private Rigidbody2D rb;
    private bool isWaitingAtPoint; // 新增：等待状态标志

    public NPCMomPatrolState(NPCMomP manager, NPCMomParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        this.patrolPosition = 0;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    public void OnEnter()
    {
        parameter.animator.Play("Mom_walk");
        
    }
    public void OnUpdate()

    {
        

        

        manager.FlipTo(parameter.patrolPoints[patrolPosition]);

        if (Vector2.Distance(new Vector3(manager.transform.position.x,0f,0f),new Vector3(parameter.patrolPoints[patrolPosition].position.x,0f,0f)) < 0.1f)
        {
            if (timer <= parameter.IdleTime)
            {
                patrolPosition = (patrolPosition + 1) % parameter.patrolPoints.Length;
                manager.TransitionState(NPCMomStateType.Idle);
            }
            else
            {
                
                timer += Time.deltaTime;
            }
        }
        else
        {
            manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            parameter.patrolPoints[patrolPosition].position,
            parameter.PatrolSpeed * Time.deltaTime);
        }
    }


    public void OnExit()
    {

    }
}
