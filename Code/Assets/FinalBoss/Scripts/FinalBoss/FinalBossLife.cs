using System.Collections;
using UnityEngine;
using System;

public class FinalBossLife : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 500;
    [SerializeField] private int currentHealth;
    
    // Events for UI updates
    public event Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public event Action OnDeath;

    [Header("Death Settings")]
    [SerializeField] private float deathAnimationDuration = 3.0f;
    [SerializeField] private GameObject deathEffect;

    [Header("Drop Settings")]
    [SerializeField] private GameObject[] possibleDrops;
    [SerializeField] private float dropChance = 0.9f;

    [Header("Player Death Detection")]
    [SerializeField] private bool healOnPlayerDeath = true;  // 是否在玩家死亡时回血
    [SerializeField] private float healDelay = 1f;           // 回血延迟时间

    // Components
    private Animator anim;
    private FinalBossMove finalBossMove;
    private bool isDead = false;
    private GameObject player;  // 玩家对象引用

    // Properties
    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Start()
    {
        anim = GetComponent<Animator>();
        finalBossMove = GetComponent<FinalBossMove>();
        
        currentHealth = maxHealth;
        
        // 查找玩家对象
        FindPlayer();
        
        Debug.Log($"Final Boss Life initialized with {maxHealth} health");
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Final Boss Life: Player not found!");
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
                Debug.Log("Final Boss healed to full health due to player death!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;
            
        // 检查是否无敌或冥想状态（由FinalBossMove管理）
        if (finalBossMove != null && (finalBossMove.IsInvulnerable || finalBossMove.IsMeditating))
            return;
            
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"Final Boss took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 通知FinalBossMove处理伤害
        if (finalBossMove != null)
            finalBossMove.TakeHurt();
            
        // Check if boss is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        Debug.Log("Final Boss has died!");
        
        // 触发死亡事件
        OnDeath?.Invoke();
        
        // Spawn death effect if available
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Disable components to prevent further actions
        if (finalBossMove != null)
            finalBossMove.enabled = false;
            
        if (GetComponent<FinalBossAttack>() != null)
            GetComponent<FinalBossAttack>().enabled = false;
            
        // Disable collider to prevent further interactions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
            
        // Drop item with specified chance
        if (possibleDrops.Length > 0 && UnityEngine.Random.value <= dropChance)
        {
            int dropIndex = UnityEngine.Random.Range(0, possibleDrops.Length);
            Instantiate(possibleDrops[dropIndex], transform.position, Quaternion.identity);
            Debug.Log($"Final Boss dropped item: {possibleDrops[dropIndex].name}");
        }
        
        // Destroy boss after delay
        Destroy(gameObject, deathAnimationDuration);
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;
            
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        Debug.Log($"Final Boss healed for {amount}. Current health: {currentHealth}/{maxHealth}");
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 设置最大生命值（用于不同难度或阶段）
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        
        Debug.Log($"Final Boss max health set to {maxHealth}");
    }

    // 获取生命值百分比
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public void HealToFull()
    {
        if (isDead) return;
        
        currentHealth = maxHealth;
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"Final Boss healed to full health: {currentHealth}/{maxHealth}");
    }
}
