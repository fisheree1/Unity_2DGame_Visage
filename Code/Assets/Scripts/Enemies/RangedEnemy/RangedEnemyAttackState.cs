using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyAttackState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;
    private float attackTimer;
    private bool hasAttacked;

    public RangedEnemyAttackState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Attack State");
        
        // 设置攻击标记
        parameter.isAttacking = true;
        parameter.isCharging = false;
        hasAttacked = false;
        attackTimer = 0f;
        
        // 播放攻击动画
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Attack");
        }
        
        // 停止移动
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // 面向目标
        if (parameter.target != null)
        {
            manager.FlipTo(parameter.target);
        }
    }

    public void OnUpdate()
    {
        // 如果被击中，中断攻击并进入受伤状态
        if (parameter.isHit)
        {
            parameter.isAttacking = false;
            manager.TransitionState(RangedEnemyStateType.Hurt);
            return;
        }

        // 如果敌人死亡，进入死亡状态
        if (manager.IsDead || parameter.isDead)
        {
            parameter.isAttacking = false;
            manager.TransitionState(RangedEnemyStateType.Dead);
            return;
        }

        // 更新攻击计时器
        attackTimer += Time.deltaTime;

        // 在攻击动画的中间时刻执行攻击
        if (!hasAttacked && attackTimer >= 0.3f)
        {
            PerformAttack();
            hasAttacked = true;
        }

        // 攻击动画结束，返回待机状态
        if (attackTimer >= 0.6f)
        {
            parameter.isAttacking = false;
            manager.SetAttackTime(); // 设置攻击冷却时间
            manager.TransitionState(RangedEnemyStateType.Idle);
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Attack State");
        parameter.isAttacking = false;
    }

    private void PerformAttack()
    {
        if (parameter.target == null)
        {
            Debug.LogWarning("RangedEnemy: No target for attack");
            return;
        }

        // 检查目标是否仍在攻击范围内
        float distanceToTarget = Vector2.Distance(manager.transform.position, parameter.target.position);
        if (distanceToTarget > parameter.attackRange)
        {
            Debug.LogWarning("RangedEnemy: Target out of range during attack");
            return;
        }

        // 根据攻击模式执行不同的攻击
        if (manager.ShouldUseTrackingAttack())
        {
            // 执行追踪弹攻击
            manager.PerformTrackingAttack();
            Debug.Log("RangedEnemy: Performing tracking attack");
        }
        else
        {
            // 执行扇形弹幕攻击
            manager.PerformFanAttack();
            Debug.Log("RangedEnemy: Performing fan attack");
        }
    }
}