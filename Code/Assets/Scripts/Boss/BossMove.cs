using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BossMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rollSpeed = 7f;
    [SerializeField] private float rollDistance = 4f;

    [Header("Three-Zone Detection System")]
    [SerializeField] private float outerDetectionRange = 20f; // 外围检测圆：魔法压制区域
    [SerializeField] private float midDetectionRange = 10f;   // 中距离检测圆：突进区域
    [SerializeField] private float innerDetectionRange = 4f;  // 内围检测圆：近战追击区域
    [SerializeField] private float attackRange = 2.5f;        // 实际攻击范围
    [SerializeField] private bool startInCombatMode = false;  // 是否开始就进入战斗模式
    [SerializeField] private float loseTargetDelay = 2f;      // 失去目标后多久停止追踪

    [Header("AI Behavior")]
    [SerializeField] private float idleTime = 0.5f; // 减少idle时间
    [SerializeField] private float actionCooldown = 0.3f; // 减少行动冷却
    [SerializeField] private float crouchTime = 0.5f; // 减少蹲下时间
    
    [Header("Three-Phase Tactical Settings")]
    [SerializeField] private float magicSuppressionCooldown = 3f;    // 外围魔法压制冷却时间
    [SerializeField] private float meleeChaseTimeout = 3f;          // 内围近战追击超时时间
    [SerializeField] private float rollAwayDistance = 6f;           // 翻滚远离距离
    [SerializeField] private float rollTowardsDistance = 4f;        // 翻滚突进距离
    
    [Header("Magic Attack Settings")]
    [SerializeField] private float magicAttackCooldown = 2f; // 魔法攻击冷却时间
    [SerializeField] private float magicAttackMinDistance = 3f; // 魔法攻击最小距离
    [SerializeField] private float magicAttackMaxDistance = 12f; // 魔法攻击最大距离

    [Header("Distance-Based Probabilities")]
    [Range(0, 100)]
    [SerializeField] private int farRollChance = 40;
    [Range(0, 100)]
    [SerializeField] private int farMagicChance = 60;
    [Range(0, 100)]
    [SerializeField] private int midRangeRunChance = 70;
    [Range(0, 100)]
    [SerializeField] private int midRangeMagicChance = 30;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Animator anim;
    private BossAttack attackController;
    private BossLife bossLife;

    // Target
    private Transform player;

    // State
    private BossMovementState currentState = BossMovementState.Idle;
    private bool isFacingRight = true;
    private float actionTimer = 0f;
    private float stateTimer = 0f;
    private bool isGrounded = true;

    // State flags
    private bool isAttacking = false;
    private bool isHurt = false;
    private bool isCrouching = false;
    private bool isCastingMagic = false;
    private bool isRolling = false;
    
    // Three-phase tactical state tracking
    private ThreePhaseTacticalState currentTacticalPhase = ThreePhaseTacticalState.OutOfRange;
    private float lastMagicAttackTime = 0f;           // 上次魔法攻击时间
    private float meleeChaseStartTime = 0f;           // 近战追击开始时间
    private float lastMagicSuppressionTime = 0f;      // 上次魔法压制时间
    private float outerZoneEntryTime = 0f;            // 进入外围区域的时间
    private bool isInOuterZone = false;               // 是否在外围压制区域
    private bool isInMidZone = false;                 // 是否在中距离突进区域
    private bool isInInnerZone = false;               // 是否在内围追击区域
    private bool playerInDetectionRange = false;      // 玩家是否在任意检测范围内
    private bool isPlayerDetected = false;            // 是否已侦测到玩家
    private float loseTargetTimer = 0f;               // 失去目标计时器

    private enum BossMovementState
    {
        Idle,
        Run,
        Roll,
        Crouch,
        CastMagic
    }

    public enum ThreePhaseTacticalState
    {
        OutOfRange,      // 超出检测范围
        OuterZone,       // 外围魔法压制区域 (20m内)
        MidZone,         // 中距离突进区域 (10m内)
        InnerZone        // 内围近战追击区域 (4m内)
    }

    // Properties
    public bool IsFacingRight => isFacingRight;
    public bool IsAttacking => isAttacking;
    public bool IsHurt => isHurt;
    public bool IsCrouching => isCrouching;
    public bool IsCastingMagic => isCastingMagic;
    public bool IsRolling => isRolling;
    public bool IsPlayerDetected => isPlayerDetected; // 是否侦测到玩家
    public Transform Player => player;
    public float GetDistanceToPlayer() => player ? Vector2.Distance(transform.position, player.position) : float.MaxValue;
    
    // Three-Phase Tactical System Properties
    public ThreePhaseTacticalState CurrentTacticalPhase => currentTacticalPhase;
    public bool IsInOuterZone => isInOuterZone;
    public bool IsInMidZone => isInMidZone;
    public bool IsInInnerZone => isInInnerZone;
    public float TimeSinceLastMagicAttack => Time.time - lastMagicAttackTime;
    public float MeleeChaseElapsedTime => currentTacticalPhase == ThreePhaseTacticalState.InnerZone ? Time.time - meleeChaseStartTime : 0f;

    void Start()
    {
        InitializeComponents();
        FindPlayer();
        SetupGroundCheck();
        
        // 根据设置决定是否开始就进入战斗模式
        isPlayerDetected = startInCombatMode;
        if (startInCombatMode)
        {
            Debug.Log("Boss starting in combat mode");
        }
        else
        {
            Debug.Log("Boss starting in patrol mode - waiting for player detection");
        }
    }

    void Update()
    {
        if (bossLife != null && bossLife.IsDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        UpdateTimers();
        CheckGrounded();
        
        if (player != null)
        {
            UpdateFacingDirection();
            HandleAI();
        }

        UpdateAnimationState();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        attackController = GetComponent<BossAttack>();
        bossLife = GetComponent<BossLife>();
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
            Debug.LogWarning("Boss: No Player found with 'Player' tag!");
        }
    }

    private void SetupGroundCheck()
    {
        if (groundCheck == null)
        {
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(transform);
            groundCheck.localPosition = new Vector3(0, -boxCollider.bounds.extents.y - 0.1f, 0);
        }
    }

    private void UpdateTimers()
    {
        if (actionTimer > 0) actionTimer -= Time.deltaTime;
        if (stateTimer > 0) stateTimer -= Time.deltaTime;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void UpdateFacingDirection()
    {
        // 只有在侦测到玩家时才调整朝向
        if (player == null || isAttacking || isRolling || !isPlayerDetected) return;

        bool shouldFaceRight = player.position.x > transform.position.x;
        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }

    private void HandleAI()
    {
        // 调试信息
        if (Time.frameCount % 60 == 0) // 每秒打印一次
        {
            Debug.Log($"Boss AI State - Hurt: {isHurt}, Attacking: {isAttacking}, Crouching: {isCrouching}, Magic: {isCastingMagic}, Rolling: {isRolling}, ActionTimer: {actionTimer:F2}");
        }

        // 如果处于特殊状态，不进行AI决策
        if (isHurt || isAttacking || isCrouching || isCastingMagic)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // 如果正在翻滚，不要重置速度，让翻滚继续，也不要进行其他AI决策
        if (isRolling)
        {
            return;
        }

        // 检查玩家距离并更新战术阶段
        float distanceToPlayer = GetDistanceToPlayer();
        UpdateTacticalPhase(distanceToPlayer);
        
        // 处理侦测状态变化
        HandlePlayerDetection(distanceToPlayer);
        
        // 如果没有侦测到玩家，Boss保持idle状态
        if (!isPlayerDetected)
        {
            SetState(BossMovementState.Idle);
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // 如果冷却时间未结束，保持当前状态
        if (actionTimer > 0)
        {
            if (currentState == BossMovementState.Idle)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            return;
        }

        if (Time.frameCount % 120 == 0) // 每两秒打印一次
        {
            Debug.Log($"Boss Three-Phase Tactics - Distance: {distanceToPlayer:F2}, Phase: {currentTacticalPhase}, Last Magic: {Time.time - lastMagicAttackTime:F2}s ago");
        }
        
        // 执行三阶段战术决策
        ExecuteThreePhaseTactics(distanceToPlayer);
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        float direction = player.position.x > transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
    }

    private IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        // 触发攻击动画 - 使用你的Animator Controller参数
        if (anim != null)
        {
            anim.SetBool("isAttack", true);
            Debug.Log("Boss attack animation triggered - isAttack set to true");
        }

        // 执行攻击
        if (attackController != null)
            attackController.PerformMeleeAttack();

        yield return new WaitForSeconds(0.5f); // 进一步减少攻击持续时间

        isAttacking = false;
        
        // 停止攻击动画
        if (anim != null)
        {
            anim.SetBool("isAttack", false);
            Debug.Log("Boss attack animation completed - isAttack set to false");
        }
        
        SetState(BossMovementState.Idle);
        actionTimer = actionCooldown * 0.3f; // 进一步减少攻击后的冷却时间
    }

    private IEnumerator PerformRoll()
    {
        isRolling = true;
        SetState(BossMovementState.Roll);

        float rollDirection = isFacingRight ? 1f : -1f;
        float currentRollSpeed = this.rollSpeed;
        
        // 检查前方是否有障碍物，如果有则降低速度
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * rollDirection, rollDistance, groundLayer);
        if (hit.collider != null)
        {
            currentRollSpeed *= 0.5f; // 减速
        }

        // 确保Rigidbody2D是Dynamic类型
        if (rb != null)
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                Debug.LogWarning("Boss Rigidbody2D should be set to Dynamic for rolling to work!");
                rb.bodyType = RigidbodyType2D.Dynamic; // 自动设置为Dynamic
            }
            
            // 设置翻滚速度 - 不重置y轴速度，保持重力效果
            rb.velocity = new Vector2(rollDirection * currentRollSpeed, rb.velocity.y);
            Debug.Log($"Boss rolling with velocity: {rb.velocity}");
        }

        // 持续翻滚一段时间
        float rollTime = 0.4f; // 减少翻滚时间
        float elapsedTime = 0f;
        
        while (elapsedTime < rollTime && isRolling)
        {
            // 在翻滚过程中保持速度
            if (rb != null)
            {
                rb.velocity = new Vector2(rollDirection * currentRollSpeed, rb.velocity.y);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 翻滚结束
        isRolling = false;
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        SetState(BossMovementState.Idle);
        actionTimer = actionCooldown * 0.3f; // 减少翻滚后的冷却时间
    }

    private IEnumerator PerformCrouchAndMagic()
    {
        // 蹲下阶段
        isCrouching = true;
        SetState(BossMovementState.Crouch);
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(crouchTime);

        // 魔法攻击阶段
        isCrouching = false;
        isCastingMagic = true;
        SetState(BossMovementState.CastMagic);

        // 使用带冷却的魔法攻击
        PerformMagicAttackWithCooldown();

        yield return new WaitForSeconds(1.0f); // 减少魔法攻击持续时间

        isCastingMagic = false;
        SetState(BossMovementState.Idle);
        actionTimer = actionCooldown * 1.5f; // 减少魔法攻击后的冷却时间
    }

    private void UpdateAnimationState()
    {
        if (anim != null && rb != null)
        {
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"Boss Animation State - BossMoveState: {anim.GetInteger("BossMoveState")}, " +
                         $"isAttack: {anim.GetBool("isAttack")}, " +
                         $"isHurt: {anim.GetBool("isHurt")}, " +
                         $"Current State: {currentState}, " +
                         $"Player Detected: {isPlayerDetected}");
            }
        }
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
        
        // 如果有攻击控制器，通知它朝向发生了变化
        if (attackController != null)
        {
            // 可以在这里添加朝向变化的通知逻辑
        }
    }
    
    private void SetState(BossMovementState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            
            // 根据状态设置动画 - 使用你的Animator Controller参数
            if (anim != null)
            {
                switch (newState)
                {
                    case BossMovementState.Idle:
                        anim.SetInteger("BossMoveState", 0); // Idle
                        Debug.Log("Boss animation: Setting BossMoveState to 0 (Idle)");
                        break;
                    case BossMovementState.Run:
                        anim.SetInteger("BossMoveState", 1); // Run
                        Debug.Log("Boss animation: Setting BossMoveState to 1 (Run)");
                        break;
                    case BossMovementState.Roll:
                        anim.SetInteger("BossMoveState", 2); // Roll
                        Debug.Log("Boss animation: Setting BossMoveState to 2 (Roll)");
                        break;
                    case BossMovementState.Crouch:
                        anim.SetInteger("BossMoveState", 3); // Crouch
                        Debug.Log("Boss animation: Setting BossMoveState to 3 (Crouch)");
                        break;
                    case BossMovementState.CastMagic:
                        anim.SetInteger("BossMoveState", 3); // 魔法施法使用Crouch状态
                        Debug.Log("Boss animation: Setting BossMoveState to 3 (CastMagic as Crouch)");
                        break;
                }
                
                Debug.Log($"Boss state changed to: {newState}");
            }
        }
    }

    // 检查是否可以进行魔法攻击
    private bool CanPerformMagicAttack()
    {
        float timeSinceLastMagic = Time.time - lastMagicAttackTime;
        float distanceToPlayer = GetDistanceToPlayer();
        
        // 检查冷却时间、距离范围和玩家是否在侦测范围内
        bool cooldownReady = timeSinceLastMagic >= magicAttackCooldown;
        bool inRange = distanceToPlayer >= magicAttackMinDistance && distanceToPlayer <= magicAttackMaxDistance;
        bool playerInDetectionRange = distanceToPlayer <= outerDetectionRange;
        
        return cooldownReady && inRange && playerInDetectionRange;
    }
    
    // 执行魔法攻击并更新冷却时间
    private void PerformMagicAttackWithCooldown()
    {
        if (CanPerformMagicAttack() && attackController != null)
        {
            attackController.PerformMagicAttack();
            lastMagicAttackTime = Time.time;
            Debug.Log($"Boss performed magic attack. Next magic attack available in {magicAttackCooldown} seconds.");
        }
        else
        {
            Debug.Log("Magic attack on cooldown or player out of range. Moving towards player instead.");
            MoveTowardsPlayer();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制三个同心检测圆圈 - 根据是否侦测到玩家改变颜色
        Gizmos.color = isPlayerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, outerDetectionRange);

        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        Gizmos.DrawWireSphere(transform.position, midDetectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, innerDetectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 绘制地面检测
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // 如果侦测到玩家，绘制连线
        if (isPlayerDetected && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, player.position);
        }
        
        // 显示当前战术阶段
        if (Application.isPlaying)
        {
#if UNITY_EDITOR
            Handles.Label(transform.position + Vector3.up * 2f, $"Phase: {currentTacticalPhase}");
#endif
        }
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

    // 受伤处理方法
    public void TakeHurt()
    {
        if (bossLife != null && bossLife.IsDead) return;
        
        isHurt = true;
        
        // 播放受伤动画 - 使用你的Animator Controller参数
        if (anim != null)
        {
            anim.SetBool("isHurt", true);
            Debug.Log("Boss hurt animation triggered - isHurt set to true");
        }
        
        // 受伤状态持续一小段时间后恢复
        StartCoroutine(HurtRecovery());
    }
    
    private IEnumerator HurtRecovery()
    {
        yield return new WaitForSeconds(0.5f); // 受伤状态持续时间
        isHurt = false;
        
        // 恢复受伤动画状态
        if (anim != null)
        {
            anim.SetBool("isHurt", false);
            Debug.Log("Boss hurt recovery completed - isHurt set to false");
        }
    }

    // 三阶段战术系统核心方法
    private void UpdateTacticalPhase(float distanceToPlayer)
    {
        ThreePhaseTacticalState previousPhase = currentTacticalPhase;
        
        // 根据距离更新当前战术阶段
        if (distanceToPlayer > outerDetectionRange)
        {
            currentTacticalPhase = ThreePhaseTacticalState.OutOfRange;
            isInOuterZone = false;
            isInMidZone = false;
            isInInnerZone = false;
        }
        else if (distanceToPlayer <= innerDetectionRange)
        {
            currentTacticalPhase = ThreePhaseTacticalState.InnerZone;
            isInOuterZone = true;
            isInMidZone = true;
            isInInnerZone = true;
            
            // 进入内围时开始计时近战追击
            if (previousPhase != ThreePhaseTacticalState.InnerZone)
            {
                meleeChaseStartTime = Time.time;
                Debug.Log("Boss entered Inner Zone - Starting melee chase timer");
            }
        }
        else if (distanceToPlayer <= midDetectionRange)
        {
            currentTacticalPhase = ThreePhaseTacticalState.MidZone;
            isInOuterZone = true;
            isInMidZone = true;
            isInInnerZone = false;
        }
        else if (distanceToPlayer <= outerDetectionRange)
        {
            currentTacticalPhase = ThreePhaseTacticalState.OuterZone;
            isInOuterZone = true;
            isInMidZone = false;
            isInInnerZone = false;
            
            // 首次进入外围区域时记录时间
            if (previousPhase != ThreePhaseTacticalState.OuterZone)
            {
                outerZoneEntryTime = Time.time;
                Debug.Log("Boss entered Outer Zone - Starting magic suppression timer");
            }
        }
        
        // 阶段变化时的调试日志
        if (previousPhase != currentTacticalPhase)
        {
            Debug.Log($"Boss tactical phase changed from {previousPhase} to {currentTacticalPhase} at distance {distanceToPlayer:F2}");
        }
    }
    
    private void HandlePlayerDetection(float distanceToPlayer)
    {
        // 检查玩家是否在任意检测范围内
        playerInDetectionRange = distanceToPlayer <= outerDetectionRange;
        
        // 处理侦测状态变化
        if (playerInDetectionRange)
        {
            // 玩家在侦测范围内
            loseTargetTimer = 0f; // 重置失去目标计时器
            
            if (!isPlayerDetected)
            {
                Debug.Log("Player detected! Boss entering combat mode.");
                isPlayerDetected = true;
            }
        }
        else
        {
            // 玩家不在侦测范围内
            if (isPlayerDetected)
            {
                // 开始计时失去目标
                loseTargetTimer += Time.deltaTime;
                
                if (loseTargetTimer >= loseTargetDelay)
                {
                    Debug.Log("Player lost! Boss returning to patrol mode.");
                    isPlayerDetected = false;
                    // 重置AI状态
                    meleeChaseStartTime = 0f;
                    loseTargetTimer = 0f;
                }
            }
        }
    }
    
    private void ExecuteThreePhaseTactics(float distanceToPlayer)
    {
        switch (currentTacticalPhase)
        {
            case ThreePhaseTacticalState.OuterZone:
                ExecuteOuterZoneTactics(distanceToPlayer);
                break;
            case ThreePhaseTacticalState.MidZone:
                ExecuteMidZoneTactics(distanceToPlayer);
                break;
            case ThreePhaseTacticalState.InnerZone:
                ExecuteInnerZoneTactics(distanceToPlayer);
                break;
            case ThreePhaseTacticalState.OutOfRange:
                // 超出范围，保持idle
                SetState(BossMovementState.Idle);
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }
    
    // 外围区域战术：魔法压制
    private void ExecuteOuterZoneTactics(float distanceToPlayer)
    {
        Debug.Log("Executing Outer Zone Tactics - Magic Suppression");
        
        // 停止移动并面向Hero
        rb.velocity = new Vector2(0, rb.velocity.y);
        SetState(BossMovementState.Idle);
        
        // 周期性释放魔法进行远程压制
        float timeSinceLastSuppression = Time.time - lastMagicSuppressionTime;
        if (timeSinceLastSuppression >= magicSuppressionCooldown)
        {
            Debug.Log("Boss performing magic suppression attack");
            StartCoroutine(PerformMagicSuppression());
            lastMagicSuppressionTime = Time.time;
        }
    }
    
    // 中距离区域战术：突进攻击
    private void ExecuteMidZoneTactics(float distanceToPlayer)
    {
        Debug.Log("Executing Mid Zone Tactics - Rush Attack");
        
        // 检查是否满足突进条件：在外围区域释放魔法攻击超过3秒
        float timeInOuterZone = Time.time - outerZoneEntryTime;
        if (timeInOuterZone >= 3f) // 在外围区域待了3秒以上
        {
            Debug.Log($"Rush condition met - Boss has been in outer zone for {timeInOuterZone:F2}s (threshold: 3s)");
            StartCoroutine(PerformRushAttack());
            // 重置外围区域计时器，避免连续突进
            outerZoneEntryTime = Time.time;
        }
        else
        {
            // 未满足突进条件，继续魔法压制
            Debug.Log($"Rush condition not met - Boss has been in outer zone for {timeInOuterZone:F2}s (need 3s)");
            ExecuteOuterZoneTactics(distanceToPlayer);
        }
    }
    
    // 内围区域战术：近战追击
    private void ExecuteInnerZoneTactics(float distanceToPlayer)
    {
        Debug.Log("Executing Inner Zone Tactics - Melee Chase");
        
        // 如果正在翻滚，不要进行任何其他操作
        if (isRolling)
        {
            Debug.Log("Boss is rolling, skipping inner zone tactics");
            return;
        }
        
        // 检查近战追击是否超时
        float meleeChaseElapsed = Time.time - meleeChaseStartTime;
        if (meleeChaseElapsed >= meleeChaseTimeout)
        {
            Debug.Log($"Melee chase timeout ({meleeChaseElapsed:F2}s >= {meleeChaseTimeout}s) - Rolling away for magic attack");
            // 确保不会重复触发翻滚远离
            if (!isRolling && !isAttacking && !isCastingMagic)
            {
                StartCoroutine(PerformRollAwayAndMagic());
            }
            return;
        }
        
        // 在攻击范围内：直接攻击
        if (distanceToPlayer <= attackRange)
        {
            StartCoroutine(PerformMeleeAttack());
        }
        else
        {
            // 主动追击Hero
            SetState(BossMovementState.Run);
            MoveTowardsPlayer();
        }
    }

    // 三阶段战术专用协程方法
    private IEnumerator PerformMagicSuppression()
    {
        Debug.Log("Boss performing magic suppression");
        
        // 蹲下准备魔法
        isCrouching = true;
        SetState(BossMovementState.Crouch);
        rb.velocity = Vector2.zero;
        
        yield return new WaitForSeconds(crouchTime * 0.5f); // 减少蹲下时间
        
        // 释放魔法攻击
        isCrouching = false;
        isCastingMagic = true;
        SetState(BossMovementState.CastMagic);
        
        if (attackController != null)
        {
            attackController.PerformMagicAttack();
            lastMagicAttackTime = Time.time; // 更新最后魔法攻击时间
        }
        
        yield return new WaitForSeconds(1.0f);
        
        isCastingMagic = false;
        SetState(BossMovementState.Idle);
        actionTimer = actionCooldown;
    }
    
    private IEnumerator PerformRushAttack()
    {
        Debug.Log("Boss performing rush attack");
        
        // 翻滚突进到Hero附近
        isRolling = true;
        SetState(BossMovementState.Roll);
        
        Vector2 rushDirection = (player.position - transform.position).normalized;
        
        // 确保Rigidbody2D是Dynamic类型
        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            Debug.LogWarning("Boss Rigidbody2D should be set to Dynamic for rolling to work!");
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // 执行翻滚突进
        float rushSpeed = rollSpeed * 1.2f; // 突进时速度更快
        float rushTime = 0.6f;
        float elapsedTime = 0f;
        
        Debug.Log($"Boss rush attack: direction={rushDirection}, speed={rushSpeed}");
        
        while (elapsedTime < rushTime && isRolling)
        {
            // 设置翻滚速度，保持y轴重力
            rb.velocity = new Vector2(rushDirection.x * rushSpeed, rb.velocity.y);
            Debug.Log($"Boss rushing with velocity: {rb.velocity}");
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        isRolling = false;
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        // 突进完成后立即尝试近战攻击
        float distanceAfterRush = GetDistanceToPlayer();
        if (distanceAfterRush <= attackRange)
        {
            yield return StartCoroutine(PerformMeleeAttack());
        }
        else
        {
            SetState(BossMovementState.Idle);
            actionTimer = actionCooldown * 0.5f;
        }
    }
    
    private IEnumerator PerformRollAwayAndMagic()
    {
        Debug.Log("Boss rolling away from melee chase and preparing magic attack");
        
        // 翻滚远离Hero
        isRolling = true;
        SetState(BossMovementState.Roll);
        
        // 计算远离方向，确保不是零向量
        Vector2 rollAwayDirection = (transform.position - player.position).normalized;
        
        // 如果方向计算有问题，使用当前朝向的反方向
        if (rollAwayDirection.magnitude < 0.1f)
        {
            rollAwayDirection = isFacingRight ? Vector2.left : Vector2.right;
            Debug.LogWarning("Boss roll away direction was too small, using facing direction");
        }
        
        // 确保Rigidbody2D设置正确
        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            Debug.LogWarning("Boss Rigidbody2D should be set to Dynamic for rolling to work!");
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // 清除任何可能的约束
        rb.freezeRotation = true; // 只约束旋转，不约束位置
        
        float rollTime = 0.5f;
        float elapsedTime = 0f;
        float rollSpeed = this.rollSpeed; // 使用明确的变量名
        
        Debug.Log($"Boss roll away: direction={rollAwayDirection}, speed={rollSpeed}, player pos={player.position}, boss pos={transform.position}");
        
        while (elapsedTime < rollTime && isRolling)
        {
            // 强制设置速度，确保翻滚效果
            Vector2 rollVelocity = new Vector2(rollAwayDirection.x * rollSpeed, rb.velocity.y);
            rb.velocity = rollVelocity;
            
            Debug.Log($"Boss rolling away - elapsed: {elapsedTime:F2}s, velocity: {rb.velocity}, position: {transform.position}");
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        isRolling = false;
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        Debug.Log($"Boss roll away completed. Final position: {transform.position}");
        
        // 重置近战追击计时器
        meleeChaseStartTime = Time.time;
        
        // 短暂停顿后进行魔法攻击
        yield return new WaitForSeconds(0.3f);
        
        // 执行魔法攻击
        yield return StartCoroutine(PerformMagicSuppression());
    }
}
