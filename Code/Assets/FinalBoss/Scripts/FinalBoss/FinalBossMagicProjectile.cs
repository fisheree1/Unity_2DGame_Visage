using System.Collections;
using UnityEngine;

// 魔法攻击类型枚举
public enum FinalBossMagicType
{
    SmallSparkHorizontal,   // 水平移动的SmallSpark
    SmallSparkFan,          // 260度扇形的SmallSpark
    BigBolt,                // 从上而下的雷电攻击
    Fire                    // 从地底向上的火焰攻击
}

public class FinalBossMagicProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask targetLayer;
    
    [Header("Magic Type")]
    [SerializeField] private FinalBossMagicType magicType = FinalBossMagicType.SmallSparkHorizontal;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject trailEffectPrefab;
    
    // Components
    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasHitTarget = false;
    private Vector2 moveDirection;
    
    // Special attack settings
    private bool isSpecialAttack = false;
    private float specialAttackTimer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        
        if (targetLayer == 0)
        {
            targetLayer = LayerMask.GetMask("Player");
        }
        
        // 根据魔法类型设置行为
        SetupMagicBehavior();
        
        // 设置生命周期
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        if (!hasHitTarget)
        {
            MoveMagicProjectile();
        }
    }
    
    private void SetupMagicBehavior()
    {
        switch (magicType)
        {
            case FinalBossMagicType.SmallSparkHorizontal:
                SetupHorizontalMovement();
                break;
                
            case FinalBossMagicType.SmallSparkFan:
                SetupHorizontalMovement(); // 扇形攻击由生成器控制方向
                break;
                
            case FinalBossMagicType.BigBolt:
                SetupBigBoltAttack();
                break;
                
            case FinalBossMagicType.Fire:
                SetupFireAttack();
                break;
        }
    }
    
    private void SetupHorizontalMovement()
    {
        // SmallSpark在MagicSpawnPoint处不移动，作为固定的攻击点
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // 不移动
            rb.isKinematic = true; // 设置为运动学刚体，不受物理影响
        }
        
        // SmallSpark在固定位置持续一段时间
        StartCoroutine(FixedPositionAttack());
    }
    
    private void SetupBigBoltAttack()
    {
        // 雷电攻击从固定点闪电，不移动
        isSpecialAttack = true;
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // 不移动
            rb.isKinematic = true; // 设置为运动学刚体
        }
        
        // 雷电攻击伤害更高
        damage = 35;
        
        // 雷电在固定位置持续一段时间
        StartCoroutine(FixedPositionAttack());
    }
    
    private void SetupFireAttack()
    {
        // 火焰攻击从固定点喷火，不移动
        isSpecialAttack = true;
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // 不移动
            rb.isKinematic = true; // 设置为运动学刚体
        }
        
        // 火焰攻击伤害适中
        damage = 30;
        
        // 火焰在固定位置持续一段时间
        StartCoroutine(FixedPositionAttack());
    }
    
    // 固定位置攻击的协程
    private IEnumerator FixedPositionAttack()
    {
        float attackDuration = 2f; // 攻击持续时间
        float damageInterval = 0.5f; // 伤害间隔
        float elapsedTime = 0f;
        float lastDamageTime = 0f;
        
        // 启用碰撞检测
        if (col != null)
        {
            col.enabled = true;
        }
        
        while (elapsedTime < attackDuration && !hasHitTarget)
        {
            elapsedTime += Time.deltaTime;
            
            // 定期检查范围内的玩家并造成伤害
            if (elapsedTime - lastDamageTime >= damageInterval)
            {
                CheckForPlayerInRange();
                lastDamageTime = elapsedTime;
            }
            
            yield return null;
        }
        
        // 攻击结束后销毁
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
    
    // 检查范围内的玩家
    private void CheckForPlayerInRange()
    {
        float detectionRadius = 1.5f; // 攻击范围
        
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);
        
        foreach (Collider2D target in hitTargets)
        {
            HeroLife heroLife = target.GetComponent<HeroLife>();
            if (heroLife != null && !heroLife.IsDead)
            {
                heroLife.TakeDamage(damage);
                Debug.Log($"Final Boss {magicType} dealt {damage} damage to player at fixed position");
                
                // 生成击中特效
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, target.transform.position, Quaternion.identity);
                }
                
                hasHitTarget = true;
                break;
            }
        }
    }
    
    private void MoveMagicProjectile()
    {
        // 所有魔法攻击现在都是固定位置的，不需要移动逻辑
        // 移动逻辑已经在SetupXXXAttack方法中的协程中处理
    }
    
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        
        // 立即应用方向（如果不是特殊攻击）
        if (!isSpecialAttack && rb != null)
        {
            rb.velocity = moveDirection * speed;
        }
        
        // 旋转投射物朝向移动方向
        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    public void SetMagicType(FinalBossMagicType type)
    {
        magicType = type;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitTarget) return;
        
        // 检查是否击中玩家
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Debug.Log($"Final Boss magic projectile ({magicType}) hit target: {other.name}");
            
            HeroLife heroLife = other.GetComponent<HeroLife>();
            if (heroLife != null && !heroLife.IsDead)
            {
                heroLife.TakeDamage(damage);
                Debug.Log($"Final Boss magic dealt {damage} damage to player");
                
                // 生成击中特效
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }
                
                hasHitTarget = true;
                
                // 销毁投射物
                Destroy(gameObject, 0.1f);
            }
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            // 击中地面或墙壁
            Debug.Log($"Final Boss magic projectile ({magicType}) hit {other.tag}");
            
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            hasHitTarget = true;
            Destroy(gameObject, 0.1f);
        }
    }
    
    private void OnBecameInvisible()
    {
        // 当投射物离开屏幕时销毁
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
