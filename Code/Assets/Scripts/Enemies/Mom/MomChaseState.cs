using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MomChaseState : IState
{
    private MomP manager;
    private MomParameter parameter;
    private Rigidbody2D rb;
    private bool isInAttackRange;
    private float Timer = 0f;

    public MomChaseState(MomP manager, MomParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    public void OnEnter()
    {
        parameter.animator.Play("Mom_chase");
        isInAttackRange = false;
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MomStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        else
        {
            // 1. 目标丢失或超出追击范围 -> 退回Idle
            

            // 2. 保持面向目标
            manager.FlipTo(parameter.target);

            // 3. 移动逻辑（在未进入攻击范围时移动）
            if (!isInAttackRange)
            {
                
                MoveTowardsTarget();
            }
            else
            {
                StopMovement(); // 在攻击范围内时停止移动
            }

            // 4. 攻击范围检测
            isInAttackRange = CheckAttackRange();

            // 5. 检测到攻击范围立即切换状态
            if (isInAttackRange)
            {
                if (Timer <= 0f)
                {
                    Debug.Log("攻击");
                    manager.TransitionState(MomStateType.Attack);
                    Timer = parameter.attackCooldown;
                    return; // 立即切换状态，避免重复触发
                }
                else
                {
                    Timer -= Time.deltaTime;
                    parameter.animator.Play("Mom_idle");
                }
                StopMovement(); // 确保在攻击状态前停止移动

            }
            else
            {
                // 如果不在攻击范围内，继续追击
                if (parameter.target != null)
                {
                    parameter.animator.Play("Mom_walk");
                    MoveTowardsTarget();
                }
            }
        }
    }

    

    private void MoveTowardsTarget()
    {
        Vector3 targetPosition = new Vector3(
            parameter.target.position.x,
            manager.transform.position.y,
            0
        );

        Vector3 moveDir = (targetPosition - manager.transform.position).normalized;

        // 物理移动
        if (rb != null)
        {
            rb.velocity = new Vector2(
                moveDir.x * parameter.ChaseSpeed,
                rb.velocity.y
            );
        }
        else // 非物理移动
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                targetPosition,
                parameter.ChaseSpeed * Time.deltaTime
            );
        }
    }

    private bool CheckAttackRange()
    {
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void OnExit()
    {
        StopMovement();
        isInAttackRange = false;
    }
}
