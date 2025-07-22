using System.Collections;
using UnityEngine;

public class BossSlimeProjectile : MonoBehaviour
{
    private int damage;
    private Vector2 direction;
    private LayerMask targetLayer;
    private float speed;
    private bool isHoming;
    private bool hasHitTarget = false;
    private Transform target;
    private float homingStrength = 2f;
    private float homingUpdateDelay = 0.1f;
    private float lastHomingUpdate = 0f;
    
    // 组件
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    public void Initialize(int damage, Vector2 direction, LayerMask targetLayer, float speed, bool isHoming = false)
    {
        this.damage = damage;
        this.direction = direction;
        this.targetLayer = targetLayer;
        this.speed = speed;
        this.isHoming = isHoming;
        
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 如果是追踪弹幕，寻找目标
        if (isHoming)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("BossSlime弹幕已初始化追踪目标");
            }
        }
        
        // 设置初始速度
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // 处理精灵翻转
        HandleSpriteFlipping(direction);
        
        Debug.Log($"BossSlime弹幕已初始化 - 伤害: {damage}, 方向: {direction}, 追踪: {isHoming}");
    }
    
    private void HandleSpriteFlipping(Vector2 moveDirection)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection.x < 0;
        }
    }
    
    private void Update()
    {
        if (hasHitTarget) return;
        
        // 处理追踪行为
        if (isHoming && target != null)
        {
            UpdateHomingBehavior();
        }
        
        // 根据速度更新精灵翻转
        if (rb != null && spriteRenderer != null)
        {
            spriteRenderer.flipX = rb.velocity.x < 0;
        }
    }
    
    private void UpdateHomingBehavior()
    {
        if (Time.time - lastHomingUpdate < homingUpdateDelay) return;
        
        lastHomingUpdate = Time.time;
        
        // 计算朝向目标的方向
        Vector2 targetDirection = (target.position - transform.position).normalized;
        
        // 应用追踪
        if (rb != null)
        {
            Vector2 currentVelocity = rb.velocity;
            Vector2 newVelocity = Vector2.Lerp(currentVelocity.normalized, targetDirection, homingStrength * Time.deltaTime);
            rb.velocity = newVelocity * speed;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitTarget) return;
        
        Debug.Log($"BossSlime弹幕击中: {collision.name}, 层级: {collision.gameObject.layer}, 标签: {collision.tag}");
        
        // 检查是否击中玩家
        bool hitPlayer = false;
        
        // 按层级检查
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            hitPlayer = true;
        }
        
        // 按标签检查（备用）
        if (collision.CompareTag("Player"))
        {
            hitPlayer = true;
        }
        
        if (hitPlayer)
        {
            // 对玩家造成伤害
            HeroLife playerLife = collision.GetComponent<HeroLife>();
            if (playerLife != null)
            {
                playerLife.TakeDamage(damage);
                hasHitTarget = true;
                Debug.Log($"BossSlime弹幕对玩家造成了 {damage} 点伤害");
                
                // 创建击中特效
                CreateHitEffect();
            }
            else
            {
                Debug.LogWarning("击中了玩家但未找到HeroLife组件！");
            }
            
            // 销毁弹幕
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall") || collision.CompareTag("Platform"))
        {
            // 击中环境
            Debug.Log("BossSlime弹幕击中环境，正在销毁");
            Destroy(gameObject);
        }
    }
    
    private void CreateHitEffect()
    {
        // 创建简单的击中特效
        GameObject hitEffect = new GameObject("ProjectileHitEffect");
        hitEffect.transform.position = transform.position;
        
        // 如果需要，可以在这里添加粒子系统或精灵特效
        // 现在只是创建一个简单的扩散圆形
        SpriteRenderer effectSprite = hitEffect.AddComponent<SpriteRenderer>();
        
        // 创建简单的圆形纹理
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        Vector2 center = new Vector2(16, 16);
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 15)
                {
                    colors[y * 32 + x] = new Color(1f, 0.5f, 0f, 0.8f); // 橙色
                }
                else
                {
                    colors[y * 32 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite effectSprite2 = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        effectSprite.sprite = effectSprite2;
        effectSprite.sortingOrder = 10;
        
        // 动画播放特效
        StartCoroutine(AnimateHitEffect(hitEffect));
    }
    
    private IEnumerator AnimateHitEffect(GameObject effect)
    {
        float duration = 0.3f;
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * 1.5f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            effect.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = sr.color;
                color.a = 1f - t;
                sr.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(effect);
    }
    
    private void OnBecameInvisible()
    {
        // 弹幕离开屏幕时销毁
        Destroy(gameObject);
    }
}