using System;
using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int currentHealth;
    
    // 事件
    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action<int> OnDamageTaken; // (damage)
    public event Action<int> OnHealed; // (healAmount)
    
    // 属性
    public bool IsDead { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    
    private void Start()
    {
        currentHealth = maxHealth;
        IsDead = false;
        
        // 触发初始血量事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        
        int actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // 触发伤害事件
        OnDamageTaken?.Invoke(actualDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (IsDead) return;
        
        IsDead = true;
        Debug.Log($"{gameObject.name} has died!");
        
        // 触发死亡事件
        OnDeath?.Invoke();
    }
    
    public void Heal(int amount)
    {
        if (IsDead) return;
        
        int actualHeal = Mathf.Min(amount, maxHealth - currentHealth);
        if (actualHeal > 0)
        {
            currentHealth += actualHeal;
            
            // 触发治疗事件
            OnHealed?.Invoke(actualHeal);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
    
    // 强制死亡（用于外部调用）
    public void ForceDeath()
    {
        if (!IsDead)
        {
            currentHealth = 0;
            Die();
        }
    }
    
    // 设置最大血量
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // 恢复满血
    public void RestoreToFullHealth()
    {
        if (IsDead) return;
        
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // 获取血量比例
    public bool IsHealthLow(float threshold = 0.3f)
    {
        return HealthPercentage <= threshold;
    }
    
    public bool IsHealthCritical(float threshold = 0.1f)
    {
        return HealthPercentage <= threshold;
    }
}