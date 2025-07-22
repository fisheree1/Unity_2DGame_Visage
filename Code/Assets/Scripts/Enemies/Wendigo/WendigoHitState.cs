using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WendigoHitState : IState
{
    private WendigoP manager;
    private WendigoParameter parameter;
    private AnimatorStateInfo info;
    

    public WendigoHitState(WendigoP manager, WendigoParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;

    }
    public void OnEnter()
    {

        parameter.animator.Play("Wendigo_hurt");
        
    }

    public void OnUpdate()
    {

        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.IsDead)
            {
                manager.TransitionState(WendigoStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // 重置被击中状态
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // 重新获取目标
                manager.TransitionState(WendigoStateType.Chase); // 切换到追逐状态
            }

        }

    }


    public void OnExit()
    {
       parameter.isHit = false; // 确保退出时重置被击中状态
    }
    
}


