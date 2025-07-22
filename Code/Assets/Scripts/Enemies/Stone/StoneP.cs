using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Stone敌人状态类型枚举
/// 定义Stone敌人的所有可能状态
/// </summary>
public enum StoneStateType
{
    Idle,       // 待机状态：敌人静止不动，等待玩家进入视野
    Walk,       // 行走状态：敌人缓慢移动巡逻
    React,      // 反应状态：检测到玩家后的准备阶段（0.3秒）
    Attack,     // 攻击状态：释放一击毙命的光束攻击
    Defence,    // 防御状态：受到3次伤害后进入，免疫所有伤害2秒
    Dead        // 死亡状态：敌人被击败
}

/// <summary>
/// Stone敌人参数配置类
/// 包含Stone敌人的所有可调节参数和状态数据
/// </summary>
[System.Serializable]
public class StoneParameter
{
    [Header("Movement Settings - 移动设置")]
    [Tooltip("Stone的行走速度")]
    public float walkSpeed = 2f;
    [Tooltip("Stone的巡逻点数组，如果为空则使用随机方向巡逻")]
    public Transform[] patrolPoints;
    [Tooltip("在巡逻点停留的时间（秒）")]
    public float idleAtPatrolPointTime = 2f;
    
    [Header("Sight Settings - 视野设置")]
    [Tooltip("用于检测玩家的子物体sight")]
    public Transform sightObject;
    
    [Header("React Settings - 反应设置")]
    [Tooltip("检测到玩家后的反应时间（秒）")]
    public float reactDuration = 0.3f;
    
    [Header("Attack Settings - 攻击设置")]
    [Tooltip("光束攻击的起始点")]
    public Transform attackPoint;
    [Tooltip("攻击目标的图层")]
    public LayerMask targetLayer;
    [Tooltip("光束的最大距离")]
    public float beamMaxDistance = 10f;
    [Tooltip("光束的宽度")]
    public float beamWidth = 0.5f;
    [Tooltip("用于显示光束效果的LineRenderer")]
    public LineRenderer beamRenderer;
    [Tooltip("光束击中效果预制体")]
    public GameObject beamImpactEffect;
    
    [Header("Defence Settings - 防御设置")]
    [Tooltip("进入防御状态所需的受击次数")]
    public int hitsToDefence = 3;
    [Tooltip("防御状态持续时间（秒）")]
    public float defenceDuration = 2f;
    
    [Header("Animation - 动画设置")]
    [Tooltip("动画控制器")]
    public Animator animator;
    
    [Header("State Tracking - 状态跟踪")]
    [Tooltip("当前目标（玩家）")]
    public Transform target;
    [Tooltip("是否正在反应中")]
    public bool isReacting = false;
    [Tooltip("是否被击中")]
    public bool isHit = false;
    [Tooltip("当前受击次数")]
    public int hitCount = 0;
    [Tooltip("是否处于防御状态")]
    public bool isInDefence = false;
    
    [Header("Audio - 音频设置")]
    [Tooltip("音频源组件")]
    public AudioSource audioSource;
    [Tooltip("光束攻击音效")]
    public AudioClip beamAttackSound;
    [Tooltip("防御状态音效")]
    public AudioClip defenceSound;
}

/// <summary>
/// Stone敌人主控制器
/// 管理Stone敌人的状态机、组件和行为逻辑
/// 
/// Stone敌人特点：
/// - 有子物体sight用于检测玩家
/// - 检测到玩家时进入React状态（0.3秒准备时间）
/// - 攻击为一击毙命的光束攻击
/// - 受到3次伤害后进入Defence状态，免疫所有伤害2秒
/// - 支持Animator控制器进行动画管理
/// </summary>
public class StoneP : MonoBehaviour
{
    [Header("Components - 组件设置")]
    [Tooltip("Stone敌人的所有参数配置")]
    public StoneParameter parameter;
    
    // 状态机相关
    private IState currentState;
    private Dictionary<StoneStateType, IState> states = new Dictionary<StoneStateType, IState>();
    
    // 组件引用
    private Rigidbody2D rb;
    private Collider2D col;
    private EnemyLife enemyLife;
    private CinemachineImpulseSource impulseSource;
    
    // 属性访问器
    public int CurrentHealth => enemyLife?.CurrentHealth ?? 0;
    public int MaxHealth => enemyLife?.MaxHealth ?? 0;
    public bool IsDead => enemyLife?.IsDead ?? false;

    /// <summary>
    /// 初始化Stone敌人
    /// </summary>
    void Start()
    {
        InitializeComponents();
        InitializeStates();
        
        // 默认进入Idle状态
        TransitionState(StoneStateType.Idle);
        
        Debug.Log("Stone敌人初始化完成！");
    }

    /// <summary>
    /// 初始化所有必要的组件
    /// </summary>
    private void InitializeComponents()
    {
        // 获取基础组件
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        enemyLife = GetComponent<EnemyLife>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        
        // 初始化参数
        if (parameter.audioSource == null)
            parameter.audioSource = GetComponent<AudioSource>();
        
        if (parameter.animator == null)
            parameter.animator = GetComponent<Animator>();
        
        // 检查必要组件
        if (parameter.sightObject == null)
            Debug.LogWarning("Stone: 缺少sight子物体！请在Inspector中指定sightObject。");
        
        if (parameter.attackPoint == null)
            Debug.LogWarning("Stone: 缺少攻击点！请在Inspector中指定attackPoint。");
        
        if (parameter.beamRenderer == null)
            Debug.LogWarning("Stone: 缺少LineRenderer组件！请在Inspector中指定beamRenderer。");
    }

    /// <summary>
    /// 初始化状态机
    /// </summary>
    private void InitializeStates()
    {
        states[StoneStateType.Idle] = new StoneIdleState(this, parameter);
        states[StoneStateType.Walk] = new StoneWalkState(this, parameter);
        states[StoneStateType.React] = new StoneReactState(this, parameter);
        states[StoneStateType.Attack] = new StoneAttackState(this, parameter);
        states[StoneStateType.Defence] = new StoneDefenceState(this, parameter);
        states[StoneStateType.Dead] = new StoneDeadState(this, parameter);
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    void Update()
    {
        // 如果已死亡，停止所有更新
        if (IsDead) return;
        
        // 更新当前状态
        currentState?.OnUpdate();
    }

    /// <summary>
    /// 状态转换方法
    /// </summary>
    /// <param name="type">目标状态类型</param>
    public void TransitionState(StoneStateType type)
    {
        // 如果已死亡，只允许转换到死亡状态
        if (IsDead && type != StoneStateType.Dead)
        {
            Debug.LogWarning($"Stone: 尝试从死亡状态转换到 {type}，操作被忽略。");
            return;
        }
        
        // 退出当前状态
        currentState?.OnExit();
        
        // 进入新状态
        currentState = states[type];
        currentState?.OnEnter();
        
        Debug.Log($"Stone状态转换: → {type}");
    }

    /// <summary>
    /// 让Stone面向指定目标
    /// </summary>
    /// <param name="target">目标Transform</param>
    public void FlipTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("FlipTo: 目标为空！");
            return;
        }

        Vector3 direction = target.position - transform.position;
        float newScaleX = Mathf.Sign(direction.x) < 0 ? -1f : 1f;
        transform.localScale = new Vector3(newScaleX, 1f, 1f);

        // 调试用射线
        Debug.DrawRay(transform.position, direction, Color.red, 0.1f);
    }

    #region 碰撞检测系统
    /// <summary>
    /// 处理碰撞进入事件
    /// 主要用于sight子物体检测玩家
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 如果已死亡，不处理任何触发器事件
        if (IsDead) return;
        
        HandleSightTrigger(other);
    }

    /// <summary>
    /// 处理sight检测逻辑
    /// </summary>
    private void HandleSightTrigger(Collider2D other)
    {
        GameObject hero = GameObject.FindGameObjectWithTag("Player");
        
        // 检测玩家进入视野
        if (other.CompareTag("Player"))
        {
            parameter.target = other.transform;
            
            // 根据当前状态决定转换
            if (currentState is StoneIdleState || currentState is StoneWalkState)
            {
                TransitionState(StoneStateType.React);
            }
            
            Debug.Log("Stone检测到玩家，进入反应状态！");
        }
        
        // 检测玩家攻击
        if (other.CompareTag("PlayerAttack"))
        {
            AttackHitbox attackHitbox = other.GetComponent<AttackHitbox>();
            if (attackHitbox != null)
            {
                int damage = attackHitbox.damage;
                TakeDamage(damage);
                
                // 处理击退效果
                if (hero != null)
                {
                    Vector2 knockbackDirection = hero.transform.localRotation.y == 0 ? Vector2.right : Vector2.left;
                    GetComponent<Enemy>()?.GetHit(knockbackDirection);
                }
            }
            else
            {
                Debug.LogWarning("检测到PlayerAttack但没有AttackHitbox组件！");
            }
        }
    }

    /// <summary>
    /// 处理碰撞退出事件
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        // 如果已死亡，不处理任何触发器事件
        if (IsDead) return;
        
        if (other.CompareTag("Player"))
        {
            // 玩家离开视野后的处理
            parameter.target = null;
            
            // 如果正在反应或攻击状态，回到Idle
            if (currentState is StoneReactState || currentState is StoneAttackState)
            {
                TransitionState(StoneStateType.Idle);
            }
            
            Debug.Log("玩家离开Stone视野");
        }
    }
    #endregion

    #region 伤害系统
    /// <summary>
    /// 受到伤害处理
    /// Stone特殊机制：受到3次伤害后进入防御状态
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(int damage)
    {
        // 死亡或防御状态中不受伤害
        if (IsDead || parameter.isInDefence) 
        {
            if (parameter.isInDefence)
                Debug.Log("Stone处于防御状态，免疫伤害！");
            return;
        }
        
        parameter.isHit = true;
        parameter.hitCount++;
        
        // 触发屏幕震动
        if (impulseSource != null)
        {
            CamaraShakeManager.Instance.CamaraShake(impulseSource);
        }
        
        // 使用EnemyLife组件处理伤害
        enemyLife?.TakeDamage(damage);
        
        Debug.Log($"Stone受到 {damage} 点伤害，当前受击次数: {parameter.hitCount}/{parameter.hitsToDefence}");
        
        // 检查是否死亡
        if (IsDead)
        {
            enabled = false; // 禁用更新防止状态混乱
            TransitionState(StoneStateType.Dead);
            return;
        }
        
        // 检查是否达到防御条件
        if (parameter.hitCount >= parameter.hitsToDefence)
        {
            TransitionState(StoneStateType.Defence);
        }
    }
    #endregion

    #region 攻击系统
    /// <summary>
    /// 执行光束攻击
    /// Stone的招牌技能：一击毙命的光束攻击
    /// </summary>
    public void FireBeamAttack()
    {
        if (parameter.attackPoint == null || parameter.target == null) return;
        
        Vector2 attackDirection = (parameter.target.position - parameter.attackPoint.position).normalized;
        
        // 播放攻击音效
        if (parameter.audioSource != null && parameter.beamAttackSound != null)
        {
            parameter.audioSource.PlayOneShot(parameter.beamAttackSound);
        }
        
        // 进行射线检测
        RaycastHit2D hit = Physics2D.Raycast(
            parameter.attackPoint.position,
            attackDirection,
            parameter.beamMaxDistance,
            parameter.targetLayer
        );
        
        // 显示光束效果
        StartCoroutine(ShowBeamEffect(attackDirection, hit));
        
        // 处理击中伤害
        if (hit.collider != null)
        {
            // 检查是否击中玩家
            if (hit.collider.CompareTag("Player"))
            {
                // 尝试多种可能的玩家生命值组件
                var heroLife = hit.collider.GetComponent<HeroLife>();
                var enemyLife = hit.collider.GetComponent<EnemyLife>();
                
                if (heroLife != null)
                {
                    // 使用HeroLife组件
                    heroLife.TakeDamage(9999); // Stone的光束攻击是一击毙命
                    Debug.Log("Stone光束攻击命中玩家！一击毙命！");
                }
                else if (enemyLife != null)
                {
                    // 如果玩家使用EnemyLife组件
                    enemyLife.TakeDamage(9999);
                    Debug.Log("Stone光束攻击命中玩家！一击毙命！");
                }
                else
                {
                    // 通用伤害处理方法
                    var damageReceiver = hit.collider.GetComponent<MonoBehaviour>();
                    if (damageReceiver != null)
                    {
                        // 尝试通过反射调用TakeDamage方法
                        var method = damageReceiver.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(int) });
                        if (method != null)
                        {
                            method.Invoke(damageReceiver, new object[] { 9999 });
                            Debug.Log("Stone光束攻击命中玩家！一击毙命！");
                        }
                        else
                        {
                            Debug.LogWarning("Stone: 无法对玩家造成伤害，未找到合适的生命值组件或TakeDamage方法");
                        }
                    }
                }
            }
        }
        
        Debug.Log("Stone释放光束攻击！");
    }

    /// <summary>
    /// 显示光束视觉效果
    /// </summary>
    private IEnumerator ShowBeamEffect(Vector2 direction, RaycastHit2D hit)
    {
        if (parameter.beamRenderer == null) yield break;
        
        // 计算光束终点
        Vector3 startPoint = parameter.attackPoint.position;
        Vector3 endPoint;
        
        if (hit.collider != null)
        {
            endPoint = hit.point;
            
            // 在击中点生成特效
            if (parameter.beamImpactEffect != null)
            {
                GameObject effect = Instantiate(parameter.beamImpactEffect, hit.point, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        else
        {
            endPoint = startPoint + (Vector3)(direction * parameter.beamMaxDistance);
        }
        
        // 设置LineRenderer
        parameter.beamRenderer.enabled = true;
        parameter.beamRenderer.startWidth = parameter.beamWidth;
        parameter.beamRenderer.endWidth = parameter.beamWidth;
        parameter.beamRenderer.positionCount = 2;
        parameter.beamRenderer.SetPosition(0, startPoint);
        parameter.beamRenderer.SetPosition(1, endPoint);
        
        // 光束持续时间
        yield return new WaitForSeconds(0.2f);
        
        // 关闭光束
        parameter.beamRenderer.enabled = false;
    }
    #endregion

    #region 调试和可视化
    /// <summary>
    /// 绘制调试信息
    /// </summary>
    private void OnDrawGizmos()
    {
        if (parameter == null) return;
        
        // 绘制攻击点
        if (parameter.attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(parameter.attackPoint.position, 0.3f);
            
            // 绘制攻击方向
            if (parameter.target != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 direction = (parameter.target.position - parameter.attackPoint.position).normalized;
                Gizmos.DrawRay(parameter.attackPoint.position, direction * parameter.beamMaxDistance);
            }
        }
        
        // 显示状态信息
        if (Application.isPlaying)
        {
            Gizmos.color = parameter.isInDefence ? Color.blue : Color.white;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2, Vector3.one * 0.5f);
        }
    }
    #endregion

    /// <summary>
    /// 销毁时的清理工作
    /// </summary>
    private void OnDestroy()
    {
        Debug.Log("Stone对象被销毁");
    }
}
