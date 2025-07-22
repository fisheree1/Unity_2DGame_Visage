using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinAttackState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;

    private bool isInAttackRange = false;
    private bool hasDealtDamage = false;
    private float damageWindowStart = 0.2f; // 伤害检测窗口开始时间 - 提前
    private float damageWindowEnd = 0.6f;   // 伤害检测窗口结束时间 - 缩短
    private float animStartTime;
    private bool hasTriggeredNextAttack = false;

    public MGoBlinAttackState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_attack");
        hasDealtDamage = false;
        hasTriggeredNextAttack = false;
        animStartTime = Time.time;
        
        // 增加连击计数
        manager.IncrementCombo(MGoBlinStateType.Attack);
        
        Debug.Log($"MGoBlinAttackState: 进入攻击状态 (连击数: {parameter.currentComboCount})");
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return;
        }
        
        float timeSinceStart = Time.time - animStartTime;
        
        // 处理伤害检测
        HandleDamageDetection(timeSinceStart);
        
        // 轻微的攻击时移动（更流畅）
        HandleAttackMovement();
        
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // 提前判断下一个攻击 - 更流畅的连击
        if (!hasTriggeredNextAttack && info.normalizedTime >= 0.7f && info.IsName("MGoBlin_attack"))
        {
            hasTriggeredNextAttack = true;
            DecideNextAction();
        }
        
        // 动画完成后的最终状态转换
        if (info.normalizedTime >= 0.95f && info.IsName("MGoBlin_attack"))
        {
            if (!hasTriggeredNextAttack)
            {
                DecideNextAction();
            }
        }
    }
    
    private void DecideNextAction()
    {
        // 检查目标是否有效
        if (!manager.IsTargetValid())
        {
            Debug.Log("MGoBlinAttackState: Target invalid, switching to patrol");
            manager.TransitionState(MGoBlinStateType.Patrol);
            return;
        }
        
        // 更智能的下一步行动决策
        if (manager.CanChainAttack() && isInAttackRange)
        {
            MGoBlinStateType nextAttack = manager.GetNextAttackType();
            if (nextAttack != MGoBlinStateType.Chase)
            {
                Debug.Log($"MGoBlinAttackState: 链接到 {nextAttack} 状态");
                manager.TransitionState(nextAttack);
                return;
            }
        }
        
        // 如果不能连击或不在攻击范围内
        if (isInAttackRange && UnityEngine.Random.value < parameter.comboChance * 0.6f)
        {
            Debug.Log("MGoBlinAttackState: 在攻击范围内，切换到攻击2状态");
            manager.TransitionState(MGoBlinStateType.Attack2);
        }
        else
        {
            Debug.Log("MGoBlinAttackState: 不在攻击范围内，切换到追击状态");
            manager.TransitionState(MGoBlinStateType.Chase);
        }
    }
    
    private void HandleAttackMovement()
    {
        // 在攻击过程中轻微向目标移动，使攻击更流畅
        if (manager.IsTargetValid() && !hasDealtDamage)
        {
            Vector3 targetDirection = (parameter.target.position - manager.transform.position).normalized;
            float moveDistance = parameter.aggressiveMovementSpeed * Time.deltaTime * 0.5f;
            
            // 只在攻击前半段移动
            AnimatorStateInfo currentInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (currentInfo.normalizedTime < 0.5f)
            {
                manager.transform.position += targetDirection * moveDistance;
            }
        }
    }
    
    private void HandleDamageDetection(float timeSinceStart)
    {
        // 在伤害检测窗口期间检测碰撞
        if (!hasDealtDamage && timeSinceStart >= damageWindowStart && timeSinceStart <= damageWindowEnd)
        {
            Collider2D hitTarget = Physics2D.OverlapCircle(
                parameter.attackPoint.position,
                parameter.attackArea,
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
            // 基于连击数增加伤害
            int comboBonus = Mathf.FloorToInt(parameter.currentComboCount * 0.5f);
            int totalDamage = parameter.damage + comboBonus;
            
            heroLife.TakeDamage(totalDamage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin 攻击命中! 造成 {totalDamage} 伤害 (连击奖励: {comboBonus})");
            
            // 对目标施加轻微击退效果
            ApplyKnockback(target);
        }
    }
    
    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            // 计算击退方向
            Vector2 knockbackDirection = (target.transform.position - manager.transform.position).normalized;
            float knockbackForce = 2.5f; // 减少击退力度，使连击更流畅
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void OnExit()
    {
        hasDealtDamage = false;
        hasTriggeredNextAttack = false;
        Debug.Log("MGoBlinAttackState: 退出攻击状态");
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


