using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WendigoPatrolState : IState
{
    private WendigoP manager;
    private WendigoParameter parameter;
    private int patrolPosition;
    private float timer=0f;
    private Rigidbody2D rb;
    private bool isWaitingAtPoint; // 新增：等待状态标志

    public WendigoPatrolState(WendigoP manager, WendigoParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        this.patrolPosition = 0;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    public void OnEnter()
    {
        parameter.animator.Play("Wendigo_walk");
        
    }
    public void OnUpdate()

    {
        if (parameter.isHit)
        {
            manager.TransitionState(WendigoStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        if (parameter.patrolPoints == null || parameter.patrolPoints.Length == 0)
            return;

        if (parameter.target != null &&
            parameter.target.position.x >= parameter.chasePoints[0].position.x &&
            parameter.target.position.x <= parameter.chasePoints[1].position.x)
        {
            manager.TransitionState(WendigoStateType.Chase);
            return;
        }

        manager.FlipTo(parameter.patrolPoints[patrolPosition]);

        if (Vector2.Distance(manager.transform.position, parameter.patrolPoints[patrolPosition].position) < 0.1f)
        {
            if (timer <= parameter.IdleTime)
            {
                patrolPosition = (patrolPosition + 1) % parameter.patrolPoints.Length;
                manager.TransitionState(WendigoStateType.Idle);
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
