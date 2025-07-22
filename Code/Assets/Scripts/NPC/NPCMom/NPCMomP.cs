using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Cinemachine;

public enum NPCMomStateType
{
    Idle,
    Patrol,
    Attack,
    Chase,
    Hit,
    Dead
}
[System.Serializable]
public class NPCMomParameter
{
    public float PatrolSpeed;
    public float ChaseSpeed;
    public float IdleTime;
    public Transform[] patrolPoints;
    
    public Animator animator;
    public Transform target;
  

}

public class NPCMomP
: MonoBehaviour
{

    // Components
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;

    // Health properties
    
    public NPCMomParameter parameter;
    private IState currentState;
    private Dictionary<NPCMomStateType, IState> states = new Dictionary<NPCMomStateType, IState>();
    private CinemachineImpulseSource impulseSource;


    // 在动画事件调用的方法


    void Start()
    {
        if (parameter == null)
            parameter = new NPCMomParameter();

        parameter.animator = GetComponent<Animator>();

        // 获取物理组件
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // 设置物理属性确保敌人能站立在地面上
        if (rb != null)
        {
            rb.freezeRotation = true; // 防止旋转
            rb.gravityScale = 1f; // 设置重力

        }

        // 如果没有Rigidbody2D，自动添加
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
            rb.drag = 5f;
            Debug.Log("Added Rigidbody2D to NPCMom");
        }

        // 如果没有Collider2D，自动添加
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            Debug.Log("Added CapsuleCollider2D to NPCMom");
        }

        states.Add(NPCMomStateType.Idle, new NPCMomIdleState(this, parameter));
        states.Add(NPCMomStateType.Patrol, new NPCMomPatrolState(this, parameter));
        

        TransitionState(NPCMomStateType.Idle);

        
        
    }
    void Update()
    {
        currentState.OnUpdate();

    }

    public void TransitionState(NPCMomStateType type)
    {
        if (currentState != null)
            currentState.OnExit();
        currentState = states[type];
        currentState.OnEnter();
    }

    public void FlipTo(Transform target)
    {

        if (target == null)
        {
            Debug.LogWarning("FlipTo called with null target!");
            return;
        }

        Vector3 direction = target.position - transform.position;


        float newScaleX = Mathf.Sign(direction.x) < 0 ? 1f : -1f;
        transform.localScale = new Vector3(newScaleX, 1f, 1f);


        Debug.DrawRay(transform.position, direction, Color.yellow, 0.1f);
    }
    






}
