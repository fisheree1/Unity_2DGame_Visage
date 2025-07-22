using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone敌人的死亡状态
/// 当Stone的生命值降至0时进入此状态
/// 处理死亡动画播放和对象清理工作
/// </summary>
public class StoneDeadState : IState
{
    private StoneP manager;              // Stone敌人管理器引用
    private StoneParameter parameter;    // Stone敌人参数引用
    private float deathTimer = 0f;       // 死亡状态计时器
    private float deathDuration = 2f;    // 死亡状态持续时间
    private Rigidbody2D rb;             // 刚体组件
    private Collider2D col;             // 碰撞器组件
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="manager">Stone敌人管理器</param>
    /// <param name="parameter">Stone敌人参数</param>
    public StoneDeadState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
        col = manager.GetComponent<Collider2D>();
    }

    /// <summary>
    /// 进入死亡状态时的初始化
    /// </summary>
    public void OnEnter()
    {
        deathTimer = 0f;                // 重置计时器
        
        // 重置所有状态标志
        parameter.isReacting = false;
        parameter.isHit = false;
        parameter.isInDefence = false;
        parameter.target = null;
        
        // 停止所有移动
        StopMovement();
        
        // 禁用碰撞器防止进一步交互
        DisableCollisions();
        
        // 播放死亡动画
        PlayDeathAnimation();
        
        Debug.Log("Stone进入死亡状态");
    }

    /// <summary>
    /// 死亡状态的每帧更新
    /// </summary>
    public void OnUpdate()
    {
        deathTimer += Time.deltaTime;
        
        // 检查是否完成死亡处理
        if (deathTimer >= deathDuration)
        {
            CleanupAndDestroy();
        }
    }

    /// <summary>
    /// 播放死亡动画
    /// 尝试多个可能的动画名称，确保兼容性
    /// </summary>
    private void PlayDeathAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试的死亡动画名称列表（按优先级排序）
        string[] deathAnimations = {
            "Stone_dead",       // Stone特定死亡动画
            "stone_dead",       // 小写版本
            "Dead",             // 通用死亡动画
            "dead",             // 小写通用版本
            "StoneDead",        // 驼峰命名
            "Stone_death",      // 死亡动画
            "death",            // 通用死亡
            "Stone_die",        // 死亡动画
            "die",              // 通用死亡
            "Stone_destroy",    // 销毁动画
            "destroy"           // 通用销毁
        };

        // 尝试播放找到的第一个有效动画
        foreach (string animName in deathAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到动画状态，尝试使用触发器
        try
        {
            parameter.animator.SetTrigger("Dead");
            Debug.Log("Stone: 使用Dead触发器播放死亡动画");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Stone: 无法触发Dead动画: {e.Message}");
        }
        
        try
        {
            parameter.animator.SetTrigger("Death");
        }
        catch { }
        
        try
        {
            parameter.animator.SetBool("isDead", true);
        }
        catch { }
    }

    /// <summary>
    /// 检查动画控制器中是否存在指定的动画状态
    /// </summary>
    /// <param name="stateName">动画状态名称</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    private bool HasAnimationState(string stateName)
    {
        if (parameter?.animator == null) return false;

        var controller = parameter.animator.runtimeAnimatorController;
        if (controller == null) return false;

        // 遍历所有动画片段查找匹配的名称
        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 停止所有移动
    /// </summary>
    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // 设置为运动学模式，停止物理模拟
        }
    }

    /// <summary>
    /// 禁用碰撞器
    /// </summary>
    private void DisableCollisions()
    {
        if (col != null)
        {
            col.enabled = false;
        }
        
        // 同时禁用子物体的碰撞器（如sight）
        Collider2D[] childColliders = manager.GetComponentsInChildren<Collider2D>();
        foreach (var collider in childColliders)
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// 清理和销毁对象
    /// </summary>
    private void CleanupAndDestroy()
    {
        Debug.Log("Stone死亡处理完成，准备销毁对象");
        
        // 可选：通知游戏管理器Stone被击败
        // GameManager.Instance?.OnEnemyDefeated("Stone");
        
        // 可选：掉落物品或奖励
        // SpawnRewards();
        
        // 销毁Stone对象
        Object.Destroy(manager.gameObject);
    }

    /// <summary>
    /// 退出死亡状态时的清理工作
    /// 注意：通常不会从死亡状态退出，这个方法主要用于完整性
    /// </summary>
    public void OnExit()
    {
        // 通常死亡状态不会被退出，但为了完整性保留此方法
        Debug.Log("Stone离开死亡状态（这通常不应该发生）");
    }
}