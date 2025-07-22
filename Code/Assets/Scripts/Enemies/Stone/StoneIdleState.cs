using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone���˵Ĵ���״̬
/// Stone�ڴ�״̬�±��־�ֹ���ȴ���ҽ�����Ұ��Χ
/// ����Stone��Ĭ��״̬��Ҳ�������״̬
/// </summary>
public class StoneIdleState : IState
{
    private StoneP manager;              // Stone���˹���������
    private StoneParameter parameter;    // Stone���˲�������
    private Rigidbody2D rb;             // �������
    
    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="manager">Stone���˹�����</param>
    /// <param name="parameter">Stone���˲���</param>
    public StoneIdleState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// �������״̬ʱ�ĳ�ʼ��
    /// </summary>
    public void OnEnter()
    {
        // ����״̬��־
        parameter.isReacting = false;
        parameter.isHit = false;
        
        // ֹͣ�ƶ�
        StopMovement();
        
        // ���Ŵ�������
        PlayIdleAnimation();
        
        Debug.Log("Stone�������״̬ - �ȴ���ҽ�����Ұ");
    }

    /// <summary>
    /// ����״̬��ÿ֡����
    /// �ڴ�״̬��Stone������ִ���κβ������ȴ��ⲿ������
    /// </summary>
    public void OnUpdate()
    {
        // ����״̬����Ҫ������������⣬����������һЩ�����߼�
        
        // �����Ŀ�굫���ڷ�Ӧ״̬��������Ҫ��������
        if (parameter.target != null && !parameter.isReacting)
        {
            // �������ͨ����sight��������������ֻ�Ǳ��ü��
            Debug.LogWarning("Stone: ��Idle״̬�¼�⵽Ŀ�꣬��δ����React״̬");
        }
    }

    /// <summary>
    /// ���Ŵ�������
    /// ֧�ֶ��ֿ��ܵĶ���������ʽ��ȷ��������
    /// </summary>
    private void PlayIdleAnimation()
    {
        if (parameter?.animator == null) return;

        // ���ԵĴ������������б������ȼ�����
        string[] idleAnimations = {
            "Stone_idle", "stone_idle", "Idle", "idle", "StoneIdle",
            "Stone_default", "default", "Stone_stand", "stand"
        };

        // ���Բ����ҵ��ĵ�һ����Ч����
        foreach (string animName in idleAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // ���û���ҵ�����״̬������ʹ�ò�������
        try
        {
            parameter.animator.SetInteger("State", 0);
            parameter.animator.SetBool("IsReacting", false);
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
    /// ��鶯�����������Ƿ����ָ���Ķ���״̬
    /// </summary>
    private bool HasAnimationState(string stateName)
    {
        if (parameter?.animator == null) return false;
        var controller = parameter.animator.runtimeAnimatorController;
        if (controller == null) return false;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName) return true;
        }
        return false;
    }

    /// <summary>
    /// �˳�����״̬ʱ��������
    /// </summary>
    public void OnExit()
    {
        Debug.Log("Stone�뿪����״̬");
    }
}