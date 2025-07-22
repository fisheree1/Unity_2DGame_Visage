using System.Collections;
using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("Lightning Settings")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float strikeRange = 2f;
    [SerializeField] private float strikeDuration = 0.5f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private bool useAnimationCollider = true; // 使用动画中的碰撞体
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject lightningEffectPrefab;
    [SerializeField] private GameObject groundImpactEffectPrefab;
    
    private Vector3 targetPosition;
    private bool hasDealtDamage = false;
    private Animator animator;
    private AudioSource audioSource;
    
    // 音效
    [Header("Audio")]
    [SerializeField] private AudioClip lightningSound;
    [SerializeField] private AudioClip thunderSound;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 调试信息
        Debug.Log($"LightningStrike Awake - Animator: {(animator != null ? "Found" : "Missing")}");
        
        // 如果没有动画器，禁用动画驱动模式
        if (animator == null)
        {
            useAnimationCollider = false;
            Debug.LogWarning("No Animator found, disabling animation-driven mode");
        }
    }
    
    private void Start()
    {
        // 检查预制体设置
        Collider2D[] colliders = GetComponents<Collider2D>();
        Debug.Log($"LightningStrike has {colliders.Length} colliders");
        
        foreach (var col in colliders)
        {
            Debug.Log($"Collider: {col.GetType().Name}, IsTrigger: {col.isTrigger}");
        }
        
        // 检查动画控制器
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            Debug.Log($"Animator Controller: {animator.runtimeAnimatorController.name}");
        }
    }
    
    public void Initialize(int damage, Vector3 targetPosition, float range, LayerMask targetLayer)
    {
        this.damage = damage;
        this.targetPosition = targetPosition;
        this.strikeRange = range;
        this.targetLayer = targetLayer;
        
        Debug.Log($"Lightning Strike initialized - Damage: {damage}, Target: {targetPosition}, Range: {range}");
        Debug.Log($"Target Layer Mask: {targetLayer.value}");
        
        // 设置位置到目标位置
        transform.position = targetPosition;
        
        // 开始雷电攻击序列
        StartCoroutine(ExecuteLightningStrike());
    }
    
    private IEnumerator ExecuteLightningStrike()
    {
        // 如果使用动画驱动的碰撞体，直接播放动画
        if (useAnimationCollider && animator != null)
        {
            Debug.Log("Using animation-driven lightning strike");
            
            // 播放雷电音效
            if (lightningSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(lightningSound);
            }
            
            // 动画会自动播放（从Entry开始），等待动画完成
            // 获取动画长度
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float animationLength = stateInfo.length;
            
            Debug.Log($"Lightning animation length: {animationLength}");
            
            // 如果动画长度为0或太小，使用默认时间
            if (animationLength < 0.1f)
            {
                animationLength = 2f; // 默认2秒
            }
            
            // 等待动画完成
            yield return new WaitForSeconds(animationLength);
        }
        else
        {
            // 使用原来的脚本驱动方式
            // 第一阶段：雷电从天而降
            yield return StartCoroutine(DescendFromSky());
            
            // 第二阶段：击中地面，造成伤害
            yield return StartCoroutine(GroundImpact());
            
            // 第三阶段：消散
            yield return StartCoroutine(Dissipate());
        }
        
        // 销毁雷电对象
        Destroy(gameObject);
    }
    
    // 第一阶段：从天而降
    private IEnumerator DescendFromSky()
    {
        // 播放雷电音效
        if (lightningSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lightningSound);
        }
        
        Vector3 startPosition = new Vector3(targetPosition.x, targetPosition.y + 15f, targetPosition.z);
        Vector3 endPosition = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
        
        transform.position = startPosition;
        
        // 雷电下降动画
        float elapsedTime = 0f;
        float descentTime = 0.3f;
        
        while (elapsedTime < descentTime)
        {
            float progress = elapsedTime / descentTime;
            progress = Mathf.SmoothStep(0f, 1f, progress); // 平滑插值
            
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = endPosition;
    }
    
    // 第二阶段：地面冲击
    private IEnumerator GroundImpact()
    {
        // 播放雷鸣音效
        if (thunderSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(thunderSound);
        }
        
        // 创建地面冲击特效
        if (groundImpactEffectPrefab != null)
        {
            GameObject impactEffect = Instantiate(groundImpactEffectPrefab, targetPosition, Quaternion.identity);
            Destroy(impactEffect, 2f);
        }
        
        // 造成伤害
        DealDamage();
        
        // 屏幕震动效果（如果有摄像机震动系统）
        CameraShake();
        
        yield return new WaitForSeconds(strikeDuration);
    }
    
    // 第三阶段：消散
    private IEnumerator Dissipate()
    {
        // 渐隐效果
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            float fadeTime = 0.2f;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    // 造成伤害
    private void DealDamage()
    {
        if (hasDealtDamage) return;
        
        // 检测范围内的所有碰撞体
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(targetPosition, strikeRange, targetLayer);
        
        foreach (Collider2D target in hitTargets)
        {
            // 检查是否是玩家
            if (target.CompareTag("Player"))
            {
                HeroLife playerLife = target.GetComponent<HeroLife>();
                if (playerLife != null)
                {
                    playerLife.TakeDamage(damage);
                    Debug.Log($"Lightning strike dealt {damage} damage to player");
                    
                    // 添加击退效果
                    Rigidbody2D playerRb = target.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;
                        playerRb.AddForce(knockbackDirection * 500f, ForceMode2D.Impulse);
                    }
                }
            }
        }
        
        hasDealtDamage = true;
    }
    
    // 摄像机震动
    private void CameraShake()
    {
        // 寻找摄像机震动组件
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // 这里可以调用摄像机震动系统
            // 例如：CameraShaker.Instance.ShakeCamera(0.5f, 0.3f);
            Debug.Log("Camera shake triggered by lightning strike");
        }
    }
    
    // 当进入触发器时（用于检测玩家） - 主要的伤害检测方法
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDealtDamage) return;
        
        Debug.Log($"Lightning strike trigger hit: {other.name}, Tag: {other.tag}, Layer: {other.gameObject.layer}");
        
        // 检查是否是目标层级
        bool isTargetLayer = ((1 << other.gameObject.layer) & targetLayer) != 0;
        bool isPlayerTag = other.CompareTag("Player");
        
        Debug.Log($"Is target layer: {isTargetLayer}, Is player tag: {isPlayerTag}");
        
        if (isTargetLayer || isPlayerTag)
        {
            HeroLife playerLife = other.GetComponent<HeroLife>();
            if (playerLife != null)
            {
                playerLife.TakeDamage(damage);
                hasDealtDamage = true;
                Debug.Log($"Lightning strike (trigger) dealt {damage} damage to player");
                
                // 播放雷鸣音效
                if (thunderSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(thunderSound);
                }
                
                // 添加击退效果
                Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                    if (knockbackDirection.magnitude < 0.1f) // 如果距离太近，给一个默认方向
                    {
                        knockbackDirection = Vector2.up;
                    }
                    playerRb.AddForce(knockbackDirection * 1f, ForceMode2D.Impulse);
                }
                
                // 创建冲击特效
                if (groundImpactEffectPrefab != null)
                {
                    GameObject impactEffect = Instantiate(groundImpactEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(impactEffect, 2f);
                }
                
                // 摄像机震动
                CameraShake();
            }
            else
            {
                Debug.LogWarning($"Hit target but no HeroLife component found on {other.name}!");
            }
        }
        else
        {
            Debug.Log($"Hit non-target object: {other.name}");
        }
    }
    
    // 可以从动画事件中调用的方法
    public void TriggerDamage()
    {
        Debug.Log("=== Lightning strike damage triggered from animation event ===");
        Debug.Log($"Position: {transform.position}, Range: {strikeRange}");
        Debug.Log($"Target Layer Mask: {targetLayer.value}");
        
        if (hasDealtDamage) 
        {
            Debug.Log("Damage already dealt, skipping");
            return;
        }
        
        // 检测范围内的所有碰撞体
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, strikeRange, targetLayer);
        Debug.Log($"Found {hitTargets.Length} potential targets in range with correct layer");
        
        // 同时检测所有对象（忽略层级）
        Collider2D[] allTargets = Physics2D.OverlapCircleAll(transform.position, strikeRange);
        Debug.Log($"Found {allTargets.Length} total objects in range (all layers)");
        
        foreach (Collider2D target in allTargets)
        {
            Debug.Log($"Object in range: {target.name}, Tag: {target.tag}, Layer: {target.gameObject.layer} ({LayerMask.LayerToName(target.gameObject.layer)})");
        }
        
        foreach (Collider2D target in hitTargets)
        {
            Debug.Log($"Checking target: {target.name}, Tag: {target.tag}, Layer: {target.gameObject.layer}");
            
            // 检查是否是玩家
            if (target.CompareTag("Player"))
            {
                HeroLife playerLife = target.GetComponent<HeroLife>();
                if (playerLife != null)
                {
                    playerLife.TakeDamage(damage);
                    hasDealtDamage = true;
                    Debug.Log($"✓ Lightning strike (animation event) dealt {damage} damage to player");
                    
                    // 播放雷鸣音效
                    if (thunderSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(thunderSound);
                    }
                    
                    // 添加击退效果
                    Rigidbody2D playerRb = target.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;
                        if (knockbackDirection.magnitude < 0.1f)
                        {
                            knockbackDirection = Vector2.up;
                        }
                        playerRb.AddForce(knockbackDirection * 500f, ForceMode2D.Impulse);
                    }
                    
                    // 创建冲击特效
                    if (groundImpactEffectPrefab != null)
                    {
                        GameObject impactEffect = Instantiate(groundImpactEffectPrefab, transform.position, Quaternion.identity);
                        Destroy(impactEffect, 2f);
                    }
                    
                    break; // 只伤害一次
                }
                else
                {
                    Debug.LogWarning($"Player found but no HeroLife component!");
                }
            }
        }
        
        if (!hasDealtDamage)
        {
            Debug.LogWarning("No valid targets found for lightning strike damage");
            
            // 尝试直接查找Player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                Debug.Log($"Player found at distance: {distance}, Strike range: {strikeRange}");
                Debug.Log($"Player layer: {player.layer} ({LayerMask.LayerToName(player.layer)})");
            }
            else
            {
                Debug.LogError("No GameObject with Player tag found in scene!");
            }
        }
    }
    
    // 可以从动画事件调用的音效方法
    public void PlayLightningSound()
    {
        if (lightningSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lightningSound);
            Debug.Log("Lightning sound played from animation event");
        }
    }
    
    public void PlayThunderSound()
    {
        if (thunderSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(thunderSound);
            Debug.Log("Thunder sound played from animation event");
        }
    }
    
    // 绘制攻击范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, strikeRange);
        
        // 绘制雷电路径
        Gizmos.color = Color.cyan;
        Vector3 skyPosition = new Vector3(targetPosition.x, targetPosition.y + 15f, targetPosition.z);
        Gizmos.DrawLine(skyPosition, targetPosition);
    }
}
