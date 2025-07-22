using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGoBlinHitState : IState
{
    private SGoBlinP manager;
    private SGoBlinParameter parameter;
    private AnimatorStateInfo info;
    

    public SGoBlinHitState(SGoBlinP manager, SGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;

    }
    public void OnEnter()
    {

        parameter.animator.Play("S_GoBlin_hurt");
        
    }

    public void OnUpdate()
    {

        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.IsDead)
            {
                manager.TransitionState(SGoBlinStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // 重置被击中状态
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // 重新获取目标
                manager.TransitionState(SGoBlinStateType.Chase); // 切换回追击状态
            }

        }

    }


    public void OnExit()
    {
       parameter.isHit = false; // 确保退出时重置被击中状态
    }
    
}


