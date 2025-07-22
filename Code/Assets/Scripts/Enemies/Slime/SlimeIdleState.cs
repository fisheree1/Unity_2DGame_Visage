using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeIdleState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private float idleTimer = 0f;
    private Rigidbody2D rb;

    public SlimeIdleState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    public void OnEnter()
    {
        // 尝试播放idle动画，如果不存在则尝试备用动画
        PlayIdleAnimation();
        
        idleTimer = 0f;
        StopMovement();
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SlimeStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        
        // 1. 优先检测：确保target存在
        if (parameter.target != null)
        {
            // 2. 检查目标是否在追击范围
            if (parameter.chasePoints != null && parameter.chasePoints.Length >= 2)
            {
                bool inChaseZone =
                    parameter.target.position.x >= parameter.chasePoints[0].position.x &&
                    parameter.target.position.x <= parameter.chasePoints[1].position.x;

                if (inChaseZone && parameter.target.CompareTag("Player"))
                {
                    manager.TransitionState(SlimeStateType.Chase);
                    return; // 立即退出状态更新
                }
            }
        }

        // 3. 计时等待
        idleTimer += Time.deltaTime;

        // 4. 计时结束后切换到巡逻状态（如果存在巡逻点）
        if (idleTimer >= parameter.IdleTime)
        {
            // 检查是否有有效的巡逻点
            if (parameter.patrolPoints != null && parameter.patrolPoints.Length > 0)
            {
                manager.TransitionState(SlimeStateType.Patrol);
            }
            else
            {
                // 没有巡逻点就保持待机，重置计时器
                idleTimer = 0;
            }
        }
    }

    private void PlayIdleAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试播放不同的idle动画，按优先级顺序
        string[] idleAnimations = {
            "Slime_idle",
            "slime_idle", 
            "Idle",
            "idle",
            "SlimeIdle",
            "Default"
        };

        foreach (string animName in idleAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到任何idle动画，尝试使用Animator参数
        if (parameter.animator.parameters.Length > 0)
        {
            // 尝试设置常见的animator参数
            try
            {
                parameter.animator.SetBool("isMoving", false);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 0); // 0 通常表示idle
            }
            catch { }
            
            try
            {
                parameter.animator.SetTrigger("Idle");
            }
            catch { }
        }

        Debug.LogWarning($"SlimeIdleState: No suitable idle animation found for {manager.gameObject.name}");
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private bool HasAnimationState(string stateName)
    {
        if (parameter?.animator == null) return false;

        // 检查动画控制器是否有指定状态
        var controller = parameter.animator.runtimeAnimatorController;
        if (controller == null) return false;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }

        return false;
    }

    public void OnExit()
    {
        // 退出时重置状态数据
        idleTimer = 0;
    }
}
