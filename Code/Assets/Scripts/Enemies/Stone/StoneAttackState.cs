using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone敌人的攻击状态
/// 执行Stone的招牌技能：一击毙命的光束攻击
/// </summary>
public class StoneAttackState : IState
{
    private StoneP manager;              // Stone敌人管理器引用
    private StoneParameter parameter;    // Stone敌人参数引用
    private bool hasAttacked = false;    // 是否已经执行过攻击
    private float attackTimer = 0f;      // 攻击状态计时器
    private float attackDuration = 1f;   // 攻击状态持续时间
    private Rigidbody2D rb;             // 刚体组件
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="manager">Stone敌人管理器</param>
    /// <param name="parameter">Stone敌人参数</param>
    public StoneAttackState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 进入攻击状态时的初始化
    /// </summary>
    public void OnEnter()
    {
        hasAttacked = false;        // 重置攻击标志
        attackTimer = 0f;           // 重置计时器
        parameter.isReacting = false; // 清除反应标志
        
        // 停止移动
        StopMovement();
        
        // 最后一次面向目标
        if (parameter.target != null)
        {
            manager.FlipTo(parameter.target);
        }
        
        // 播放攻击动画
        PlayAttackAnimation();
        
        Debug.Log("Stone进入攻击状态 - 准备释放致命光束！");
    }

    /// <summary>
    /// 攻击状态的每帧更新
    /// </summary>
    public void OnUpdate()
    {
        attackTimer += Time.deltaTime;
        
        // 在攻击动画的适当时机执行光束攻击
        if (!hasAttacked && attackTimer >= 0.3f)
        {
            ExecuteBeamAttack();
            hasAttacked = true;
        }
        
        // 攻击状态持续一段时间后返回Idle
        if (attackTimer >= attackDuration)
        {
            manager.TransitionState(StoneStateType.Idle);
        }
    }

    /// <summary>
    /// 执行光束攻击
    /// </summary>
    private void ExecuteBeamAttack()
    {
        if (parameter.attackPoint == null)
        {
            Debug.LogWarning("Stone: 攻击点未设置，无法执行光束攻击！");
            return;
        }
        
        // 使用StoneP中的光束攻击方法
        manager.FireBeamAttack();
        
        Debug.Log("Stone释放光束攻击！");
    }

    /// <summary>
    /// 播放攻击动画
    /// </summary>
    private void PlayAttackAnimation()
    {
        if (parameter?.animator == null) return;

        try
        {
            parameter.animator.SetTrigger("Attack");
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
    /// 退出攻击状态时的清理工作
    /// </summary>
    public void OnExit()
    {
        hasAttacked = false;        // 重置攻击标志
        attackTimer = 0f;           // 重置计时器
        
        Debug.Log("Stone离开攻击状态");
    }
}