using UnityEngine;

public class BossFireball : MonoBehaviour
{
    [Header("Fireball Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private LayerMask targetLayer = -1;
    
    [Header("Fire Effects")]
    [SerializeField] private Color fireColor = Color.red;
    [SerializeField] private float fireSize = 0.4f;
    [SerializeField] private float burnDuration = 2f;
    [SerializeField] private float burnDamagePerSecond = 5f;
    
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
        col.radius = fireSize;
        
        // Add SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }
    
    private void SetupVisuals()
    {
        // Create fireball sprite if none exists
        if (spriteRenderer.sprite == null)
        {
            CreateFireballSprite();
        }
        
        spriteRenderer.color = fireColor;
        transform.localScale = Vector3.one * fireSize;
        
        // Add some rotation for visual effect
        StartCoroutine(RotateFireball());
    }
    
    private void CreateFireballSprite()
    {
        // Create a fireball sprite with gradient
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        Vector2 center = new Vector2(16, 16);
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 14f)
                {
                    float normalizedDistance = distance / 14f;
                    Color color;
                    
                    if (normalizedDistance < 0.3f)
                    {
                        // Core: bright yellow-white
                        color = Color.Lerp(Color.white, Color.yellow, normalizedDistance / 0.3f);
                    }
                    else if (normalizedDistance < 0.7f)
                    {
                        // Middle: yellow to orange
                        color = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (normalizedDistance - 0.3f) / 0.4f);
                    }
                    else
                    {
                        // Outer: orange to red
                        color = Color.Lerp(new Color(1f, 0.5f, 0f), Color.red, (normalizedDistance - 0.7f) / 0.3f);
                    }
                    
                    // Add some transparency at the edges
                    color.a = 1f - (normalizedDistance * 0.3f);
                    colors[y * 32 + x] = color;
                }
                else
                {
                    colors[y * 32 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = sprite;
    }
    
    private System.Collections.IEnumerator RotateFireball()
    {
        while (gameObject != null)
        {
            transform.Rotate(0f, 0f, 90f * Time.deltaTime);
            yield return null;
        }
    }
    
    public void SetDirection(Vector2 direction)
    {
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
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
        
        // Deal immediate damage
        HeroLife playerLife = player.GetComponent<HeroLife>();
        if (playerLife != null)
        {
            playerLife.TakeDamage(Mathf.RoundToInt(damage));
            Debug.Log($"Boss fireball hit player for {damage} damage");
            
            // Apply burn effect
            ApplyBurnEffect(player.gameObject);
        }
        
        // Create explosion effect
        CreateExplosionEffect();
        
        // Destroy fireball
        Destroy(gameObject);
    }
    
    private void HitEnvironment()
    {
        hasHit = true;
        
        // Create explosion effect
        CreateExplosionEffect();
        
        // Destroy fireball
        Destroy(gameObject);
    }
    
    private void ApplyBurnEffect(GameObject player)
    {
        // Check if player already has burn effect
        BurnEffect existingBurn = player.GetComponent<BurnEffect>();
        if (existingBurn != null)
        {
            // Refresh burn duration
            existingBurn.RefreshBurn(burnDuration);
        }
        else
        {
            // Add new burn effect
            BurnEffect burnEffect = player.AddComponent<BurnEffect>();
            burnEffect.Initialize(burnDamagePerSecond, burnDuration);
        }
    }
    
    private void CreateExplosionEffect()
    {
        // Create fire explosion effect
        GameObject explosion = new GameObject("FireExplosion");
        explosion.transform.position = transform.position;
        
        // Add visual effect
        SpriteRenderer explosionRenderer = explosion.AddComponent<SpriteRenderer>();
        explosionRenderer.sprite = CreateExplosionSprite();
        explosionRenderer.color = Color.red;
        explosionRenderer.sortingOrder = 10;
        
        // Animate the explosion
        StartCoroutine(AnimateExplosion(explosion));
    }
    
    private Sprite CreateExplosionSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 28f)
                {
                    float normalizedDistance = distance / 28f;
                    Color color;
                    
                    if (normalizedDistance < 0.4f)
                    {
                        // Core: bright white-yellow
                        color = Color.Lerp(Color.white, Color.yellow, normalizedDistance / 0.4f);
                    }
                    else if (normalizedDistance < 0.8f)
                    {
                        // Middle: yellow to orange
                        color = Color.Lerp(Color.yellow, new Color(1f, 0.3f, 0f), (normalizedDistance - 0.4f) / 0.4f);
                    }
                    else
                    {
                        // Outer: orange to red
                        color = Color.Lerp(new Color(1f, 0.3f, 0f), Color.red, (normalizedDistance - 0.8f) / 0.2f);
                    }
                    
                    // Add transparency
                    color.a = 1f - normalizedDistance;
                    colors[y * 64 + x] = color;
                }
                else
                {
                    colors[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
    
    private System.Collections.IEnumerator AnimateExplosion(GameObject explosion)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * 2f;
        
        SpriteRenderer explosionRenderer = explosion.GetComponent<SpriteRenderer>();
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            explosion.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            Color color = explosionRenderer.color;
            color.a = 1f - t;
            explosionRenderer.color = color;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(explosion);
    }
    
    void OnBecameInvisible()
    {
        // Destroy fireball when it goes off screen
        Destroy(gameObject);
    }
}