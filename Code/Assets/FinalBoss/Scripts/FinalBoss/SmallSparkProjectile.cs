using System.Collections;
using UnityEngine;

public class SmallSparkProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 5f;
    
    [Header("Damage Settings")]
    [SerializeField] private int damage = 20;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;
    
    private Vector2 direction;
    private Rigidbody2D rb;
    private bool hasHit = false;
    private bool useAnimationCollider = false; // 是否使用动画碰撞器
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 0;
        playerLayer = LayerMask.GetMask("Player");
        
        // 检查是否有动画碰撞器（支持Box Collider 2D和Polygon Collider 2D）
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        
        if (boxCollider != null)
        {
            useAnimationCollider = true;
            // 确保碰撞器是trigger
            boxCollider.isTrigger = true;
            Debug.Log("SmallSpark: Using BoxCollider2D for damage detection");
        }
        else if (polyCollider != null)
        {
            useAnimationCollider = true;
            // 确保碰撞器是trigger
            polyCollider.isTrigger = true;
            Debug.Log("SmallSpark: Using PolygonCollider2D for damage detection");
        }
        else if (circleCollider != null)
        {
            useAnimationCollider = true;
            // 确保碰撞器是trigger
            circleCollider.isTrigger = true;
            Debug.Log("SmallSpark: Using CircleCollider2D for damage detection");
        }
        else
        {
            useAnimationCollider = false;
            Debug.Log("SmallSpark: No collider found for damage detection");
        }
        
        // 设置生命周期
        StartCoroutine(DestroyAfterLifetime());
    }
    
    void Update()
    {
        if (rb != null && direction != Vector2.zero)
        {
            rb.velocity = direction * speed;
        }
    }
    
    // 设置移动方向
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        
        // 设置旋转朝向
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    // 设置伤害
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    // 设置速度
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    // 设置生命周期
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        Debug.Log($"SmallSpark collided with: {other.gameObject.name} on layer {other.gameObject.layer}");
        
        // 检查是否击中玩家
        if (IsPlayerTarget(other))
        {
            Debug.Log($"SmallSpark detected player hit!");
            DealDamage(other);
            hasHit = true;
            DestroyProjectile();
            return;
        }
        
        // 检查是否击中墙壁或其他障碍物
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            Debug.Log($"SmallSpark hit obstacle: {other.gameObject.name}");
            hasHit = true;
            DestroyProjectile();
        }
    }
    
    // 检查是否为玩家目标的辅助函数
    private bool IsPlayerTarget(Collider2D other)
    {
        // 方法1: 通过Layer检测
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            return true;
        }
        
        // 方法2: 通过Tag检测
        if (other.CompareTag("Player"))
        {
            return true;
        }
        
        // 方法3: 通过组件检测
        HeroLife heroLife = other.GetComponent<HeroLife>();
        if (heroLife != null)
        {
            return true;
        }
        
        return false;
    }
    
    // 造成伤害 - 仿照LightningStrike的DealDamage函数
    private void DealDamage(Collider2D target)
    {
        HeroLife heroLife = target.GetComponent<HeroLife>();
        if (heroLife != null && !heroLife.IsDead)
        {
            heroLife.TakeDamage(damage);
            Debug.Log($"SmallSpark dealt {damage} damage to player");
            
            // 生成击中特效
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // 添加轻微击退效果
            Rigidbody2D playerRb = target.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDirection = direction.normalized;
                if (knockbackDirection.magnitude < 0.1f)
                {
                    knockbackDirection = Vector2.right;
                }
                playerRb.AddForce(knockbackDirection * 3f, ForceMode2D.Impulse);
            }
        }
    }
    
    // 可以从动画事件中调用的方法 - 仿照LightningStrike
    public void TriggerDamage()
    {
        Debug.Log("=== SmallSpark damage triggered from animation event ===");
        Debug.Log($"Position: {transform.position}");
        
        if (hasHit)
        {
            Debug.Log("SmallSpark damage already dealt, skipping");
            return;
        }
        
        // 检测附近的玩家
        Collider2D[] nearbyTargets = Physics2D.OverlapCircleAll(transform.position, 0.5f, playerLayer);
        
        foreach (Collider2D target in nearbyTargets)
        {
            if (IsPlayerTarget(target))
            {
                Debug.Log($"SmallSpark found player target in animation event");
                DealDamage(target);
                hasHit = true;
                DestroyProjectile();
                return;
            }
        }
    }
    
    // 备用碰撞检测方法
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        
        Collider2D other = collision.collider;
        Debug.Log($"SmallSpark collision with: {other.gameObject.name} on layer {other.gameObject.layer}");
        
        // 检查是否击中玩家
        if (IsPlayerTarget(other))
        {
            Debug.Log($"SmallSpark collision detected player hit!");
            DealDamage(other);
            hasHit = true;
            DestroyProjectile();
            return;
        }
        
        // 检查是否击中墙壁或其他障碍物
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            Debug.Log($"SmallSpark collision hit obstacle: {other.gameObject.name}");
            hasHit = true;
            DestroyProjectile();
        }
    }
    
    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        if (!hasHit)
        {
            DestroyProjectile();
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
}
