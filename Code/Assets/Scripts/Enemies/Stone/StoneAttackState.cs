using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone���˵Ĺ���״̬
/// ִ��Stone�����Ƽ��ܣ�һ�������Ĺ�������
/// </summary>
public class StoneAttackState : IState
{
    private StoneP manager;              // Stone���˹���������
    private StoneParameter parameter;    // Stone���˲�������
    private bool hasAttacked = false;    // �Ƿ��Ѿ�ִ�й�����
    private float attackTimer = 0f;      // ����״̬��ʱ��
    private float attackDuration = 1f;   // ����״̬����ʱ��
    private Rigidbody2D rb;             // �������
    
    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="manager">Stone���˹�����</param>
    /// <param name="parameter">Stone���˲���</param>
    public StoneAttackState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// ���빥��״̬ʱ�ĳ�ʼ��
    /// </summary>
    public void OnEnter()
    {
        hasAttacked = false;        // ���ù�����־
        attackTimer = 0f;           // ���ü�ʱ��
        parameter.isReacting = false; // �����Ӧ��־
        
        // ֹͣ�ƶ�
        StopMovement();
        
        // ���һ������Ŀ��
        if (parameter.target != null)
        {
            manager.FlipTo(parameter.target);
        }
        
        // ���Ź�������
        PlayAttackAnimation();
        
        Debug.Log("Stone���빥��״̬ - ׼���ͷ�����������");
    }

    /// <summary>
    /// ����״̬��ÿ֡����
    /// </summary>
    public void OnUpdate()
    {
        attackTimer += Time.deltaTime;
        
        // �ڹ����������ʵ�ʱ��ִ�й�������
        if (!hasAttacked && attackTimer >= 0.3f)
        {
            ExecuteBeamAttack();
            hasAttacked = true;
        }
        
        // ����״̬����һ��ʱ��󷵻�Idle
        if (attackTimer >= attackDuration)
        {
            manager.TransitionState(StoneStateType.Idle);
        }
    }

    /// <summary>
    /// ִ�й�������
    /// </summary>
    private void ExecuteBeamAttack()
    {
        if (parameter.attackPoint == null)
        {
            Debug.LogWarning("Stone: ������δ���ã��޷�ִ�й���������");
            return;
        }
        
        // ʹ��StoneP�еĹ�����������
        manager.FireBeamAttack();
        
        Debug.Log("Stone�ͷŹ���������");
    }

    /// <summary>
    /// ���Ź�������
    /// </summary>
    private void PlayAttackAnimation()
    {
        if (parameter?.animator == null) return;

        try
        {
            parameter.animator.SetTrigger("Attack");
        }
        catch { }
    }

    /// <summary>
    /// ֹͣ�����ƶ�
    /// </summary>
    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    /// <summary>
    /// �˳�����״̬ʱ��������
    /// </summary>
    public void OnExit()
    {
        hasAttacked = false;        // ���ù�����־
        attackTimer = 0f;           // ���ü�ʱ��
        
        Debug.Log("Stone�뿪����״̬");
    }
}