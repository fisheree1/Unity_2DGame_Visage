using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpiderDeadState : IState
{
    private SpiderP manager;
    private SpiderParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.5f;
    private readonly float maxDestroyTime = 1.0f; // ���ȴ�ʱ�䣬��ֹ������������
    private bool isDestroying = false;

    public SpiderDeadState(SpiderP manager, SpiderParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Spider entering Dead State");
        
        // ��֤�������Ͳ���
        if (manager == null)
        {
            Debug.LogError("SpiderDeadState: Manager is null!");
            return;
        }
        
        // ��������������������������ڣ�
        if (parameter?.animator != null)
        {
            parameter.animator.Play("Spider_death");
        }
        else
        {
            Debug.LogWarning("SpiderDeadState: Animator is null, skipping death animation");
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
            DestroySpider();
        }
    }
    
    private void DestroySpider()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // ���������ӵ�����Ʒ����Ч
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log($"Spider destroyed after {destroyTimer:F2} seconds");
        
        if (manager != null && manager.gameObject != null)
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Debug.LogWarning("SpiderDeadState: Manager or GameObject is null during destruction");
        }
    }
    
    public void OnExit()
    {
        // ����״̬ͨ�������˳�����Ϊ����ᱻ����
        Debug.Log("SpiderDeadState: OnExit called (this should not happen)");
    }
}