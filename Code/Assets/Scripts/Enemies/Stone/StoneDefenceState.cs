using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone���˵ķ���״̬
/// ��Stone�ܵ�3�ι���������״̬����2�������������˺�
/// ����ʹ��bool IsDefence�������Ʒ�������
/// </summary>
public class StoneDefenceState : IState
{
    private StoneP manager;              // Stone���˹���������
    private StoneParameter parameter;    // Stone���˲�������
    private float defenceTimer = 0f;     // ����״̬��ʱ��
    private Rigidbody2D rb;             // �������
    private Color originalColor;         // ԭʼ��ɫ�����ڻָ���
    private SpriteRenderer spriteRenderer; // ������Ⱦ���������Ӿ�Ч����

    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="manager">Stone���˹�����</param>
    /// <param name="parameter">Stone���˲���</param>
    public StoneDefenceState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
        spriteRenderer = manager.GetComponent<SpriteRenderer>();
        
        // ����ԭʼ��ɫ�Ա����������ָ�
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// �������״̬ʱ�ĳ�ʼ��
    /// </summary>
    public void OnEnter()
    {
        parameter.isInDefence = true;  // ���÷�����־
        defenceTimer = 0f;             // ���ü�ʱ��
        
        // �����ܻ�������������������¼��㣩
        parameter.hitCount = 0;
        
        StopMovement();          // ֹͣ�ƶ�
        PlayDefenceAnimation();  // ���ŷ�������
        
        // �Ӿ�Ч�� - ��ɫ����ʾ����״̬
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 1f); // ��ɫ��
        }
        
        // ���ŷ�����Ч
        if (parameter.audioSource != null && parameter.defenceSound != null)
        {
            parameter.audioSource.PlayOneShot(parameter.defenceSound);
        }
        
        Debug.Log("Stone�������ģʽ - �� " + parameter.defenceDuration + " �������������˺���");
    }

    /// <summary>
    /// ����״̬��ÿ֡����
    /// </summary>
    public void OnUpdate()
    {
        defenceTimer += Time.deltaTime;
        
        // ��ѡ���ڷ����ڼ��������Ч��
        if (spriteRenderer != null)
        {
            float pulse = Mathf.Sin(Time.time * 8f) * 0.2f + 0.8f; // ��0.6��1.0֮������
            spriteRenderer.color = new Color(0.5f, 0.5f, 1f, pulse);
        }
        
        // ����ʱ��������˳�����״̬
        if (defenceTimer >= parameter.defenceDuration)
        {
            manager.TransitionState(StoneStateType.Idle);
        }
    }

    /// <summary>
    /// ���ŷ�������
    /// ʹ��bool IsDefence���������Ǵ�����
    /// </summary>
    private void PlayDefenceAnimation()
    {
        if (parameter?.animator == null) return;

        // ���Եķ������������б������ȼ�����
        string[] defenceAnimations = {
            "Stone_defence",    // Stone�ض���������
            "stone_defence",    // Сд�汾
            "Defence",          // ͨ�÷�������
            "defence",          // Сдͨ�ð汾
            "StoneDefence",     // �շ�����
            "Stone_shield",     // ���ܶ���
            "shield",           // ͨ�û���
            "Stone_protect",    // ��������
            "protect",          // ͨ�ñ���
            "Stone_block",      // �񵲶���
            "block"             // ͨ�ø�
        };

        // ���Բ����ҵ��ĵ�һ����Ч����
        foreach (string animName in defenceAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // ���û���ҵ�����״̬��ʹ��bool IsDefence��������
        try
        {
            // ����bool����IsDefenceΪtrue��������������
            parameter.animator.SetBool("IsDefence", true);
            Debug.Log("Stone: ʹ��bool IsDefence����������������");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Stone: �޷�����IsDefence����: {e.Message}");
        }
        
        // ���ã������������ܵ�bool��������
        try
        {
            parameter.animator.SetBool("isDefending", true);
        }
        catch { }
        
        try
        {
            parameter.animator.SetBool("IsDefending", true);
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
            rb.velocity = new Vector2(0, rb.velocity.y); // ����Y���ٶȣ�������
        }
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
    /// �˳�����״̬ʱ��������
    /// </summary>
    public void OnExit()
    {
        parameter.isInDefence = false;  // ���������־
        defenceTimer = 0f;              // ���ü�ʱ��
        
        // �ָ�ԭʼ��ɫ
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // ���÷�������״̬ - ʹ��bool IsDefence����
        try
        {
            // ����bool����IsDefenceΪfalse��������������
            parameter.animator.SetBool("IsDefence", false);
            Debug.Log("Stone: ʹ��bool IsDefence����������������");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Stone: �޷�����IsDefence����: {e.Message}");
        }
        
        // �����������ܵķ������bool����
        try
        {
            parameter.animator.SetBool("isDefending", false);
        }
        catch { }
        
        try
        {
            parameter.animator.SetBool("IsDefending", false);
        }
        catch { }
        
        Debug.Log("Stone����ģʽ���� - ���ڿ����ٴ��ܵ��˺�");
    }
}