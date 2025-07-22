using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask targetLayer = -1;
    
    [Header("Visual")]
    [SerializeField] private Color bulletColor = Color.red;
    [SerializeField] private float bulletSize = 0.2f;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool hasHit = false;
    
    void Start()
    {
        SetupComponents();
        SetupVisuals();
        
        // Set target layer if not set
        if (targetLayer == -1)
        {
            targetLayer = LayerMask.GetMask("Player");
        }
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    private void SetupComponents()
    {
        // Add Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        // Add Collider
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = bulletSize;
        
        // Add SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }
    
    private void SetupVisuals()
    {
        // Create simple bullet sprite if none exists
        if (spriteRenderer.sprite == null)
        {
            CreateBulletSprite();
        }
        
        spriteRenderer.color = bulletColor;
        transform.localScale = Vector3.one * bulletSize;
    }
    
    private void CreateBulletSprite()
    {
        // Create a simple circular sprite
        Texture2D texture = new Texture2D(16, 16);
        Color[] colors = new Color[16 * 16];
        Vector2 center = new Vector2(8, 8);
        
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 7f)
                {
                    colors[y * 16 + x] = Color.white;
                }
                else
                {
                    colors[y * 16 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = sprite;
    }
    
    public void SetDirection(Vector2 direction)
    {
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }
        
        // Rotate bullet to face direction
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb != null)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Check if hit player
        if (other.CompareTag("Player") || ((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            HitPlayer(other);
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Platform"))
        {
            HitEnvironment();
        }
    }
    
    private void HitPlayer(Collider2D player)
    {
        hasHit = true;
        
        // Deal damage
        HeroLife playerLife = player.GetComponent<HeroLife>();
        if (playerLife != null)
        {
            playerLife.TakeDamage(Mathf.RoundToInt(damage));
            Debug.Log($"Boss bullet hit player for {damage} damage");
        }
        
        // Create hit effect
        CreateHitEffect();
        
        // Destroy bullet
        Destroy(gameObject);
    }
    
    private void HitEnvironment()
    {
        hasHit = true;
        
        // Create hit effect
        CreateHitEffect();
        
        // Destroy bullet
        Destroy(gameObject);
    }
    
    private void CreateHitEffect()
    {
        // Create simple hit effect
        GameObject effect = new GameObject("BulletHitEffect");
        effect.transform.position = transform.position;
        
        // Add visual effect
        SpriteRenderer effectRenderer = effect.AddComponent<SpriteRenderer>();
        effectRenderer.sprite = CreateExplosionSprite();
        effectRenderer.color = Color.yellow;
        
        // Animate the effect
        StartCoroutine(AnimateHitEffect(effect));
    }
    
    private Sprite CreateExplosionSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        Vector2 center = new Vector2(16, 16);
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 12f)
                {
                    float alpha = 1f - (distance / 12f);
                    colors[y * 32 + x] = new Color(1f, 1f, 0f, alpha);
                }
                else
                {
                    colors[y * 32 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    private System.Collections.IEnumerator AnimateHitEffect(GameObject effect)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * 1.5f;
        
        SpriteRenderer effectRenderer = effect.GetComponent<SpriteRenderer>();
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            effect.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            Color color = effectRenderer.color;
            color.a = 1f - t;
            effectRenderer.color = color;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(effect);
    }
    
    void OnBecameInvisible()
    {
        // Destroy bullet when it goes off screen
        Destroy(gameObject);
    }
}