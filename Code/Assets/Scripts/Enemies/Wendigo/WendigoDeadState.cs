using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WendigoDeadState : IState
{
    private WendigoP manager;
    private WendigoParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private readonly float maxDestroyTime = 2.0f; // ���ȴ�ʱ�䣬��ֹ������������
    private bool isDestroying = false;

    public WendigoDeadState(WendigoP manager, WendigoParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Wendigo entering Dead State");
        
        // ��֤������
        if (manager == null)
        {
            Debug.LogError("WendigoDeadState: Manager is null!");
            return;
        }
        
        // ��������������������������ڣ�
        if (parameter?.animator != null)
        {
            parameter.animator.Play("Wendigo_death");
        }
        else
        {
            Debug.LogWarning("WendigoDeadState: Animator is null, skipping death animation");
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
            DestroyWendigo();
        }
    }
    
    private void DestroyWendigo()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // ���������ӵ�����Ʒ����Ч
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log($"Wendigo destroyed after {destroyTimer:F2} seconds");
        
        if (manager != null && manager.gameObject != null)
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Debug.LogWarning("WendigoDeadState: Manager or GameObject is null during destruction");
        }
    }
    
    public void OnExit()
    {
        // ����״̬ͨ�������˳�����Ϊ����ᱻ����
        Debug.Log("WendigoDeadState: OnExit called (this should not happen)");
    }
}