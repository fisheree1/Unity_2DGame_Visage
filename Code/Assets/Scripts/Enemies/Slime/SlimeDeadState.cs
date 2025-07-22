using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeDeadState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private bool isDestroying = false;

    public SlimeDeadState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime entering Dead State");

        // 验证动画器
        if (parameter?.animator == null)
        {
            Debug.LogError("SlimeDeadState: Animator is null!");
            DestroySlime();
            return;
        }

        // 尝试播放死亡动画
        PlayDeathAnimation();

        // 禁用碰撞器，防止死亡后还能被攻击
        var colliders = manager.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // 停止移动
        var rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        destroyTimer = 0f;
        isDestroying = false;
    }

    public void OnUpdate()
    {
        if (isDestroying) return;

        destroyTimer += Time.deltaTime;

        if (destroyTimer >= destroyDelay)
        {
            DestroySlime();
        }
    }

    private void PlayDeathAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试播放不同的死亡动画，按优先级顺序
        string[] deathAnimations = {
            "Slime_death",
            "slime_death",
            "Death",
            "death",
            "SlimeDeath",
            "Die",
            "die",
            "SlimeDie",
            "Dead",
            "dead",
            "SlimeDead"
        };

        foreach (string animName in deathAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到任何死亡动画，尝试使用Animator参数
        if (parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetTrigger("Death");
            }
            catch { }
            
            try
            {
                parameter.animator.SetTrigger("Die");
            }
            catch { }
            
            try
            {
                parameter.animator.SetBool("isDead", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 5); // 5 通常表示death
            }
            catch { }
        }

        Debug.LogWarning($"SlimeDeadState: No suitable death animation found for {manager.gameObject.name}");
    }

    private void DestroySlime()
    {
        if (isDestroying) return;

        isDestroying = true;

        // 可以在这里添加死亡效果，比如掉落物品、播放音效等
        // DropLoot();
        // PlayDeathSound();

        Debug.Log("Slime destroyed after death animation");
        Object.Destroy(manager.gameObject);
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
        // 死亡状态通常不会退出，但为了完整性保留
        Debug.Log("SlimeDeadState: OnExit called (this should not happen)");
    }
}