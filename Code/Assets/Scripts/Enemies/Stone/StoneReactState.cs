using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone���˵ķ�Ӧ״̬
/// ��Stone��sight��⵽���ʱ�����״̬
/// ����0.3���׼��ʱ�䣬Ȼ��ת�빥��״̬
/// </summary>
public class StoneReactState : IState
{
    private StoneP manager;              // Stone���˹���������
    private StoneParameter parameter;    // Stone���˲�������
    private float reactTimer = 0f;       // ��Ӧ״̬��ʱ��
    private Rigidbody2D rb;             // �������
    
    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="manager">Stone���˹�����</param>
    /// <param name="parameter">Stone���˲���</param>
    public StoneReactState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// ���뷴Ӧ״̬ʱ�ĳ�ʼ��
    /// </summary>
    public void OnEnter()
    {
        parameter.isReacting = true;   // ���÷�Ӧ��־
        parameter.isHit = false;       // �����ܻ���־
        reactTimer = 0f;               // ���ü�ʱ��
        
        // ֹͣ�ƶ�
        StopMovement();
        
        // ����Ŀ�����
        if (parameter.target != null)
        {
            manager.FlipTo(parameter.target);
        }
        
        // ���ŷ�Ӧ����
        PlayReactAnimation();
        
        Debug.Log($"Stone��⵽��Ҳ����뷴Ӧ״̬ - ���� {parameter.reactDuration} ��󹥻���");
    }

    /// <summary>
    /// ��Ӧ״̬��ÿ֡����
    /// </summary>
    public void OnUpdate()
    {
        reactTimer += Time.deltaTime;
        
        // ��������Ŀ�꣨���Ŀ�껹���ڣ�
        if (parameter.target != null)
        {
            manager.FlipTo(parameter.target);
        }
        
        // ��鷴Ӧʱ���Ƿ����
        if (reactTimer >= parameter.reactDuration)
        {
            // ���Ŀ���Ƿ���Ȼ����
            if (parameter.target != null)
            {
                manager.TransitionState(StoneStateType.Attack);
            }
            else
            {
                manager.TransitionState(StoneStateType.Idle);
            }
        }
    }

    /// <summary>
    /// ���ŷ�Ӧ����
    /// </summary>
    private void PlayReactAnimation()
    {
        if (parameter?.animator == null) return;

        try
        {
            parameter.animator.SetBool("IsReacting", true);
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
    /// �˳���Ӧ״̬ʱ��������
    /// </summary>
    public void OnExit()
    {
        parameter.isReacting = false;   // �����Ӧ��־
        reactTimer = 0f;                // ���ü�ʱ��
        
        try
        {
            parameter.animator.SetBool("IsReacting", false);
        }
        catch { }
        
        Debug.Log("Stone�뿪��Ӧ״̬");
    }
}