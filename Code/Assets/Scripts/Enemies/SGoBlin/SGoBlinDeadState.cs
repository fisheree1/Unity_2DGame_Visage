using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SGoBlinDeadState : IState
{
    private SGoBlinP manager;
    private SGoBlinParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private readonly float maxDestroyTime = 2.0f; // ���ȴ�ʱ�䣬��ֹ������������
    private bool isDestroying = false;

    public SGoBlinDeadState(SGoBlinP manager, SGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("SGoBlin entering Dead State");
        
        // ��֤������
        if (manager == null)
        {
            Debug.LogError("SGoBlinDeadState: Manager is null!");
            return;
        }
        
        // ��������������������������ڣ�
        if (parameter?.animator != null)
        {
            parameter.animator.Play("S_GoBlin_death");
        }
        else
        {
            Debug.LogWarning("SGoBlinDeadState: Animator is null, skipping death animation");
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
            DestroySGoBlin();
        }
    }
    
    private void DestroySGoBlin()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // ���������ӵ�����Ʒ����Ч
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log($"SGoBlin destroyed after {destroyTimer:F2} seconds");
        
        if (manager != null && manager.gameObject != null)
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Debug.LogWarning("SGoBlinDeadState: Manager or GameObject is null during destruction");
        }
    }
    
    public void OnExit()
    {
        // ����״̬ͨ�������˳�����Ϊ����ᱻ����
        Debug.Log("SGoBlinDeadState: OnExit called (this should not happen)");
    }
}