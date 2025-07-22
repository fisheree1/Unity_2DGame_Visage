using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SlimeChaseState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private Rigidbody2D rb;
    private bool isInAttackRange;
    private float Timer = 0f;

    public SlimeChaseState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    public void OnEnter()
    {
        PlayChaseAnimation();
        isInAttackRange = false;
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SlimeStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        
        // 检查目标是否存在
        if (parameter.target == null)
        {
            manager.TransitionState(SlimeStateType.Idle);
            return;
        }
        
        // 保持面向目标
        manager.FlipTo(parameter.target);

        // 攻击范围检测
        isInAttackRange = CheckAttackRange();

        // 检测到攻击范围立即切换状态
        if (isInAttackRange)
        {
            if (Timer <= 0f)
            {
                Debug.Log("攻击");
                manager.TransitionState(SlimeStateType.Attack);
                Timer = parameter.attackCooldown;
                return; // 立即切换状态，避免重复触发
            }
            else
            {
                Timer -= Time.deltaTime;
                PlayIdleAnimation();
                StopMovement(); // 在攻击冷却时停止移动
            }
        }
        else
        {
            // 如果不在攻击范围内，继续追击
            PlayChaseAnimation();
            MoveTowardsTarget();
        }
    }

    private void PlayChaseAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试播放不同的追击动画，按优先级顺序
        string[] chaseAnimations = {
            "Slime_chase",
            "slime_chase",
            "Chase",
            "chase",
            "SlimeChase",
            "Slime_walk",
            "slime_walk",
            "Walk",
            "walk",
            "SlimeWalk",
            "Move",
            "move"
        };

        foreach (string animName in chaseAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到任何追击动画，尝试使用Animator参数
        if (parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetBool("isMoving", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetBool("isChasing", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 2); // 2 通常表示chasing
            }
            catch { }
            
            try
            {
                parameter.animator.SetTrigger("Chase");
            }
            catch { }
        }

        Debug.LogWarning($"SlimeChaseState: No suitable chase animation found for {manager.gameObject.name}");
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
                parameter.animator.SetBool("isChasing", false);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 0); // 0 通常表示idle
            }
            catch { }
        }
    }

    private void MoveTowardsTarget()
    {
        if (parameter.target == null) return;
        
        Vector3 targetPosition = new Vector3(
            parameter.target.position.x,
            manager.transform.position.y,
            0
        );

        Vector3 moveDir = (targetPosition - manager.transform.position).normalized;

        // 物理移动
        if (rb != null)
        {
            rb.velocity = new Vector2(
                moveDir.x * parameter.ChaseSpeed,
                rb.velocity.y
            );
        }
        else // 非物理移动
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                targetPosition,
                parameter.ChaseSpeed * Time.deltaTime
            );
        }
    }

    private bool CheckAttackRange()
    {
        if (parameter.attackPoint == null) return false;
        
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
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
        isInAttackRange = false;
        
        // 重置追击相关的animator参数
        if (parameter?.animator != null && parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetBool("isChasing", false);
            }
            catch { }
        }
    }
}
