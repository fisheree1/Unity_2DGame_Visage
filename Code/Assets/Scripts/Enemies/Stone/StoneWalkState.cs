using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stone敌人的行走状态
/// Stone在此状态下在巡逻点之间移动，如果没有巡逻点则进行简单的左右巡逻
/// 支持灵活的巡逻模式配置
/// </summary>
public class StoneWalkState : IState
{
    private StoneP manager;              // Stone敌人管理器引用
    private StoneParameter parameter;    // Stone敌人参数引用
    private Rigidbody2D rb;             // 刚体组件
    
    // 巡逻点相关变量
    private int currentPatrolIndex = 0;  // 当前目标巡逻点索引
    private bool isWaitingAtPoint = false; // 是否正在巡逻点等待
    private float waitTimer = 0f;        // 等待计时器
    private bool isMovingToNextPoint = false; // 是否正在移动到下一个巡逻点
    
    // 随机巡逻相关变量（当没有巡逻点时使用）
    private float walkDirection = 1f;    // 行走方向（1=右，-1=左）
    private float walkTimer = 0f;        // 行走计时器
    private float maxWalkTime = 3f;      // 最大连续行走时间
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="manager">Stone敌人管理器</param>
    /// <param name="parameter">Stone敌人参数</param>
    public StoneWalkState(StoneP manager, StoneParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 进入行走状态时的初始化
    /// </summary>
    public void OnEnter()
    {
        // 重置状态标志
        parameter.isReacting = false;
        parameter.isHit = false;
        
        // 检查是否有巡逻点
        if (HasValidPatrolPoints())
        {
            InitializePatrolMode();
        }
        else
        {
            InitializeRandomWalkMode();
        }
        
        // 播放行走动画
        PlayWalkAnimation();
        
        Debug.Log($"Stone进入行走状态 - 模式: {(HasValidPatrolPoints() ? "巡逻点模式" : "随机移动模式")}");
    }

    /// <summary>
    /// 行走状态的每帧更新
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
    /// 检查是否有有效的巡逻点
    /// </summary>
    /// <returns>是否有有效巡逻点</returns>
    private bool HasValidPatrolPoints()
    {
        return parameter.patrolPoints != null && 
               parameter.patrolPoints.Length > 0 && 
               parameter.patrolPoints[0] != null;
    }

    /// <summary>
    /// 初始化巡逻模式
    /// </summary>
    private void InitializePatrolMode()
    {
        // 重置巡逻状态
        isWaitingAtPoint = false;
        isMovingToNextPoint = true;
        waitTimer = 0f;
        
        // 找到最近的巡逻点作为起始点
        currentPatrolIndex = FindNearestPatrolPointIndex();
        
        Debug.Log($"Stone巡逻模式初始化 - 目标巡逻点: {currentPatrolIndex}");
    }

    /// <summary>
    /// 初始化随机行走模式
    /// </summary>
    private void InitializeRandomWalkMode()
    {
        walkTimer = 0f;
        walkDirection = Random.value > 0.5f ? 1f : -1f;
        
        Debug.Log($"Stone随机移动模式 - 方向: {(walkDirection > 0 ? "右" : "左")}");
    }

    /// <summary>
    /// 更新巡逻模式
    /// </summary>
    private void UpdatePatrolMode()
    {
        Vector3 targetPosition = parameter.patrolPoints[currentPatrolIndex].position;
        float distanceToTarget = Vector2.Distance(manager.transform.position, targetPosition);
        
        if (!isWaitingAtPoint)
        {
            // 移动到巡逻点
            if (distanceToTarget > 0.1f)
            {
                MoveTowardsPatrolPoint(targetPosition);
                
                // 面向目标巡逻点
                FaceDirection(targetPosition);
            }
            else
            {
                // 到达巡逻点，开始等待
                isWaitingAtPoint = true;
                isMovingToNextPoint = false;
                waitTimer = 0f;
                StopMovement();
                
                Debug.Log($"Stone到达巡逻点 {currentPatrolIndex}，开始等待");
            }
        }
        else
        {
            // 在巡逻点等待
            waitTimer += Time.deltaTime;
            
            if (waitTimer >= parameter.idleAtPatrolPointTime)
            {
                // 等待完成，移动到下一个巡逻点
                MoveToNextPatrolPoint();
                isWaitingAtPoint = false;
                isMovingToNextPoint = true;
                
                Debug.Log($"Stone等待完成，前往下一个巡逻点 {currentPatrolIndex}");
            }
        }
        
        // 检查是否应该回到Idle状态（可选的超时机制）
        CheckForIdleTransition();
    }

    /// <summary>
    /// 更新随机行走模式
    /// </summary>
    private void UpdateRandomWalkMode()
    {
        walkTimer += Time.deltaTime;
        
        // 执行移动
        PerformRandomWalk();
        
        // 检查是否需要改变方向或回到Idle状态
        if (walkTimer >= maxWalkTime)
        {
            manager.TransitionState(StoneStateType.Idle);
        }
    }

    /// <summary>
    /// 移动到巡逻点
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    private void MoveTowardsPatrolPoint(Vector3 targetPosition)
    {
        Vector3 currentPosition = manager.transform.position;
        Vector3 direction = (targetPosition - currentPosition).normalized;
        
        if (rb != null)
        {
            // 使用物理移动，只在X轴移动，保持Y轴速度（重力）
            rb.velocity = new Vector2(direction.x * parameter.walkSpeed, rb.velocity.y);
        }
        else
        {
            // 非物理移动
            Vector3 newPosition = Vector3.MoveTowards(
                currentPosition,
                new Vector3(targetPosition.x, currentPosition.y, currentPosition.z),
                parameter.walkSpeed * Time.deltaTime
            );
            manager.transform.position = newPosition;
        }
    }

    /// <summary>
    /// 执行随机行走移动
    /// </summary>
    private void PerformRandomWalk()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(walkDirection * parameter.walkSpeed, rb.velocity.y);
        }
    }

    /// <summary>
    /// 移动到下一个巡逻点
    /// </summary>
    private void MoveToNextPatrolPoint()
    {
        if (parameter.patrolPoints.Length == 1)
        {
            // 只有一个巡逻点，保持原地
            return;
        }
        
        // 循环到下一个巡逻点
        currentPatrolIndex = (currentPatrolIndex + 1) % parameter.patrolPoints.Length;
    }

    /// <summary>
    /// 找到最近的巡逻点索引
    /// </summary>
    /// <returns>最近巡逻点的索引</returns>
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
    /// 面向指定方向
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    private void FaceDirection(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - manager.transform.position;
        if (Mathf.Abs(direction.x) > 0.1f) // 避免在目标点时频繁翻转
        {
            float newScaleX = Mathf.Sign(direction.x);
            manager.transform.localScale = new Vector3(newScaleX, 1f, 1f);
        }
    }

    /// <summary>
    /// 停止所有移动
    /// </summary>
    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    /// <summary>
    /// 检查是否应该转换到Idle状态
    /// </summary>
    private void CheckForIdleTransition()
    {
        // 可以在这里添加特定条件来决定何时回到Idle状态
        // 例如：巡逻了一定时间后休息，或者检测到特定条件等
        
        // 当前实现：巡逻点模式下不会自动回到Idle，除非被外部事件触发
    }

    /// <summary>
    /// 播放行走动画
    /// 支持多种可能的动画命名方式，确保兼容性
    /// </summary>
    private void PlayWalkAnimation()
    {
        if (parameter?.animator == null) return;

        // 尝试的行走动画名称列表（按优先级排序）
        string[] walkAnimations = {
            "Stone_walk",       // Stone特定行走动画
            "stone_walk",       // 小写版本
            "Walk",             // 通用行走动画
            "walk",             // 小写通用版本
            "StoneWalk",        // 驼峰命名
            "Stone_move",       // 移动动画
            "move",             // 通用移动
            "Stone_patrol",     // 巡逻动画
            "patrol"            // 通用巡逻
        };

        // 尝试播放找到的第一个有效动画
        foreach (string animName in walkAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // 如果没有找到动画状态，尝试使用参数控制
        try
        {
            parameter.animator.SetInteger("State", 1); // 1 通常表示Walk状态
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Stone: 无法设置State参数: {e.Message}");
        }
        
        // 备用：尝试其他可能的参数
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
    /// 检查动画控制器中是否存在指定的动画状态
    /// </summary>
    /// <param name="stateName">动画状态名称</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    private bool HasAnimationState(string stateName)
    {
        if (parameter?.animator == null) return false;

        var controller = parameter.animator.runtimeAnimatorController;
        if (controller == null) return false;

        // 遍历所有动画片段查找匹配的名称
        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 退出行走状态时的清理工作
    /// </summary>
    public void OnExit()
    {
        // 停止移动
        StopMovement();
        
        // 重置状态
        isWaitingAtPoint = false;
        isMovingToNextPoint = false;
        waitTimer = 0f;
        walkTimer = 0f;
        
        // 重置动画参数
        try
        {
            parameter.animator.SetInteger("State", 0); // 回到Idle状态
            parameter.animator.SetBool("isWalking", false);
            parameter.animator.SetBool("IsMoving", false);
        }
        catch { }
        
        Debug.Log("Stone离开行走状态");
    }
}