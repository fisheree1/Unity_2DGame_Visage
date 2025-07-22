using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SGoBlinDeadState : IState
{
    private SGoBlinP manager;
    private SGoBlinParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private readonly float maxDestroyTime = 2.0f; // 最大等待时间，防止对象永不销毁
    private bool isDestroying = false;

    public SGoBlinDeadState(SGoBlinP manager, SGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("SGoBlin entering Dead State");
        
        // 验证管理器
        if (manager == null)
        {
            Debug.LogError("SGoBlinDeadState: Manager is null!");
            return;
        }
        
        // 播放死亡动画（如果动画器存在）
        if (parameter?.animator != null)
        {
            parameter.animator.Play("S_GoBlin_death");
        }
        else
        {
            Debug.LogWarning("SGoBlinDeadState: Animator is null, skipping death animation");
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
            DestroySGoBlin();
        }
    }
    
    private void DestroySGoBlin()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // 这里可以添加掉落物品或特效
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log($"SGoBlin destroyed after {destroyTimer:F2} seconds");
        
        if (manager != null && manager.gameObject != null)
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Debug.LogWarning("SGoBlinDeadState: Manager or GameObject is null during destruction");
        }
    }
    
    public void OnExit()
    {
        // 死亡状态通常不会退出，因为对象会被销毁
        Debug.Log("SGoBlinDeadState: OnExit called (this should not happen)");
    }
}