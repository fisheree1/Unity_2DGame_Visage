using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAttackState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private AnimatorStateInfo info;
    private int AttackCount = 0;
    private bool isInAttackRange = false;

    public SlimeAttackState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        PlayAttackAnimation();
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SlimeStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        if (info.normalizedTime >= 0.95f)
        {
            if (isInAttackRange)
            {
                manager.TransitionState(SlimeStateType.Attack);
            }
            else
            {
                manager.TransitionState(SlimeStateType.Idle);
            }
        }
    }

    private void PlayAttackAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试播放不同的攻击动画，按优先级顺序
        string[] attackAnimations = {
            "Slime_attack",
            "slime_attack",
            "Attack",
            "attack",
            "SlimeAttack",
            "Hit",
            "hit",
            "SlimeHit",
            "Strike",
            "strike",
            "SlimeStrike"
        };

        foreach (string animName in attackAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到任何攻击动画，尝试使用Animator参数
        if (parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetTrigger("Attack");
            }
            catch { }
            
            try
            {
                parameter.animator.SetBool("isAttacking", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 4); // 4 通常表示attack
            }
            catch { }
        }

        Debug.LogWarning($"SlimeAttackState: No suitable attack animation found for {manager.gameObject.name}");
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
        Debug.Log("Attack Count: " + AttackCount);
        
        // 重置攻击相关的animator参数
        if (parameter?.animator != null && parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetBool("isAttacking", false);
            }
            catch { }
        }
    }
}


