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
        Debug.Log("Bossʷ��ķ�����ս����״̬");
        isAttacking = true;
        attackTimer = parameter.meleeAttackCooldown;
        
        // ���ù�������
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isAttacking", true);
            parameter.animator.SetBool("isIdle", false);
            parameter.animator.SetBool("isHurt", false);
            parameter.animator.SetTrigger("meleeAttack");
        }
        
        // �����ڼ�ֹͣ�ƶ�
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // ��ʼ����Э��
        boss.StartCoroutine(PerformMeleeAttack());
    }
    
    public void OnUpdate()
    {
        if (boss.IsDead) return;
        
        // ������ʱ������ʱ
        attackTimer -= Time.deltaTime;
        
        // ��ʱ�������Ҳ��ڹ���ʱ��������״̬
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
        
        // ��¼�������͵�����ģʽ�л�ϵͳ
        boss.RecordAttackType(BossSlimeStateType.MeleeAttack);
    }
    
    private IEnumerator PerformMeleeAttack()
    {
        // �ȴ�����������������֡
        yield return new WaitForSeconds(0.5f);
        
        // ִ�н�ս����
        PerformMeleeHit();
        
        // �ȴ�������������
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
    }
    
    private void PerformMeleeHit()
    {
        if (parameter.meleeAttackPoint == null) return;
        
        // ����ս��Χ�ڵ�Ŀ��
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
                    Debug.Log($"Bossʷ��ķ��ս�������������� {parameter.meleeDamage} ���˺�");
                    
                    // Ӧ�û���
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