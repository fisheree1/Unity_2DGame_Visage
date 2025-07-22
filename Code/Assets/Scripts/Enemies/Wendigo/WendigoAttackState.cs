using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WendigoAttackState : IState
{
    private WendigoP manager;
    private WendigoParameter parameter;
    private AnimatorStateInfo info;
    private int AttackCount = 0;  
    
    private bool isInAttackRange = false;

    public WendigoAttackState(WendigoP manager, WendigoParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        
    }
    public void OnEnter()
    {
        
            parameter.animator.Play("Wendigo_attack");
            
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(WendigoStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            
                if (isInAttackRange)
                {
                    manager.TransitionState(WendigoStateType.Attack);


                }
                else
                {
                    manager.TransitionState(WendigoStateType.Chase);
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


