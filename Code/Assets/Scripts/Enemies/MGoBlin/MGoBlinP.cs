using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Cinemachine;

public enum MGoBlinStateType
{
    Idle,
    Patrol,
    Attack,
    Attack2,
    HeavyAtk,
    Chase,
    Hit,
    Dead
}
[System.Serializable] public class MGoBlinParameter
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
    public float dashAttackArea;
    public float sightArea;
    public int damage = 1; // 攻击伤害
    public float attackCooldown = 0.2f; // 攻击冷却时间 - 进一步减少为更有攻击性
    public float dashAtkCooldown = 0.6f; // 冲刺攻击冷却时间 - 进一步减少
    public bool isHit = false; // 是否被击中
    
    // 新增攻击性相关参数
    public float aggressionMultiplier = 2f; // 攻击性倍数 - 增加
    public float comboChance = 0.8f; // 连击几率 - 增加
    public float recoverySpeed = 2.5f; // 从被击中状态恢复的速度 - 增加
    public float persistentChaseTime = 3f; // 失去目标后持续追击的时间 - 增加
    public float lastHitTime = 0f; // 上次被击中的时间
    
    // 新增流畅攻击参数
    public float attackChainWindow = 1.5f; // 攻击连击窗口时间
    public float attackPredictionRange = 1.2f; // 攻击预测距离倍数
    public float aggressiveMovementSpeed = 1.3f; // 攻击状态下的移动速度倍数
    public int maxComboCount = 4; // 最大连击数
    public float comboResetTime = 2f; // 连击重置时间
    
    // 攻击状态跟踪
    [System.NonSerialized] public int currentComboCount = 0;
    [System.NonSerialized] public float lastAttackTime = 0f;
    [System.NonSerialized] public bool isInCombatMode = false;
    [System.NonSerialized] public MGoBlinStateType lastAttackType = MGoBlinStateType.Attack;
    
    // Hero生存状态监控
    [System.NonSerialized] public HeroLife heroLife;
    [System.NonSerialized] public bool wasHeroAlivePreviously = true;
}

public class MGoBlinP : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.5f;

    // Components
    private Rigidbody2D rb;
    private Collider2D col;
    private EnemyLife enemyLife; // EnemyLife组件引用

    // Health properties - 现在通过EnemyLife组件获取
    public int CurrentHealth => enemyLife != null ? enemyLife.CurrentHealth : 0;
    public int MaxHealth => enemyLife != null ? enemyLife.MaxHealth : 0;
    public bool IsDead => enemyLife != null ? enemyLife.IsDead : false;
    
    public MGoBlinParameter parameter;
    private IState currentState;
    private Dictionary<MGoBlinStateType, IState> states = new Dictionary<MGoBlinStateType, IState>();
    private CinemachineImpulseSource impulseSource;


    // 在动画事件调用的方法


    void Start()
    {
        if (parameter == null)
            parameter = new MGoBlinParameter();

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
            Debug.Log("Added Rigidbody2D to MGoBlin");
        }

        // 如果没有Collider2D，自动添加
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            Debug.Log("Added CapsuleCollider2D to MGoBlin");
        }

        // 获取或添加EnemyLife组件
        enemyLife = GetComponent<EnemyLife>();
        if (enemyLife == null)
        {
            enemyLife = gameObject.AddComponent<EnemyLife>();
        }

        // 初始化Hero生存状态监控
        InitializeHeroMonitoring();

        states.Add(MGoBlinStateType.Idle, new MGoBlinIdleState(this, parameter));
        states.Add(MGoBlinStateType.Patrol, new MGoBlinPatrolState(this, parameter));
        states.Add(MGoBlinStateType.Attack, new MGoBlinAttackState(this, parameter));
        states.Add(MGoBlinStateType.Attack2, new MGoBlinAttack2State(this, parameter));
        states.Add(MGoBlinStateType.HeavyAtk, new MGoBlinHeavyAtkState(this, parameter));
        states.Add(MGoBlinStateType.Chase, new MGoBlinChaseState(this, parameter));
        states.Add(MGoBlinStateType.Hit, new MGoBlinHitState(this, parameter));
        states.Add(MGoBlinStateType.Dead, new MGoBlinDeadState(this, parameter));
        

        TransitionState(MGoBlinStateType.Idle);
        
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
        
        // 检查Hero生存状态
        CheckHeroAliveStatus();
        
        currentState?.OnUpdate();
        
        // 检查连击重置
        if (parameter.isInCombatMode && Time.time - parameter.lastAttackTime > parameter.comboResetTime)
        {
            ResetCombo();
        }
    }

    public void TransitionState(MGoBlinStateType type)
    {
        // 如果已死亡，只允许转换到死亡状态
        if (enemyLife != null && enemyLife.IsDead && type != MGoBlinStateType.Dead)
        {
            Debug.LogWarning($"MGoBlin: Attempted to transition to {type} while dead. Ignoring.");
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
    
    // 新增攻击流畅性辅助方法
    public bool CanChainAttack()
    {
        return parameter.currentComboCount < parameter.maxComboCount && 
               Time.time - parameter.lastAttackTime < parameter.attackChainWindow;
    }
    
    public void ResetCombo()
    {
        parameter.currentComboCount = 0;
        parameter.isInCombatMode = false;
    }
    
    public void IncrementCombo(MGoBlinStateType attackType)
    {
        parameter.currentComboCount++;
        parameter.lastAttackTime = Time.time;
        parameter.isInCombatMode = true;
        parameter.lastAttackType = attackType;
    }
    
    public MGoBlinStateType GetNextAttackType()
    {
        // 智能攻击选择系统
        if (!IsTargetValid()) return MGoBlinStateType.Chase;
        
        float distanceToTarget = Vector2.Distance(transform.position, parameter.target.position);
        
        // 根据距离和连击数选择攻击类型
        if (distanceToTarget <= parameter.attackArea)
        {
            // 近距离攻击选择
            if (parameter.currentComboCount == 0)
                return MGoBlinStateType.Attack;
            else if (parameter.currentComboCount == 1 && UnityEngine.Random.value < parameter.comboChance)
                return MGoBlinStateType.Attack2;
            else if (parameter.currentComboCount >= 2 && UnityEngine.Random.value < parameter.comboChance * 0.7f)
                return MGoBlinStateType.Attack;
            else
                return MGoBlinStateType.Chase;
        }
        else if (distanceToTarget <= parameter.dashAttackArea * parameter.attackPredictionRange)
        {
            // 中距离冲刺攻击
            if (UnityEngine.Random.value < parameter.comboChance * 0.8f)
                return MGoBlinStateType.HeavyAtk;
            else
                return MGoBlinStateType.Chase;
        }
        else
        {
            return MGoBlinStateType.Chase;
        }
    }
    
    #region Hero Death Detection System
    /// <summary>
    /// 初始化Hero生存状态监控系统
    /// </summary>
    private void InitializeHeroMonitoring()
    {
        // 查找Hero对象
        GameObject heroObject = GameObject.FindGameObjectWithTag("Player");
        if (heroObject != null)
        {
            parameter.heroLife = heroObject.GetComponent<HeroLife>();
            if (parameter.heroLife != null)
            {
                parameter.wasHeroAlivePreviously = !parameter.heroLife.IsDead;
                Debug.Log($"MGoBlin: Hero monitoring initialized. Hero alive: {parameter.wasHeroAlivePreviously}");
            }
            else
            {
                Debug.LogWarning("MGoBlin: Hero object found but no HeroLife component!");
            }
        }
        else
        {
            Debug.LogWarning("MGoBlin: No Hero object found with 'Player' tag!");
        }
    }
    
    /// <summary>
    /// 检查Hero生存状态，如果死亡则清除目标
    /// </summary>
    private void CheckHeroAliveStatus()
    {
        // 如果没有找到HeroLife组件，尝试重新查找
        if (parameter.heroLife == null)
        {
            GameObject heroObject = GameObject.FindGameObjectWithTag("Player");
            if (heroObject != null)
            {
                parameter.heroLife = heroObject.GetComponent<HeroLife>();
            }
        }
        
        // 如果找到了HeroLife组件，检查生存状态
        if (parameter.heroLife != null)
        {
            bool isHeroAliveNow = !parameter.heroLife.IsDead;
            
            // 检查Hero死亡状态变化
            if (parameter.wasHeroAlivePreviously && !isHeroAliveNow)
            {
                // Hero刚刚死亡
                OnHeroDeath();
            }
            else if (!parameter.wasHeroAlivePreviously && isHeroAliveNow)
            {
                // Hero复活了
                OnHeroRespawn();
            }
            
            // 更新上一次状态
            parameter.wasHeroAlivePreviously = isHeroAliveNow;
            
            // 如果Hero死亡，确保目标为null
            if (!isHeroAliveNow && parameter.target != null)
            {
                // 检查当前目标是否是死亡的Hero
                if (parameter.target.CompareTag("Player"))
                {
                    ClearTarget("Hero has died");
                }
            }
        }
    }
    
    /// <summary>
    /// 处理Hero死亡事件
    /// </summary>
    private void OnHeroDeath()
    {
        Debug.Log("MGoBlin: Hero has died, clearing target and stopping combat");
        
        // 清除目标
        ClearTarget("Hero died");
        
        // 重置连击状态
        ResetCombo();
        
        // 根据当前状态决定下一步行动
        if (currentState is MGoBlinAttackState || 
            currentState is MGoBlinAttack2State || 
            currentState is MGoBlinHeavyAtkState || 
            currentState is MGoBlinChaseState)
        {
            // 如果正在战斗相关状态，转换到巡逻状态
            TransitionState(MGoBlinStateType.Patrol);
        }
    }
    
    /// <summary>
    /// 处理Hero复活事件
    /// </summary>
    private void OnHeroRespawn()
    {
        Debug.Log("MGoBlin: Hero has respawned");
        
        // 重置攻击性倍数到默认值
        parameter.aggressionMultiplier = 2f;
        
        // 可以在这里添加其他复活后的处理逻辑
        // 比如重置某些状态或者播放特定动画
    }
    
    /// <summary>
    /// 清除目标并记录原因
    /// </summary>
    /// <param name="reason">清除目标的原因</param>
    private void ClearTarget(string reason)
    {
        if (parameter.target != null)
        {
            Debug.Log($"MGoBlin: Clearing target. Reason: {reason}");
            parameter.target = null;
        }
    }
    
    /// <summary>
    /// 检查当前目标是否有效（存在且活着）
    /// </summary>
    /// <returns>目标是否有效</returns>
    public bool IsTargetValid()
    {
        if (parameter.target == null) return false;
        
        // 如果目标是Player，检查是否还活着
        if (parameter.target.CompareTag("Player"))
        {
            if (parameter.heroLife != null && parameter.heroLife.IsDead)
            {
                return false;
            }
        }
        
        return true;
    }
    #endregion
    
    #region sight_test
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 如果已死亡，不处理任何触发器事件
        if (enemyLife != null && enemyLife.IsDead) return;
        
        GameObject hero = GameObject.FindGameObjectWithTag("Player");
        // 检测玩家进入视野
        if (other.CompareTag("Player"))
        {
            // 检查Hero是否还活着
            if (parameter.heroLife != null && parameter.heroLife.IsDead)
            {
                Debug.Log("MGoBlin: Hero is dead, not setting as target");
                return;
            }
            
            parameter.target = other.transform;
            if (currentState is MGoBlinAttackState)
                TransitionState(MGoBlinStateType.Attack); // 如果已经在攻击状态，保持攻击状态
            else if (currentState is MGoBlinIdleState || currentState is MGoBlinPatrolState || currentState is MGoBlinChaseState)
            {
                TransitionState(MGoBlinStateType.Chase); // 进入追逐状态
            }
            else
            {
                TransitionState(MGoBlinStateType.Idle);
                return;
            }
        }
        
        // 检测玩家攻击
        if (other.CompareTag("PlayerAttack"))
        {
            // 检查Hero是否还活着（死亡的Hero不应该能够攻击）
            if (parameter.heroLife != null && parameter.heroLife.IsDead)
            {
                Debug.Log("MGoBlin: Ignoring attack from dead Hero");
                return;
            }
            
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
            // 检查Hero是否还活着
            if (parameter.heroLife != null && parameter.heroLife.IsDead)
            {
                Debug.Log("MGoBlin: Hero is dead, clearing target");
                parameter.target = null;
                TransitionState(MGoBlinStateType.Patrol);
                return;
            }
            
            if (parameter.target != null && (distance < parameter.sightArea) || (distance > -parameter.sightArea))
                TransitionState(MGoBlinStateType.Chase);
            else
            {
                parameter.target = null;
                TransitionState(MGoBlinStateType.Patrol); // 退出时切换到Patrol状态
            }
        }
    }
    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(parameter.attackPoint.position, parameter.attackArea);
        Gizmos.DrawWireSphere(parameter.attackPoint.position, parameter.dashAttackArea);
        Gizmos.color = Color.red;
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
            TransitionState(MGoBlinStateType.Dead);
        }
        else
        {
            // 受击状态
            TransitionState(MGoBlinStateType.Hit);
        }
    }
    #endregion
    
    // 添加额外的死亡处理方法
    private void OnDestroy()
    {
        Debug.Log("MGoBlin object is being destroyed");
    }
}
