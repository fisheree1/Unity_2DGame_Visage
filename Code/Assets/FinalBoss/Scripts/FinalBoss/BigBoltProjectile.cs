using System.Collections;
using UnityEngine;

public class BigBoltProjectile : MonoBehaviour
{
    [Header("Lightning Settings")]
    [SerializeField] private int damage = 35;
    [SerializeField] private float strikeRange = 2f;
    [SerializeField] private float strikeDuration = 0.5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool useAnimationCollider = true; // 使用动画中的碰撞体
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject warningEffectPrefab;
    [SerializeField] private GameObject strikeEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;
    
    [Header("Strike Settings")]
    [SerializeField] private int strikeCount = 1;
    [SerializeField] private float strikeDelay = 0.3f;
    [SerializeField] private float warningTime = 1.5f;
    
    private Vector3 targetPosition;
    private bool hasDealtDamage = false;
    private Animator animator;
    private AudioSource audioSource;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 调试信息
        Debug.Log($"BigBoltProjectile Awake - Animator: {(animator != null ? "Found" : "Missing")}");
        
        // 如果没有动画器，禁用动画驱动模式
        if (animator == null)
        {
            useAnimationCollider = false;
            Debug.LogWarning("No Animator found, disabling animation-driven mode");
        }
    }
    
    private void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        
        // 如果没有设置targetPosition，使用当前位置
        if (targetPosition == Vector3.zero)
        {
            targetPosition = transform.position;
        }
        
        // 检查预制体设置
        Collider2D[] colliders = GetComponents<Collider2D>();
        Debug.Log($"BigBoltProjectile has {colliders.Length} colliders");
        
        foreach (var col in colliders)
        {
            Debug.Log($"Collider: {col.GetType().Name}, IsTrigger: {col.isTrigger}");
            col.isTrigger = true; // 确保所有碰撞器都是触发器
        }
        
        // 检查动画控制器
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            Debug.Log($"Animator Controller: {animator.runtimeAnimatorController.name}");
        }
        
        Debug.Log($"BigBolt started at {transform.position}, targeting {targetPosition}");
        
        // 开始雷电攻击序列
        StartCoroutine(LightningStrikeSequence());
    }
    
    // 设置伤害
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    // 设置攻击范围
    public void SetRange(float newRange)
    {
        strikeRange = newRange;
    }
    
    // 设置目标位置
    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
    }
    
    // 设置雷击次数
    public void SetStrikeCount(int count)
    {
        strikeCount = count;
    }
    
    // 设置警告时间
    public void SetWarningTime(float time)
    {
        warningTime = time;
    }

    // 初始化方法
    public void Initialize(int damage, Vector3 targetPosition, float range, LayerMask targetLayer)
    {
        this.damage = damage;
        this.targetPosition = targetPosition;
        this.strikeRange = range;
        this.playerLayer = targetLayer;
        
        Debug.Log($"BigBolt initialized - Damage: {damage}, Target: {targetPosition}, Range: {range}");
        
        // 设置位置到目标位置
        transform.position = new Vector3(targetPosition.x, targetPosition.y + 10f, targetPosition.z);
        
        // 开始雷电攻击序列
        StartCoroutine(LightningStrikeSequence());
    }
    
    private IEnumerator LightningStrikeSequence()
    {
        // 连续雷击
        for (int i = 0; i < strikeCount; i++)
        {
            Vector3 currentTarget = targetPosition;
            
            // 为后续雷击添加随机偏移
            if (i > 0)
            {
                float randomOffset = Random.Range(-2f, 2f);
                currentTarget.x += randomOffset;
            }
            
            // 执行单次雷击
            yield return StartCoroutine(ExecuteLightningStrike(currentTarget));
            
            // 等待下次雷击
            if (i < strikeCount - 1)
            {
                yield return new WaitForSeconds(strikeDelay);
            }
        }
        
        // 所有雷击完成后销毁
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
    
    private IEnumerator ExecuteLightningStrike(Vector3 strikeTarget)
    {
        // 创建警告指示器
        GameObject warningIndicator = CreateWarningIndicator(strikeTarget);
        
        // 等待警告时间
        yield return new WaitForSeconds(warningTime);
        
        // 移除警告指示器
        if (warningIndicator != null)
        {
            Destroy(warningIndicator);
        }
        
        // 执行雷电攻击
        PerformLightningStrike(strikeTarget);
    }
    
    private GameObject CreateWarningIndicator(Vector3 position)
    {
        if (warningEffectPrefab != null)
        {
            return Instantiate(warningEffectPrefab, position, Quaternion.identity);
        }
        else
        {
            // 创建简单的警告指示器
            GameObject warning = new GameObject("LightningWarning");
            warning.transform.position = position;
            
            // 添加视觉组件
            SpriteRenderer spriteRenderer = warning.AddComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(1f, 1f, 0f, 0.5f); // 半透明黄色
            
            // 创建简单的圆形指示器
            CreateCircleSprite(spriteRenderer, strikeRange);
            
            return warning;
        }
    }
    
    private void CreateCircleSprite(SpriteRenderer spriteRenderer, float radius)
    {
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float pixelRadius = radius * 10f; // 调整大小
        
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);
                
                if (distance <= pixelRadius)
                {
                    float alpha = 1f - (distance / pixelRadius);
                    texture.SetPixel(x, y, new Color(1f, 1f, 0f, alpha * 0.5f));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
    
    private void PerformLightningStrike(Vector3 strikePosition)
    {
        // 生成雷击特效
        if (strikeEffectPrefab != null)
        {
            Instantiate(strikeEffectPrefab, strikePosition, Quaternion.identity);
        }
        
        // 根据是否有动画碰撞器选择不同的伤害检测方式
        if (useAnimationCollider)
        {
            // 使用动画碰撞器，伤害检测由OnTriggerEnter2D处理
            Debug.Log($"BigBolt lightning strike at {strikePosition} - using animation collider");
        }
        else
        {
            // 使用传统的圆形范围检测
            DealDamage(strikePosition);
        }
        
        hasDealtDamage = true;
        Debug.Log($"BigBolt lightning strike at {strikePosition} with range {strikeRange}");
    }
    
    // 造成伤害 - 仿照LightningStrike的DealDamage函数
    private void DealDamage(Vector3 strikePosition)
    {
        // 检测范围内的所有碰撞体
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(strikePosition, strikeRange, playerLayer);
        
        Debug.Log($"BigBolt DealDamage: Found {hitTargets.Length} targets in range {strikeRange} at {strikePosition}");
        
        foreach (Collider2D target in hitTargets)
        {
            // 检查是否是玩家
            if (target.CompareTag("Player"))
            {
                HeroLife playerLife = target.GetComponent<HeroLife>();
                if (playerLife != null && !playerLife.IsDead)
                {
                    playerLife.TakeDamage(damage);
                    Debug.Log($"BigBolt dealt {damage} damage to player at {strikePosition}");
                    
                    // 添加击退效果
                    Rigidbody2D playerRb = target.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDirection = (target.transform.position - strikePosition).normalized;
                        if (knockbackDirection.magnitude < 0.1f)
                        {
                            knockbackDirection = Vector2.up;
                        }
                        playerRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
    
    // 动画碰撞器触发时的伤害处理
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useAnimationCollider && ((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            // 使用统一的伤害处理方法
            if (other.CompareTag("Player"))
            {
                HeroLife playerLife = other.GetComponent<HeroLife>();
                if (playerLife != null && !playerLife.IsDead)
                {
                    playerLife.TakeDamage(damage);
                    Debug.Log($"BigBolt (trigger) dealt {damage} damage to player");
                    
                    // 添加击退效果
                    Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                        if (knockbackDirection.magnitude < 0.1f)
                        {
                            knockbackDirection = Vector2.up;
                        }
                        playerRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
    
    // 可以从动画事件中调用的方法 - 仿照LightningStrike
    public void TriggerDamage()
    {
        Debug.Log("=== BigBolt damage triggered from animation event ===");
        Debug.Log($"Position: {transform.position}, Range: {strikeRange}");
        
        if (hasDealtDamage)
        {
            Debug.Log("BigBolt damage already dealt, skipping");
            return;
        }
        
        // 使用统一的伤害处理方法
        DealDamage(transform.position);
        hasDealtDamage = true;
    }
    
    private void DestroyProjectile()
    {
        // 生成销毁特效
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
    
    // 在Scene视图中绘制攻击范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, strikeRange);
    }
}
