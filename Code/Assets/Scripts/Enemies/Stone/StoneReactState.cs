using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone敌人的反应状态
/// 当Stone的sight检测到玩家时进入此状态
/// 持续0.3秒的准备时间，然后转入攻击状态
/// </summary>
public class StoneReactState : IState
{
    private StoneP manager;              // Stone敌人管理器引用
    private StoneParameter parameter;    // Stone敌人参数引用
    private float reactTimer = 0f;       // 反应状态计时器
    private Rigidbody2D rb;             // 刚体组件
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="manager">Stone敌人管理器</param>
    /// <param name="parameter">Stone敌人参数</param>
    public StoneReactState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 进入反应状态时的初始化
    /// </summary>
    public void OnEnter()
    {
        parameter.isReacting = true;   // 设置反应标志
        parameter.isHit = false;       // 重置受击标志
        reactTimer = 0f;               // 重置计时器
        
        // 停止移动
        StopMovement();
        
        // 面向目标玩家
        if (parameter.target != null)
        {
            manager.FlipTo(parameter.target);
        }
        
        // 播放反应动画
        PlayReactAnimation();
        
        Debug.Log($"Stone检测到玩家并进入反应状态 - 将在 {parameter.reactDuration} 秒后攻击！");
    }

    /// <summary>
    /// 反应状态的每帧更新
    /// </summary>
    public void OnUpdate()
    {
        reactTimer += Time.deltaTime;
        
        // 持续面向目标（如果目标还存在）
        if (parameter.target != null)
        {
            manager.FlipTo(parameter.target);
        }
        
        // 检查反应时间是否结束
        if (reactTimer >= parameter.reactDuration)
        {
            // 检查目标是否仍然存在
            if (parameter.target != null)
            {
                manager.TransitionState(StoneStateType.Attack);
            }
            else
            {
                manager.TransitionState(StoneStateType.Idle);
            }
        }
    }

    /// <summary>
    /// 播放反应动画
    /// </summary>
    private void PlayReactAnimation()
    {
        if (parameter?.animator == null) return;

        try
        {
            parameter.animator.SetBool("IsReacting", true);
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
    /// 退出反应状态时的清理工作
    /// </summary>
    public void OnExit()
    {
        parameter.isReacting = false;   // 清除反应标志
        reactTimer = 0f;                // 重置计时器
        
        try
        {
            parameter.animator.SetBool("IsReacting", false);
        }
        catch { }
        
        Debug.Log("Stone离开反应状态");
    }
}