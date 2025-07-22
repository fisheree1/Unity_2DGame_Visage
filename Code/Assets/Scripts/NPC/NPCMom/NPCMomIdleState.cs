using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMomIdleState : IState
{
    private NPCMomP manager;
    private NPCMomParameter parameter;
    private float idleTimer = 0f;

    public NPCMomIdleState(NPCMomP manager, NPCMomParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        parameter.animator.Play("Mom_idle");
        idleTimer = 0f;
    }

    public void OnUpdate()
    {
        // 待机计时
        idleTimer += Time.deltaTime;

        // 计时结束后切换到巡逻状态（如果存在巡逻点）
        if (idleTimer >= parameter.IdleTime)
        {
            // 检查是否存在有效的巡逻点
            if (parameter.patrolPoints != null && parameter.patrolPoints.Length > 0)
            {
                manager.TransitionState(NPCMomStateType.Patrol);
            }
            else
            {
                // 没有巡逻点则保持待机，重置计时器
                idleTimer = 0;
            }
        }
    }

    public void OnExit()
    {
        // 退出时重置状态变量
        idleTimer = 0;
    }
}
