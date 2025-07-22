using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimePatrolState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private int patrolPosition;
    private float timer = 0f;
    private Rigidbody2D rb;
    private bool isWaitingAtPoint; // 在巡逻点等待状态标志

    public SlimePatrolState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        this.patrolPosition = 0;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    
    public void OnEnter()
    {
        // 尝试播放行走动画
        PlayWalkAnimation();
        
        isWaitingAtPoint = false;
        timer = 0f;
    }
    
    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SlimeStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        
        // 检查巡逻点配置
        if (parameter.patrolPoints == null || parameter.patrolPoints.Length == 0)
        {
            manager.TransitionState(SlimeStateType.Idle);
            return;
        }

        // 检查是否应该切换到追击状态
        if (parameter.target != null && parameter.chasePoints != null && parameter.chasePoints.Length >= 2)
        {
            if (parameter.target.position.x >= parameter.chasePoints[0].position.x &&
                parameter.target.position.x <= parameter.chasePoints[1].position.x)
            {
                manager.TransitionState(SlimeStateType.Chase);
                return;
            }
        }

        // 面向当前巡逻点
        if (parameter.patrolPoints[patrolPosition] != null)
        {
            manager.FlipTo(parameter.patrolPoints[patrolPosition]);
        }

        // 检查是否到达巡逻点
        if (Vector2.Distance(manager.transform.position, parameter.patrolPoints[patrolPosition].position) < 0.1f)
        {
            if (!isWaitingAtPoint)
            {
                // 刚到达巡逻点，开始等待
                isWaitingAtPoint = true;
                timer = 0f;
                PlayIdleAnimation();
                StopMovement();
            }
            else
            {
                // 在巡逻点等待
                timer += Time.deltaTime;
                if (timer >= parameter.IdleTime)
                {
                    // 等待时间结束，移动到下一个巡逻点
                    patrolPosition = (patrolPosition + 1) % parameter.patrolPoints.Length;
                    isWaitingAtPoint = false;
                    PlayWalkAnimation();
                }
            }
        }
        else
        {
            // 移动到巡逻点
            if (isWaitingAtPoint)
            {
                isWaitingAtPoint = false;
                PlayWalkAnimation();
            }
            
            MoveTowardsPatrolPoint();
        }
    }

    private void PlayWalkAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试播放不同的行走动画，按优先级顺序
        string[] walkAnimations = {
            "Slime_walk",
            "slime_walk",
            "Walk",
            "walk",
            "SlimeWalk",
            "Move",
            "move",
            "SlimeMove"
        };

        foreach (string animName in walkAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到任何行走动画，尝试使用Animator参数
        if (parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetBool("isMoving", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 1); // 1 通常表示walking
            }
            catch { }
            
            try
            {
                parameter.animator.SetTrigger("Walk");
            }
            catch { }
        }

        Debug.LogWarning($"SlimePatrolState: No suitable walk animation found for {manager.gameObject.name}");
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
        }
    }

    private void MoveTowardsPatrolPoint()
    {
        if (parameter.patrolPoints[patrolPosition] == null) return;
        
        Vector3 targetPosition = parameter.patrolPoints[patrolPosition].position;
        Vector3 moveDir = (targetPosition - manager.transform.position).normalized;

        // 物理移动
        if (rb != null)
        {
            rb.velocity = new Vector2(
                moveDir.x * parameter.PatrolSpeed,
                rb.velocity.y
            );
        }
        else // 非物理移动
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                targetPosition,
                parameter.PatrolSpeed * Time.deltaTime
            );
        }
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
        StopMovement();
        isWaitingAtPoint = false;
    }
}
