using UnityEngine;

/// <summary>
/// 示例脚本，演示如何使用集成后的 EnemyLifeBar 和 EnemyLife 系统
/// </summary>
public class EnemyLifeBarExample : MonoBehaviour
{
    [Header("示例设置")]
    [SerializeField] private bool enableExample = true;
    [SerializeField] private KeyCode damageKey = KeyCode.D;
    [SerializeField] private KeyCode healKey = KeyCode.H;
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    
    [Header("测试参数")]
    [SerializeField] private int damageAmount = 5;
    [SerializeField] private int healAmount = 3;
    
    private EnemyLife enemyLife;
    private EnemyLifeBar enemyLifeBar;
    
    void Start()
    {
        if (!enableExample) return;
        
        // 获取组件
        enemyLife = GetComponent<EnemyLife>();
        enemyLifeBar = GetComponent<EnemyLifeBar>();
        
        if (enemyLife == null)
        {
            Debug.LogWarning($"EnemyLifeBarExample on {gameObject.name} couldn't find EnemyLife component!");
            return;
        }
        
        if (enemyLifeBar == null)
        {
            Debug.LogWarning($"EnemyLifeBarExample on {gameObject.name} couldn't find EnemyLifeBar component!");
            return;
        }
        
        // 订阅事件以显示日志
        SubscribeToEvents();
        
        // 显示使用说明
        Debug.Log($"=== EnemyLifeBar Integration Example ===");
        Debug.Log($"Press {damageKey} to damage enemy ({damageAmount} damage)");
        Debug.Log($"Press {healKey} to heal enemy ({healAmount} heal)");
        Debug.Log($"Press {resetKey} to reset enemy to full health");
        Debug.Log($"Enemy: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth} HP");
        Debug.Log($"=========================================");
    }
    
    void Update()
    {
        if (!enableExample) return;
        
        // 按键控制
        if (Input.GetKeyDown(damageKey))
        {
            TestDamage();
        }
        
        if (Input.GetKeyDown(healKey))
        {
            TestHeal();
        }
        
        if (Input.GetKeyDown(resetKey))
        {
            TestReset();
        }
    }
    
    private void TestDamage()
    {
        if (enemyLife != null && !enemyLife.IsDead)
        {
            enemyLife.TakeDamage(damageAmount);
            Debug.Log($"Applied {damageAmount} damage to {gameObject.name}");
        }
    }
    
    private void TestHeal()
    {
        if (enemyLife != null && !enemyLife.IsDead)
        {
            enemyLife.Heal(healAmount);
            Debug.Log($"Applied {healAmount} healing to {gameObject.name}");
        }
    }
    
    private void TestReset()
    {
        if (enemyLife != null)
        {
            enemyLife.RestoreToFullHealth();
            Debug.Log($"Restored {gameObject.name} to full health");
        }
    }
    
    private void SubscribeToEvents()
    {
        if (enemyLife != null)
        {
            enemyLife.OnHealthChanged += OnHealthChanged;
            enemyLife.OnDamageTaken += OnDamageTaken;
            enemyLife.OnHealed += OnHealed;
            enemyLife.OnDeath += OnDeath;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (enemyLife != null)
        {
            enemyLife.OnHealthChanged -= OnHealthChanged;
            enemyLife.OnDamageTaken -= OnDamageTaken;
            enemyLife.OnHealed -= OnHealed;
            enemyLife.OnDeath -= OnDeath;
        }
    }
    
    // 事件处理
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        Debug.Log($"[{gameObject.name}] Health: {currentHealth}/{maxHealth} ({healthPercent:P1}) - Bar width should be {healthPercent:P1}");
    }
    
    private void OnDamageTaken(int damage)
    {
        Debug.Log($"[{gameObject.name}] Took {damage} damage - Bar width should decrease");
    }
    
    private void OnHealed(int healAmount)
    {
        Debug.Log($"[{gameObject.name}] Healed {healAmount} HP - Bar width should increase");
    }
    
    private void OnDeath()
    {
        Debug.Log($"[{gameObject.name}] Died - Bar should be hidden");
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    // 公共方法用于外部调用
    public void DamageEnemy(int damage)
    {
        if (enemyLife != null)
        {
            enemyLife.TakeDamage(damage);
        }
    }
    
    public void HealEnemy(int heal)
    {
        if (enemyLife != null)
        {
            enemyLife.Heal(heal);
        }
    }
    
    public void ResetEnemy()
    {
        if (enemyLife != null)
        {
            enemyLife.RestoreToFullHealth();
        }
    }
}