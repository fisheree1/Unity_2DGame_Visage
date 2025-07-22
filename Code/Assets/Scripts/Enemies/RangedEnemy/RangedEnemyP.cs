using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum RangedEnemyStateType
{
    Idle,
    Charge,
    Attack,
    Hurt,
    Dead
}

[System.Serializable]
public class RangedEnemyParameter
{
    [Header("基本参数")]
    public float detectionRange = 8f;
    public float attackRange = 6f;
    public float retreatRange = 3f;
    public Animator animator;
    public Transform target;
    public LayerMask targetLayer;
    
    [Header("攻击参数")]
    public Transform attackPoint;
    public float attackCooldown = 2f;
    public float chargeDuration = 1f;
    public int damage = 15;
    
    [Header("弹幕参数")]
    public GameObject projectilePrefab;
    public GameObject trackingProjectilePrefab;
    public float projectileSpeed = 5f;
    public float projectileLifetime = 3f;
    
    [Header("扇形弹幕参数")]
    public int fanProjectileCount = 5;
    public float fanAngle = 45f;
    
    [Header("状态标记")]
    public bool isCharging = false;
    public bool isAttacking = false;
    public bool isHit = false;
    public bool isDead = false;
    
    [Header("受伤参数")]
    public float hurtDuration = 0.5f;
    public float hurtKnockbackForce = 5f;
}

public class RangedEnemyP : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.5f;

    // Components
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;
    private EnemyLife enemyLife;

    // Health properties - 通过EnemyLife组件获取
    public int CurrentHealth => enemyLife != null ? enemyLife.CurrentHealth : 0;
    public int MaxHealth => enemyLife != null ? enemyLife.MaxHealth : 0;
    public bool IsDead => enemyLife != null ? enemyLife.IsDead : false;

    public RangedEnemyParameter parameter;
    private IState currentState;
    private Dictionary<RangedEnemyStateType, IState> states = new Dictionary<RangedEnemyStateType, IState>();
    private CinemachineImpulseSource impulseSource;

    // 攻击模式控制
    private bool useTrackingAttack = true; // 交替使用追踪弹和扇形弹
    private float lastAttackTime = 0f;

    void Start()
    {
        if (parameter == null)
            parameter = new RangedEnemyParameter();

        parameter.animator = GetComponent<Animator>();

        // 获取物理组件
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // 设置物理属性
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
        }

        // 如果没有Rigidbody2D，自动添加
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
            rb.drag = 5f;
            Debug.Log("Added Rigidbody2D to RangedEnemy");
        }

        // 如果没有Collider2D，自动添加
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            Debug.Log("Added CapsuleCollider2D to RangedEnemy");
        }

        // 获取或添加EnemyLife组件
        enemyLife = GetComponent<EnemyLife>();
        if (enemyLife == null)
        {
            enemyLife = gameObject.AddComponent<EnemyLife>();
        }

        // 设置攻击点
        if (parameter.attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            parameter.attackPoint = attackPointObj.transform;
        }

        // 初始化状态机
        states.Add(RangedEnemyStateType.Idle, new RangedEnemyIdleState(this, parameter));
        states.Add(RangedEnemyStateType.Charge, new RangedEnemyChargeState(this, parameter));
        states.Add(RangedEnemyStateType.Attack, new RangedEnemyAttackState(this, parameter));
        states.Add(RangedEnemyStateType.Hurt, new RangedEnemyHurtState(this, parameter));
        states.Add(RangedEnemyStateType.Dead, new RangedEnemyDeadState(this, parameter));

        TransitionState(RangedEnemyStateType.Idle);

        // 初始化相机震动源
        impulseSource = GetComponent<CinemachineImpulseSource>();
        
        // 获取精灵渲染器用于颜色变化
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        currentState.OnUpdate();
    }

    public void TransitionState(RangedEnemyStateType type)
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
        float newScaleX = Mathf.Sign(direction.x) < 0 ? -1f : 1f;
        transform.localScale = new Vector3(newScaleX, 1f, 1f);

        // 更新攻击点位置
        if (parameter.attackPoint != null)
        {
            Vector3 attackPos = parameter.attackPoint.localPosition;
            attackPos.x = Mathf.Abs(attackPos.x) * newScaleX;
            parameter.attackPoint.localPosition = attackPos;
        }
    }

    // 执行追踪弹攻击
    public void PerformTrackingAttack()
    {
        if (parameter.target == null || parameter.trackingProjectilePrefab == null) return;

        Vector3 spawnPos = parameter.attackPoint.position;
        Vector2 direction = (parameter.target.position - spawnPos).normalized;

        GameObject projectile = Instantiate(parameter.trackingProjectilePrefab, spawnPos, Quaternion.identity);
        
        // 设置弹射物
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb == null)
        {
            projectileRb = projectile.AddComponent<Rigidbody2D>();
            projectileRb.gravityScale = 0f;
        }
        
        projectileRb.velocity = direction * parameter.projectileSpeed;

        // 添加追踪弹脚本
        TrackingProjectile trackingScript = projectile.GetComponent<TrackingProjectile>();
        if (trackingScript == null)
        {
            trackingScript = projectile.AddComponent<TrackingProjectile>();
        }
        trackingScript.Initialize(parameter.damage, parameter.target, parameter.targetLayer);

        // 确保有碰撞体
        if (projectile.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;
        }

        Destroy(projectile, parameter.projectileLifetime);
        
        Debug.Log("RangedEnemy: Tracking projectile launched");
    }

    // 执行扇形弹幕攻击
    public void PerformFanAttack()
    {
        if (parameter.target == null || parameter.projectilePrefab == null) return;

        Vector3 spawnPos = parameter.attackPoint.position;
        Vector2 baseDirection = (parameter.target.position - spawnPos).normalized;

        float angleStep = parameter.fanAngle / (parameter.fanProjectileCount - 1);
        float startAngle = -parameter.fanAngle / 2;

        for (int i = 0; i < parameter.fanProjectileCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 direction = RotateVector(baseDirection, angle);

            GameObject projectile = Instantiate(parameter.projectilePrefab, spawnPos, Quaternion.identity);
            
            // 设置弹射物
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            if (projectileRb == null)
            {
                projectileRb = projectile.AddComponent<Rigidbody2D>();
                projectileRb.gravityScale = 0f;
            }
            
            projectileRb.velocity = direction * parameter.projectileSpeed;

            // 添加普通弹射物脚本
            RangedProjectile projectileScript = projectile.GetComponent<RangedProjectile>();
            if (projectileScript == null)
            {
                projectileScript = projectile.AddComponent<RangedProjectile>();
            }
            projectileScript.Initialize(parameter.damage, parameter.targetLayer);

            // 确保有碰撞体
            if (projectile.GetComponent<Collider2D>() == null)
            {
                CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
                collider.radius = 0.3f;
            }

            Destroy(projectile, parameter.projectileLifetime);
        }

        Debug.Log("RangedEnemy: Fan attack launched");
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

    // 检查是否可以攻击
    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= parameter.attackCooldown;
    }

    // 设置攻击时间
    public void SetAttackTime()
    {
        lastAttackTime = Time.time;
        useTrackingAttack = !useTrackingAttack; // 交替攻击模式
    }

    // 获取当前攻击模式
    public bool ShouldUseTrackingAttack()
    {
        return useTrackingAttack;
    }

    // 重置精灵颜色
    public void ResetSpriteColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // 设置精灵颜色（用于受伤效果）
    public void SetSpriteColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    #region 视线和伤害检测
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hero = GameObject.FindGameObjectWithTag("Player");
        
        // 检测玩家进入视野
        if (other.CompareTag("Player"))
        {
            parameter.target = other.transform;
            
            // 如果已经死亡，不响应玩家
            if (parameter.isDead) return;
            
            // 根据当前状态和距离决定状态转换
            float distance = Vector2.Distance(transform.position, other.transform.position);
            
            if (distance <= parameter.attackRange && CanAttack())
            {
                TransitionState(RangedEnemyStateType.Charge);
            }
            else if (distance <= parameter.detectionRange)
            {
                TransitionState(RangedEnemyStateType.Idle);
            }
        }
        
        // 检测玩家攻击
        if (other.CompareTag("PlayerAttack"))
        {
            // 如果已经死亡，不响应攻击
            if (parameter.isDead) return;
            
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
                        GetComponent<Enemy>()?.GetHit(Vector2.left);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 如果已经死亡，不响应玩家离开
            if (parameter.isDead) return;
            
            float distance = Vector2.Distance(transform.position, other.transform.position);
            
            if (distance > parameter.detectionRange)
            {
                parameter.target = null;
                TransitionState(RangedEnemyStateType.Idle);
            }
        }
    }
    #endregion

    #region 伤害系统
    public void TakeDamage(int damage)
    {
        if (enemyLife == null || enemyLife.IsDead || parameter.isDead) return;

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
            parameter.isDead = true;
            // 进入死亡状态
            TransitionState(RangedEnemyStateType.Dead);
        }
        else
        {
            // 进入受伤状态
            TransitionState(RangedEnemyStateType.Hurt);
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, parameter.detectionRange);
        
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, parameter.attackRange);
        
        // 绘制后退范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, parameter.retreatRange);
        
        // 绘制攻击点
        if (parameter.attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(parameter.attackPoint.position, 0.2f);
        }
    }
}