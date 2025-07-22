using UnityEngine;

public class BossSlimeIdleState : IState
{
    private BossSlime boss;
    private BossSlimeParameter parameter;
    private float idleTimer;
    
    public BossSlimeIdleState(BossSlime boss, BossSlimeParameter parameter)
    {
        this.boss = boss;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Boss史莱姆进入待机状态");
        idleTimer = parameter.idleTime;
        
        // 设置待机动画
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isIdle", true);
            parameter.animator.SetBool("isAttacking", false);
            parameter.animator.SetBool("isHurt", false);
        }
    }
    
    public void OnUpdate()
    {
        if (boss.IsDead) return;
        
        // 待机计时器倒计时
        idleTimer -= Time.deltaTime;
        
        // 检查目标
        if (parameter.target != null)
        {
            float distanceToTarget = boss.GetDistanceToTarget();
            
            // 如果目标在视野范围内且待机时间结束，决定行动
            if (distanceToTarget <= parameter.sightRange && idleTimer <= 0f)
            {
                // 根据距离和阶段决定攻击类型
                if (distanceToTarget <= parameter.meleeAttackRange)
                {
                    boss.TransitionState(BossSlimeStateType.MeleeAttack);
                }
                else if (distanceToTarget <= parameter.rangedAttackRange)
                {
                    boss.TransitionState(BossSlimeStateType.RangedAttack);
                }
            }
        }
    }
    
    public void OnExit()
    {
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isIdle", false);
        }
    }
}