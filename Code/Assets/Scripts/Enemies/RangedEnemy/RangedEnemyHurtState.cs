using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyHurtState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;
    private float hurtTimer;
    private Vector2 knockbackDirection;
    private bool hasAppliedKnockback;

    public RangedEnemyHurtState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Hurt State");
        
        // 设置受伤标记
        parameter.isHit = true;
        parameter.isCharging = false;
        parameter.isAttacking = false;
        hasAppliedKnockback = false;
        
        // 重置受伤计时器
        hurtTimer = 0f;
        
        // 播放受伤动画
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Hurt");
        }
        
        // 设置受伤颜色效果
        manager.SetSpriteColor(Color.red);
        
        // 计算击退方向
        if (parameter.target != null)
        {
            Vector2 directionFromPlayer = (manager.transform.position - parameter.target.position).normalized;
            knockbackDirection = directionFromPlayer;
        }
        else
        {
            // 如果没有目标，随机选择一个方向
            knockbackDirection = new Vector2(Random.Range(-1f, 1f), 0).normalized;
        }
        
        // 停止当前移动
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void OnUpdate()
    {
        // 如果敌人死亡，立即转换到死亡状态
        if (manager.IsDead || parameter.isDead)
        {
            parameter.isHit = false;
            manager.TransitionState(RangedEnemyStateType.Dead);
            return;
        }

        // 在受伤状态开始时应用击退效果
        if (!hasAppliedKnockback && parameter.hurtKnockbackForce > 0)
        {
            Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockbackForce = knockbackDirection * parameter.hurtKnockbackForce;
                rb.AddForce(knockbackForce, ForceMode2D.Impulse);
                hasAppliedKnockback = true;
                Debug.Log($"RangedEnemy: Applied knockback force {knockbackForce}");
            }
        }

        // 更新受伤计时器
        hurtTimer += Time.deltaTime;

        // 受伤状态结束，返回待机状态
        if (hurtTimer >= parameter.hurtDuration)
        {
            parameter.isHit = false;
            manager.TransitionState(RangedEnemyStateType.Idle);
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Hurt State");
        
        // 重置受伤标记
        parameter.isHit = false;
        hasAppliedKnockback = false;
        
        // 恢复原始颜色
        manager.ResetSpriteColor();
        
        // 停止击退效果
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
}