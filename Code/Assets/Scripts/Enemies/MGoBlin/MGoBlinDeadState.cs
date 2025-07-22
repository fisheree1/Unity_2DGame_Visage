using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MGoBlinDeadState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private readonly float maxDestroyTime = 2.0f; // ���ȴ�ʱ�䣬��ֹ������������
    private bool isDestroying = false;
    private AnimatorStateInfo info;

    public MGoBlinDeadState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("MGoBlin entering Dead State");
        
        // ��֤������
        if (manager == null)
        {
            Debug.LogError("MGoBlinDeadState: Manager is null!");
            return;
        }
        
        // ��ȫ������������
        if (parameter?.animator != null)
        {
            if (HasAnimationState("MGoBlin_death"))
            {
                parameter.animator.Play("MGoBlin_death");
            }
            else
            {
                Debug.LogWarning("MGoBlinDeadState: Animation state 'MGoBlin_death' not found!");
            }
        }
        else
        {
            Debug.LogWarning("MGoBlinDeadState: Animator is null, skipping death animation");
        }
        
        // ������ײ���Է�ֹ��һ������
        var colliders = manager.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
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
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (isDestroying) return;
        
        destroyTimer += Time.deltaTime;
        
        // ��������ʱ�䵽��򳬹����ȴ�ʱ��
        if (destroyTimer >= destroyDelay || destroyTimer >= maxDestroyTime||info.normalizedTime >0.95f)
        {
            DestroyMGoBlin();
        }
    }
    
    private void DestroyMGoBlin()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // ���������ӵ�����Ʒ����Ч����������Ч��
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log($"MGoBlin destroyed after {destroyTimer:F2} seconds");
        
        if (manager != null && manager.gameObject != null)
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Debug.LogWarning("MGoBlinDeadState: Manager or GameObject is null during destruction");
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
        Debug.Log("MGoBlinDeadState: OnExit called (this should not happen)");
    }
}