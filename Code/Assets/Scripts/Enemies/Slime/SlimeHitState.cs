using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHitState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private AnimatorStateInfo info;
    private Rigidbody2D rb;
    private float knockbackTimer = 0f;
    private float knockbackDuration = 0.3f; // 击退持续时间
    private Vector2 knockbackDirection;
    private float knockbackForce = 8f; // 击退力度

    public SlimeHitState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    
    public void OnEnter()
    {
        // 尝试播放受击动画
        PlayHurtAnimation();
        
        // 获取击退方向 - 从玩家位置到敌人位置
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 direction = (manager.transform.position - player.transform.position).normalized;
            knockbackDirection = new Vector2(direction.x, 0); // 只在水平方向击退
        }
        else
        {
            // 如果找不到玩家，使用当前面向的反方向
            knockbackDirection = new Vector2(manager.transform.localScale.x > 0 ? -1 : 1, 0);
        }
        
        knockbackTimer = 0f;
        
        // 应用初始击退力
        if (rb != null)
        {
            rb.velocity = new Vector2(knockbackDirection.x * knockbackForce, rb.velocity.y);
        }
    }

    public void OnUpdate()
    {
        knockbackTimer += Time.deltaTime;
        
        // 在击退期间逐渐减少击退速度
        if (knockbackTimer < knockbackDuration && rb != null)
        {
            float knockbackLerp = 1f - (knockbackTimer / knockbackDuration);
            float currentKnockbackSpeed = knockbackForce * knockbackLerp;
            rb.velocity = new Vector2(knockbackDirection.x * currentKnockbackSpeed, rb.velocity.y);
        }
        else if (rb != null)
        {
            // 击退结束后停止水平移动
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.IsDead)
            {
                manager.TransitionState(SlimeStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // 重置被击中状态
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // 重新获取目标
                manager.TransitionState(SlimeStateType.Chase); // 切换到追逐状态
            }
        }
    }

    private void PlayHurtAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试播放不同的受击动画，按优先级顺序
        string[] hurtAnimations = {
            "Slime_hurt",
            "slime_hurt",
            "Hurt",
            "hurt",
            "SlimeHurt",
            "Hit",
            "hit",
            "SlimeHit",
            "Damaged",
            "damaged",
            "SlimeDamaged"
        };

        foreach (string animName in hurtAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到任何受击动画，尝试使用Animator参数
        if (parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetTrigger("Hit");
            }
            catch { }
            
            try
            {
                parameter.animator.SetTrigger("Hurt");
            }
            catch { }
            
            try
            {
                parameter.animator.SetBool("isHit", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 3); // 3 通常表示hurt
            }
            catch { }
        }

        Debug.LogWarning($"SlimeHitState: No suitable hurt animation found for {manager.gameObject.name}");
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
        parameter.isHit = false; // 确保退出时重置被击中状态
        
        // 停止击退移动
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // 重置击中相关的animator参数
        if (parameter?.animator != null && parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetBool("isHit", false);
            }
            catch { }
        }
    }
}


