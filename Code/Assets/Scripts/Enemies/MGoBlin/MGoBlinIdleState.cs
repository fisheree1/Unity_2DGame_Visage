using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinIdleState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private float idleTimer=0f;

    public MGoBlinIdleState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        
        parameter.animator.Play("MGoBlin_idle");
        idleTimer = 0f;
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        // 1. 空引用检查：确保target存在
        if (parameter.target != null)
        {
            // 2. 优先检测玩家是否进入追击范围
            bool inChaseZone =
                parameter.target.position.x >= parameter.chasePoints[0].position.x &&
                parameter.target.position.x <= parameter.chasePoints[1].position.x;

            if (inChaseZone && parameter.target.CompareTag("Player"))
            {
                manager.TransitionState(MGoBlinStateType.Chase);
                return; // 立即退出状态更新
            }
        }

        // 3. 待机计时
        idleTimer += Time.deltaTime;

        // 4. 计时结束后切换到巡逻状态（如果存在巡逻点）
        if (idleTimer >= parameter.IdleTime)
        {
            // 检查是否存在有效的巡逻点
            if (parameter.patrolPoints != null && parameter.patrolPoints.Length > 0)
            {
                manager.TransitionState(MGoBlinStateType.Patrol);
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
