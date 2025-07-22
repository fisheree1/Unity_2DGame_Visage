using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MomDeadState : IState
{
    private MomP manager;
    private MomParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.5f;
    private readonly float maxDestroyTime = 1.0f; // ���ȴ�ʱ�䣬��ֹ������������
    private bool isDestroying = false;

    public MomDeadState(MomP manager, MomParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Mom entering Dead State");
        
        // ��֤������
        if (manager == null)
        {
            Debug.LogError("MomDeadState: Manager is null!");
            return;
        }
        
        // ��ȫ������������
        if (parameter?.animator != null)
        {
            if (HasAnimationState("Mom_death"))
            {
                parameter.animator.Play("Mom_death");
            }
            else
            {
                Debug.LogWarning("MomDeadState: Animation state 'Mom_death' not found!");
            }
        }
        else
        {
            Debug.LogWarning("MomDeadState: Animator is null, skipping death animation");
        }
        
        // ������ײ���Է�ֹ��һ������
        var collider = manager.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // ֹͣ�ƶ�
        var rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        destroyTimer = 0f;
        isDestroying = false;
    }

    public void OnUpdate()
    {
        if (isDestroying) return;
        
        destroyTimer += Time.deltaTime;
        
        // ��������ʱ�䵽��򳬹����ȴ�ʱ��
        if (destroyTimer >= destroyDelay || destroyTimer >= maxDestroyTime)
        {
            DestroyMom();
        }
    }
    
    private void DestroyMom()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // ���������ӵ�����Ʒ����Ч����������Ч��
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log($"Mom destroyed after {destroyTimer:F2} seconds");
        
        if (manager != null && manager.gameObject != null)
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Debug.LogWarning("MomDeadState: Manager or GameObject is null during destruction");
        }
    }
    
    private bool HasAnimationState(string stateName)
    {
        if (parameter?.animator == null) return false;
        
        // ��鶯���������Ƿ���ָ��״̬
        var controller = parameter.animator.runtimeAnimatorController;
        if (controller == null) return false;
        
        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }
        
        return false;
    }
    
    public void OnExit()
    {
        // ����״̬ͨ�������˳�����Ϊ����ᱻ����
        Debug.Log("MomDeadState: OnExit called (this should not happen)");
    }
}