using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAttackState : IState
{
    private SpiderP manager;
    private SpiderParameter parameter;
    private AnimatorStateInfo info;
    private int AttackCount = 0;  
    
    private bool isInAttackRange = false;

    public SpiderAttackState(SpiderP manager, SpiderParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        
    }
    public void OnEnter()
    {
        
            parameter.animator.Play("Spider_attack");
            
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SpiderStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            
                if (isInAttackRange)
                {
                    manager.TransitionState(SpiderStateType.Attack);


                }
                else
                {
                    manager.TransitionState(SpiderStateType.Idle);
                }
            }
            

        
    }


    public void OnExit() 
    { 
        Debug.Log("Attack Count: " + AttackCount);
    }
    private bool CheckAttackRange()
    {
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
    }
}


