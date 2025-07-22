using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MomDeadState : IState
{
    private MomP manager;
    private MomParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.5f;
    private readonly float maxDestroyTime = 1.0f; // 最大等待时间，防止对象永不销毁
    private bool isDestroying = false;

    public MomDeadState(MomP manager, MomParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Mom entering Dead State");
        
        // 验证管理器
        if (manager == null)
        {
            Debug.LogError("MomDeadState: Manager is null!");
            return;
        }
        
        // 安全播放死亡动画
        if (parameter?.animator != null)
        {
            if (HasAnimationState("Mom_death"))
            {
                parameter.animator.Play("Mom_death");
            }
            else
            {
                Debug.LogWarning("MomDeadState: Animation state 'Mom_death' not found!");
            }
        }
        else
        {
            Debug.LogWarning("MomDeadState: Animator is null, skipping death animation");
        }
        
        // 禁用碰撞体以防止进一步交互
        var collider = manager.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
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
        
        // 正常销毁时间到达或超过最大等待时间
        if (destroyTimer >= destroyDelay || destroyTimer >= maxDestroyTime)
        {
            DestroyMom();
        }
    }
    
    private void DestroyMom()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // 这里可以添加掉落物品或特效等其他死亡效果
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log($"Mom destroyed after {destroyTimer:F2} seconds");
        
        if (manager != null && manager.gameObject != null)
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Debug.LogWarning("MomDeadState: Manager or GameObject is null during destruction");
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
        Debug.Log("MomDeadState: OnExit called (this should not happen)");
    }
}