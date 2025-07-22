using System.Collections;
using UnityEngine;

public class BossSlimeRangedAttackState : IState
{
    private BossSlime boss;
    private BossSlimeParameter parameter;
    private bool isAttacking;
    private float attackTimer;
    private bool attackExecuted = false;
    
    public BossSlimeRangedAttackState(BossSlime boss, BossSlimeParameter parameter)
    {
        this.boss = boss;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Bossʷ��ķ����Զ�̹���״̬");
        isAttacking = true;
        attackTimer = parameter.rangedAttackCooldown;
        attackExecuted = false;
        
        // ���ù�������
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isAttacking", true);
            parameter.animator.SetBool("isIdle", false);
            parameter.animator.SetBool("isHurt", false);
            parameter.animator.SetTrigger("rangedAttack");
        }
        
        // �����ڼ�ֹͣ�ƶ�
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // ��ʼ����Э��
        boss.StartCoroutine(PerformRangedAttack());
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
        boss.RecordAttackType(BossSlimeStateType.RangedAttack);
    }
    
    private IEnumerator PerformRangedAttack()
    {
        // �ȴ�������������ʩ��֡
        yield return new WaitForSeconds(0.3f);
        
        // �׶�3�����飺�����Ծ����׼��������ִ����Ծ����
        if (boss.IsInPhase3 && boss.ShouldPerformJumpAttack())
        {
            Debug.Log("[�׶�3����] ִ����Ծ��Ļ�����������Զ�̹���");
            yield return boss.StartCoroutine(boss.ExecuteJumpBarrageAttack());
        }
        else
        {
            // ���ݽ׶�ѡ�񳣹湥��ģʽ
            if (boss.IsInPhase3)
            {
                // �׶�3���������Ĺ���ģʽ
                yield return boss.StartCoroutine(PerformPhase3AttackCoroutine());
            }
            else if (boss.IsInPhase2)
            {
                // �׶�2�����ι���
                PerformPhase2Attack();
            }
            else
            {
                // �׶�1������׷�ٵ�Ļ
                yield return boss.StartCoroutine(PerformPhase1Attack());
            }
        }
        
        // �ȴ�������������
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
    }
    
    private IEnumerator PerformPhase1Attack()
    {
        Debug.Log("Bossʷ��ķִ�н׶�1����������׷�ٵ�Ļ");
        
        // ����3������׷�ٵ�Ļ
        for (int i = 0; i < 3; i++)
        {
            FireTrackingProjectile();
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    private void PerformPhase2Attack()
    {
        Debug.Log("Bossʷ��ķִ�н׶�2�������෢����׷�ٵ�Ļ");
        
        // ��������׷�ٵ�Ļ
        if (parameter.rangedAttackPoints != null && parameter.rangedAttackPoints.Length > 0)
        {
            Transform attackPoint = parameter.rangedAttackPoints[0];
            if (attackPoint != null && parameter.target != null)
            {
                boss.CreateFanProjectiles(
                    attackPoint.position,
                    parameter.target.position,
                    parameter.fanProjectileCount,
                    parameter.fanAngle
                );
            }
        }
    }
    
    private IEnumerator PerformPhase3AttackCoroutine()
    {
        Debug.Log("Bossʷ��ķִ�н׶�3��������ǿ���ι���");
        
        // �����������ε�Ļ
        for (int wave = 0; wave < 2; wave++)
        {
            if (parameter.rangedAttackPoints != null && parameter.rangedAttackPoints.Length > 0)
            {
                Transform attackPoint = parameter.rangedAttackPoints[0];
                if (attackPoint != null && parameter.target != null)
                {
                    boss.CreateFanProjectiles(
                        attackPoint.position,
                        parameter.target.position,
                        parameter.fanProjectileCount + 2, // ����ĵ�Ļ
                        parameter.fanAngle + 20f // ����ĽǶ�
                    );
                }
            }
            
            yield return new WaitForSeconds(0.4f);
        }
    }
    
    private void FireTrackingProjectile()
    {
        if (parameter.rangedAttackPoints == null || parameter.rangedAttackPoints.Length == 0) return;
        if (parameter.target == null) return;
        
        // ѡ�����������
        Transform attackPoint = parameter.rangedAttackPoints[Random.Range(0, parameter.rangedAttackPoints.Length)];
        if (attackPoint == null) return;
        
        // ����׷�ٵ�Ļ
        GameObject projectile = boss.CreateProjectile(
            parameter.trackingProjectilePrefab,
            attackPoint.position,
            parameter.target.position,
            parameter.rangedDamage,
            true // ����׷��
        );
        
        Debug.Log($"Bossʷ��ķ�� {attackPoint.name} ����׷�ٵ�Ļ");
    }
}