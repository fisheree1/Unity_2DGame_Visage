using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpiderDeadState : IState
{
    private SpiderP manager;
    private SpiderParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.5f;
    private readonly float maxDestroyTime = 1.0f; // 最大等待时间，防止对象永不销毁
    private bool isDestroying = false;

    public SpiderDeadState(SpiderP manager, SpiderParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Spider entering Dead State");
        
        // 验证管理器和参数
        if (manager == null)
        {
            Debug.LogError("SpiderDeadState: Manager is null!");
            return;
        }
        
        // 播放死亡动画（如果动画器存在）
        if (parameter?.animator != null)
        {
            parameter.animator.Play("Spider_death");
        }
        else
        {
            Debug.LogWarning("SpiderDeadState: Animator is null, skipping death animation");
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
            DestroySpider();
        }
    }
    
    private void DestroySpider()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // 这里可以添加掉落物品或特效
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log($"Spider destroyed after {destroyTimer:F2} seconds");
        
        if (manager != null && manager.gameObject != null)
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Debug.LogWarning("SpiderDeadState: Manager or GameObject is null during destruction");
        }
    }
    
    public void OnExit()
    {
        // 死亡状态通常不会退出，因为对象会被销毁
        Debug.Log("SpiderDeadState: OnExit called (this should not happen)");
    }
}