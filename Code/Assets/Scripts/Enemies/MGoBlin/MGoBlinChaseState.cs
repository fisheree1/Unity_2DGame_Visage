using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MGoBlinChaseState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private Rigidbody2D rb;
    private bool isInAttackRange;
    private bool isInDashAtkRange;
    private float attackTimer = 0f;
    private float dashTimer = 0f;
    private float lastTargetCheckTime = 0f;
    private Vector3 lastKnownTargetPosition;
    private float persistentChaseTimer = 0f;
    private float anticipationTimer = 0f;

    public MGoBlinChaseState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_chase");
        isInAttackRange = false;
        persistentChaseTimer = 0f;
        anticipationTimer = 0f;
        
        // 更激进的初始设置，基于攻击性倍数
        attackTimer = Mathf.Max(0f, attackTimer - (0.1f * parameter.aggressionMultiplier));
        dashTimer = Mathf.Max(0f, dashTimer - (0.1f * parameter.aggressionMultiplier));
        
        Debug.Log($"MGoBlin 进入追击状态，攻击性: {parameter.aggressionMultiplier}");
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return;
        }

        // 更频繁地检查目标
        if (Time.time - lastTargetCheckTime > 0.05f)
        {
            UpdateTargetInfo();
            lastTargetCheckTime = Time.time;
        }

        // 如果目标无效（null或死亡），处理持续追击
        if (!manager.IsTargetValid())
        {
            HandlePersistentChase();
            return;
        }

        // 面向目标并移动
        manager.FlipTo(parameter.target);
        
        // 检测攻击范围
        isInAttackRange = CheckAttackRange();
        isInDashAtkRange = CheckDashAtkRange();
        
        // 更新计时器
        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
        dashTimer = Mathf.Max(0f, dashTimer - Time.deltaTime);
        anticipationTimer += Time.deltaTime;
        
        // 攻击决策逻辑 - 更加激进和智能
        DecideAttackAction();
    }
    
    private void DecideAttackAction()
    {
        // 检查目标是否有效
        if (!manager.IsTargetValid())
        {
            parameter.target = null;
            manager.TransitionState(MGoBlinStateType.Patrol);
            return;
        }
        
        float distanceToTarget = Vector2.Distance(manager.transform.position, parameter.target.position);
        
        // 预测性攻击 - 如果目标即将进入攻击范围
        bool willBeInAttackRange = distanceToTarget <= parameter.attackArea * parameter.attackPredictionRange;
        bool willBeInDashRange = distanceToTarget <= parameter.dashAttackArea * parameter.attackPredictionRange;
        
        // 基于连击状态和攻击性调整攻击决策
        float comboBonus = parameter.currentComboCount > 0 ? 0.3f : 0f;
        float aggressionBonus = (parameter.aggressionMultiplier - 1f) * 0.2f;
        
        if (isInAttackRange && attackTimer <= 0f)
        {
            // 立即攻击，无延迟
            MGoBlinStateType nextAttack = manager.GetNextAttackType();
            manager.TransitionState(nextAttack);
            attackTimer = (parameter.attackCooldown / parameter.aggressionMultiplier) - comboBonus;
            return;
        }
        else if (isInDashAtkRange && !isInAttackRange && dashTimer <= 0f)
        {
            // 冲刺攻击，减少冷却
            if (UnityEngine.Random.value < (parameter.comboChance + aggressionBonus))
            {
                manager.TransitionState(MGoBlinStateType.HeavyAtk);
                dashTimer = (parameter.dashAtkCooldown / parameter.aggressionMultiplier) - comboBonus;
                return;
            }
        }
        else if (willBeInAttackRange && anticipationTimer > 0.5f && attackTimer <= 0.2f)
        {
            // 预测性攻击 - 提前开始攻击动作
            if (UnityEngine.Random.value < (parameter.comboChance * 0.8f + aggressionBonus))
            {
                MGoBlinStateType nextAttack = manager.GetNextAttackType();
                manager.TransitionState(nextAttack);
                return;
            }
        }
        else if (willBeInDashRange && anticipationTimer > 0.3f && dashTimer <= 0.3f)
        {
            // 预测性冲刺攻击
            if (UnityEngine.Random.value < (parameter.comboChance * 0.6f + aggressionBonus))
            {
                manager.TransitionState(MGoBlinStateType.HeavyAtk);
                return;
            }
        }
        else if (isInAttackRange && attackTimer > 0f)
        {
            // 在攻击范围内等待时，播放威胁动画并缓慢移动
            parameter.animator.Play("MGoBlin_idle");
            SlowApproachTarget();
        }
        else
        {
            // 继续追击 - 更快的速度和更智能的移动
            parameter.animator.Play("MGoBlin_chase");
            MoveTowardsTarget();
        }
    }

    private void UpdateTargetInfo()
    {
        if (parameter.target != null)
        {
            lastKnownTargetPosition = parameter.target.position;
            persistentChaseTimer = 0f;
        }
    }
    
    private void HandlePersistentChase()
    {
        persistentChaseTimer += Time.deltaTime;
        
        if (persistentChaseTimer < parameter.persistentChaseTime)
        {
            // 继续向最后已知位置移动
            MoveTowardsLastKnownPosition();
        }
        else
        {
            // 超时后返回巡逻
            manager.TransitionState(MGoBlinStateType.Patrol);
        }
    }
    
    private void MoveTowardsTarget()
    {
        if (parameter.target == null) return;
        
        Vector3 targetPosition = new Vector3(
            parameter.target.position.x,
            manager.transform.position.y,
            0
        );

        Vector3 moveDir = (targetPosition - manager.transform.position).normalized;
        
        // 基于攻击性和连击状态调整追击速度
        float comboSpeedBonus = parameter.currentComboCount > 0 ? 0.3f : 0f;
        float adjustedSpeed = parameter.ChaseSpeed * parameter.aggressionMultiplier * (1f + comboSpeedBonus);

        // 物理移动
        if (rb != null)
        {
            rb.velocity = new Vector2(
                moveDir.x * adjustedSpeed,
                rb.velocity.y
            );
        }
        else
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                targetPosition,
                adjustedSpeed * Time.deltaTime
            );
        }
    }
    
    private void SlowApproachTarget()
    {
        if (parameter.target == null) return;
        
        Vector3 targetPosition = new Vector3(
            parameter.target.position.x,
            manager.transform.position.y,
            0
        );

        Vector3 moveDir = (targetPosition - manager.transform.position).normalized;
        float slowSpeed = parameter.ChaseSpeed * 0.3f * parameter.aggressionMultiplier;

        if (rb != null)
        {
            rb.velocity = new Vector2(
                moveDir.x * slowSpeed,
                rb.velocity.y
            );
        }
        else
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                targetPosition,
                slowSpeed * Time.deltaTime
            );
        }
    }
    
    private void MoveTowardsLastKnownPosition()
    {
        Vector3 targetPosition = new Vector3(
            lastKnownTargetPosition.x,
            manager.transform.position.y,
            0
        );

        Vector3 moveDir = (targetPosition - manager.transform.position).normalized;
        float adjustedSpeed = parameter.ChaseSpeed * 0.8f * parameter.aggressionMultiplier;

        if (rb != null)
        {
            rb.velocity = new Vector2(
                moveDir.x * adjustedSpeed,
                rb.velocity.y
            );
        }
        else
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                targetPosition,
                adjustedSpeed * Time.deltaTime
            );
        }
    }

    private bool CheckAttackRange()
    {
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
    }
    
    private bool CheckDashAtkRange()
    {
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.dashAttackArea,
            parameter.targetLayer
        );
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void OnExit()
    {
        StopMovement();
        isInAttackRange = false;
        anticipationTimer = 0f;
        
        Debug.Log("MGoBlin 退出追击状态");
    }
}
