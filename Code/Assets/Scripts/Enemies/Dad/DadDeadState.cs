using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class DadDeadState : IState
{
    private DadP manager;
    private DadParameter parameter;
    
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private readonly float maxDestroyTime = 2.0f; // 最大等待时间，防止对象永不销毁
    private bool isDestroying = false;
    private AnimatorStateInfo info;

    public DadDeadState(DadP manager, DadParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;

    }
    
    public void OnEnter()
    {
        Debug.Log("Dad entering Dead State");
        
        // 验证管理器
        if (manager == null)
        {
            Debug.LogError("DadDeadState: Manager is null!");
            return;
        }
        
        // 安全播放死亡动画
        if (parameter?.animator != null)
        {
            if (HasAnimationState("Dad_death"))
            {
                parameter.animator.Play("Dad_death");
            }
            else
            {
                Debug.LogWarning("DadDeadState: Animation state 'Dad_death' not found!");
            }
        }
        else
        {
            Debug.LogWarning("DadDeadState: Animator is null, skipping death animation");
        }
        
        // 禁用碰撞体以防止进一步交互
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
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (isDestroying) return;
        
        destroyTimer += Time.deltaTime;
        
        // 正常销毁时间到达或超过最大等待时间
        if (destroyTimer >= destroyDelay || destroyTimer >= maxDestroyTime||info.normalizedTime >=0.95f)
        {
            DestroyDad();
        }
    }
    
    private void DestroyDad()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // 这里可以添加掉落物品或特效等其他死亡效果
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log($"Dad destroyed after {destroyTimer:F2} seconds");
        
        if (manager != null && manager.gameObject != null)
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Debug.LogWarning("DadDeadState: Manager or GameObject is null during destruction");
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
        // 死亡状态通常不会退出，因为对象会被销毁
        Debug.Log("DadDeadState: OnExit called (this should not happen)");
    }
}