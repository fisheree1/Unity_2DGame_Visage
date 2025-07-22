using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum BossSlimeStateType
{
    Idle,
    Hurt,
    MeleeAttack,
    RangedAttack,
    Death
}

[System.Serializable]
public class BossSlimeParameter
{
    [Header("基础设置")]
    public float maxHealth = 300f;
    public float currentHealth;
    public Animator animator;
    public Transform target;
    public LayerMask targetLayer;
    public float sightRange = 15f;
    public float meleeAttackRange = 3f;
    public float rangedAttackRange = 10f;
    public bool isHit = false;
    
    [Header("攻击设置")]
    public int meleeDamage = 25;
    public int rangedDamage = 15;
    public float meleeAttackCooldown = 2f;
    public float rangedAttackCooldown = 1.5f;
    public Transform meleeAttackPoint;
    public Transform[] rangedAttackPoints;
    public float meleeAttackRadius = 2f;
    
    [Header("攻击模式切换系统")]
    public bool enableAttackModeSwitch = true; // 启用攻击模式切换
    public bool forceRangedAfterMelee = false; // 近战后强制远程攻击标志
    public bool lastAttackWasMelee = false; // 上次攻击是否为近战
    public float attackModeSwitchCooldown = 0.5f; // 攻击模式切换冷却时间
    
    [Header("弹幕设置")]
    public GameObject trackingProjectilePrefab;
    public GameObject fanProjectilePrefab;
    public GameObject barrageProjectilePrefab;
    public float projectileSpeed = 8f;
    public float projectileLifetime = 5f;
    
    [Header("阶段系统")]
    public float phase2HealthThreshold = 0.6f; // 60% 血量阈值
    public float phase3HealthThreshold = 0.4f; // 40% 血量阈值
    public int phase3HitCounter = 0;
    public int phase3HitThreshold = 2;
    public bool phase3JumpAttackReady = false; // 标记第三阶段跳跃攻击准备状态
    public float jumpHeight = 15f; // 跳跃攻击高度
    public float jumpDuration = 0.5f;
    
    [Header("扇形攻击设置")]
    public int fanProjectileCount = 5;
    public float fanAngle = 60f;
    
    [Header("弹幕雨攻击设置")]
    public int barrageProjectileCount = 20;
    public float barrageSpawnRadius = 8f;
    public float barrageSpawnDelay = 0.1f;
    
    [Header("AI设置")]
    public float stateChangeDelay = 0.5f;
    public float idleTime = 2f;
    public float hurtRecoveryTime = 1f;
    
    [Header("跳跃弹幕攻击设置")]
    public float jumpForce = 20f; // 跳跃力度
    public float fallThreshold = 10f; // 判断开始下落的阈值
    public float barrageHeight = 10f; // 弹幕发射高度
    public bool debugJumpAttack = false; // 调试模式
    
    [Header("继承Boss设置")]
    public GameObject nextBossPrefab; // 下一个Boss的预制体（如BossDemon）
    public float nextBossHealthPercentage = 0.7f; // 下一个Boss的初始血量百分比
    public bool enableBossTransition = true; // 是否启用Boss切换
}

public class BossSlime : MonoBehaviour
{
    [Header("相机震动设置")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.5f;
    
    public BossSlimeParameter parameter;
    private IState currentState;
    private Dictionary<BossSlimeStateType, IState> states = new Dictionary<BossSlimeStateType, IState>();
    
    // 组件
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    private CinemachineImpulseSource impulseSource;
    private BossLife bossLife;
    private Enemy enemyComponent; // 与现有敌人系统兼容
    
    // 状态管理
    private float lastAttackTime;
    private bool isFacingRight = true;
    private bool isDead = false;
    
    // 阶段管理
    private int currentPhase = 1;
    private bool isInPhase2 = false;
    private bool isInPhase3 = false;
    
    // 跳跃弹幕攻击状态
    private bool isPerformingJumpAttack = false;
    private Vector3 jumpStartPosition;
    
    // 属性
    public bool IsDead => isDead;
    public bool IsFacingRight => isFacingRight;
    public float CurrentHealthPercentage => parameter.currentHealth / parameter.maxHealth;
    public int CurrentPhase => currentPhase;
    public bool IsInPhase2 => isInPhase2;
    public bool IsInPhase3 => isInPhase3;
    public bool IsPerformingJumpAttack => isPerformingJumpAttack;
    
    // 攻击模式切换系统属性
    public bool ShouldForceRangedAttack => parameter.enableAttackModeSwitch && parameter.forceRangedAfterMelee;
    public bool LastAttackWasMelee => parameter.lastAttackWasMelee;
    
    void Start()
    {
        InitializeComponents();
        InitializeParameters();
        InitializeStates();
        
        TransitionState(BossSlimeStateType.Idle);
    }
    
    void Update()
    {
        if (isDead) return;
        
        UpdatePhaseState();
        UpdateFacingDirection();
        currentState?.OnUpdate();
        
        // 调试跳跃攻击
        if (parameter.debugJumpAttack)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("调试键J被按下，触发跳跃弹幕攻击");
                parameter.phase3HitCounter = parameter.phase3HitThreshold;
                // 移除有问题的方法调用，添加跳跃攻击准备状态
                parameter.phase3JumpAttackReady = true;
            }
            
            // 添加纯跳跃测试
            if (Input.GetKeyDown(KeyCode.K))
            {
                Debug.Log("调试键K被按下，测试纯跳跃");
                StartCoroutine(TestJumpOnly());
            }
        }
        
        // 显示当前物理状态（调试模式下）
        if (parameter.debugJumpAttack && isPerformingJumpAttack)
        {
            Debug.Log($"[调试] 当前速度: {rb.velocity}, 位置: {transform.position}, 重力: {rb.gravityScale}");
        }
    }
    
    // 纯跳跃测试方法
    private IEnumerator TestJumpOnly()
    {
        Debug.Log("开始纯跳跃测试");
        
        Vector3 startPos = transform.position;
        
        // 清除速度
        rb.velocity = Vector2.zero;
        yield return new WaitForFixedUpdate();
        
        // 应用跳跃力
        rb.AddForce(Vector2.up * parameter.jumpForce, ForceMode2D.Impulse);
        Debug.Log($"应用跳跃力: {parameter.jumpForce}");
        
        // 监控跳跃过程
        float timer = 0f;
        while (timer < 5f)
        {
            timer += Time.deltaTime;
            Debug.Log($"跳跃测试 - 时间: {timer:F2}s, 高度: {transform.position.y:F2}, 速度: {rb.velocity.y:F2}");
            
            if (transform.position.y < startPos.y - 1f)
            {
                Debug.Log("跳跃测试完成，已落地");
                break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("纯跳跃测试结束");
    }
    
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        bossLife = GetComponent<BossLife>();
        enemyComponent = GetComponent<Enemy>();
        
        if (parameter == null)
            parameter = new BossSlimeParameter();
            
        parameter.animator = GetComponent<Animator>();
        
        // 设置物理
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 确保物理设置正确
        rb.freezeRotation = true;
        rb.gravityScale = 2f; // 设置合适的重力
        rb.drag = 0.5f; // 添加轻微阻力
        
        // 确保质量足够进行跳跃
        if (rb.mass < 1f)
        {
            rb.mass = 1f;
        }
        
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
        }
        
        // 设置Enemy组件以实现兼容性
        if (enemyComponent == null)
        {
            enemyComponent = gameObject.AddComponent<Enemy>();
        }
        
        // 设置脉冲源用于相机震动
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }
        
        // 设置BossLife组件
        if (bossLife == null)
        {
            bossLife = gameObject.AddComponent<BossLife>();
        }
        
        // 订阅血量事件
        if (bossLife != null)
        {
            bossLife.OnHealthChanged += OnHealthChanged;
            bossLife.OnDeath += OnDeath;
        }
        
        Debug.Log($"Boss物理设置完成 - 重力: {rb.gravityScale}, 阻力: {rb.drag}, 质量: {rb.mass}");
    }
    
    private void InitializeParameters()
    {
        parameter.currentHealth = parameter.maxHealth;
        
        // 寻找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            parameter.target = player.transform;
        }
        
        // 如果未分配则设置攻击点
        if (parameter.meleeAttackPoint == null)
        {
            GameObject meleePoint = new GameObject("MeleeAttackPoint");
            meleePoint.transform.SetParent(transform);
            meleePoint.transform.localPosition = new Vector3(2f, 0f, 0f);
            parameter.meleeAttackPoint = meleePoint.transform;
        }
        
        if (parameter.rangedAttackPoints == null || parameter.rangedAttackPoints.Length == 0)
        {
            List<Transform> rangedPoints = new List<Transform>();
            
            // 创建多个远程攻击点
            for (int i = 0; i < 3; i++)
            {
                GameObject rangedPoint = new GameObject($"RangedAttackPoint_{i}");
                rangedPoint.transform.SetParent(transform);
                rangedPoint.transform.localPosition = new Vector3(1.5f, 0.5f + i * 0.5f, 0f);
                rangedPoints.Add(rangedPoint.transform);
            }
            
            parameter.rangedAttackPoints = rangedPoints.ToArray();
        }
        
        parameter.targetLayer = LayerMask.GetMask("Player");
    }
    
    private void InitializeStates()
    {
        states.Add(BossSlimeStateType.Idle, new BossSlimeIdleState(this, parameter));
        states.Add(BossSlimeStateType.Hurt, new BossSlimeHurtState(this, parameter));
        states.Add(BossSlimeStateType.MeleeAttack, new BossSlimeMeleeAttackState(this, parameter));
        states.Add(BossSlimeStateType.RangedAttack, new BossSlimeRangedAttackState(this, parameter));
        states.Add(BossSlimeStateType.Death, new BossSlimeDeathState(this, parameter));
    }
    
    private void UpdatePhaseState()
    {
        float healthPercentage = CurrentHealthPercentage;
        
        // 检查阶段转换
        if (healthPercentage <= parameter.phase2HealthThreshold && !isInPhase2)
        {
            EnterPhase2();
        }
        
        if (healthPercentage <= parameter.phase3HealthThreshold && !isInPhase3)
        {
            EnterPhase3();
        }
    }
    
    private void EnterPhase2()
    {
        isInPhase2 = true;
        currentPhase = 2;
        Debug.Log("Boss史莱姆进入阶段2 - 启用多发扇形追踪弹幕");
        
        // 为阶段3重置受击计数器
        parameter.phase3HitCounter = 0;
    }
    
    private void EnterPhase3()
    {
        isInPhase3 = true;
        currentPhase = 3;
        Debug.Log("Boss史莱姆进入阶段3 - 启用3次受击后全屏弹幕");
    }
    
    private void UpdateFacingDirection()
    {
        if (parameter.target == null || isDead || isPerformingJumpAttack) return;
        
        Vector3 direction = parameter.target.position - transform.position;
        bool shouldFaceRight = direction.x > 0;
        
        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        
        // 翻转精灵渲染器 - 修复朝向逻辑
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight; // 朝右时不翻转(false)，朝左时翻转(true)
        }
        
        // 更新攻击点位置
        UpdateAttackPointsPosition();
    }
    
    private void UpdateAttackPointsPosition()
    {
        float direction = isFacingRight ? 1f : -1f;
        
        if (parameter.meleeAttackPoint != null)
        {
            Vector3 pos = parameter.meleeAttackPoint.localPosition;
            pos.x = Mathf.Abs(pos.x) * direction;
            parameter.meleeAttackPoint.localPosition = pos;
        }
        
        if (parameter.rangedAttackPoints != null)
        {
            foreach (var point in parameter.rangedAttackPoints)
            {
                if (point != null)
                {
                    Vector3 pos = point.localPosition;
                    pos.x = Mathf.Abs(pos.x) * direction;
                    point.localPosition = pos;
                }
            }
        }
    }
    
    public void TransitionState(BossSlimeStateType type)
    {
        if (isDead && type != BossSlimeStateType.Death) return;
        
        currentState?.OnExit();
        currentState = states[type];
        currentState?.OnEnter();
        
        Debug.Log($"Boss史莱姆转换到状态: {type}");
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead || isPerformingJumpAttack) return; // 跳跃攻击期间不受伤害
        
        parameter.isHit = true;
        
        Debug.Log($"=== TakeDamage调试 ===");
        Debug.Log($"[伤害系统] Boss受到 {damage} 点伤害，当前阶段: {currentPhase}, 血量百分比: {CurrentHealthPercentage:F2}");
        Debug.Log($"受伤前血量(parameter): {parameter.currentHealth}");
        Debug.Log($"BossLife组件: {(bossLife != null ? "存在" : "不存在")}");
        
        // 触发相机震动
        if (impulseSource != null)
        {
            CamaraShakeManager.Instance.CamaraShake(impulseSource);
        }
        
        // 通过BossLife组件应用伤害
        if (bossLife != null)
        {
            Debug.Log($"通过BossLife组件应用伤害: {damage}");
            bossLife.TakeDamage(damage);
        }
        else
        {
            // 备用伤害系统
            Debug.Log("使用备用伤害系统");
            parameter.currentHealth -= damage;
            parameter.currentHealth = Mathf.Max(0, parameter.currentHealth);
            
            Debug.Log($"受伤后血量(parameter): {parameter.currentHealth}");
            
            if (parameter.currentHealth <= 0)
            {
                Debug.Log("血量归零，触发死亡");
                Die();
                return;
            }
        }
        
        // 阶段3特殊机制 - 修改为设置跳跃攻击准备状态
        if (isInPhase3)
        {
            parameter.phase3HitCounter++;
            Debug.Log($"[阶段3机制] 受击计数器: {parameter.phase3HitCounter}/{parameter.phase3HitThreshold}");
            
            if (parameter.phase3HitCounter >= parameter.phase3HitThreshold)
            {
                Debug.Log("[阶段3机制] 达到触发条件，下次远程攻击将变为跳跃弹幕攻击");
                parameter.phase3HitCounter = 0;
                parameter.phase3JumpAttackReady = true; // 设置跳跃攻击准备状态
                // 不再直接触发跳跃攻击，而是等待下次远程攻击
            }
        }
        else
        {
            // 如果不在阶段3，显示为什么不触发
            Debug.Log($"[阶段3机制] 不在阶段3，当前阶段: {currentPhase}, 血量: {CurrentHealthPercentage:F2}, 阈值: {parameter.phase3HealthThreshold}");
        }
        
        // 转换到受伤状态
        Debug.Log("[伤害系统] 转换到受伤状态");
        TransitionState(BossSlimeStateType.Hurt);
    }
    
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"=== BossLife血量变化事件 ===");
        Debug.Log($"新血量: {currentHealth}/{maxHealth}");
        parameter.currentHealth = currentHealth;
        parameter.maxHealth = maxHealth;
    }
    
    private void OnDeath()
    {
        Debug.Log("=== BossLife死亡事件触发 ===");
        Die();
    }
    
    private void Die()
    {
        if (isDead) 
        {
            Debug.Log("Die()被调用，但Boss已经死亡，跳过");
            return;
        }
        
        Debug.Log("=== Die()方法调试 ===");
        Debug.Log($"Boss开始死亡流程");
        Debug.Log($"当前血量(parameter): {parameter.currentHealth}");
        Debug.Log($"BossLife血量: {(bossLife != null ? $"{bossLife.CurrentHealth}/{bossLife.MaxHealth}" : "N/A")}");
        Debug.Log($"转换配置 - enableBossTransition: {parameter.enableBossTransition}");
        Debug.Log($"即将转换到死亡状态");
        
        isDead = true;
        Debug.Log("Boss史莱姆死亡");
        
        // 停止所有移动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        Debug.Log("成功转换到死亡状态");
        TransitionState(BossSlimeStateType.Death);
    }
    
    public float GetDistanceToTarget()
    {
        if (parameter.target == null) return float.MaxValue;
        return Vector2.Distance(transform.position, parameter.target.position);
    }
    
    public bool IsTargetInRange(float range)
    {
        return GetDistanceToTarget() <= range;
    }
    
    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= parameter.meleeAttackCooldown;
    }
    
    public void SetLastAttackTime()
    {
        lastAttackTime = Time.time;
    }
    
    /// <summary>
    /// 记录攻击类型并管理攻击模式切换
    /// </summary>
    /// <param name="attackType">攻击类型</param>
    public void RecordAttackType(BossSlimeStateType attackType)
    {
        if (!parameter.enableAttackModeSwitch) return;
        
        if (attackType == BossSlimeStateType.MeleeAttack)
        {
            parameter.lastAttackWasMelee = true;
            parameter.forceRangedAfterMelee = true; // 近战后强制下次使用远程攻击
            Debug.Log("Boss记录了近战攻击，下次攻击将强制使用远程攻击");
        }
        else if (attackType == BossSlimeStateType.RangedAttack)
        {
            parameter.lastAttackWasMelee = false;
            parameter.forceRangedAfterMelee = false; // 远程攻击后重置强制标志
            Debug.Log("Boss记录了远程攻击，攻击模式标志已重置");
        }
        
        SetLastAttackTime();
    }
    
    /// <summary>
    /// 根据攻击模式切换系统决定下一个攻击类型
    /// </summary>
    /// <param name="playerDistance">玩家距离</param>
    /// <returns>推荐的攻击类型</returns>
    public BossSlimeStateType DetermineAttackType(float playerDistance)
    {
        if (!parameter.enableAttackModeSwitch)
        {
            // 如果未启用攻击模式切换，使用原有逻辑
            return DetermineAttackTypeByDistance(playerDistance);
        }
        
        // 如果需要强制远程攻击
        if (parameter.forceRangedAfterMelee)
        {
            Debug.Log("Boss强制执行远程攻击（因为上次是近战攻击）");
            return BossSlimeStateType.RangedAttack;
        }
        
        // 否则根据距离决定
        return DetermineAttackTypeByDistance(playerDistance);
    }
    
    /// <summary>
    /// 根据距离决定攻击类型（原有逻辑）
    /// </summary>
    /// <param name="playerDistance">玩家距离</param>
    /// <returns>攻击类型</returns>
    private BossSlimeStateType DetermineAttackTypeByDistance(float playerDistance)
    {
        if (playerDistance <= parameter.meleeAttackRange)
        {
            return BossSlimeStateType.MeleeAttack;
        }
        else if (playerDistance <= parameter.rangedAttackRange)
        {
            return BossSlimeStateType.RangedAttack;
        }
        else
        {
            return BossSlimeStateType.Idle; // 超出攻击范围
        }
    }
    
    /// <summary>
    /// 重置攻击模式切换系统（可用于调试）
    /// </summary>
    [ContextMenu("重置攻击模式")]
    public void ResetAttackMode()
    {
        parameter.forceRangedAfterMelee = false;
        parameter.lastAttackWasMelee = false;
        Debug.Log("Boss攻击模式已重置");
    }
    
    // 供RangedAttackState调用的跳跃攻击方法
    public IEnumerator ExecuteJumpBarrageAttack()
    {
        // 重置跳跃攻击准备状态
        parameter.phase3JumpAttackReady = false;
        
        // 执行跳跃弹幕攻击
        yield return StartCoroutine(PerformJumpBarrageAttack());
    }
    
    // 检查是否应该执行跳跃攻击
    public bool ShouldPerformJumpAttack()
    {
        return isInPhase3 && parameter.phase3JumpAttackReady && !isPerformingJumpAttack;
    }
    
    // 阶段3特殊攻击 - 重新实现跳跃弹幕攻击
    private IEnumerator PerformJumpBarrageAttack()
    {
        if (isPerformingJumpAttack) yield break; // 防止重复触发
        
        isPerformingJumpAttack = true;
        jumpStartPosition = transform.position;
        
        Debug.Log($"Boss史莱姆执行跳跃弹幕雨攻击！起始位置: {jumpStartPosition}");
        
        // 保存原始物理设置
        float originalGravityScale = rb.gravityScale;
        float originalDrag = rb.drag;
        
        // 暂停正常AI行为，但不禁用组件
        currentState?.OnExit();
        currentState = null;
        
        // 第一阶段：跳跃起飞
        Debug.Log("阶段1：Boss开始跳跃");
        if (rb != null)
        {
            // 优化物理设置用于跳跃
            rb.gravityScale = 1.5f; // 减少重力避免过快下落
            rb.drag = 0f; // 移除阻力
            rb.freezeRotation = true; // 确保不旋转
            
            // 清除当前速度，然后应用跳跃力
            rb.velocity = Vector2.zero;
            
            // 等待一帧确保速度被清零
            yield return new WaitForFixedUpdate();
            
            // 应用跳跃力
            rb.AddForce(Vector2.up * parameter.jumpForce, ForceMode2D.Impulse);
            
            Debug.Log($"应用跳跃力: {parameter.jumpForce}");
            
            // 等待一帧查看实际速度
            yield return new WaitForFixedUpdate();
            Debug.Log($"跳跃后速度: {rb.velocity}");
        }
        
        // 等待达到跳跃高度的顶点
        float jumpTimer = 0f;
        float maxJumpTime = 3f; // 最大跳跃时间
        float maxHeight = jumpStartPosition.y;
        
        while (jumpTimer < maxJumpTime)
        {
            jumpTimer += Time.deltaTime;
            
            if (rb != null)
            {
                // 记录最大高度
                if (transform.position.y > maxHeight)
                {
                    maxHeight = transform.position.y;
                }
                
                Debug.Log($"跳跃中... 时间: {jumpTimer:F2}s, 高度: {transform.position.y:F2}, 速度: {rb.velocity.y:F2}, 最大高度: {maxHeight:F2}");
                
                // 修改跳跃顶点检测逻辑 - 基于相对高度而非绝对高度
                float relativeHeight = transform.position.y - jumpStartPosition.y;
                
                // 检查是否达到跳跃顶点
                // 条件1: 速度开始向下且达到了合理的相对高度 (至少5单位)
                // 条件2: 或者达到了足够的时间 (1.5秒)
                if ((rb.velocity.y <= 0f && relativeHeight >= 5f) || jumpTimer > 1.5f)
                {
                    Debug.Log($"Boss达到跳跃顶点，开始发射弹幕 - 相对高度: {relativeHeight:F2}");
                    break;
                }
            }
            
            yield return null;
        }
        
        // 第二阶段：在空中发射弹幕
        Debug.Log("阶段2：Boss在空中发射全屏弹幕");
        yield return StartCoroutine(CreateAirborneBarrage());
        
        // 第三阶段：等待落地
        Debug.Log("阶段3：Boss等待落地");
        yield return StartCoroutine(WaitForLanding());
        
        // 第四阶段：恢复正常行为
        Debug.Log("阶段4：Boss恢复正常行为");
        
        // 恢复物理设置
        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
            rb.drag = originalDrag;
            rb.velocity = Vector2.zero; // 确保落地后停止
        }
        
        isPerformingJumpAttack = false;
        
        // 重新启用正常AI行为
        TransitionState(BossSlimeStateType.Idle);
        
        // 短暂停顿
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("跳跃弹幕攻击完成！");
    }
    
    // 在空中创建弹幕雨
    private IEnumerator CreateAirborneBarrage()
    {
        if (parameter.target == null) yield break;
        
        Debug.Log($"开始创建空中弹幕雨，弹幕数量: {parameter.barrageProjectileCount}");
        
        for (int i = 0; i < parameter.barrageProjectileCount; i++)
        {
            // 在Boss周围创建弹幕
            float angle = (360f / parameter.barrageProjectileCount) * i;
            Vector2 spawnOffset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * parameter.barrageSpawnRadius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * parameter.barrageSpawnRadius
            );
            
            Vector3 spawnPosition = transform.position + (Vector3)spawnOffset;
            
            // 创建弹幕
            GameObject projectile = CreateProjectile(
                parameter.barrageProjectilePrefab,
                spawnPosition,
                parameter.target.position,
                parameter.rangedDamage,
                true // 启用追踪
            );
            
            if (projectile != null)
            {
                Debug.Log($"创建弹幕 {i + 1}/{parameter.barrageProjectileCount} 位置: {spawnPosition}");
                
            }
            
            yield return new WaitForSeconds(parameter.barrageSpawnDelay);
        }
        
        Debug.Log("弹幕雨创建完成");
    }
    
    // 等待Boss落地
    private IEnumerator WaitForLanding()
    {
        float landingTimer = 0f;
        float maxLandingTime = 5f; // 最大落地等待时间
        
        while (landingTimer < maxLandingTime)
        {
            landingTimer += Time.deltaTime;
            
            if (rb != null)
            {
                float relativeHeight = transform.position.y - jumpStartPosition.y;
                
                Debug.Log($"等待落地... 时间: {landingTimer:F2}s, 高度: {transform.position.y:F2}, 相对高度: {relativeHeight:F2}, 速度: {rb.velocity.y:F2}");
                
                // 检查是否已经落地 - 使用相对高度判断
                // 条件：相对高度接近起始位置（允许1单位误差）且向下速度较小
                if (relativeHeight <= 1f && rb.velocity.y <= 1f)
                {
                    Debug.Log($"Boss已落地 - 相对高度: {relativeHeight:F2}");
                    break;
                }
            }
            
            yield return null;
        }
        
        // 确保Boss稳定落地
        if (rb != null)
        {
            rb.velocity = new Vector2(0, Mathf.Min(rb.velocity.y, 0f));
        }
        
        yield return new WaitForSeconds(0.2f);
    }
    
    
    private void CreateFullScreenBarrage()
    {
        StartCoroutine(SpawnBarrageProjectiles());
    }
    
    private IEnumerator SpawnBarrageProjectiles()
    {
        for (int i = 0; i < parameter.barrageProjectileCount; i++)
        {
            // 计算Boss周围的随机生成位置
            float angle = (360f / parameter.barrageProjectileCount) * i;
            Vector2 spawnOffset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * parameter.barrageSpawnRadius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * parameter.barrageSpawnRadius
            );
            
            Vector3 spawnPosition = transform.position + (Vector3)spawnOffset;
            
            // 创建弹幕
            GameObject projectile = CreateProjectile(
                parameter.barrageProjectilePrefab,
                spawnPosition,
                parameter.target.position,
                parameter.rangedDamage,
                true // 启用追踪
            );
            
            yield return new WaitForSeconds(parameter.barrageSpawnDelay);
        }
    }
    
    public GameObject CreateProjectile(GameObject prefab, Vector3 position, Vector3 targetPosition, int damage, bool isHoming = false)
    {
        if (prefab == null)
        {
            Debug.LogWarning("弹幕预制体为空！");
            return null;
        }
        
        GameObject projectile = Instantiate(prefab, position, Quaternion.identity);
        
        // 计算方向
        Vector2 direction = (targetPosition - position).normalized;
        
        // 设置物理
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb == null)
        {
            projectileRb = projectile.AddComponent<Rigidbody2D>();
            projectileRb.gravityScale = 0f;
        }
        
        // 设置碰撞体
        if (projectile.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;
        }
        
        // 添加弹幕脚本
        BossSlimeProjectile projectileScript = projectile.GetComponent<BossSlimeProjectile>();
        if (projectileScript == null)
        {
            projectileScript = projectile.AddComponent<BossSlimeProjectile>();
        }
        
        projectileScript.Initialize(damage, direction, parameter.targetLayer, parameter.projectileSpeed, isHoming);
        
        // 设置初始速度
        projectileRb.velocity = direction * parameter.projectileSpeed;
        
        // 生命周期后销毁
        Destroy(projectile, parameter.projectileLifetime);
        
        return projectile;
    }
    
    public void CreateFanProjectiles(Vector3 centerPosition, Vector3 targetPosition, int count, float angle)
    {
        Vector2 centerDirection = (targetPosition - centerPosition).normalized;
        float angleStep = angle / (count - 1);
        float startAngle = -angle / 2f;
        
        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 direction = RotateVector(centerDirection, currentAngle);
            
            CreateProjectile(
                parameter.fanProjectilePrefab ?? parameter.trackingProjectilePrefab,
                centerPosition,
                centerPosition + (Vector3)direction * 10f,
                parameter.rangedDamage,
                true
            );
        }
    }
    
    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }
}