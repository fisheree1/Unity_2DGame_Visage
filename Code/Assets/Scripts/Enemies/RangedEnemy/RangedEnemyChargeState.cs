using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyChargeState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;
    private float chargeTimer;

    public RangedEnemyChargeState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Charge State");
        
        // 设置蓄力标记
        parameter.isCharging = true;
        parameter.isAttacking = false;
        
        // 重置蓄力计时器
        chargeTimer = 0f;
        
        // 播放蓄力动画
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Charge");
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
        // 如果被击中，中断蓄力并进入受伤状态
        if (parameter.isHit)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Hurt);
            return;
        }

        // 如果敌人死亡，进入死亡状态
        if (manager.IsDead || parameter.isDead)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Dead);
            return;
        }

        // 如果没有目标，返回待机状态
        if (parameter.target == null)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Idle);
            return;
        }

        // 检查目标是否仍在攻击范围内
        float distanceToTarget = Vector2.Distance(manager.transform.position, parameter.target.position);
        if (distanceToTarget > parameter.attackRange)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Idle);
            return;
        }

        // 继续面向目标
        manager.FlipTo(parameter.target);

        // 更新蓄力计时器
        chargeTimer += Time.deltaTime;

        // 蓄力完成，进入攻击状态
        if (chargeTimer >= parameter.chargeDuration)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Attack);
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Charge State");
        parameter.isCharging = false;
    }
}