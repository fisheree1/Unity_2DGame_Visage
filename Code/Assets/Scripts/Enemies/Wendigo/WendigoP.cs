using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Cinemachine;

public enum WendigoStateType
{
    Idle,
    Patrol,
    Attack,
    Chase,
    Hit,
    Dead
}
[System.Serializable]
public class WendigoParameter
{
    public float PatrolSpeed;
    public float ChaseSpeed;
    public float IdleTime;
    public Transform[] patrolPoints;
    public Transform[] chasePoints;
    public Animator animator;
    public Transform target;
    public LayerMask targetLayer;
    public Transform attackPoint;
    public float attackArea;
    public float sightArea;
    public int damage = 1; // 攻击伤害
    public float attackCooldown = 0.7f; // 攻击冷却时间
    public bool isHit = false; // 是否被击中

}

public class WendigoP : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.5f;

    // Components
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;
    private EnemyLife enemyLife; // EnemyLife组件引用

    // Health properties - 现在通过EnemyLife组件获取
    public int CurrentHealth => enemyLife != null ? enemyLife.CurrentHealth : 0;
    public int MaxHealth => enemyLife != null ? enemyLife.MaxHealth : 0;
    public bool IsDead => enemyLife != null ? enemyLife.IsDead : false;

    public WendigoParameter parameter;
    private IState currentState;
    private Dictionary<WendigoStateType, IState> states = new Dictionary<WendigoStateType, IState>();
    private CinemachineImpulseSource impulseSource;


    // 在动画事件调用的方法


    void Start()
    {
        if (parameter == null)
            parameter = new WendigoParameter();

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
            Debug.Log("Added Rigidbody2D to Wendigo");
        }

        // 如果没有Collider2D，自动添加
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            Debug.Log("Added CapsuleCollider2D to Wendigo");
        }

        // 获取或添加EnemyLife组件
        enemyLife = GetComponent<EnemyLife>();
        if (enemyLife == null)
        {
            enemyLife = gameObject.AddComponent<EnemyLife>();
        }

        states.Add(WendigoStateType.Idle, new WendigoIdleState(this, parameter));
        states.Add(WendigoStateType.Patrol, new WendigoPatrolState(this, parameter));
        states.Add(WendigoStateType.Attack, new WendigoAttackState(this, parameter));
        states.Add(WendigoStateType.Chase, new WendigoChaseState(this, parameter));
        states.Add(WendigoStateType.Hit, new WendigoHitState(this, parameter));
        states.Add(WendigoStateType.Dead, new WendigoDeadState(this, parameter));


        TransitionState(WendigoStateType.Idle);

        //初始化相机震动源
        impulseSource = GetComponent<CinemachineImpulseSource>();


    }
    void Update()
    {
        // 安全检查：如果已死亡且组件仍启用，强制禁用
        if (enemyLife != null && enemyLife.IsDead && enabled)
        {
            enabled = false;
            return;
        }
        
        currentState?.OnUpdate();
    }

    public void TransitionState(WendigoStateType type)
    {
        // 如果已死亡，只允许转换到死亡状态
        if (enemyLife != null && enemyLife.IsDead && type != WendigoStateType.Dead)
        {
            Debug.LogWarning($"Wendigo: Attempted to transition to {type} while dead. Ignoring.");
            return;
        }
        
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
    #region sight_test
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 如果已死亡，不处理任何触发器事件
        if (enemyLife != null && enemyLife.IsDead) return;
        
        GameObject hero = GameObject.FindGameObjectWithTag("Player");
        // 检测玩家进入视野
        if (other.CompareTag("Player"))
        {
            parameter.target = other.transform;
            if (currentState is WendigoAttackState)
                TransitionState(WendigoStateType.Attack); // 如果已经在攻击状态，保持攻击状态
            else if (currentState is WendigoIdleState || currentState is WendigoPatrolState || currentState is WendigoChaseState)
            {
                TransitionState(WendigoStateType.Chase); // 进入追逐状态

            }
            else
            {
                TransitionState(WendigoStateType.Idle);
                return;
            }
        }

        // 检测玩家攻击
        if (other.CompareTag("PlayerAttack"))
        {
            AttackHitbox attackHitbox = other.GetComponent<AttackHitbox>();
            if (attackHitbox != null)
            {
                int damage = attackHitbox.damage;
                TakeDamage(damage);

                if (hero != null)
                {
                    if (hero.transform.localRotation.y == 0)
                    {
                        GetComponent<Enemy>()?.GetHit(Vector2.right);
                    }
                    else if (hero.transform.localRotation.y == -1)
                    {
                        Debug.Log(hero.transform.localRotation.y);
                        GetComponent<Enemy>()?.GetHit(Vector2.left);
                    }
                }
            }
            else
            {
                Debug.LogWarning("PlayerAttack object found but no AttackHitbox component!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 如果已死亡，不处理任何触发器事件
        if (enemyLife != null && enemyLife.IsDead) return;
        
        float distance = transform.position.x - other.transform.position.x;
        if (other.CompareTag("Player"))
        {
            if (parameter.target != null && (distance < parameter.sightArea) || (distance > -parameter.sightArea))
                TransitionState(WendigoStateType.Chase);

            else
            {
                parameter.target = null;

                TransitionState(WendigoStateType.Patrol); // 退出时切换到Idle状态
            }
        }
    }
    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(parameter.attackPoint.position, parameter.attackArea);
    }
    //受到伤害
    #region Damage System
    public void TakeDamage(int damage)
    {
        if (enemyLife == null || enemyLife.IsDead) return; // 已死亡时不再受到伤害

        parameter.isHit = true;

        // 触发屏幕震动
        if (impulseSource != null)
        {
            CamaraShakeManager.Instance.CamaraShake(impulseSource);
        }

        // 使用EnemyLife组件处理伤害
        enemyLife.TakeDamage(damage);
        
        // 检查是否死亡
        if (enemyLife.IsDead)
        {
            // 立即禁用更新以防止状态混乱
            enabled = false;
            
            // 死亡状态
            TransitionState(WendigoStateType.Dead);
        }
        else
        {
            // 受击状态
            TransitionState(WendigoStateType.Hit);
        }
    }
    #endregion
    
    // 添加额外的死亡处理方法
    private void OnDestroy()
    {
        Debug.Log("Wendigo object is being destroyed");
    }
}
