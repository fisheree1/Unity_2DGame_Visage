using System.Collections;
using UnityEngine;

using System;
using UnityEngine.UIElements;

public class BossLife : MonoBehaviour
{
    [Header("Health Settings")]

    [SerializeField] private int bossID;

    [SerializeField] private GameObject portal;
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;
    
    // Events for UI updates
    public event Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public event Action OnDeath;

    [Header("Damage Response")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float invulnerabilityTime = 0.8f;

    [Header("Death Settings")]
    [SerializeField] private float deathAnimationDuration = 2.0f;
    [SerializeField] private GameObject deathEffect;

    [Header("UI References")]
    [SerializeField] private Slider healthBar;

    [Header("Drop Settings")]
    [SerializeField] private GameObject[] possibleDrops;
    [SerializeField] private float dropChance = 0.8f;

    [Header("Player Death Detection")]
    [SerializeField] private bool healOnPlayerDeath = true;  // 是否在玩家死亡时回血
    [SerializeField] private float healDelay = 1f;           // 回血延迟时间

    // Components
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private BossMove bossMove;
    private Color originalColor;
    private bool isDead = false;
    private bool isInvulnerable = false;
    private GameObject player;  // 玩家对象引用

    // Properties
    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => isInvulnerable;

    private void Start()
    {
        portal.SetActive(false);
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossMove = GetComponent<BossMove>();
        
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
            
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        // 查找玩家对象
        FindPlayer();
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Boss Life: Player not found!");
        }
    }

    private void Update()
    {
        // 检测玩家死亡
        if (healOnPlayerDeath && player != null)
        {
            CheckPlayerDeath();
        }
    }

    private void CheckPlayerDeath()
    {
        // 检查玩家是否有HeroLife组件并且已死亡
        HeroLife playerLife = player.GetComponent<HeroLife>();
        if (playerLife != null && playerLife.IsDead && !isDead)
        {
            StartCoroutine(HealOnPlayerDeathCoroutine());
        }
    }

    private IEnumerator HealOnPlayerDeathCoroutine()
    {
        // 等待一段时间后回血
        yield return new WaitForSeconds(healDelay);
        
        // 检查玩家是否仍然死亡且Boss还活着
        if (player != null && !isDead)
        {
            HeroLife playerLife = player.GetComponent<HeroLife>();
            if (playerLife != null && playerLife.IsDead)
            {
                // 回满血
                HealToFull();
                Debug.Log("Boss healed to full health due to player death!");
            }
        }
    }

    public void HealToFull()
    {
        if (isDead) return;
        
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"Boss healed to full health: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable)
            return;
            
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthBar();
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Flash red when taking damage
        if (spriteRenderer != null)
            StartCoroutine(DamageFlashEffect());
            
        // Check if boss is hurt and apply hurt animation
        if (bossMove != null)
            bossMove.TakeHurt();

        // Start invulnerability
        StartCoroutine(InvulnerabilitySequence());
            
        // Check if boss is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxHealth;
        }
    }

    private IEnumerator InvulnerabilitySequence()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // 触发死亡事件
        OnDeath?.Invoke();
        
        // 启用传送门
        if (portal != null)
        {
            portal.SetActive(true);
            Debug.Log("Portal enabled due to boss death!");
        }
        
        // Spawn death effect if available
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Disable components to prevent further actions
        if (bossMove != null)
            bossMove.enabled = false;
            
        if (GetComponent<BossAttack>() != null)
            GetComponent<BossAttack>().enabled = false;
            
        // Disable collider to prevent further interactions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
            
        // Drop item with specified chance
        if (possibleDrops.Length > 0 && UnityEngine.Random.value <= dropChance)
        {
            int dropIndex = UnityEngine.Random.Range(0, possibleDrops.Length);
            Instantiate(possibleDrops[dropIndex], transform.position, Quaternion.identity);
        }
        
        // Destroy boss after delay
        Destroy(gameObject, deathAnimationDuration);
    }

    private IEnumerator DamageFlashEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;
            
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthBar();
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 获取生命值百分比
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}