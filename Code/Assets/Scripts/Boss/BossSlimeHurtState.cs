using UnityEngine;

public class BossSlimeHurtState : IState
{
    private BossSlime boss;
    private BossSlimeParameter parameter;
    private float hurtTimer;
    
    public BossSlimeHurtState(BossSlime boss, BossSlimeParameter parameter)
    {
        this.boss = boss;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Bossʷ��ķ��������״̬");
        hurtTimer = parameter.hurtRecoveryTime;
        parameter.isHit = true;
        
        // �������˶���
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isHurt", true);
            parameter.animator.SetBool("isIdle", false);
            parameter.animator.SetBool("isAttacking", false);
        }
        
        // �����ڼ�ֹͣ�ƶ�
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
    
    public void OnUpdate()
    {
        if (boss.IsDead) return;
        
        // ���˼�ʱ������ʱ
        hurtTimer -= Time.deltaTime;
        
        // ������״̬�ָ�
        if (hurtTimer <= 0f)
        {
            parameter.isHit = false;
            boss.TransitionState(BossSlimeStateType.Idle);
        }
    }
    
    public void OnExit()
    {
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isHurt", false);
        }
        
        parameter.isHit = false;
    }
}