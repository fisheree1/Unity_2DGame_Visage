using System.Collections;
using UnityEngine;

public class BossSlimeRangedAttackState : IState
{
    private BossSlime boss;
    private BossSlimeParameter parameter;
    private bool isAttacking;
    private float attackTimer;
    private bool attackExecuted = false;
    
    public BossSlimeRangedAttackState(BossSlime boss, BossSlimeParameter parameter)
    {
        this.boss = boss;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Boss史莱姆进入远程攻击状态");
        isAttacking = true;
        attackTimer = parameter.rangedAttackCooldown;
        attackExecuted = false;
        
        // 设置攻击动画
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isAttacking", true);
            parameter.animator.SetBool("isIdle", false);
            parameter.animator.SetBool("isHurt", false);
            parameter.animator.SetTrigger("rangedAttack");
        }
        
        // 攻击期间停止移动
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // 开始攻击协程
        boss.StartCoroutine(PerformRangedAttack());
    }
    
    public void OnUpdate()
    {
        if (boss.IsDead) return;
        
        // 攻击计时器倒计时
        attackTimer -= Time.deltaTime;
        
        // 计时器结束且不在攻击时结束攻击状态
        if (attackTimer <= 0f && !isAttacking)
        {
            boss.SetLastAttackTime();
            boss.TransitionState(BossSlimeStateType.Idle);
        }
    }
    
    public void OnExit()
    {
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isAttacking", false);
        }
        
        isAttacking = false;
        
        // 记录攻击类型到攻击模式切换系统
        boss.RecordAttackType(BossSlimeStateType.RangedAttack);
    }
    
    private IEnumerator PerformRangedAttack()
    {
        // 等待攻击动画到达施法帧
        yield return new WaitForSeconds(0.3f);
        
        // 阶段3特殊检查：如果跳跃攻击准备就绪，执行跳跃攻击
        if (boss.IsInPhase3 && boss.ShouldPerformJumpAttack())
        {
            Debug.Log("[阶段3机制] 执行跳跃弹幕攻击替代常规远程攻击");
            yield return boss.StartCoroutine(boss.ExecuteJumpBarrageAttack());
        }
        else
        {
            // 根据阶段选择常规攻击模式
            if (boss.IsInPhase3)
            {
                // 阶段3：更激进的攻击模式
                yield return boss.StartCoroutine(PerformPhase3AttackCoroutine());
            }
            else if (boss.IsInPhase2)
            {
                // 阶段2：扇形攻击
                PerformPhase2Attack();
            }
            else
            {
                // 阶段1：基础追踪弹幕
                yield return boss.StartCoroutine(PerformPhase1Attack());
            }
        }
        
        // 等待攻击动画结束
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
    }
    
    private IEnumerator PerformPhase1Attack()
    {
        Debug.Log("Boss史莱姆执行阶段1攻击：连续追踪弹幕");
        
        // 发射3发连续追踪弹幕
        for (int i = 0; i < 3; i++)
        {
            FireTrackingProjectile();
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    private void PerformPhase2Attack()
    {
        Debug.Log("Boss史莱姆执行阶段2攻击：多发扇形追踪弹幕");
        
        // 发射扇形追踪弹幕
        if (parameter.rangedAttackPoints != null && parameter.rangedAttackPoints.Length > 0)
        {
            Transform attackPoint = parameter.rangedAttackPoints[0];
            if (attackPoint != null && parameter.target != null)
            {
                boss.CreateFanProjectiles(
                    attackPoint.position,
                    parameter.target.position,
                    parameter.fanProjectileCount,
                    parameter.fanAngle
                );
            }
        }
    }
    
    private IEnumerator PerformPhase3AttackCoroutine()
    {
        Debug.Log("Boss史莱姆执行阶段3攻击：增强扇形攻击");
        
        // 发射两波扇形弹幕
        for (int wave = 0; wave < 2; wave++)
        {
            if (parameter.rangedAttackPoints != null && parameter.rangedAttackPoints.Length > 0)
            {
                Transform attackPoint = parameter.rangedAttackPoints[0];
                if (attackPoint != null && parameter.target != null)
                {
                    boss.CreateFanProjectiles(
                        attackPoint.position,
                        parameter.target.position,
                        parameter.fanProjectileCount + 2, // 更多的弹幕
                        parameter.fanAngle + 20f // 更大的角度
                    );
                }
            }
            
            yield return new WaitForSeconds(0.4f);
        }
    }
    
    private void FireTrackingProjectile()
    {
        if (parameter.rangedAttackPoints == null || parameter.rangedAttackPoints.Length == 0) return;
        if (parameter.target == null) return;
        
        // 选择随机攻击点
        Transform attackPoint = parameter.rangedAttackPoints[Random.Range(0, parameter.rangedAttackPoints.Length)];
        if (attackPoint == null) return;
        
        // 创建追踪弹幕
        GameObject projectile = boss.CreateProjectile(
            parameter.trackingProjectilePrefab,
            attackPoint.position,
            parameter.target.position,
            parameter.rangedDamage,
            true // 启用追踪
        );
        
        Debug.Log($"Boss史莱姆从 {attackPoint.name} 发射追踪弹幕");
    }
}