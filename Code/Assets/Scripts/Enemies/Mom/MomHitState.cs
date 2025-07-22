using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MomHitState : IState
{
    private MomP manager;
    private MomParameter parameter;
    private AnimatorStateInfo info;
    
    public MomHitState(MomP manager, MomParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {

        parameter.animator.Play("Mom_hurt");
        
    }

    public void OnUpdate()
    {

        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.IsDead)
            {
                manager.TransitionState(MomStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // 重置被击中状态
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // 重新获取目标
                manager.TransitionState(MomStateType.Chase); // 切换到追逐状态
            }

        }

    }


    public void OnExit()
    {
       parameter.isHit = false; // 确保退出时重置被击中状态
    }
    
}


