using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone���˵�����״̬
/// ��Stone������ֵ����0ʱ�����״̬
/// ���������������źͶ���������
/// </summary>
public class StoneDeadState : IState
{
    private StoneP manager;              // Stone���˹���������
    private StoneParameter parameter;    // Stone���˲�������
    private float deathTimer = 0f;       // ����״̬��ʱ��
    private float deathDuration = 2f;    // ����״̬����ʱ��
    private Rigidbody2D rb;             // �������
    private Collider2D col;             // ��ײ�����
    
    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="manager">Stone���˹�����</param>
    /// <param name="parameter">Stone���˲���</param>
    public StoneDeadState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
        col = manager.GetComponent<Collider2D>();
    }

    /// <summary>
    /// ��������״̬ʱ�ĳ�ʼ��
    /// </summary>
    public void OnEnter()
    {
        deathTimer = 0f;                // ���ü�ʱ��
        
        // ��������״̬��־
        parameter.isReacting = false;
        parameter.isHit = false;
        parameter.isInDefence = false;
        parameter.target = null;
        
        // ֹͣ�����ƶ�
        StopMovement();
        
        // ������ײ����ֹ��һ������
        DisableCollisions();
        
        // ������������
        PlayDeathAnimation();
        
        Debug.Log("Stone��������״̬");
    }

    /// <summary>
    /// ����״̬��ÿ֡����
    /// </summary>
    public void OnUpdate()
    {
        deathTimer += Time.deltaTime;
        
        // ����Ƿ������������
        if (deathTimer >= deathDuration)
        {
            CleanupAndDestroy();
        }
    }

    /// <summary>
    /// ������������
    /// ���Զ�����ܵĶ������ƣ�ȷ��������
    /// </summary>
    private void PlayDeathAnimation()
    {
        if (parameter?.animator == null) return;

        // ���Ե��������������б������ȼ�����
        string[] deathAnimations = {
            "Stone_dead",       // Stone�ض���������
            "stone_dead",       // Сд�汾
            "Dead",             // ͨ����������
            "dead",             // Сдͨ�ð汾
            "StoneDead",        // �շ�����
            "Stone_death",      // ��������
            "death",            // ͨ������
            "Stone_die",        // ��������
            "die",              // ͨ������
            "Stone_destroy",    // ���ٶ���
            "destroy"           // ͨ������
        };

        // ���Բ����ҵ��ĵ�һ����Ч����
        foreach (string animName in deathAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // ���û���ҵ�����״̬������ʹ�ô�����
        try
        {
            parameter.animator.SetTrigger("Dead");
            Debug.Log("Stone: ʹ��Dead������������������");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Stone: �޷�����Dead����: {e.Message}");
        }
        
        try
        {
            parameter.animator.SetTrigger("Death");
        }
        catch { }
        
        try
        {
            parameter.animator.SetBool("isDead", true);
        }
        catch { }
    }

    /// <summary>
    /// ��鶯�����������Ƿ����ָ���Ķ���״̬
    /// </summary>
    /// <param name="stateName">����״̬����</param>
    /// <returns>������ڷ���true�����򷵻�false</returns>
    private bool HasAnimationState(string stateName)
    {
        if (parameter?.animator == null) return false;

        var controller = parameter.animator.runtimeAnimatorController;
        if (controller == null) return false;

        // �������ж���Ƭ�β���ƥ�������
        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// ֹͣ�����ƶ�
    /// </summary>
    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // ����Ϊ�˶�ѧģʽ��ֹͣ����ģ��
        }
    }

    /// <summary>
    /// ������ײ��
    /// </summary>
    private void DisableCollisions()
    {
        if (col != null)
        {
            col.enabled = false;
        }
        
        // ͬʱ�������������ײ������sight��
        Collider2D[] childColliders = manager.GetComponentsInChildren<Collider2D>();
        foreach (var collider in childColliders)
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// ��������ٶ���
    /// </summary>
    private void CleanupAndDestroy()
    {
        Debug.Log("Stone����������ɣ�׼�����ٶ���");
        
        // ��ѡ��֪ͨ��Ϸ������Stone������
        // GameManager.Instance?.OnEnemyDefeated("Stone");
        
        // ��ѡ��������Ʒ����
        // SpawnRewards();
        
        // ����Stone����
        Object.Destroy(manager.gameObject);
    }

    /// <summary>
    /// �˳�����״̬ʱ��������
    /// ע�⣺ͨ�����������״̬�˳������������Ҫ����������
    /// </summary>
    public void OnExit()
    {
        // ͨ������״̬���ᱻ�˳�����Ϊ�������Ա����˷���
        Debug.Log("Stone�뿪����״̬����ͨ����Ӧ�÷�����");
    }
}