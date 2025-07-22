using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DadAttackState : IState
{
    private DadP manager;
    private DadParameter parameter;
    private AnimatorStateInfo info;
    private int AttackCount = 0;  
    
    private bool isInAttackRange = false;

    public DadAttackState(DadP manager, DadParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        
    }
    public void OnEnter()
    {
        
            parameter.animator.Play("Dad_attack");
            
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(DadStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            
                if (isInAttackRange)
                {
                    manager.TransitionState(DadStateType.Attack);


                }
                else
                {
                    manager.TransitionState(DadStateType.Idle);
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


