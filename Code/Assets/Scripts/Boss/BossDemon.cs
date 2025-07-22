using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// 重构后的恶魔Boss系统
/// 支持阶段性攻击变化和英雄行为检测
/// 已集成BossLife组件进行血量管理
/// </summary>
public class BossDemon : MonoBehaviour
{
    [Header("Boss状态")]
    [SerializeField] private DemonState currentState = DemonState.Idle;
    
    [Header("Boss属性 - 由BossLife管理")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    
    [Header("阶段系统")]
    [SerializeField] private bool isInPhase2 = false; // 血量低于50%时进入第二阶段
    [SerializeField] private float healthPhaseThreshold = 0.5f; // 50%血量阈值
    
    [Header("第一阶段攻击设置")]
    [SerializeField] private float meleeAttackDamage = 25f;
    [SerializeField] private float shockwaveDamage = 30f;
    [SerializeField] private float shockwaveRadius = 8f;
    
    [Header("第二阶段攻击设置")]
    [SerializeField] private float fireBreathDamage = 15f;
    [SerializeField] private float fireBreathDuration = 3f;
    [SerializeField] private float fireBreathRange = 3f;
    
    [Header("近战检测系统")]
    [SerializeField] private Transform meleeAttackPoint; // 近战攻击点
    [SerializeField] private float meleeRangeRadius = 2.5f; // 近战范围半径
    [SerializeField] private float meleeDetectionInterval = 2f; // 近战检测间隔（2秒）
    [SerializeField] private LayerMask playerLayer = -1; // 玩家层级
    
    [Header("冲刺攻击")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDistance = 10f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashHeight = 5f; // 冲刺高度
    [SerializeField] private float landingCheckInterval = 0.1f; // 落地检测间隔
    
    [Header("小怪召唤系统")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionsPerSpawn = 3;
    [SerializeField] private float minionSpawnRadius = 5f;
    [SerializeField] private float minionHealthThreshold = 0.33f; // 每1/3血量召唤小怪
    
    [Header("投射物系统")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] fireBreathPoints;
    [SerializeField] private float fireballSpeed = 8f;
    [SerializeField] private float bulletSpeed = 12f;
    
    [Header("AI反应系统")]
    [SerializeField] private float reactionCooldown = 2f;
    [SerializeField] private HeroActionTracker heroTracker;
    [SerializeField] private float slideReactionChance = 0.6f; // 对滑铲反应的概率
    [SerializeField] private float jumpReactionChance = 0.7f; // 对跳跃反应的概率
    
    [Header("屏幕震动设置")]
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private float shockwaveShakeForce = 2f; // 冲击波震动强度
    [SerializeField] private float attackShakeForce = 0.8f; // 攻击震动强度
    [SerializeField] private float phaseTransitionShakeForce = 1.5f; // 阶段转换震动强度
    
    [Header("视觉效果")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject hurtEffect;
    [SerializeField] private GameObject fireBreathEffect;
    [SerializeField] private GameObject shockwaveEffect;
    
    [Header("伤害判定Hitbox")]
    [SerializeField] private GameObject attackHitbox; // 近战攻击的伤害判定范围
    [SerializeField] private GameObject fireBreathHitbox; // 火焰吐息攻击的伤害判定范围
    
    [Header("受伤动画系统")]
    [SerializeField] private float hurtAnimationHealthThreshold = 0.2f; // 1/5 HP 阈值
    
    // 组件
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private BossDemonAnimationEvents animationEvents;
    private CinemachineImpulseSource impulseSource;
    private BossLife bossLife; // 血量管理组件
    
    // 引用
    private Transform player;
    private HeroMovement heroMovement;
    private HeroAttackController heroAttack;
    private HeroLife heroLife;
    
    // 状态追踪
    private bool isDead = false;
    private bool isHurt = false;
    private bool isAttacking = false;
    private bool isFacingRight = true;
    private bool isDashing = false;
    private bool isInCombat = false;
    private bool isWaitingForLanding = false; // 等待落地状态
    
    // 阶段和召唤追踪
    private float lastMinionSpawnHealth = 1f;
    private List<GameObject> spawnedMinions = new List<GameObject>();
    
    // AI反应系统
    private float lastReactionTime = 0f;
    private int consecutiveHeroAttacks = 0;
    private float lastHeroAttackTime = 0f;
    
    // 计时器
    private float stateTimer = 0f;
    private float nextAttackTime = 0f;
    private float fireBreathTimer = 0f;
    
    // 受伤动画追踪变量
    private float lastHurtAnimationHealth = 1f; // 上次播放受伤动画时的血量百分比
    
    // 近战检测系统变量
    private float lastMeleeDetectionTime = 0f; // 上次近战检测时间
    private bool isPlayerInMeleeRange = false; // 玩家是否在近战范围内
    
    /// <summary>
    /// 恶魔Boss状态枚举
    /// </summary>
    public enum DemonState
    {
        Spawn,          // 出生状态
        Idle,           // 空闲
        Walk,           // 行走
        Attack,         // 第一阶段普通攻击
        FireBreath,     // 第二阶段火焰吐息攻击
        Hurt,           // 受伤
        Death,          // 死亡
        Dash,           // 冲刺
        Shockwave       // 冲击波
    }
    
    // 属性 - 通过BossLife组件获取
    public bool IsDead => bossLife != null ? bossLife.IsDead : isDead;
    public bool IsInPhase2 => isInPhase2;
    public float HealthPercentage => bossLife != null ? (float)bossLife.CurrentHealth / bossLife.MaxHealth : 0f;
    public int CurrentHealth => bossLife != null ? bossLife.CurrentHealth : 0;
    public int MaxHealth => bossLife != null ? bossLife.MaxHealth : 0;
    public bool IsSpawning => currentState == DemonState.Spawn; // 是否正在出生
    public bool IsFacingRight => isFacingRight; // 添加朝向属性
    
    // 事件
    public System.Action<int, int> OnHealthChanged;
    public System.Action OnDeath;
    public System.Action OnPhase2Enter;
    public System.Action OnSpawnComplete; // 出生完成事件
    
    void Start()
    {
        InitializeComponents();
        InitializeReferences();
        InitializeStats();
        InitializeHeroTracker();
        SetupBossLifeIntegration();
        
        // 从出生状态开始
        StartSpawn();
    }
    
    void Update()
    {
        if (IsDead) return;
        
        UpdateTimers();
        UpdatePhaseSystem();
        UpdateMinionSpawning();
        
        // 只有在出生完成后才进行AI更新
        if (currentState != DemonState.Spawn)
        {
            UpdateAI();
        }
        
        UpdateAnimations();
        UpdateFacingDirection();
        
        if (isInPhase2)
        {
            UpdateReactionSystem();
        }
    }
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        animationEvents = GetComponent<BossDemonAnimationEvents>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        bossLife = GetComponent<BossLife>();
        
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        if (anim == null) anim = gameObject.AddComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        if (animationEvents == null) animationEvents = gameObject.AddComponent<BossDemonAnimationEvents>();
        
        // 初始化屏幕震动组件
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            Debug.Log("为BossDemon添加CinemachineImpulseSource组件");
        }
        
        // 确保有BossLife组件
        if (bossLife == null)
        {
            bossLife = gameObject.AddComponent<BossLife>();
            Debug.Log("为BossDemon添加BossLife组件");
        }
        
        // 初始化攻击hitbox
        InitializeAttackHitboxes();
        
        rb.gravityScale = 1f;
        rb.freezeRotation = true;
    }
    
    /// <summary>
    /// 初始化攻击hitbox
    /// </summary>
    private void InitializeAttackHitboxes()
    {
        // 创建近战攻击hitbox
        if (attackHitbox == null)
        {
            attackHitbox = CreateAttackHitbox("AttackHitbox", Vector3.right * 1.5f, new Vector2(3f, 2f));
            Debug.Log("创建近战攻击hitbox");
        }
        
        // 创建火焰吐息hitbox
        if (fireBreathHitbox == null)
        {
            fireBreathHitbox = CreateAttackHitbox("FireBreathHitbox", Vector3.right * 2f, new Vector2(4f, 3f));
            Debug.Log("创建火焰吐息hitbox");
        }
        
        // 初始化近战攻击点
        if (meleeAttackPoint == null)
        {
            GameObject meleePoint = new GameObject("MeleeAttackPoint");
            meleePoint.transform.SetParent(transform);
            meleePoint.transform.localPosition = new Vector3(1.5f, 0f, 0f);
            meleeAttackPoint = meleePoint.transform;
            Debug.Log("创建近战攻击点");
        }
        
        // 同时为BossDemonAnimationEvents设置hitbox引用
        if (animationEvents != null)
        {
            var field = typeof(BossDemonAnimationEvents).GetField("fireBreathHitbox", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(animationEvents, fireBreathHitbox);
                Debug.Log("为BossDemonAnimationEvents设置火焰吐息hitbox引用");
            }
        }
    }
    
    /// <summary>
    /// 创建攻击hitbox
    /// </summary>
    /// <param name="name">hitbox名称</param>
    /// <param name="localPosition">相对位置</param>
    /// <param name="size">hitbox大小</param>
    /// <returns>创建的hitbox游戏对象</returns>
    private GameObject CreateAttackHitbox(string name, Vector3 localPosition, Vector2 size)
    {
        GameObject hitbox = new GameObject(name);
        hitbox.transform.SetParent(transform);
        hitbox.transform.localPosition = localPosition;
        
        // 添加BoxCollider2D
        BoxCollider2D boxCollider = hitbox.AddComponent<BoxCollider2D>();
        boxCollider.size = size;
        boxCollider.isTrigger = true;
        
        // 添加BossAttackHitBox组件（正确的组件）
        BossAttackHitBox attackHitboxComponent = hitbox.AddComponent<BossAttackHitBox>();
        // 使用反射设置私有字段的damage值
        var damageField = typeof(BossAttackHitBox).GetField("damage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (damageField != null)
        {
            damageField.SetValue(attackHitboxComponent, (int)meleeAttackDamage);
        }
        
        // 设置标签和层级
        hitbox.tag = "EnemyAttack"; // 确保不会攻击敌人自己
        
        // 初始时禁用hitbox
        hitbox.SetActive(false);
        
        Debug.Log($"创建攻击hitbox: {name}, 大小: {size}, 位置: {localPosition}");
        
        return hitbox;
    }
    
    /// <summary>
    /// 设置BossLife集成
    /// </summary>
    private void SetupBossLifeIntegration()
    {
        if (bossLife != null)
        {
            // 订阅BossLife事件
            bossLife.OnHealthChanged += OnBossLifeHealthChanged;
            bossLife.OnDeath += OnBossLifeDeath;
            
            Debug.Log($"BossDemon集成BossLife完成 - 血量: {bossLife.CurrentHealth}/{bossLife.MaxHealth}");
        }
        else
        {
            Debug.LogError("BossDemon: 无法找到或创建BossLife组件！");
        }
    }
    
    /// <summary>
    /// BossLife血量变化事件处理
    /// </summary>
    private void OnBossLifeHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"BossDemon血量变化: {currentHealth}/{maxHealth} ({HealthPercentage * 100:F1}%)");
        
        // 转发事件给外部系统
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// BossLife死亡事件处理
    /// </summary>
    private void OnBossLifeDeath()
    {
        Debug.Log("BossDemon通过BossLife死亡");
        Die();
    }
    
    /// <summary>
    /// 受到伤害 - 集成BossLife
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        
        // 出生期间不受伤害
        if (currentState == DemonState.Spawn)
        {
            Debug.Log("Boss正在出生，无法受到伤害");
            return;
        }
        
        Debug.Log($"BossDemon受到伤害: {damage}");
        
        // 记录受伤前的血量百分比
        float healthPercentageBeforeDamage = HealthPercentage;
        
        // 通过BossLife处理伤害
        if (bossLife != null)
        {
            bossLife.TakeDamage(damage);
        }
        
        // 检查是否死亡
        if (IsDead) return;
        
        // 受伤动画触发逻辑：只有当血量减少超过1/5时才播放
        float currentHealthPercentage = HealthPercentage;
        bool shouldPlayHurtAnimation = CheckShouldPlayHurtAnimation(healthPercentageBeforeDamage, currentHealthPercentage);
        
        if (shouldPlayHurtAnimation)
        {
            Debug.Log($"血量从 {healthPercentageBeforeDamage * 100:F1}% 降到 {currentHealthPercentage * 100:F1}%，触发受伤动画");
            StartCoroutine(HurtCoroutine());
            
            // 更新上次播放受伤动画的血量
            lastHurtAnimationHealth = currentHealthPercentage;
        }
        else
        {
            Debug.Log($"血量从 {healthPercentageBeforeDamage * 100:F1}% 降到 {currentHealthPercentage * 100:F1}%，未达到受伤动画阈值");
        }
    }
    
    /// <summary>
    /// 检查是否应该播放受伤动画
    /// </summary>
    /// <param name="healthBefore">受伤前血量百分比</param>
    /// <param name="healthAfter">受伤后血量百分比</param>
    /// <returns>是否应该播放受伤动画</returns>
    private bool CheckShouldPlayHurtAnimation(float healthBefore, float healthAfter)
    {
        // 计算血量下降了多少个1/5阈值
        float healthDropped = healthBefore - healthAfter;
        
        // 检查是否跨越了1/5血量的界限
        float thresholdsPassed = healthDropped / hurtAnimationHealthThreshold;
        
        // 如果血量下降超过1/5，或者从上次受伤动画到现在累计下降超过1/5
        float totalHealthDropSinceLastHurt = lastHurtAnimationHealth - healthAfter;
        
        bool shouldPlay = totalHealthDropSinceLastHurt >= hurtAnimationHealthThreshold;
        
        Debug.Log($"受伤动画检查 - 本次下降: {healthDropped * 100:F1}%, 累计下降: {totalHealthDropSinceLastHurt * 100:F1}%, 阈值: {hurtAnimationHealthThreshold * 100:F1}%, 应播放: {shouldPlay}");
        
        return shouldPlay;
    }
    
    /// <summary>
    /// 受伤协程 - 保持原有受伤动画
    /// </summary>
    private IEnumerator HurtCoroutine()
    {
        isHurt = true;
        if (anim != null) anim.SetTrigger("IsHit");
        
        // 播放受伤特效
        if (hurtEffect != null)
        {
            Instantiate(hurtEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Boss受伤动画播放");
        
        yield return new WaitForSeconds(0.5f);
        
        isHurt = false;
        SetAnimationState(1); // 回到Idle
    }
    
    /// <summary>
    /// 初始化引用
    /// </summary>
    private void InitializeReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            heroMovement = playerObj.GetComponent<HeroMovement>();
            heroAttack = playerObj.GetComponent<HeroAttackController>();
            heroLife = playerObj.GetComponent<HeroLife>();
        }
        
        if (fireBreathPoints == null || fireBreathPoints.Length == 0)
        {
            InitializeFireBreathPoints();
        }
    }
    
    /// <summary>
    /// 初始化统计数据
    /// </summary>
    private void InitializeStats()
    {
        lastMinionSpawnHealth = 1f;
        lastHurtAnimationHealth = 1f; // 初始化受伤动画系统
        
        // 初始血量现在由BossLife管理
        if (bossLife != null)
        {
            OnHealthChanged?.Invoke(bossLife.CurrentHealth, bossLife.MaxHealth);
        }
        
        Debug.Log($"BossDemon统计数据初始化完成 - 受伤动画阈值: {hurtAnimationHealthThreshold * 100:F1}%");
    }
    
    /// <summary>
    /// 初始化英雄动作追踪器
    /// </summary>
    private void InitializeHeroTracker()
    {
        if (heroTracker == null)
        {
            GameObject trackerObj = new GameObject("HeroActionTracker");
            trackerObj.transform.SetParent(transform);
            heroTracker = trackerObj.AddComponent<HeroActionTracker>();
            heroTracker.Initialize(this);
        }
    }
    
    /// <summary>
    /// 初始化火焰吐息点
    /// </summary>
    private void InitializeFireBreathPoints()
    {
        List<Transform> points = new List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            GameObject point = new GameObject($"FireBreathPoint_{i}");
            point.transform.SetParent(transform);
            point.transform.localPosition = new Vector3(1f + i * 0.5f, 0f, 0f);
            points.Add(point.transform);
        }
        fireBreathPoints = points.ToArray();
    }
    
    /// <summary>
    /// 触发屏幕震动
    /// </summary>
    /// <param name="shakeForce">震动强度</param>
    private void TriggerScreenShake(float shakeForce)
    {
        if (!enableScreenShake) return;
        
        if (impulseSource != null && CamaraShakeManager.Instance != null)
        {
            // 直接使用CamaraShakeManager系统
            CamaraShakeManager.Instance.CamaraShake(impulseSource);
            Debug.Log($"触发屏幕震动，强度: {shakeForce}");
        }
        else
        {
            Debug.LogWarning("无法触发屏幕震动：CinemachineImpulseSource 或 CamaraShakeManager 未找到");
        }
    }
    
    /// <summary>
    /// 更新计时器
    /// </summary>
    private void UpdateTimers()
    {
        stateTimer += Time.deltaTime;
        
        if (fireBreathTimer > 0)
        {
            fireBreathTimer -= Time.deltaTime;
        }
        
        if (Time.time - lastHeroAttackTime > 1f)
        {
            consecutiveHeroAttacks = 0;
        }
    }
    
    /// <summary>
    /// 更新阶段系统
    /// </summary>
    private void UpdatePhaseSystem()
    {
        float healthPercentage = HealthPercentage;
        
        // 检查第二阶段转换
        if (!isInPhase2 && healthPercentage <= healthPhaseThreshold)
        {
            EnterPhase2();
        }
    }
    
    /// <summary>
    /// 更新小怪召唤系统
    /// </summary>
    private void UpdateMinionSpawning()
    {
        float currentHealthPercentage = HealthPercentage;
        
        // 每1/3血量召唤小怪
        if (currentHealthPercentage <= lastMinionSpawnHealth - minionHealthThreshold)
        {
            SpawnMinions();
            lastMinionSpawnHealth = currentHealthPercentage;
        }
    }
    
    /// <summary>
    /// 更新AI行为
    /// </summary>
    private void UpdateAI()
    {
        // 如果正在出生，不进行AI更新
        if (currentState == DemonState.Spawn)
        {
            return;
        }
        
        if (isHurt || isDashing || isWaitingForLanding) return;
        
        if (player == null || heroLife == null || heroLife.IsDead)
        {
            SetAnimationState(1); // Idle
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 更新近战范围检测（每2秒检测一次）
        UpdateMeleeRangeDetection();
        
        // 检查玩家是否在检测范围内
        if (distanceToPlayer <= detectionRange)
        {
            isInCombat = true;
            
            // 使用新的近战检测系统
            if (isPlayerInMeleeRange)
            {
                // 近距离攻击
                if (Time.time >= nextAttackTime)
                {
                    if (isInPhase2)
                    {
                        StartFireBreathAttack(); // 第二阶段使用火焰吐息
                    }
                    else
                    {
                        StartMeleeAttack(); // 第一阶段使用普通近战
                    }
                }
            }
            else
            {
                // 中远距离 - 根据阶段选择攻击
                if (Time.time >= nextAttackTime)
                {
                    ChooseRangedAttack(distanceToPlayer);
                }
                else
                {
                    // 向玩家移动
                    SetAnimationState(2); // Walk
                    MoveTowardsPlayer();
                }
            }
        }
        else
        {
            isInCombat = false;
            SetAnimationState(1); // Idle
        }
    }
    
    /// <summary>
    /// 更新近战范围检测（每2秒检测一次）
    /// </summary>
    private void UpdateMeleeRangeDetection()
    {
        // 检查是否到了检测时间
        if (Time.time - lastMeleeDetectionTime >= meleeDetectionInterval)
        {
            lastMeleeDetectionTime = Time.time;
            
            // 执行近战范围检测
            bool wasInRange = isPlayerInMeleeRange;
            isPlayerInMeleeRange = CheckMeleeRange();
            
            // 如果玩家不在近战范围内，且之前在范围内，触发dash攻击
            if (wasInRange && !isPlayerInMeleeRange && isInCombat && !isAttacking && !isDashing)
            {
                Debug.Log("玩家离开近战范围，触发冲刺攻击和冲击波");
                StartDashAttack();
            }
            
            // 调试日志
            if (wasInRange != isPlayerInMeleeRange)
            {
                Debug.Log($"近战范围检测变化: {wasInRange} -> {isPlayerInMeleeRange}");
            }
        }
    }
    
    /// <summary>
    /// 检查玩家是否在近战范围内（以攻击点为圆心）
    /// </summary>
    /// <returns>玩家是否在近战范围内</returns>
    private bool CheckMeleeRange()
    {
        if (meleeAttackPoint == null || player == null) return false;
        
        // 使用Physics2D.OverlapCircle进行检测
        Collider2D playerCollider = Physics2D.OverlapCircle(
            meleeAttackPoint.position,
            meleeRangeRadius,
            playerLayer
        );
        
        bool inRange = playerCollider != null && playerCollider.CompareTag("Player");
        
        // 调试日志（每次检测时输出）
        Debug.Log($"近战范围检测 - 攻击点: {meleeAttackPoint.position}, 半径: {meleeRangeRadius}, 结果: {inRange}");
        
        return inRange;
    }
    
    /// <summary>
    /// 选择远程攻击
    /// </summary>
    /// <param name="distance">与玩家的距离</param>
    private void ChooseRangedAttack(float distance)
    {
        // 优先使用冲刺+落地冲击波攻击
        StartDashAttack();
    }
    
    /// <summary>
    /// 第一阶段：开始普通近战攻击
    /// </summary>
    private void StartMeleeAttack()
    {
        isAttacking = true;
        if (anim != null) anim.SetTrigger("Attack");
        nextAttackTime = Time.time + 2f;
        
        StartCoroutine(MeleeAttackCoroutine());
    }
    
    /// <summary>
    /// 第一阶段：普通近战攻击协程
    /// </summary>
    private IEnumerator MeleeAttackCoroutine()
    {
        Debug.Log("Boss执行第一阶段普通近战攻击");
        
        yield return new WaitForSeconds(0.5f); // 蓄力时间
        
        // 触发攻击震动
        TriggerScreenShake(attackShakeForce);
        
        // 使用BossAttackHitBox进行伤害判定
        if (attackHitbox != null)
        {
            // 激活攻击hitbox
            attackHitbox.SetActive(true);
            
            // 获取BossAttackHitBox组件并激活
            var hitboxComponent = attackHitbox.GetComponent<BossAttackHitBox>();
            if (hitboxComponent != null)
            {
                // 设置伤害值
                var damageField = typeof(BossAttackHitBox).GetField("damage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (damageField != null)
                {
                    damageField.SetValue(hitboxComponent, Mathf.RoundToInt(meleeAttackDamage));
                }
                
                // 激活hitbox
                hitboxComponent.ActivateForDuration(0.3f);
                Debug.Log($"激活近战攻击hitbox，伤害: {meleeAttackDamage}");
            }
            
            // 等待攻击完成
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            Debug.LogWarning("AttackHitbox未配置，使用传统伤害判定");
            
            // 备用方案：使用原有的距离判定
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance <= attackRange)
                {
                    DamagePlayer(meleeAttackDamage);
                    Debug.Log($"普通近战攻击命中，造成{meleeAttackDamage}点伤害");
                }
            }
        }
        
        yield return new WaitForSeconds(1f); // 恢复时间
        
        isAttacking = false;
        SetAnimationState(1); // 回到Idle
    }
    
    /// <summary>
    /// 第二阶段：开始火焰吐息攻击
    /// </summary>
    private void StartFireBreathAttack()
    {
        isAttacking = true;
        if (anim != null) anim.SetTrigger("Attack");
        nextAttackTime = Time.time + 4f;
        fireBreathTimer = fireBreathDuration;
        
        Debug.Log("Boss执行第二阶段火焰吐息攻击");
        
        // 调用动画事件系统处理火焰吐息
        if (animationEvents != null)
        {
            StartCoroutine(FireBreathAttackCoroutine());
        }
    }
    
    /// <summary>
    /// 第二阶段：火焰吐息攻击协程
    /// </summary>
    private IEnumerator FireBreathAttackCoroutine()
    {
        Debug.Log("Boss执行第二阶段火焰吐息攻击");
        
        // 激活火焰吐息hitbox
        if (fireBreathHitbox != null)
        {
            var hitboxComponent = fireBreathHitbox.GetComponent<BossAttackHitBox>();
            if (hitboxComponent != null)
            {
                // 设置伤害值
                var damageField = typeof(BossAttackHitBox).GetField("damage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (damageField != null)
                {
                    damageField.SetValue(hitboxComponent, Mathf.RoundToInt(fireBreathDamage));
                }
                
                // 激活hitbox持续整个火焰吐息期间
                hitboxComponent.ActivateForDuration(fireBreathDuration);
                Debug.Log($"激活火焰吐息hitbox，伤害: {fireBreathDamage}，持续时间: {fireBreathDuration}");
            }
        }
        
        // 动画事件系统会处理具体的伤害逻辑
        yield return new WaitForSeconds(fireBreathDuration);
        
        isAttacking = false;
        SetAnimationState(1); // 回到Idle
    }
    
    /// <summary>
    /// 开始冲刺攻击
    /// </summary>
    private void StartDashAttack()
    {
        Debug.Log("Boss开始冲刺攻击");
        
        // 设置冲刺状态
        isDashing = true;
        isAttacking = true;
        
        // 设置动画状态为Dash (State = 3)
        SetAnimationState(3);
        
        // 设置攻击冷却
        nextAttackTime = Time.time + 4f;
        
        // 启动冲刺协程
        StartCoroutine(DashAttackCoroutine());
    }
    
    /// <summary>
    /// 冲刺攻击协程 - 新版本：冲刺后等待落地
    /// </summary>
    private IEnumerator DashAttackCoroutine()
    {
        if (player == null) yield break;
        
        Vector2 dashDirection = (player.position - transform.position).normalized;
        Vector3 targetPosition = player.position;
        
        Debug.Log("Boss开始冲刺攻击协程");
        
        // 等待短暂时间让动画开始播放
        yield return new WaitForSeconds(0.1f);
        
        // 第一阶段：向上冲刺
        Vector2 jumpDirection = new Vector2(dashDirection.x, 1f).normalized;
        rb.velocity = jumpDirection * dashSpeed;
        
        Debug.Log("Boss向上冲刺");
        
        // 等待冲刺持续时间
        yield return new WaitForSeconds(dashDuration);
        
        // 第二阶段：向目标位置冲刺
        Vector2 finalDirection = (targetPosition - transform.position).normalized;
        rb.velocity = finalDirection * dashSpeed;
        
        Debug.Log("Boss向目标冲刺");
        
        // 等待到达目标附近
        yield return new WaitForSeconds(dashDuration * 0.5f);
        
        // 第三阶段：等待落地
        isWaitingForLanding = true;
        isDashing = false;
        
        Debug.Log("Boss等待落地");
        
        // 开始落地检测协程
        StartCoroutine(WaitForLandingCoroutine());
    }
    
    /// <summary>
    /// 等待落地协程
    /// </summary>
    private IEnumerator WaitForLandingCoroutine()
    {
        float landingCheckTimer = 0f;
        
        while (isWaitingForLanding)
        {
            landingCheckTimer += Time.deltaTime;
            
            // 定期检查是否落地
            if (landingCheckTimer >= landingCheckInterval)
            {
                landingCheckTimer = 0f;
                
                // 检查是否接近地面或速度很小
                if (IsNearGround() || Mathf.Abs(rb.velocity.y) < 0.5f)
                {
                    Debug.Log("Boss落地，释放冲击波");
                    
                    // 落地，释放冲击波
                    isWaitingForLanding = false;
                    yield return StartCoroutine(LandingShockwaveAttack());
                    break;
                }
            }
            
            yield return null;
        }
        
        // 重置状态
        isAttacking = false;
        SetAnimationState(1); // 回到Idle
    }
    
    /// <summary>
    /// 检查是否接近地面
    /// </summary>
    private bool IsNearGround()
    {
        // 向下发射射线检测地面
        float rayDistance = 1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, LayerMask.GetMask("Ground"));
        
        return hit.collider != null;
    }
    
    /// <summary>
    /// 落地冲击波攻击
    /// </summary>
    private IEnumerator LandingShockwaveAttack()
    {
        Debug.Log("Boss释放落地冲击波");
        
        // 确保Boss停止移动
        rb.velocity = Vector2.zero;
        
        // 触发冲击波动画
        if (anim != null) 
        {
            anim.SetTrigger("ShakeWave");
        }
        
        // 创建冲击波效果
        if (shockwaveEffect != null)
        {
            Instantiate(shockwaveEffect, transform.position, Quaternion.identity);
        }
        
        // 触发强烈的屏幕震动
        TriggerScreenShake(shockwaveShakeForce);
        
        // 等待一小段时间让动画播放
        yield return new WaitForSeconds(0.3f);
        
        // 执行冲击波伤害
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius);
        foreach (var target in hitTargets)
        {
            if (target.CompareTag("Player"))
            {
                DamagePlayer(shockwaveDamage);
                Debug.Log($"落地冲击波命中，造成{shockwaveDamage}点伤害");
                break;
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 开始扇形弹幕攻击
    /// </summary>
    private void StartFanBarrageAttack()
    {
        StartCoroutine(FanBarrageCoroutine());
    }
    
    /// <summary>
    /// 扇形弹幕攻击协程
    /// </summary>
    private IEnumerator FanBarrageCoroutine()
    {
        Debug.Log("Boss释放扇形弹幕攻击");
        
        int bulletCount = isInPhase2 ? 8 : 5;
        float fanAngle = isInPhase2 ? 60f : 45f;
        
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = -fanAngle / 2 + (fanAngle / (bulletCount - 1)) * i;
            Vector2 direction = RotateVector2(isFacingRight ? Vector2.right : Vector2.left, angle);
            
            CreateBullet(transform.position, direction);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    /// <summary>
    /// 召唤小怪
    /// </summary>
    private void SpawnMinions()
    {
        if (minionPrefab == null) return;
        
        Debug.Log($"Boss召唤{minionsPerSpawn}个小怪");
        
        for (int i = 0; i < minionsPerSpawn; i++)
        {
            float angle = (360f / minionsPerSpawn) * i;
            Vector2 spawnPos = (Vector2)transform.position + (Vector2)(Quaternion.Euler(0, 0, angle) * Vector2.up * minionSpawnRadius);
            
            GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            spawnedMinions.Add(minion);
            
            // 初始化小怪
            DemonMinion minionScript = minion.GetComponent<DemonMinion>();
            if (minionScript == null)
            {
                minionScript = minion.AddComponent<DemonMinion>();
            }
            minionScript.Initialize(this, player);
        }
    }
    
    /// <summary>
    /// 进入第二阶段
    /// </summary>
    private void EnterPhase2()
    {
        isInPhase2 = true;
        moveSpeed *= 1.2f; // 增加移动速度，提升攻击欲望
        OnPhase2Enter?.Invoke();
        
        Debug.Log("Boss进入第二阶段 - 攻击欲望增强！");
        
        // 触发阶段转换震动
        TriggerScreenShake(phaseTransitionShakeForce);
        
        // 阶段转换的视觉效果
        if (hurtEffect != null)
        {
            Instantiate(hurtEffect, transform.position, Quaternion.identity);
        }
    }
    
    /// <summary>
    /// 开始出生状态
    /// </summary>
    private void StartSpawn()
    {
        Debug.Log("BossDemon开始出生");
        
        // 设置出生状态
        currentState = DemonState.Spawn;
        
        // 确保Boss不会移动
        rb.velocity = Vector2.zero;
        
        // 禁用AI功能直到出生完成
        isInCombat = false;
        
        // 播放出生动画
        if (anim != null)
        {
            anim.SetInteger("State", (int)DemonState.Spawn);
            anim.SetTrigger("Spawn");
            Debug.Log("Boss出生动画开始播放");
        }
        
        // 创建出生特效
        if (hurtEffect != null) // 临时使用现有特效
        {
            Instantiate(hurtEffect, transform.position, Quaternion.identity);
        }
        
        // 触发出生震动
        TriggerScreenShake(phaseTransitionShakeForce);
        
        // 开始出生协程
        StartCoroutine(SpawnCoroutine());
    }
    
    /// <summary>
    /// 出生协程
    /// </summary>
    private IEnumerator SpawnCoroutine()
    {
        Debug.Log("Boss出生协程开始");
        
        // 等待出生动画播放完成
        yield return new WaitForSeconds(2f); // 出生动画持续时间
        
        // 完成出生
        CompleteSpawn();
    }
    
    /// <summary>
    /// 完成出生
    /// </summary>
    private void CompleteSpawn()
    {
        Debug.Log("Boss出生完成");
        
        // 切换到Idle状态
        currentState = DemonState.Idle;
        
        // 触发出生完成事件
        OnSpawnComplete?.Invoke();
        
        // 更新动画状态
        if (anim != null)
        {
            anim.SetInteger("State", (int)DemonState.Idle);
            Debug.Log("Boss出生动画完成，切换到Idle状态");
        }
        
        // 启用AI功能
        isInCombat = false; // 开始时不在战斗状态
        
        // 发送初始血量事件
        if (bossLife != null)
        {
            OnHealthChanged?.Invoke(bossLife.CurrentHealth, bossLife.MaxHealth);
        }
        
        Debug.Log("Boss已准备好进入战斗");
    }
    
    /// <summary>
    /// 对英雄滑铲的反应（仅第二阶段）
    /// </summary>
    private void ReactToHeroSlide()
    {
        lastReactionTime = Time.time;
        
        Debug.Log("Boss检测到英雄滑铲，执行反应");
        
        // 有几率dash并释放冲击波
        StartDashAttack();
    }
    
    /// <summary>
    /// 对英雄跳跃的反应（仅第二阶段）
    /// </summary>
    private void ReactToHeroJump()
    {
        lastReactionTime = Time.time;
        
        Debug.Log("Boss检测到英雄跳跃，释放扇形弹幕");
        
        // 向英雄释放扇形弹幕攻击
        StartFanBarrageAttack();
    }
    
    /// <summary>
    /// 创建子弹
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="direction">方向</param>
    private void CreateBullet(Vector3 position, Vector2 direction)
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * bulletSpeed;
            }
            
            // 添加伤害组件
            DemonProjectile projectile = bullet.GetComponent<DemonProjectile>();
            if (projectile == null)
            {
                projectile = bullet.AddComponent<DemonProjectile>();
            }
            projectile.Initialize(15f, false); // 单次伤害
            
            Destroy(bullet, 5f);
        }
    }
    
    /// <summary>
    /// 旋转2D向量
    /// </summary>
    /// <param name="vector">向量</param>
    /// <param name="angle">角度</param>
    /// <returns>旋转后的向量</returns>
    private Vector2 RotateVector2(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
    }
    
    /// <summary>
    /// 对玩家造成伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    private void DamagePlayer(float damage)
    {
        if (player != null && heroLife != null && !heroLife.IsDead)
        {
            heroLife.TakeDamage(Mathf.RoundToInt(damage));
        }
    }
    
    /// <summary>
    /// 死亡
    /// </summary>
    private void Die()
    {
        if (isDead) return; // 防止重复调用
        
        isDead = true;
        if (anim != null) anim.SetTrigger("Dead");
        rb.velocity = Vector2.zero;
        
        Debug.Log("BossDemon死亡");
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // 禁用碰撞器
        if (col != null)
        {
            col.enabled = false;
        }
        
        // 销毁所有召唤的小怪
        foreach (var minion in spawnedMinions)
        {
            if (minion != null)
            {
                Destroy(minion);
            }
        }
        spawnedMinions.Clear();
        
        OnDeath?.Invoke();
        
        // 在死亡动画后销毁（BossLife组件会处理这个）
        // Destroy(gameObject, 3f);
    }
    
    /// <summary>
    /// 英雄攻击动作追踪
    /// </summary>
    public void OnHeroAttack()
    {
        if (Time.time - lastHeroAttackTime < 1f)
        {
            consecutiveHeroAttacks++;
        }
        else
        {
            consecutiveHeroAttacks = 1;
        }
        lastHeroAttackTime = Time.time;
    }
    
    /// <summary>
    /// 在编辑器中绘制调试信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 攻击范围（原有的圆形范围，仅作参考）
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 冲击波范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
        
        // 近战攻击范围（以攻击点为圆心）
        if (meleeAttackPoint != null)
        {
            Gizmos.color = isPlayerInMeleeRange ? Color.red : new Color(1f, 0.5f, 0f); // 橙色
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeRangeRadius);
            
            // 绘制攻击点标记
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(meleeAttackPoint.position, Vector3.one * 0.3f);
        }
        
        // 近战攻击hitbox
        if (attackHitbox != null)
        {
            Gizmos.color = Color.red;
            BoxCollider2D attackBox = attackHitbox.GetComponent<BoxCollider2D>();
            if (attackBox != null)
            {
                Vector3 hitboxWorldPos = transform.TransformPoint(attackHitbox.transform.localPosition);
                Vector3 hitboxSize = new Vector3(attackBox.size.x, attackBox.size.y, 0.2f);
                Gizmos.DrawWireCube(hitboxWorldPos, hitboxSize);
                
                // 如果hitbox激活，用实心方块显示
                if (attackHitbox.activeInHierarchy)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                    Gizmos.DrawCube(hitboxWorldPos, hitboxSize);
                }
            }
        }
        
        // 火焰吐息hitbox
        if (fireBreathHitbox != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f); // 橙色
            BoxCollider2D fireBreathBox = fireBreathHitbox.GetComponent<BoxCollider2D>();
            if (fireBreathBox != null)
            {
                Vector3 hitboxWorldPos = transform.TransformPoint(fireBreathHitbox.transform.localPosition);
                Vector3 hitboxSize = new Vector3(fireBreathBox.size.x, fireBreathBox.size.y, 0.2f);
                Gizmos.DrawWireCube(hitboxWorldPos, hitboxSize);
                
                // 如果hitbox激活，用实心方块显示
                if (fireBreathHitbox.activeInHierarchy)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                    Gizmos.DrawCube(hitboxWorldPos, hitboxSize);
                }
            }
        }
        
        // 第二阶段标记
        if (isInPhase2)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        }
        
        // 显示近战检测状态
        if (Application.isPlaying && meleeAttackPoint != null)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(meleeAttackPoint.position + Vector3.up * 1f, 
                $"近战范围: {(isPlayerInMeleeRange ? "是" : "否")}");
#endif
        }
    }
    
    /// <summary>
    /// 组件销毁时清理
    /// </summary>
    private void OnDestroy()
    {
        // 取消订阅BossLife事件
        if (bossLife != null)
        {
            bossLife.OnHealthChanged -= OnBossLifeHealthChanged;
            bossLife.OnDeath -= OnBossLifeDeath;
        }
    }
    
    #region 调试和测试方法
    
    /// <summary>
    /// 测试受伤动画系统
    /// </summary>
    [ContextMenu("测试受伤动画系统")]
    public void TestHurtAnimationSystem()
    {
        if (IsDead)
        {
            Debug.LogWarning("Boss已死亡，无法测试受伤动画系统");
            return;
        }
        
        Debug.Log("=== 受伤动画系统测试开始 ===");
        Debug.Log($"当前血量: {CurrentHealth}/{MaxHealth} ({HealthPercentage * 100:F1}%)");
        Debug.Log($"受伤动画阈值: {hurtAnimationHealthThreshold * 100:F1}%");
        Debug.Log($"上次受伤动画血量: {lastHurtAnimationHealth * 100:F1}%");
        
        // 计算需要多少伤害才能触发下一次受伤动画
        float healthNeededToTrigger = hurtAnimationHealthThreshold * MaxHealth;
        int damageNeeded = Mathf.CeilToInt(healthNeededToTrigger);
        
        Debug.Log($"需要 {damageNeeded} 点伤害触发下一次受伤动画");
        
        // 造成伤害
        TakeDamage(damageNeeded);
        
        Debug.Log("=== 受伤动画系统测试完成 ===");
    }
    
    /// <summary>
    /// 测试多次小伤害
    /// </summary>
    [ContextMenu("测试多次小伤害")]
    public void TestMultipleSmallDamage()
    {
        if (IsDead)
        {
            Debug.LogWarning("Boss已死亡，无法测试");
            return;
        }
        
        Debug.Log("=== 多次小伤害测试开始 ===");
        
        StartCoroutine(SmallDamageTestCoroutine());
    }
    
    /// <summary>
    /// 小伤害测试协程
    /// </summary>
    private IEnumerator SmallDamageTestCoroutine()
    {
        int smallDamage = Mathf.RoundToInt(MaxHealth * 0.05f); // 5%的血量
        
        Debug.Log($"每次造成 {smallDamage} 点伤害 (约{(float)smallDamage / MaxHealth * 100:F1}%血量)");
        
        for (int i = 1; i <= 8; i++)
        {
            if (IsDead) break;
            
            Debug.Log($"--- 第 {i} 次攻击 ---");
            TakeDamage(smallDamage);
            
            yield return new WaitForSeconds(1f);
        }
        
        Debug.Log("=== 多次小伤害测试完成 ===");
    }
    
    /// <summary>
    /// 重置受伤动画系统
    /// </summary>
    [ContextMenu("重置受伤动画系统")]
    public void ResetHurtAnimationSystem()
    {
        lastHurtAnimationHealth = HealthPercentage;
        Debug.Log($"受伤动画系统已重置，当前血量百分比: {HealthPercentage * 100:F1}%");
    }
    
    /// <summary>
    /// 显示受伤动画系统状态
    /// </summary>
    [ContextMenu("显示受伤动画系统状态")]
    public void ShowHurtAnimationSystemStatus()
    {
        Debug.Log("=== 受伤动画系统状态 ===");
        Debug.Log($"当前血量: {CurrentHealth}/{MaxHealth} ({HealthPercentage * 100:F1}%)");
        Debug.Log($"上次受伤动画血量: {lastHurtAnimationHealth * 100:F1}%");
        Debug.Log($"距离下次受伤动画: {(lastHurtAnimationHealth - HealthPercentage) * 100:F1}% / {hurtAnimationHealthThreshold * 100:F1}%");
        Debug.Log($"受伤动画阈值: {hurtAnimationHealthThreshold * 100:F1}% HP");
        
        float progressToNextHurt = (lastHurtAnimationHealth - HealthPercentage) / hurtAnimationHealthThreshold;
        Debug.Log($"下次受伤动画进度: {progressToNextHurt * 100:F1}%");
        
        if (progressToNextHurt >= 1f)
        {
            Debug.Log("?? 已达到受伤动画触发条件！");
        }
    }
    
    /// <summary>
    /// 强制触发受伤动画
    /// </summary>
    [ContextMenu("强制触发受伤动画")]
    public void ForceHurtAnimation()
    {
        if (IsDead)
        {
            Debug.LogWarning("Boss已死亡，无法播放受伤动画");
            return;
        }
        
        Debug.Log("强制触发受伤动画");
        StartCoroutine(HurtCoroutine());
        
        // 更新记录
        lastHurtAnimationHealth = HealthPercentage;
    }
    
    /// <summary>
    /// 测试近战检测系统
    /// </summary>
    [ContextMenu("测试近战检测系统")]
    public void TestMeleeDetectionSystem()
    {
        Debug.Log("=== 近战检测系统测试 ===");
        Debug.Log($"近战攻击点位置: {(meleeAttackPoint != null ? meleeAttackPoint.position : Vector3.zero)}");
        Debug.Log($"近战检测半径: {meleeRangeRadius}");
        Debug.Log($"检测间隔: {meleeDetectionInterval}秒");
        Debug.Log($"玩家层级: {playerLayer.value}");
        Debug.Log($"当前玩家在近战范围内: {isPlayerInMeleeRange}");
        Debug.Log($"距离上次检测时间: {Time.time - lastMeleeDetectionTime:F2}秒");
        
        // 手动执行一次检测
        bool currentResult = CheckMeleeRange();
        Debug.Log($"手动检测结果: {currentResult}");
        
        if (player != null && meleeAttackPoint != null)
        {
            float actualDistance = Vector2.Distance(meleeAttackPoint.position, player.position);
            Debug.Log($"实际距离: {actualDistance:F2} (检测半径: {meleeRangeRadius})");
        }
    }
    
    /// <summary>
    /// 强制触发冲刺攻击
    /// </summary>
    [ContextMenu("强制触发冲刺攻击")]
    public void ForceDashAttack()
    {
        if (IsDead || isAttacking || isDashing)
        {
            Debug.LogWarning("Boss状态不允许执行冲刺攻击");
            return;
        }
        
        Debug.Log("强制触发冲刺攻击");
        StartDashAttack();
    }
    
    /// <summary>
    /// 重置近战检测系统
    /// </summary>
    [ContextMenu("重置近战检测系统")]
    public void ResetMeleeDetectionSystem()
    {
        lastMeleeDetectionTime = 0f;
        isPlayerInMeleeRange = false;
        Debug.Log("近战检测系统已重置");
    }
    
    /// <summary>
    /// 显示近战检测状态
    /// </summary>
    [ContextMenu("显示近战检测状态")]
    public void ShowMeleeDetectionStatus()
    {
        Debug.Log("=== 近战检测状态 ===");
        Debug.Log($"玩家在近战范围内: {isPlayerInMeleeRange}");
        Debug.Log($"上次检测时间: {lastMeleeDetectionTime:F2}");
        Debug.Log($"距离下次检测: {meleeDetectionInterval - (Time.time - lastMeleeDetectionTime):F2}秒");
        Debug.Log($"当前战斗状态: {isInCombat}");
        Debug.Log($"当前攻击状态: {isAttacking}");
        Debug.Log($"当前冲刺状态: {isDashing}");
        
        if (meleeAttackPoint != null && player != null)
        {
            float distance = Vector2.Distance(meleeAttackPoint.position, player.position);
            Debug.Log($"攻击点到玩家距离: {distance:F2}");
            Debug.Log($"是否在检测半径内: {distance <= meleeRangeRadius}");
        }
    }
    
    #endregion

    /// <summary>
    /// 更新反应系统（仅第二阶段）
    /// </summary>
    private void UpdateReactionSystem()
    {
        if (Time.time - lastReactionTime < reactionCooldown) return;
        
        // 检查英雄动作并相应地反应
        if (heroTracker != null)
        {
            if (heroTracker.HasHeroRolled() && Random.value < slideReactionChance)
            {
                ReactToHeroSlide();
            }
            else if (heroTracker.HasHeroJumped() && Random.value < jumpReactionChance)
            {
                ReactToHeroJump();
            }
        }
    }
    
    /// <summary>
    /// 向玩家移动
    /// </summary>
    private void MoveTowardsPlayer()
    {
        if (player == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
    }
    
    /// <summary>
    /// 设置动画状态
    /// </summary>
    /// <param name="stateValue">状态值：0=Spawn, 1=Idle, 2=Walk, 3=Dash</param>
    private void SetAnimationState(int stateValue)
    {
        if (anim != null)
        {
            anim.SetInteger("State", stateValue);
        }
    }
    
    /// <summary>
    /// 更新动画
    /// </summary>
    private void UpdateAnimations()
    {
        if (anim != null)
        {
            anim.SetBool("IsPhase2", isInPhase2);
            anim.SetBool("IsAttacking", isAttacking);
            anim.SetBool("IsSpawning", currentState == DemonState.Spawn);
        }
    }
    
    /// <summary>
    /// 更新朝向
    /// </summary>
    private void UpdateFacingDirection()
    {
        if (player != null && !isAttacking && !isDashing && !isWaitingForLanding)
        {
            bool shouldFaceRight = player.position.x > transform.position.x;
            if (shouldFaceRight != isFacingRight)
            {
                Flip();
            }
        }
    }

    /// <summary>
    /// 翻转Boss朝向
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}