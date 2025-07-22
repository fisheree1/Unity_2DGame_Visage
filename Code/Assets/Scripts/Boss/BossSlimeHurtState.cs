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
        Debug.Log("Boss史莱姆进入受伤状态");
        hurtTimer = parameter.hurtRecoveryTime;
        parameter.isHit = true;
        
        // 设置受伤动画
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isHurt", true);
            parameter.animator.SetBool("isIdle", false);
            parameter.animator.SetBool("isAttacking", false);
        }
        
        // 受伤期间停止移动
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
    
    public void OnUpdate()
    {
        if (boss.IsDead) return;
        
        // 受伤计时器倒计时
        hurtTimer -= Time.deltaTime;
        
        // 从受伤状态恢复
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