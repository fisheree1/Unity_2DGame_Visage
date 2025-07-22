using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinHitState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private float hitStartTime;
    private bool hasRecoveredTarget;
    
    public MGoBlinHitState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_hurt");
        hitStartTime = Time.time;
        hasRecoveredTarget = false;
        parameter.lastHitTime = Time.time; // 记录被击中时间
        
        // 重置连击但保持一定的怒气
        if (parameter.currentComboCount > 0)
        {
            parameter.currentComboCount = Mathf.Max(0, parameter.currentComboCount - 2);
        }
        
        // 保持目标，不重置
        if (parameter.target == null)
        {
            // 只在没有目标时才重新查找
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                parameter.target = player.transform;
            }
        }
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        float timeSinceHit = Time.time - hitStartTime;
        
        // 更快的恢复速度，基于攻击性倍数和恢复速度
        float recoveryThreshold = 0.8f / (parameter.aggressionMultiplier * parameter.recoverySpeed);
        
        // 提前恢复目标锁定，保持攻击性
        if (!hasRecoveredTarget && timeSinceHit >= 0.2f)
        {
            hasRecoveredTarget = true;
            // 确保面向目标
            if (parameter.target != null)
            {
                manager.FlipTo(parameter.target);
            }
        }
        
        // 更快的状态恢复
        if (info.normalizedTime >= recoveryThreshold)
        {
            if (manager.IsDead)
            {
                manager.TransitionState(MGoBlinStateType.Dead);
            }
            else
            {
                parameter.isHit = false;
                DecideNextAction();
            }
        }
    }
    
    private void DecideNextAction()
    {
        // 检查目标是否有效
        if (!manager.IsTargetValid())
        {
            Debug.Log("MGoBlinHitState: Target invalid, switching to patrol");
            manager.TransitionState(MGoBlinStateType.Patrol);
            return;
        }
        
        // 更激进的恢复后行动决策
        float distanceToTarget = Vector2.Distance(manager.transform.position, parameter.target.position);
        
        // 根据距离和攻击性决定下一个状态
        if (distanceToTarget <= parameter.attackArea)
        {
            // 在攻击范围内，根据连击状态和随机性决定
            if (parameter.currentComboCount > 0 && UnityEngine.Random.value < parameter.comboChance * 0.8f)
            {
                // 有连击基础时，继续攻击
                manager.TransitionState(MGoBlinStateType.Attack);
            }
            else if (UnityEngine.Random.value < parameter.comboChance * 0.6f)
            {
                // 没有连击时，有几率直接攻击
                manager.TransitionState(MGoBlinStateType.Attack);
            }
            else
            {
                // 否则先追击一下再攻击
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
        else if (distanceToTarget <= parameter.dashAttackArea)
        {
            // 在冲刺攻击范围内
            if (UnityEngine.Random.value < parameter.comboChance * 0.7f)
            {
                // 高几率使用冲刺攻击
                manager.TransitionState(MGoBlinStateType.HeavyAtk);
            }
            else
            {
                // 否则追击
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
        else
        {
            // 距离较远，进入更激进的追击
            manager.TransitionState(MGoBlinStateType.Chase);
        }
    }

    public void OnExit()
    {
        parameter.isHit = false;
        hasRecoveredTarget = false;
        
        // 退出受击状态时，增加攻击性（愤怒效果）
        parameter.aggressionMultiplier = Mathf.Min(parameter.aggressionMultiplier + 0.2f, 3f);
        
        Debug.Log($"MGoBlin 从受击状态恢复，攻击性增加到: {parameter.aggressionMultiplier}");
    }
}


