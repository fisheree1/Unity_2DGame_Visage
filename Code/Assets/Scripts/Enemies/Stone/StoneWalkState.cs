using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone���˵�����״̬
/// Stone�ڴ�״̬����Ѳ�ߵ�֮���ƶ������û��Ѳ�ߵ�����м򵥵�����Ѳ��
/// ֧������Ѳ��ģʽ����
/// </summary>
public class StoneWalkState : IState
{
    private StoneP manager;              // Stone���˹���������
    private StoneParameter parameter;    // Stone���˲�������
    private Rigidbody2D rb;             // �������
    
    // Ѳ�ߵ���ر���
    private int currentPatrolIndex = 0;  // ��ǰĿ��Ѳ�ߵ�����
    private bool isWaitingAtPoint = false; // �Ƿ�����Ѳ�ߵ�ȴ�
    private float waitTimer = 0f;        // �ȴ���ʱ��
    private bool isMovingToNextPoint = false; // �Ƿ������ƶ�����һ��Ѳ�ߵ�
    
    // ���Ѳ����ر�������û��Ѳ�ߵ�ʱʹ�ã�
    private float walkDirection = 1f;    // ���߷���1=�ң�-1=��
    private float walkTimer = 0f;        // ���߼�ʱ��
    private float maxWalkTime = 3f;      // �����������ʱ��
    
    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="manager">Stone���˹�����</param>
    /// <param name="parameter">Stone���˲���</param>
    public StoneWalkState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// ��������״̬ʱ�ĳ�ʼ��
    /// </summary>
    public void OnEnter()
    {
        // ����״̬��־
        parameter.isReacting = false;
        parameter.isHit = false;
        
        // ����Ƿ���Ѳ�ߵ�
        if (HasValidPatrolPoints())
        {
            InitializePatrolMode();
        }
        else
        {
            InitializeRandomWalkMode();
        }
        
        // �������߶���
        PlayWalkAnimation();
        
        Debug.Log($"Stone��������״̬ - ģʽ: {(HasValidPatrolPoints() ? "Ѳ�ߵ�ģʽ" : "����ƶ�ģʽ")}");
    }

    /// <summary>
    /// ����״̬��ÿ֡����
    /// </summary>
    public void OnUpdate()
    {
        if (HasValidPatrolPoints())
        {
            UpdatePatrolMode();
        }
        else
        {
            UpdateRandomWalkMode();
        }
    }

    /// <summary>
    /// ����Ƿ�����Ч��Ѳ�ߵ�
    /// </summary>
    /// <returns>�Ƿ�����ЧѲ�ߵ�</returns>
    private bool HasValidPatrolPoints()
    {
        return parameter.patrolPoints != null && 
               parameter.patrolPoints.Length > 0 && 
               parameter.patrolPoints[0] != null;
    }

    /// <summary>
    /// ��ʼ��Ѳ��ģʽ
    /// </summary>
    private void InitializePatrolMode()
    {
        // ����Ѳ��״̬
        isWaitingAtPoint = false;
        isMovingToNextPoint = true;
        waitTimer = 0f;
        
        // �ҵ������Ѳ�ߵ���Ϊ��ʼ��
        currentPatrolIndex = FindNearestPatrolPointIndex();
        
        Debug.Log($"StoneѲ��ģʽ��ʼ�� - Ŀ��Ѳ�ߵ�: {currentPatrolIndex}");
    }

    /// <summary>
    /// ��ʼ���������ģʽ
    /// </summary>
    private void InitializeRandomWalkMode()
    {
        walkTimer = 0f;
        walkDirection = Random.value > 0.5f ? 1f : -1f;
        
        Debug.Log($"Stone����ƶ�ģʽ - ����: {(walkDirection > 0 ? "��" : "��")}");
    }

    /// <summary>
    /// ����Ѳ��ģʽ
    /// </summary>
    private void UpdatePatrolMode()
    {
        Vector3 targetPosition = parameter.patrolPoints[currentPatrolIndex].position;
        float distanceToTarget = Vector2.Distance(manager.transform.position, targetPosition);
        
        if (!isWaitingAtPoint)
        {
            // �ƶ���Ѳ�ߵ�
            if (distanceToTarget > 0.1f)
            {
                MoveTowardsPatrolPoint(targetPosition);
                
                // ����Ŀ��Ѳ�ߵ�
                FaceDirection(targetPosition);
            }
            else
            {
                // ����Ѳ�ߵ㣬��ʼ�ȴ�
                isWaitingAtPoint = true;
                isMovingToNextPoint = false;
                waitTimer = 0f;
                StopMovement();
                
                Debug.Log($"Stone����Ѳ�ߵ� {currentPatrolIndex}����ʼ�ȴ�");
            }
        }
        else
        {
            // ��Ѳ�ߵ�ȴ�
            waitTimer += Time.deltaTime;
            
            if (waitTimer >= parameter.idleAtPatrolPointTime)
            {
                // �ȴ���ɣ��ƶ�����һ��Ѳ�ߵ�
                MoveToNextPatrolPoint();
                isWaitingAtPoint = false;
                isMovingToNextPoint = true;
                
                Debug.Log($"Stone�ȴ���ɣ�ǰ����һ��Ѳ�ߵ� {currentPatrolIndex}");
            }
        }
        
        // ����Ƿ�Ӧ�ûص�Idle״̬����ѡ�ĳ�ʱ���ƣ�
        CheckForIdleTransition();
    }

    /// <summary>
    /// �����������ģʽ
    /// </summary>
    private void UpdateRandomWalkMode()
    {
        walkTimer += Time.deltaTime;
        
        // ִ���ƶ�
        PerformRandomWalk();
        
        // ����Ƿ���Ҫ�ı䷽���ص�Idle״̬
        if (walkTimer >= maxWalkTime)
        {
            manager.TransitionState(StoneStateType.Idle);
        }
    }

    /// <summary>
    /// �ƶ���Ѳ�ߵ�
    /// </summary>
    /// <param name="targetPosition">Ŀ��λ��</param>
    private void MoveTowardsPatrolPoint(Vector3 targetPosition)
    {
        Vector3 currentPosition = manager.transform.position;
        Vector3 direction = (targetPosition - currentPosition).normalized;
        
        if (rb != null)
        {
            // ʹ�������ƶ���ֻ��X���ƶ�������Y���ٶȣ�������
            rb.velocity = new Vector2(direction.x * parameter.walkSpeed, rb.velocity.y);
        }
        else
        {
            // �������ƶ�
            Vector3 newPosition = Vector3.MoveTowards(
                currentPosition,
                new Vector3(targetPosition.x, currentPosition.y, currentPosition.z),
                parameter.walkSpeed * Time.deltaTime
            );
            manager.transform.position = newPosition;
        }
    }

    /// <summary>
    /// ִ����������ƶ�
    /// </summary>
    private void PerformRandomWalk()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(walkDirection * parameter.walkSpeed, rb.velocity.y);
        }
    }

    /// <summary>
    /// �ƶ�����һ��Ѳ�ߵ�
    /// </summary>
    private void MoveToNextPatrolPoint()
    {
        if (parameter.patrolPoints.Length == 1)
        {
            // ֻ��һ��Ѳ�ߵ㣬����ԭ��
            return;
        }
        
        // ѭ������һ��Ѳ�ߵ�
        currentPatrolIndex = (currentPatrolIndex + 1) % parameter.patrolPoints.Length;
    }

    /// <summary>
    /// �ҵ������Ѳ�ߵ�����
    /// </summary>
    /// <returns>���Ѳ�ߵ������</returns>
    private int FindNearestPatrolPointIndex()
    {
        if (parameter.patrolPoints.Length == 0) return 0;
        
        int nearestIndex = 0;
        float nearestDistance = Vector2.Distance(manager.transform.position, parameter.patrolPoints[0].position);
        
        for (int i = 1; i < parameter.patrolPoints.Length; i++)
        {
            if (parameter.patrolPoints[i] == null) continue;
            
            float distance = Vector2.Distance(manager.transform.position, parameter.patrolPoints[i].position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }
        
        return nearestIndex;
    }

    /// <summary>
    /// ����ָ������
    /// </summary>
    /// <param name="targetPosition">Ŀ��λ��</param>
    private void FaceDirection(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - manager.transform.position;
        if (Mathf.Abs(direction.x) > 0.1f) // ������Ŀ���ʱƵ����ת
        {
            float newScaleX = Mathf.Sign(direction.x);
            manager.transform.localScale = new Vector3(newScaleX, 1f, 1f);
        }
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
    /// ����Ƿ�Ӧ��ת����Idle״̬
    /// </summary>
    private void CheckForIdleTransition()
    {
        // ��������������ض�������������ʱ�ص�Idle״̬
        // ���磺Ѳ����һ��ʱ�����Ϣ�����߼�⵽�ض�������
        
        // ��ǰʵ�֣�Ѳ�ߵ�ģʽ�²����Զ��ص�Idle�����Ǳ��ⲿ�¼�����
    }

    /// <summary>
    /// �������߶���
    /// ֧�ֶ��ֿ��ܵĶ���������ʽ��ȷ��������
    /// </summary>
    private void PlayWalkAnimation()
    {
        if (parameter?.animator == null) return;

        // ���Ե����߶��������б������ȼ�����
        string[] walkAnimations = {
            "Stone_walk",       // Stone�ض����߶���
            "stone_walk",       // Сд�汾
            "Walk",             // ͨ�����߶���
            "walk",             // Сдͨ�ð汾
            "StoneWalk",        // �շ�����
            "Stone_move",       // �ƶ�����
            "move",             // ͨ���ƶ�
            "Stone_patrol",     // Ѳ�߶���
            "patrol"            // ͨ��Ѳ��
        };

        // ���Բ����ҵ��ĵ�һ����Ч����
        foreach (string animName in walkAnimations)
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
            parameter.animator.SetInteger("State", 1); // 1 ͨ����ʾWalk״̬
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Stone: �޷�����State����: {e.Message}");
        }
        
        // ���ã������������ܵĲ���
        try
        {
            parameter.animator.SetBool("isWalking", true);
        }
        catch { }
        
        try
        {
            parameter.animator.SetBool("IsMoving", true);
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
    /// �˳�����״̬ʱ��������
    /// </summary>
    public void OnExit()
    {
        // ֹͣ�ƶ�
        StopMovement();
        
        // ����״̬
        isWaitingAtPoint = false;
        isMovingToNextPoint = false;
        waitTimer = 0f;
        walkTimer = 0f;
        
        // ���ö�������
        try
        {
            parameter.animator.SetInteger("State", 0); // �ص�Idle״̬
            parameter.animator.SetBool("isWalking", false);
            parameter.animator.SetBool("IsMoving", false);
        }
        catch { }
        
        Debug.Log("Stone�뿪����״̬");
    }
}