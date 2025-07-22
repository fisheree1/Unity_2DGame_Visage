using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone敌人的防御状态
/// 当Stone受到3次攻击后进入此状态，在2秒内免疫所有伤害
/// 现在使用bool IsDefence参数控制防御动画
/// </summary>
public class StoneDefenceState : IState
{
    private StoneP manager;              // Stone敌人管理器引用
    private StoneParameter parameter;    // Stone敌人参数引用
    private float defenceTimer = 0f;     // 防御状态计时器
    private Rigidbody2D rb;             // 刚体组件
    private Color originalColor;         // 原始颜色（用于恢复）
    private SpriteRenderer spriteRenderer; // 精灵渲染器（用于视觉效果）

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="manager">Stone敌人管理器</param>
    /// <param name="parameter">Stone敌人参数</param>
    public StoneDefenceState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
        spriteRenderer = manager.GetComponent<SpriteRenderer>();
        
        // 保存原始颜色以便防御结束后恢复
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// 进入防御状态时的初始化
    /// </summary>
    public void OnEnter()
    {
        parameter.isInDefence = true;  // 设置防御标志
        defenceTimer = 0f;             // 重置计时器
        
        // 重置受击次数（进入防御后重新计算）
        parameter.hitCount = 0;
        
        StopMovement();          // 停止移动
        PlayDefenceAnimation();  // 播放防御动画
        
        // 视觉效果 - 蓝色调表示防御状态
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 1f); // 蓝色调
        }
        
        // 播放防御音效
        if (parameter.audioSource != null && parameter.defenceSound != null)
        {
            parameter.audioSource.PlayOneShot(parameter.defenceSound);
        }
        
        Debug.Log("Stone进入防御模式 - 在 " + parameter.defenceDuration + " 秒内免疫所有伤害！");
    }

    /// <summary>
    /// 防御状态的每帧更新
    /// </summary>
    public void OnUpdate()
    {
        defenceTimer += Time.deltaTime;
        
        // 可选：在防御期间添加脉冲效果
        if (spriteRenderer != null)
        {
            float pulse = Mathf.Sin(Time.time * 8f) * 0.2f + 0.8f; // 在0.6到1.0之间脉冲
            spriteRenderer.color = new Color(0.5f, 0.5f, 1f, pulse);
        }
        
        // 防御时间结束后退出防御状态
        if (defenceTimer >= parameter.defenceDuration)
        {
            manager.TransitionState(StoneStateType.Idle);
        }
    }

    /// <summary>
    /// 播放防御动画
    /// 使用bool IsDefence参数而不是触发器
    /// </summary>
    private void PlayDefenceAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试的防御动画名称列表（按优先级排序）
        string[] defenceAnimations = {
            "Stone_defence",    // Stone特定防御动画
            "stone_defence",    // 小写版本
            "Defence",          // 通用防御动画
            "defence",          // 小写通用版本
            "StoneDefence",     // 驼峰命名
            "Stone_shield",     // 护盾动画
            "shield",           // 通用护盾
            "Stone_protect",    // 保护动画
            "protect",          // 通用保护
            "Stone_block",      // 格挡动画
            "block"             // 通用格挡
        };

        // 尝试播放找到的第一个有效动画
        foreach (string animName in defenceAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到动画状态，使用bool IsDefence参数控制
        try
        {
            // 设置bool参数IsDefence为true，启动防御动画
            parameter.animator.SetBool("IsDefence", true);
            Debug.Log("Stone: 使用bool IsDefence参数启动防御动画");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Stone: 无法设置IsDefence参数: {e.Message}");
        }
        
        // 备用：尝试其他可能的bool参数名称
        try
        {
            parameter.animator.SetBool("isDefending", true);
        }
        catch { }
        
        try
        {
            parameter.animator.SetBool("IsDefending", true);
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
            rb.velocity = new Vector2(0, rb.velocity.y); // 保持Y轴速度（重力）
        }
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
    /// 退出防御状态时的清理工作
    /// </summary>
    public void OnExit()
    {
        parameter.isInDefence = false;  // 清除防御标志
        defenceTimer = 0f;              // 重置计时器
        
        // 恢复原始颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // 重置防御动画状态 - 使用bool IsDefence参数
        try
        {
            // 设置bool参数IsDefence为false，结束防御动画
            parameter.animator.SetBool("IsDefence", false);
            Debug.Log("Stone: 使用bool IsDefence参数结束防御动画");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Stone: 无法重置IsDefence参数: {e.Message}");
        }
        
        // 重置其他可能的防御相关bool参数
        try
        {
            parameter.animator.SetBool("isDefending", false);
        }
        catch { }
        
        try
        {
            parameter.animator.SetBool("IsDefending", false);
        }
        catch { }
        
        Debug.Log("Stone防御模式结束 - 现在可以再次受到伤害");
    }
}