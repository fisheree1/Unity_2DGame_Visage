using System.Collections;
using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [Header("Fire Settings")]
    [SerializeField] private int damage = 30;
    [SerializeField] private float fireRange = 2f;
    [SerializeField] private float fireDuration = 0.5f;
    [SerializeField] private float burnDuration = 2f;
    [SerializeField] private float burnTickRate = 0.5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool useAnimationCollider = true; // 使用动画中的碰撞体
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject warningEffectPrefab;
    [SerializeField] private GameObject fireEffectPrefab;
    [SerializeField] private GameObject burnEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;
    
    [Header("Fire Strike Settings")]
    [SerializeField] private int fireStrikeCount = 1;
    [SerializeField] private float fireStrikeDelay = 0.4f;
    [SerializeField] private float warningTime = 1.2f;
    
    private Vector3 targetPosition;
    private bool hasDealtDamage = false;
    private Animator animator;
    private AudioSource audioSource;
    
    // 音效
    [Header("Audio")]
    [SerializeField] private AudioClip fireStartSound;
    [SerializeField] private AudioClip fireImpactSound;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 调试信息
        Debug.Log($"FireProjectile Awake - Animator: {(animator != null ? "Found" : "Missing")}");
        
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
        Debug.Log($"FireProjectile has {colliders.Length} colliders");
        
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
        
        Debug.Log($"Fire started at {transform.position}, targeting {targetPosition}");
        
        // 开始火焰攻击序列
        StartCoroutine(FireStrikeSequence());
        
        // 失效保护：如果10秒后还没有销毁，强制销毁
        StartCoroutine(FailsafeDestroy(10f));
    }
    
    // 失效保护协程
    private IEnumerator FailsafeDestroy(float maxLifetime)
    {
        yield return new WaitForSeconds(maxLifetime);
        
        if (gameObject != null)
        {
            Debug.LogWarning($"Fire projectile {gameObject.name} exceeded max lifetime, force destroying");
            ForceDestroy();
        }
    }
    
    public void Initialize(int damage, Vector3 targetPosition, float range, LayerMask targetLayer)
    {
        this.damage = damage;
        this.targetPosition = targetPosition;
        this.fireRange = range;
        this.playerLayer = targetLayer;
        
        Debug.Log($"Fire Strike initialized - Damage: {damage}, Target: {targetPosition}, Range: {range}");
        
        // 设置位置到目标位置
        transform.position = targetPosition;
        
        // 开始火焰攻击序列
        StartCoroutine(FireStrikeSequence());
    }
    
    // 设置伤害
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    // 设置攻击范围
    public void SetRange(float newRange)
    {
        fireRange = newRange;
    }
    
    // 设置目标位置
    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
    }
    
    // 设置火焰次数
    public void SetFireStrikeCount(int count)
    {
        fireStrikeCount = count;
    }
    
    // 设置警告时间
    public void SetWarningTime(float time)
    {
        warningTime = time;
    }
    
    // 设置燃烧持续时间
    public void SetBurnDuration(float duration)
    {
        burnDuration = duration;
    }
    
    private IEnumerator FireStrikeSequence()
    {
        // 连续火焰攻击
        for (int i = 0; i < fireStrikeCount; i++)
        {
            Vector3 currentTarget = targetPosition;
            
            // 为后续火焰攻击添加随机偏移
            if (i > 0)
            {
                float randomOffset = Random.Range(-1.5f, 1.5f);
                currentTarget.x += randomOffset;
            }
            
            // 执行单次火焰攻击
            yield return StartCoroutine(ExecuteFireStrike(currentTarget));
            
            // 等待下次火焰攻击
            if (i < fireStrikeCount - 1)
            {
                yield return new WaitForSeconds(fireStrikeDelay);
            }
        }
        
        // 等待燃烧效果结束
        yield return new WaitForSeconds(burnDuration + 0.5f);
        DestroyProjectile();
    }
    
    private IEnumerator ExecuteFireStrike(Vector3 strikeTarget)
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
        
        // 执行火焰攻击
        yield return StartCoroutine(PerformFireStrike(strikeTarget));
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
            GameObject warning = new GameObject("FireWarning");
            warning.transform.position = position;
            
            // 添加视觉组件
            SpriteRenderer spriteRenderer = warning.AddComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(1f, 0.5f, 0f, 0.6f); // 半透明橙色
            
            // 创建简单的圆形指示器
            CreateCircleSprite(spriteRenderer, fireRange);
            
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
                    texture.SetPixel(x, y, new Color(1f, 0.5f, 0f, alpha * 0.6f));
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
    
    private IEnumerator PerformFireStrike(Vector3 strikePosition)
    {
        // 生成火焰特效
        GameObject fireEffect = null;
        if (fireEffectPrefab != null)
        {
            fireEffect = Instantiate(fireEffectPrefab, strikePosition, Quaternion.identity);
        }
        
        // 播放火焰音效
        if (fireStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireStartSound);
        }
        
        // 立即造成初始伤害
        PerformInitialFireDamage(strikePosition);
        
        // 持续燃烧伤害
        yield return StartCoroutine(ApplyBurnDamage(strikePosition));
        
        // 清理火焰特效
        if (fireEffect != null)
        {
            Destroy(fireEffect);
        }
        
        hasDealtDamage = true;
        Debug.Log($"Fire strike completed at {strikePosition}");
    }
    
    private void PerformInitialFireDamage(Vector3 strikePosition)
    {
        // 根据是否有动画碰撞器选择不同的伤害检测方式
        if (useAnimationCollider)
        {
            // 使用动画碰撞器，伤害检测由OnTriggerEnter2D处理
            Debug.Log($"Fire initial strike at {strikePosition} - using animation collider");
        }
        else
        {
            // 使用传统的圆形范围检测
            DealDamage(strikePosition, damage);
        }
    }
    
    // 造成伤害 - 仿照LightningStrike的DealDamage函数
    private void DealDamage(Vector3 strikePosition, int damageAmount)
    {
        // 检测范围内的所有碰撞体
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(strikePosition, fireRange, playerLayer);
        
        Debug.Log($"Fire DealDamage: Found {hitTargets.Length} targets in range {fireRange} at {strikePosition}");
        
        foreach (Collider2D target in hitTargets)
        {
            // 检查是否是玩家
            if (target.CompareTag("Player"))
            {
                HeroLife playerLife = target.GetComponent<HeroLife>();
                if (playerLife != null && !playerLife.IsDead)
                {
                    playerLife.TakeDamage(damageAmount);
                    Debug.Log($"Fire dealt {damageAmount} damage to player at {strikePosition}");
                    
                    // 添加击退效果
                    Rigidbody2D playerRb = target.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDirection = (target.transform.position - strikePosition).normalized;
                        if (knockbackDirection.magnitude < 0.1f)
                        {
                            knockbackDirection = Vector2.up;
                        }
                        playerRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
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
                    Debug.Log($"Fire (trigger) dealt {damage} damage to player");
                    
                    // 添加击退效果
                    Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                        if (knockbackDirection.magnitude < 0.1f)
                        {
                            knockbackDirection = Vector2.up;
                        }
                        playerRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
    
    // 可以从动画事件中调用的方法 - 仿照LightningStrike
    public void TriggerDamage()
    {
        Debug.Log("=== Fire damage triggered from animation event ===");
        Debug.Log($"Position: {transform.position}, Range: {fireRange}");
        
        if (hasDealtDamage)
        {
            Debug.Log("Fire damage already dealt, skipping");
            return;
        }
        
        // 使用统一的伤害处理方法
        DealDamage(transform.position, damage);
        hasDealtDamage = true;
        
        // 播放火焰冲击音效
        if (fireImpactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireImpactSound);
        }
    }
    
    private IEnumerator ApplyBurnDamage(Vector3 strikePosition)
    {
        // 如果使用动画碰撞器，燃烧伤害由碰撞器处理
        if (useAnimationCollider)
        {
            Debug.Log($"Fire burn damage will be handled by animation collider");
            yield return new WaitForSeconds(burnDuration);
            yield break;
        }
        
        // 传统的燃烧伤害逻辑
        float elapsedTime = 0f;
        GameObject burnEffect = null;
        
        if (burnEffectPrefab != null)
        {
            burnEffect = Instantiate(burnEffectPrefab, strikePosition, Quaternion.identity);
        }
        
        while (elapsedTime < burnDuration)
        {
            yield return new WaitForSeconds(burnTickRate);
            elapsedTime += burnTickRate;
            
            // 使用统一的伤害处理方法
            int burnDamage = Mathf.RoundToInt(damage * 0.2f); // 燃烧伤害为初始伤害的20%
            DealDamage(strikePosition, burnDamage);
        }
        
        if (burnEffect != null)
        {
            Destroy(burnEffect);
        }
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
    
    // 可以从动画事件或其他地方调用的销毁方法
    public void DestroyFromAnimation()
    {
        Debug.Log("Fire projectile destroyed from animation event");
        DestroyProjectile();
    }
    
    // 可以从外部调用的立即销毁方法
    public void ForceDestroy()
    {
        Debug.Log("Fire projectile force destroyed");
        // 停止所有协程
        StopAllCoroutines();
        Destroy(gameObject);
    }
    
    // 在对象被销毁时调用
    private void OnDestroy()
    {
        Debug.Log("Fire projectile OnDestroy called");
        // 停止所有协程
        StopAllCoroutines();
    }
    
    // 在Scene视图中绘制攻击范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, fireRange);
        
        // 绘制燃烧范围
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f); // 半透明橙色
        Gizmos.DrawSphere(targetPosition, fireRange * 0.8f);
    }
}
