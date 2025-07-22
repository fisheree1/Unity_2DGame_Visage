using UnityEngine;

/// <summary>
/// 恶魔投射物
/// 处理Boss发射的各种投射物，包括火球、子弹等
/// </summary>
public class DemonProjectile : MonoBehaviour
{
    [Header("投射物设置")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private bool isContinuousDamage = false;
    [SerializeField] private float continuousDamageInterval = 0.5f;
    [SerializeField] private LayerMask targetLayer = -1;
    
    [Header("视觉效果")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private TrailRenderer trail;
    
    private bool hasHitTarget = false;
    private float lastDamageTime = 0f;
    private Rigidbody2D rb;
    private Collider2D col;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        
        // 确保投射物有必要的组件
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // 投射物不受重力影响
        }
        
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }
        
        // 设置默认目标层为玩家
        if (targetLayer == -1)
        {
            targetLayer = LayerMask.GetMask("Player");
        }
        
        // 自动销毁以防止内存泄漏
        Destroy(gameObject, 10f);
    }
    
    /// <summary>
    /// 初始化投射物
    /// </summary>
    /// <param name="damageAmount">伤害值</param>
    /// <param name="continuous">是否为持续伤害</param>
    public void Initialize(float damageAmount, bool continuous)
    {
        damage = damageAmount;
        isContinuousDamage = continuous;
        
        Debug.Log($"恶魔投射物初始化 - 伤害: {damage}, 持续: {continuous}");
    }
    
    void Update()
    {
        // 处理持续伤害
        if (isContinuousDamage && hasHitTarget)
        {
            if (Time.time - lastDamageTime >= continuousDamageInterval)
            {
                DealDamageToPlayer();
                lastDamageTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 触发器碰撞检测
    /// </summary>
    /// <param name="other">碰撞对象</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"恶魔投射物击中: {other.name}, 层级: {other.gameObject.layer}, 标签: {other.tag}");
        
        // 检查是否击中玩家
        if (IsPlayer(other))
        {
            if (!isContinuousDamage)
            {
                // 单次伤害
                DealDamageToPlayer(other);
                CreateHitEffect();
                DestroyProjectile();
            }
            else
            {
                // 开始持续伤害
                hasHitTarget = true;
                lastDamageTime = Time.time;
                DealDamageToPlayer(other);
                CreateHitEffect();
                
                // 对于持续伤害，附着到玩家身上一段时间
                transform.SetParent(other.transform);
                
                // 在持续伤害持续时间后销毁
                Destroy(gameObject, 2f);
            }
        }
        else if (IsEnvironment(other))
        {
            // 击中环境，创建爆炸效果并销毁
            CreateExplosionEffect();
            DestroyProjectile();
        }
    }
    
    /// <summary>
    /// 触发器持续碰撞检测
    /// </summary>
    /// <param name="other">碰撞对象</param>
    void OnTriggerStay2D(Collider2D other)
    {
        // 在触发器内持续时处理持续伤害
        if (isContinuousDamage && IsPlayer(other))
        {
            if (Time.time - lastDamageTime >= continuousDamageInterval)
            {
                DealDamageToPlayer(other);
                lastDamageTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 检查是否为玩家
    /// </summary>
    /// <param name="other">碰撞对象</param>
    /// <returns>如果是玩家返回true</returns>
    private bool IsPlayer(Collider2D other)
    {
        // 检查标签和层级
        return other.CompareTag("Player") || ((1 << other.gameObject.layer) & targetLayer) != 0;
    }
    
    /// <summary>
    /// 检查是否为环境物体
    /// </summary>
    /// <param name="other">碰撞对象</param>
    /// <returns>如果是环境物体返回true</returns>
    private bool IsEnvironment(Collider2D other)
    {
        return other.CompareTag("Ground") || 
               other.CompareTag("Wall") || 
               other.CompareTag("Platform") ||
               other.CompareTag("Environment");
    }
    
    /// <summary>
    /// 对玩家造成伤害
    /// </summary>
    /// <param name="playerCollider">玩家碰撞体</param>
    private void DealDamageToPlayer(Collider2D playerCollider = null)
    {
        GameObject player = playerCollider != null ? playerCollider.gameObject : GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            HeroLife playerLife = player.GetComponent<HeroLife>();
            if (playerLife != null)
            {
                playerLife.TakeDamage(Mathf.RoundToInt(damage));
                Debug.Log($"恶魔投射物对玩家造成 {damage} 点伤害");
            }
        }
    }
    
    /// <summary>
    /// 创建击中效果
    /// </summary>
    private void CreateHitEffect()
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    /// <summary>
    /// 创建爆炸效果
    /// </summary>
    private void CreateExplosionEffect()
    {
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    /// <summary>
    /// 销毁投射物
    /// </summary>
    private void DestroyProjectile()
    {
        // 禁用拖尾效果
        if (trail != null)
        {
            trail.enabled = false;
        }
        
        // 禁用碰撞器以防止进一步碰撞
        if (col != null)
        {
            col.enabled = false;
        }
        
        // 停止移动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // 立即销毁
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 当投射物变为不可见时调用
    /// </summary>
    void OnBecameInvisible()
    {
        // 投射物离开屏幕时清理
        if (!isContinuousDamage) // 不自动销毁持续伤害投射物
        {
            Destroy(gameObject);
        }
    }
}