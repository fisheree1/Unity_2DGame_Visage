using System.Collections;
using UnityEngine;

public class BossSlimeMeleeAttackState : IState
{
    private BossSlime boss;
    private BossSlimeParameter parameter;
    private bool isAttacking;
    private float attackTimer;
    
    public BossSlimeMeleeAttackState(BossSlime boss, BossSlimeParameter parameter)
    {
        this.boss = boss;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Boss史莱姆进入近战攻击状态");
        isAttacking = true;
        attackTimer = parameter.meleeAttackCooldown;
        
        // 设置攻击动画
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isAttacking", true);
            parameter.animator.SetBool("isIdle", false);
            parameter.animator.SetBool("isHurt", false);
            parameter.animator.SetTrigger("meleeAttack");
        }
        
        // 攻击期间停止移动
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // 开始攻击协程
        boss.StartCoroutine(PerformMeleeAttack());
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
        boss.RecordAttackType(BossSlimeStateType.MeleeAttack);
    }
    
    private IEnumerator PerformMeleeAttack()
    {
        // 等待攻击动画到达命中帧
        yield return new WaitForSeconds(0.5f);
        
        // 执行近战攻击
        PerformMeleeHit();
        
        // 等待攻击动画结束
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
    }
    
    private void PerformMeleeHit()
    {
        if (parameter.meleeAttackPoint == null) return;
        
        // 检查近战范围内的目标
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(
            parameter.meleeAttackPoint.position, 
            parameter.meleeAttackRadius, 
            parameter.targetLayer
        );
        
        foreach (Collider2D target in hitTargets)
        {
            if (target.CompareTag("Player"))
            {
                HeroLife playerLife = target.GetComponent<HeroLife>();
                if (playerLife != null)
                {
                    playerLife.TakeDamage(parameter.meleeDamage);
                    Debug.Log($"Boss史莱姆近战攻击对玩家造成了 {parameter.meleeDamage} 点伤害");
                    
                    // 应用击退
                    ApplyKnockback(target.transform);
                }
            }
        }
    }
    
    private void ApplyKnockback(Transform target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            Vector2 knockbackDirection = (target.position - boss.transform.position).normalized;
            float knockbackForce = 10f;
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }
}