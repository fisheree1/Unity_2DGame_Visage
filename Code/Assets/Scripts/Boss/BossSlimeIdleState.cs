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
        Debug.Log("Bossʷ��ķ�������״̬");
        idleTimer = parameter.idleTime;
        
        // ���ô�������
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
        
        // ������ʱ������ʱ
        idleTimer -= Time.deltaTime;
        
        // ���Ŀ��
        if (parameter.target != null)
        {
            float distanceToTarget = boss.GetDistanceToTarget();
            
            // ���Ŀ������Ұ��Χ���Ҵ���ʱ������������ж�
            if (distanceToTarget <= parameter.sightRange && idleTimer <= 0f)
            {
                // ���ݾ���ͽ׶ξ�����������
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