using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone敌人的待机状态
/// Stone在此状态下保持静止，等待玩家进入视野范围
/// 这是Stone的默认状态，也是最常见的状态
/// </summary>
public class StoneIdleState : IState
{
    private StoneP manager;              // Stone敌人管理器引用
    private StoneParameter parameter;    // Stone敌人参数引用
    private Rigidbody2D rb;             // 刚体组件
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="manager">Stone敌人管理器</param>
    /// <param name="parameter">Stone敌人参数</param>
    public StoneIdleState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 进入待机状态时的初始化
    /// </summary>
    public void OnEnter()
    {
        // 重置状态标志
        parameter.isReacting = false;
        parameter.isHit = false;
        
        // 停止移动
        StopMovement();
        
        // 播放待机动画
        PlayIdleAnimation();
        
        Debug.Log("Stone进入待机状态 - 等待玩家进入视野");
    }

    /// <summary>
    /// 待机状态的每帧更新
    /// 在此状态下Stone基本不执行任何操作，等待外部触发器
    /// </summary>
    public void OnUpdate()
    {
        // 待机状态下主要依靠触发器检测，这里可以添加一些辅助逻辑
        
        // 如果有目标但不在反应状态，可能需要重新评估
        if (parameter.target != null && !parameter.isReacting)
        {
            // 这种情况通常由sight触发器处理，这里只是备用检查
            Debug.LogWarning("Stone: 在Idle状态下检测到目标，但未进入React状态");
        }
    }

    /// <summary>
    /// 播放待机动画
    /// 支持多种可能的动画命名方式，确保兼容性
    /// </summary>
    private void PlayIdleAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试的待机动画名称列表（按优先级排序）
        string[] idleAnimations = {
            "Stone_idle", "stone_idle", "Idle", "idle", "StoneIdle",
            "Stone_default", "default", "Stone_stand", "stand"
        };

        // 尝试播放找到的第一个有效动画
        foreach (string animName in idleAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到动画状态，尝试使用参数控制
        try
        {
            parameter.animator.SetInteger("State", 0);
            parameter.animator.SetBool("IsReacting", false);
        }
        catch { }
    }

    /// <summary>
    /// 停止所有移动
    /// </summary>
    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    /// <summary>
    /// 检查动画控制器中是否存在指定的动画状态
    /// </summary>
    private bool HasAnimationState(string stateName)
    {
        if (parameter?.animator == null) return false;
        var controller = parameter.animator.runtimeAnimatorController;
        if (controller == null) return false;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName) return true;
        }
        return false;
    }

    /// <summary>
    /// 退出待机状态时的清理工作
    /// </summary>
    public void OnExit()
    {
        Debug.Log("Stone离开待机状态");
    }
}