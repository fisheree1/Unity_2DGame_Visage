using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinHeavyAtkState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private bool isInAttackRange;
    private Rigidbody2D rb;
    
    // 冲刺攻击特有变量
    private bool hasDashed = false;
    private bool hasDealtDamage = false;
    private Vector2 dashDirection;
    private float dashSpeed = 35f; // 增加冲刺速度
    private float dashStartTime = 0.15f;  // 更早开始冲刺
    private float dashEndTime = 0.7f;     // 更早结束冲刺
    private float damageWindowStart = 0.2f; // 伤害检测窗口开始时间 - 提前
    private float damageWindowEnd = 0.6f;   // 伤害检测窗口结束时间 - 缩短
    private float animStartTime;
    private bool isDashing = false;
    private bool hasTriggeredNextAttack = false;

    public MGoBlinHeavyAtkState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    
    public void OnEnter()
    {
        // 重置状态
        hasDashed = false;
        hasDealtDamage = false;
        isDashing = false;
        hasTriggeredNextAttack = false;
        animStartTime = Time.time;
        
        // 增加连击计数
        manager.IncrementCombo(MGoBlinStateType.HeavyAtk);
        
        // 确定冲刺方向
        if (parameter.target != null)
        {
            Vector3 targetDirection = (parameter.target.position - manager.transform.position).normalized;
            dashDirection = new Vector2(targetDirection.x, 0); // 只有水平方向
            
            // 面向目标
            manager.FlipTo(parameter.target);
        }
        else
        {
            // 如果没有目标，根据当前朝向决定冲刺方向
            float facing = manager.transform.localScale.x > 0 ? -1f : 1f;
            dashDirection = new Vector2(facing, 0);
        }
        
        // 播放冲刺攻击动画
        parameter.animator.Play("MGoBlin_dashattack");
        
        Debug.Log($"MGoBlin 开始冲刺攻击! (连击数: {parameter.currentComboCount})");
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            StopDash();
            manager.TransitionState(MGoBlinStateType.Hit);
            return;
        }
        
        float timeSinceStart = Time.time - animStartTime;
        
        // 处理冲刺移动
        HandleDashMovement(timeSinceStart);
        
        // 处理伤害检测
        HandleDamageDetection(timeSinceStart);
        
        // 检查攻击范围（用于后续状态评估）
        isInAttackRange = CheckAttackRange();
        
        // 获取动画信息
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // 提前判断下一个攻击 - 更流畅的连击
        if (!hasTriggeredNextAttack && info.normalizedTime >= 0.8f)
        {
            hasTriggeredNextAttack = true;
            DecideNextAction();
        }
        
        // 检查动画是否完成
        if (info.normalizedTime >= 0.95f)
        {
            if (!hasTriggeredNextAttack)
            {
                DecideNextAction();
            }
        }
    }
    
    private void DecideNextAction()
    {
        // 停止冲刺
        StopDash();
        
        // 检查目标是否有效
        if (!manager.IsTargetValid())
        {
            Debug.Log("MGoBlinHeavyAtkState: Target invalid, switching to patrol");
            manager.TransitionState(MGoBlinStateType.Patrol);
            return;
        }
        
        // 更智能的下一步行动决策
        if (manager.CanChainAttack() && isInAttackRange)
        {
            MGoBlinStateType nextAttack = manager.GetNextAttackType();
            if (nextAttack != MGoBlinStateType.Chase)
            {
                Debug.Log($"MGoBlinHeavyAtkState: 链接到 {nextAttack} 状态");
                manager.TransitionState(nextAttack);
                return;
            }
        }
        
        // 决定是否继续攻击还是追击
        if (isInAttackRange && UnityEngine.Random.value < parameter.comboChance * 0.6f)
        {
            Debug.Log("MGoBlinHeavyAtkState: 在攻击范围内，切换到普通攻击");
            manager.TransitionState(MGoBlinStateType.Attack);
        }
        else
        {
            Debug.Log("MGoBlinHeavyAtkState: 切换到追击状态");
            manager.TransitionState(MGoBlinStateType.Chase);
        }
    }
    
    private void HandleDashMovement(float timeSinceStart)
    {
        // 在指定时间窗口内执行冲刺
        if (timeSinceStart >= dashStartTime && timeSinceStart <= dashEndTime)
        {
            if (!isDashing)
            {
                isDashing = true;
                Debug.Log("开始冲刺移动");
            }
            
            // 基于攻击性调整冲刺速度
            float adjustedDashSpeed = dashSpeed * parameter.aggressionMultiplier;
            
            if (rb != null)
            {
                // 物理方式设置冲刺速度
                rb.velocity = new Vector2(dashDirection.x * adjustedDashSpeed, rb.velocity.y);
            }
            else
            {
                // 变换移动方式（如果没有Rigidbody2D）
                manager.transform.Translate(dashDirection * adjustedDashSpeed * Time.deltaTime);
            }
            
            hasDashed = true;
        }
        else if (isDashing && timeSinceStart > dashEndTime)
        {
            // 冲刺结束，逐渐停止
            SlowDownDash();
            isDashing = false;
        }
    }
    
    private void HandleDamageDetection(float timeSinceStart)
    {
        // 在伤害检测窗口期间检测碰撞
        if (!hasDealtDamage && timeSinceStart >= damageWindowStart && timeSinceStart <= damageWindowEnd)
        {
            // 使用更大的攻击范围检测冲刺攻击
            Collider2D hitTarget = Physics2D.OverlapCircle(
                parameter.attackPoint.position,
                parameter.dashAttackArea,
                parameter.targetLayer
            );
            
            if (hitTarget != null)
            {
                DealDamageToTarget(hitTarget);
            }
        }
    }
    
    private void DealDamageToTarget(Collider2D target)
    {
        // 对目标造成伤害并增加连击
        HeroLife heroLife = target.GetComponent<HeroLife>();
        if (heroLife == null)
        {
            heroLife = target.GetComponentInParent<HeroLife>();
        }
        
        if (heroLife != null)
        {
            // 冲刺攻击造成更高伤害，加上连击奖励
            int comboBonus = Mathf.FloorToInt(parameter.currentComboCount * 0.7f);
            int dashDamage = Mathf.Max(1, parameter.damage * 2 + comboBonus);
            
            heroLife.TakeDamage(dashDamage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin 冲刺攻击命中! 造成 {dashDamage} 伤害 (连击奖励: {comboBonus})");
            
            // 对目标施加击退效果
            ApplyKnockback(target);
        }
    }
    
    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            Vector2 knockbackDirection = dashDirection.normalized;
            float knockbackForce = 6f; // 冲刺攻击的击退力度更大
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            
            Debug.Log("应用击退效果");
        }
    }
    
    private void SlowDownDash()
    {
        if (rb != null)
        {
            // 逐渐减速，避免突然停止造成的不自然感觉
            rb.velocity = new Vector2(rb.velocity.x * 0.5f, rb.velocity.y);
        }
    }
    
    private void StopDash()
    {
        if (rb != null)
        {
            // 确保完全停止移动
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        isDashing = false;
    }

    public void OnExit()
    {
        // 确保完全停止移动
        StopDash();
        
        // 重置状态变量
        hasDashed = false;
        hasDealtDamage = false;
        isDashing = false;
        hasTriggeredNextAttack = false;
        
        Debug.Log("MGoBlin 冲刺攻击完成");
    }
    
    private bool CheckAttackRange()
    {
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
    }
}


