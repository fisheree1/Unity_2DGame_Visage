using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGoBlinAttackState : IState
{
    private SGoBlinP manager;
    private SGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private int AttackCount = 0;  
    
    private bool isInAttackRange = false;

    public SGoBlinAttackState(SGoBlinP manager, SGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        
    }
    public void OnEnter()
    {
        
            parameter.animator.Play("S_GoBlin_attack1");
            
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SGoBlinStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            
                if (isInAttackRange)
                {
                    manager.TransitionState(SGoBlinStateType.Attack);


                }
                else
                {
                    manager.TransitionState(SGoBlinStateType.Idle);
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


