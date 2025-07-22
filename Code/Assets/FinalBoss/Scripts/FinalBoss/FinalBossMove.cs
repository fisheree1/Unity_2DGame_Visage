using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FinalBossMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rollSpeed = 8f;
    [SerializeField] private float rollDistance = 5f;

    [Header("Three-Zone Detection System")]
    [SerializeField] private float outerDetectionRange = 25f; // 外围检测圆：观察区域
    [SerializeField] private float midDetectionRange = 12f;   // 中距离检测圆：突进区域
    [SerializeField] private float innerDetectionRange = 5f;  // 内围检测圆：近战追击区域
    [SerializeField] private float attackRange = 3f;         // 实际攻击范围
    [SerializeField] private bool startInCombatMode = false;  // 是否开始就进入战斗模式
    [SerializeField] private float loseTargetDelay = 2f;      // 失去目标后多久停止追踪

    [Header("AI Behavior")]
    [SerializeField] private float idleTime = 0.5f;
    [SerializeField] private float actionCooldown = 0.4f;
    [SerializeField] private float crouchTime = 0.6f;
    
    [Header("Three-Phase Tactical Settings")]
    [SerializeField] private float meleeChaseTimeout = 4f;          // 内围近战追击超时时间
    [SerializeField] private float rollAwayDistance = 7f;           // 翻滚远离距离
    [SerializeField] private float rollTowardsDistance = 5f;        // 翻滚突进距离
    
    [Header("Attack Settings")]
    [SerializeField] private float punchAttackCooldown = 1.5f;      // 拳击攻击冷却时间
    [SerializeField] private float crouchKickCooldown = 2f;         // 蹲踢攻击冷却时间
    [SerializeField] private float comboAttackChance = 0.3f;        // 连击概率

    [Header("Meditation Settings")]
    [SerializeField] private float meditationDuration = 3f;         // 冥想持续时间
    [SerializeField] private float invulnerabilityTime = 0.8f;      // 无敌时间
    [SerializeField] private float damageFlashDuration = 0.2f;      // 伤害闪烁持续时间
    [SerializeField] private Color damageFlashColor = Color.red;    // 伤害闪烁颜色

    // Meditation trigger thresholds (75%, 50%, 25% health)
    private bool[] meditationTriggered = new bool[3];               // 记录每个阶段是否已触发
    private float[] meditationThresholds = { 0.75f, 0.5f, 0.25f }; // 冥想触发阈值

    [Header("Distance-Based Probabilities")]
    [Range(0, 100)]
    [SerializeField] private int farRollChance = 50;
    [Range(0, 100)]
    [SerializeField] private int farRunChance = 50;
    [Range(0, 100)]
    [SerializeField] private int midRangeRunChance = 70;
    [Range(0, 100)]
    [SerializeField] private int midRangeRollChance = 30;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Animator anim;
    private FinalBossAttack attackController;
    private BossLife bossLife;

    // Target
    private Transform player;

    // State
    private FinalBossMoveState currentState = FinalBossMoveState.Idle;
    private bool isFacingRight = true;
    private float actionTimer = 0f;
    private float stateTimer = 0f;
    private bool isGrounded = true;

    // State flags
    private bool isAttacking = false;
    private bool isHurt = false;
    private bool isCrouching = false;
    private bool isRolling = false;
    private bool isMeditating = false;  // 新增Meditation状态
    private bool isInvulnerable = false; // 新增无敌状态
    private Color originalColor;         // 原始颜色，用于闪烁效果
    
    // Three-phase tactical state tracking
    private ThreePhaseTacticalState currentTacticalPhase = ThreePhaseTacticalState.OutOfRange;
    private float lastPunchAttackTime = 0f;              // 上次拳击攻击时间
    private float lastCrouchKickTime = 0f;               // 上次蹲踢攻击时间
    private float meleeChaseStartTime = 0f;              // 近战追击开始时间
    private float outerZoneEntryTime = 0f;               // 进入外围区域的时间
    private bool isInOuterZone = false;                  // 是否在外围观察区域
    private bool isInMidZone = false;                    // 是否在中距离突进区域
    private bool isInInnerZone = false;                  // 是否在内围追击区域
    private bool playerInDetectionRange = false;         // 玩家是否在任意检测范围内
    private bool isPlayerDetected = false;               // 是否已侦测到玩家
    private float loseTargetTimer = 0f;                  // 失去目标计时器

    private enum FinalBossMoveState
    {
        Idle,
        Run,
        Roll,
        Crouch
    }

    public enum ThreePhaseTacticalState
    {
        OutOfRange,        // 超出范围
        OuterZone,         // 外围观察区域
        MidZone,           // 中距离突进区域
        InnerZone          // 内围追击区域
    }

    // Properties
    public bool IsFacingRight => isFacingRight;
    public bool IsAttacking => isAttacking;
    public bool IsHurt => isHurt;
    public bool IsCrouching => isCrouching;
    public bool IsRolling => isRolling;
    public bool IsPlayerDetected => isPlayerDetected;
    public bool IsMeditating => isMeditating;  // 新增Meditation状态属性
    public bool IsInvulnerable => isInvulnerable; // 新增无敌状态属性
    public Transform Player => player;
    public float GetDistanceToPlayer() => player ? Vector2.Distance(transform.position, player.position) : float.MaxValue;
    
    // Three-Phase Tactical System Properties
    public ThreePhaseTacticalState CurrentTacticalPhase => currentTacticalPhase;
    public bool IsInOuterZone => isInOuterZone;
    public bool IsInMidZone => isInMidZone;
    public bool IsInInnerZone => isInInnerZone;
    public float TimeSinceLastPunchAttack => Time.time - lastPunchAttackTime;
    public float TimeSinceLastCrouchKick => Time.time - lastCrouchKickTime;
    public float MeleeChaseElapsedTime => currentTacticalPhase == ThreePhaseTacticalState.InnerZone ? Time.time - meleeChaseStartTime : 0f;

    void Start()
    {
        InitializeComponents();
        FindPlayer();
        SetupGroundCheck();
        
        // 保存原始颜色
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        
        if (startInCombatMode)
        {
            isPlayerDetected = true;
        }
        else
        {
            isPlayerDetected = false;
        }
        
        Debug.Log($"Final Boss initialized. Start in combat mode: {startInCombatMode}");
    }

    void Update()
    {
        // 如果Boss已死亡，停止所有行为
        if (bossLife != null && bossLife.IsDead)
        {
            SetState(FinalBossMoveState.Idle);
            return;
        }
        
        // 如果Boss在Meditation状态，跳过正常的AI逻辑
        if (isMeditating)
        {
            Debug.Log("Final Boss: Skipping AI logic during Meditation");
            return;
        }
        
        // 检查冥想状态触发
        HandleMeditationTrigger();
        
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            UpdateTimers();
            CheckGrounded();
            UpdateFacingDirection();
            UpdateTacticalPhase(distanceToPlayer);
            HandlePlayerDetection(distanceToPlayer);
            ExecuteThreePhaseTactics(distanceToPlayer);
            HandleAI();
            UpdateAnimationState();
        }
        else
        {
            FindPlayer();
        }
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        attackController = GetComponent<FinalBossAttack>();
        bossLife = GetComponent<BossLife>();
        
        if (rb == null) Debug.LogError("Final Boss: Rigidbody2D component missing!");
        if (anim == null) Debug.LogError("Final Boss: Animator component missing!");
        if (attackController == null) Debug.LogError("Final Boss: FinalBossAttack component missing!");
        if (bossLife == null) Debug.LogError("Final Boss: BossLife component missing!");
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Final Boss: Player not found!");
        }
    }

    private void SetupGroundCheck()
    {
        if (groundCheck == null)
        {
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.parent = transform;
            groundCheck.localPosition = new Vector3(0, -1f, 0);
        }
    }

    private void UpdateTimers()
    {
        if (actionTimer > 0)
            actionTimer -= Time.deltaTime;
        if (stateTimer > 0)
            stateTimer -= Time.deltaTime;
    }

    private void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    private void UpdateFacingDirection()
    {
        if (player == null || isAttacking || isRolling || !isPlayerDetected)
            return;

        bool shouldFaceRight = player.position.x > transform.position.x;
        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }

    private void HandlePlayerDetection(float distanceToPlayer)
    {
        // 检查玩家是否在任意检测范围内
        playerInDetectionRange = distanceToPlayer <= outerDetectionRange;
        
        if (playerInDetectionRange)
        {
            if (!isPlayerDetected)
            {
                isPlayerDetected = true;
                Debug.Log($"Final Boss: Player detected at distance {distanceToPlayer}");
            }
            loseTargetTimer = 0f;
        }
        else
        {
            if (isPlayerDetected)
            {
                loseTargetTimer += Time.deltaTime;
                if (loseTargetTimer >= loseTargetDelay)
                {
                    isPlayerDetected = false;
                    Debug.Log("Final Boss: Player lost, returning to idle");
                }
            }
        }
    }

    private void UpdateTacticalPhase(float distanceToPlayer)
    {
        ThreePhaseTacticalState newPhase = currentTacticalPhase;
        
        // 重置区域状态
        isInOuterZone = false;
        isInMidZone = false;
        isInInnerZone = false;
        
        if (!isPlayerDetected)
        {
            newPhase = ThreePhaseTacticalState.OutOfRange;
        }
        else if (distanceToPlayer <= innerDetectionRange)
        {
            newPhase = ThreePhaseTacticalState.InnerZone;
            isInInnerZone = true;
        }
        else if (distanceToPlayer <= midDetectionRange)
        {
            newPhase = ThreePhaseTacticalState.MidZone;
            isInMidZone = true;
        }
        else if (distanceToPlayer <= outerDetectionRange)
        {
            newPhase = ThreePhaseTacticalState.OuterZone;
            isInOuterZone = true;
        }
        
        // 阶段变化处理
        if (newPhase != currentTacticalPhase)
        {
            Debug.Log($"Final Boss tactical phase changed from {currentTacticalPhase} to {newPhase}");
            
            if (newPhase == ThreePhaseTacticalState.OuterZone)
            {
                outerZoneEntryTime = Time.time;
            }
            else if (newPhase == ThreePhaseTacticalState.InnerZone)
            {
                meleeChaseStartTime = Time.time;
            }
            
            currentTacticalPhase = newPhase;
        }
    }

    private void ExecuteThreePhaseTactics(float distanceToPlayer)
    {
        if (isHurt || isAttacking || actionTimer > 0)
            return;

        switch (currentTacticalPhase)
        {
            case ThreePhaseTacticalState.OutOfRange:
                // 超出范围，保持idle
                SetState(FinalBossMoveState.Idle);
                break;
                
            case ThreePhaseTacticalState.OuterZone:
                // 外围观察区域：缓慢接近
                ExecuteOuterZoneTactics(distanceToPlayer);
                break;
                
            case ThreePhaseTacticalState.MidZone:
                // 中距离突进区域：选择突进或继续接近
                ExecuteMidZoneTactics(distanceToPlayer);
                break;
                
            case ThreePhaseTacticalState.InnerZone:
                // 内围追击区域：近战攻击
                ExecuteInnerZoneTactics(distanceToPlayer);
                break;
        }
    }

    private void ExecuteOuterZoneTactics(float distanceToPlayer)
    {
        // 外围区域：不追击玩家，只在MagicSpawnPoint处释放Crouch魔法攻击
        if (distanceToPlayer > midDetectionRange)
        {
            // 保持距离，不主动接近
            SetState(FinalBossMoveState.Idle);
            
            // 定期释放Crouch魔法攻击 - 外围区域特有的SmallSpark扇形攻击
            if (attackController != null && TimeSinceLastCrouchKick >= crouchKickCooldown)
            {
                StartCoroutine(PerformCrouchMagicAttack());
            }
            else
            {
                actionTimer = idleTime;
            }
        }
        else
        {
            // 到达中距离区域边界，停止接近
            SetState(FinalBossMoveState.Idle);
            actionTimer = idleTime;
        }
    }

    private void ExecuteMidZoneTactics(float distanceToPlayer)
    {
        // 中距离区域：利用翻滚快速接近进行CrouchKick攻击
        if (distanceToPlayer <= innerDetectionRange)
        {
            // 已经进入内围，停止中距离战术
            return;
        }
        
        // 优先使用翻滚接近并进行CrouchKick攻击
        if (TimeSinceLastCrouchKick >= crouchKickCooldown)
        {
            StartCoroutine(PerformRollToCrouchKick());
        }
        else
        {
            // 冷却中，保持距离
            SetState(FinalBossMoveState.Idle);
            actionTimer = idleTime;
        }
    }

    private void ExecuteInnerZoneTactics(float distanceToPlayer)
    {
        // 内围区域：追击利用punch攻击，3秒内未攻击则翻滚远离并释放魔法
        if (distanceToPlayer <= attackRange)
        {
            // 在攻击范围内，优先进行punch攻击
            if (TimeSinceLastPunchAttack >= punchAttackCooldown)
            {
                StartCoroutine(PerformPunchAttack());
            }
            else
            {
                // punch冷却中，继续追击
                MoveTowardsPlayer();
            }
        }
        else
        {
            // 检查是否追击超时（3秒内未攻击）
            if (MeleeChaseElapsedTime > 3f)
            {
                // 超时，执行翻滚远离并释放魔法攻击
                StartCoroutine(PerformRollAwayWithMagic());
            }
            else
            {
                // 继续追击
                MoveTowardsPlayer();
            }
        }
    }

    private void ChooseAttackType()
    {
        // 检查攻击冷却
        bool canPunch = TimeSinceLastPunchAttack >= punchAttackCooldown;
        bool canCrouchKick = TimeSinceLastCrouchKick >= crouchKickCooldown;
        
        if (!canPunch && !canCrouchKick)
        {
            // 都在冷却中，继续移动
            MoveTowardsPlayer();
            return;
        }
        
        // 选择攻击类型
        if (canPunch && canCrouchKick)
        {
            // 两种攻击都可用，随机选择
            if (UnityEngine.Random.value < 0.6f)
            {
                StartCoroutine(PerformPunchAttack());
            }
            else
            {
                StartCoroutine(PerformCrouchKickAttack());
            }
        }
        else if (canPunch)
        {
            StartCoroutine(PerformPunchAttack());
        }
        else if (canCrouchKick)
        {
            StartCoroutine(PerformCrouchKickAttack());
        }
    }

    private void HandleAI()
    {
        // 调试信息
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Final Boss AI State - Current: {currentState}, Tactical: {currentTacticalPhase}, " +
                     $"Player Detected: {isPlayerDetected}, Distance: {GetDistanceToPlayer():F1}, Meditating: {isMeditating}");
        }
        
        if (isHurt || isAttacking || isCrouching || isMeditating)
        {
            return;
        }
        
        if (isRolling)
        {
            return;
        }
        
        if (!isPlayerDetected)
        {
            SetState(FinalBossMoveState.Idle);
            return;
        }
        
        if (actionTimer > 0)
        {
            if (currentState == FinalBossMoveState.Idle)
            {
                // 在idle状态等待
                return;
            }
        }
        
        // 性能优化：每2秒调试一次
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"Final Boss AI - State: {currentState}, Phase: {currentTacticalPhase}, Action Timer: {actionTimer:F1}");
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player == null)
            return;
            
        SetState(FinalBossMoveState.Run);
        
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
    }

    private IEnumerator PerformPunchAttack()
    {
        isAttacking = true;
        SetState(FinalBossMoveState.Idle);
        rb.velocity = Vector2.zero;
        
        Debug.Log("Final Boss performing Punch Attack");
        
        if (attackController != null)
        {
            attackController.PerformPunchAttack();
            lastPunchAttackTime = Time.time;
        }
        
        yield return new WaitForSeconds(0.8f);
        
        isAttacking = false;
        actionTimer = actionCooldown;
        
        // 连击概率
        if (UnityEngine.Random.value < comboAttackChance && TimeSinceLastCrouchKick >= crouchKickCooldown)
        {
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(PerformCrouchKickAttack());
        }
    }

    private IEnumerator PerformCrouchKickAttack()
    {
        isAttacking = true;
        isCrouching = true;
        SetState(FinalBossMoveState.Crouch);
        rb.velocity = Vector2.zero;
        
        Debug.Log("Final Boss performing Crouch Kick Attack");
        
        yield return new WaitForSeconds(crouchTime * 0.5f);
        
        if (attackController != null)
        {
            attackController.PerformCrouchKickAttack();
            
            // 只在外围区域释放SmallSpark，其他情况不释放
            // 不在这里释放SmallSpark，因为这会在所有CrouchKick中释放
            
            lastCrouchKickTime = Time.time;
        }
        
        yield return new WaitForSeconds(crouchTime * 0.5f);
        
        isCrouching = false;
        isAttacking = false;
        actionTimer = actionCooldown;
    }

    private IEnumerator PerformRoll()
    {
        isRolling = true;
        SetState(FinalBossMoveState.Roll);
        
        Vector2 rollDirection = (player.position - transform.position).normalized;
        
        float rollTime = rollDistance / rollSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < rollTime)
        {
            rb.velocity = new Vector2(rollDirection.x * rollSpeed, rb.velocity.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rb.velocity = Vector2.zero;
        isRolling = false;
        SetState(FinalBossMoveState.Idle);
        actionTimer = actionCooldown;
    }

    private IEnumerator PerformRollAway()
    {
        Debug.Log("Final Boss performing roll away");
        
        isRolling = true;
        SetState(FinalBossMoveState.Roll);
        
        Vector2 rollDirection = (transform.position - player.position).normalized;
        
        float rollTime = rollAwayDistance / rollSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < rollTime)
        {
            rb.velocity = new Vector2(rollDirection.x * rollSpeed, rb.velocity.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rb.velocity = Vector2.zero;
        isRolling = false;
        SetState(FinalBossMoveState.Idle);
        actionTimer = actionCooldown;
    }

    // 外围区域的蹲下魔法攻击
    private IEnumerator PerformCrouchMagicAttack()
    {
        isAttacking = true;
        isCrouching = true;
        SetState(FinalBossMoveState.Crouch);
        rb.velocity = Vector2.zero;
        
        Debug.Log("Final Boss performing Crouch Magic Attack in Outer Zone");
        
        yield return new WaitForSeconds(crouchTime * 0.5f);
        
        if (attackController != null)
        {
            // 外围区域使用SmallSpark扇形攻击
            attackController.CastOuterZoneCrouchMagic();
            lastCrouchKickTime = Time.time; // 更新冷却时间
        }
        
        yield return new WaitForSeconds(crouchTime * 0.5f);
        
        isCrouching = false;
        isAttacking = false;
        actionTimer = actionCooldown;
    }
    
    // 中距离区域的翻滚接近并蹲踢攻击
    private IEnumerator PerformRollToCrouchKick()
    {
        Debug.Log("Final Boss performing Roll to CrouchKick in Mid Zone");
        
        // 先翻滚接近
        isRolling = true;
        SetState(FinalBossMoveState.Roll);
        
        Vector2 rollDirection = (player.position - transform.position).normalized;
        
        float rollTime = rollTowardsDistance / rollSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < rollTime)
        {
            rb.velocity = new Vector2(rollDirection.x * rollSpeed, rb.velocity.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rb.velocity = Vector2.zero;
        isRolling = false;
        
        // 立即进行蹲踢攻击
        yield return StartCoroutine(PerformCrouchKickAttack());
    }
    
    // 内围区域的翻滚远离并释放魔法攻击
    private IEnumerator PerformRollAwayWithMagic()
    {
        Debug.Log("Final Boss performing Roll Away with Magic in Inner Zone");
        
        // 先翻滚远离
        isRolling = true;
        SetState(FinalBossMoveState.Roll);
        
        Vector2 rollDirection = (transform.position - player.position).normalized;
        
        float rollTime = rollAwayDistance / rollSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < rollTime)
        {
            rb.velocity = new Vector2(rollDirection.x * rollSpeed, rb.velocity.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rb.velocity = Vector2.zero;
        isRolling = false;
        
        // 翻滚完成后不再释放魔法攻击 - 只在冥想状态下释放魔法
        Debug.Log("Roll Away completed - no longer casting magic after roll");
        
        // 重置追击计时器
        meleeChaseStartTime = Time.time;
    }

    private void UpdateAnimationState()
    {
        if (anim != null && rb != null)
        {
            // 调试信息
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"Final Boss Animation State - BossMoveState: {anim.GetInteger("BossMoveState")}, " +
                         $"Current State: {currentState}, Player Detected: {isPlayerDetected}");
            }
        }
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }
    
    private void SetState(FinalBossMoveState newState)
    {
        // 在Meditation状态期间不要更新正常的移动状态
        if (isMeditating && newState != FinalBossMoveState.Idle)
        {
            Debug.Log($"Final Boss: Blocked state change to {newState} during Meditation");
            return;
        }
        
        if (currentState != newState)
        {
            currentState = newState;
            
            if (anim != null && !isMeditating) 
            {
                switch (newState)
                {
                    case FinalBossMoveState.Idle:
                        anim.SetInteger("BossMoveState", 0); // Idle
                        break;
                    case FinalBossMoveState.Run:
                        anim.SetInteger("BossMoveState", 1); // Run
                        break;
                    case FinalBossMoveState.Roll:
                        anim.SetInteger("BossMoveState", 2); // Roll
                        break;
                    case FinalBossMoveState.Crouch:
                        anim.SetInteger("BossMoveState", 3); // Crouch
                        break;
                }
            }
        }
    }

    // 受伤处理方法
    public void TakeHurt()
    {
        if (bossLife != null && bossLife.IsDead) return;
        if (isInvulnerable || isMeditating) return; // 冥想状态或无敌状态不接受伤害
        
        isHurt = true;
        
        // 播放受伤动画 - 使用你的Animator Controller参数
        if (anim != null)
        {
            anim.SetTrigger("isHurt");
            Debug.Log("Final Boss hurt animation triggered - isHurt trigger activated");
        }
        
        // 触发伤害闪烁效果
        StartCoroutine(DamageFlashEffect());
        
        // 开始无敌序列
        StartCoroutine(InvulnerabilitySequence());
        
        // 受伤状态持续一小段时间后恢复
        StartCoroutine(HurtRecovery());
    }
    
    private IEnumerator HurtRecovery()
    {
        yield return new WaitForSeconds(0.5f); // 受伤状态持续时间
        isHurt = false;
        
        Debug.Log("Final Boss hurt recovery completed");
    }

    // 无敌状态管理
    private IEnumerator InvulnerabilitySequence()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    // 伤害闪烁效果
    private IEnumerator DamageFlashEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    // 冥想状态触发检查
    private void HandleMeditationTrigger()
    {
        if (bossLife == null || bossLife.IsDead || isInvulnerable || isMeditating)
            return;

        float healthPercentage = bossLife.GetHealthPercentage();
        
        // 检查每个阶段是否需要触发Meditation
        for (int i = 0; i < meditationThresholds.Length; i++)
        {
            if (healthPercentage <= meditationThresholds[i] && !meditationTriggered[i])
            {
                meditationTriggered[i] = true;
                TriggerMeditation(i);
                Debug.Log($"Final Boss triggered Meditation at {meditationThresholds[i] * 100}% health");
                break;
            }
        }
    }

    // 触发冥想状态
    private void TriggerMeditation(int stage)
    {
        if (isMeditating) return;
        
        // 调整不同阶段的Boss属性或行为
        switch (stage)
        {
            case 0:
                // 75% health - First meditation stage
                Debug.Log("Final Boss entered first meditation stage.");
                break;
            case 1:
                // 50% health - Second meditation stage
                Debug.Log("Final Boss entered second meditation stage.");
                break;
            case 2:
                // 25% health - Third meditation stage
                Debug.Log("Final Boss entered third meditation stage.");
                break;
        }

        // 开始冥想序列
        StartCoroutine(MeditationRoutine(stage));
    }

    // 冥想状态序列
    private IEnumerator MeditationRoutine(int stage)
    {
        isMeditating = true;
        
        Debug.Log($"Final Boss entering Meditation state (Stage {stage})");
        
        // 触发Meditation动画
        if (anim != null)
        {
            anim.SetTrigger("isMeditation");
        }
        
        // 进入冥想状态
        EnterMeditationState();
        
        // 等待Meditation动画完成
        yield return new WaitForSeconds(meditationDuration);
        
        // 冥想结束，恢复正常状态
        isMeditating = false;
        
        // 退出冥想状态
        ExitMeditationState();
        
        Debug.Log($"Final Boss meditation ended (Stage {stage})");
    }

    // Meditation状态控制方法
    public void EnterMeditationState()
    {
        isMeditating = true;
        rb.velocity = Vector2.zero; // 停止移动
        SetState(FinalBossMoveState.Idle);
        
        // 在Meditation状态下开始魔法攻击
        if (attackController != null)
        {
            attackController.StartMeditationMagicAttacks();
        }
        
        Debug.Log("Final Boss entered Meditation state");
    }
    
    public void ExitMeditationState()
    {
        isMeditating = false;
        
        // 停止魔法攻击
        if (attackController != null)
        {
            attackController.StopMeditationMagicAttacks();
        }
        
        // 恢复正常的动画状态
        if (anim != null)
        {
            anim.SetInteger("BossMoveState", 0); // 恢复到Idle状态
            Debug.Log("Final Boss: Restored BossMoveState to Idle after Meditation");
        }
        
        Debug.Log("Final Boss exited Meditation state");
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        // 绘制三个检测区域
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, outerDetectionRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, midDetectionRange);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, innerDetectionRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 绘制到玩家的线
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, player.position);
    }

    // 调试帮助方法
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // 如果正在翻滚，绘制翻滚轨迹
        if (isRolling && player != null)
        {
            Vector2 rollDirection = (transform.position - player.position).normalized;
            if (rollDirection.magnitude < 0.1f)
            {
                rollDirection = isFacingRight ? Vector2.left : Vector2.right;
            }
            
            // 绘制翻滚方向
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, rollDirection * 3f);
            
            // 绘制当前速度方向
            Gizmos.color = Color.cyan;
            if (rb != null)
            {
                Gizmos.DrawRay(transform.position, rb.velocity.normalized * 2f);
            }
        }
    }
}
