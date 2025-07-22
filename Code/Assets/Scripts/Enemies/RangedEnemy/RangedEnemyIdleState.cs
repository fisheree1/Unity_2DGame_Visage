using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyIdleState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;

    public RangedEnemyIdleState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Idle State");
        
        // 播放待机动画
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Idle");
        }
        
        // 停止移动
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // 重置状态标记
        parameter.isCharging = false;
        parameter.isAttacking = false;
    }

    public void OnUpdate()
    {
        // 如果被击中，进入受伤状态
        if (parameter.isHit)
        {
            manager.TransitionState(RangedEnemyStateType.Hurt);
            return;
        }

        // 如果敌人死亡，进入死亡状态
        if (manager.IsDead || parameter.isDead)
        {
            manager.TransitionState(RangedEnemyStateType.Dead);
            return;
        }

        // 寻找目标
        if (parameter.target == null)
        {
            FindTarget();
        }

        // 如果有目标，检查是否应该开始攻击
        if (parameter.target != null)
        {
            float distanceToTarget = Vector2.Distance(manager.transform.position, parameter.target.position);
            
            // 面向目标
            manager.FlipTo(parameter.target);
            
            // 如果目标在攻击范围内且可以攻击，开始蓄力
            if (distanceToTarget <= parameter.attackRange && manager.CanAttack())
            {
                manager.TransitionState(RangedEnemyStateType.Charge);
            }
            // 如果目标太近，可以考虑后退（可选功能）
            else if (distanceToTarget < parameter.retreatRange)
            {
                // 可以在这里添加后退逻辑
                // 目前保持在待机状态
            }
            // 如果目标太远，失去目标
            else if (distanceToTarget > parameter.detectionRange)
            {
                parameter.target = null;
            }
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Idle State");
    }

    private void FindTarget()
    {
        // 寻找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(manager.transform.position, player.transform.position);
            if (distance <= parameter.detectionRange)
            {
                parameter.target = player.transform;
                Debug.Log("RangedEnemy: Target found - Player");
            }
        }
    }
}