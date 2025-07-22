using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] public int damage = 1;
    [SerializeField] private LayerMask enemyLayers = -1;
    
    [Header("Attack Type")]
    [SerializeField] private bool isBasicAttack = true;
    [SerializeField] private bool isSpecialAttack = false;
    [SerializeField] private bool isUpAttack = false;
    [SerializeField] private bool isDownAttack = false;
    
    // Components
    private Collider2D hitboxCollider;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();
    
    void Start()
    {
        hitboxCollider = GetComponent<Collider2D>();
        
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
        }
        
        SetHitboxActive(false);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 防止攻击Hero自己
        if (IsHeroRelated(other))
        {
            return;
        }
        
        // 检查是否是敌人且未被攻击过
        if (IsEnemy(other) && !hitEnemies.Contains(other))
        {
            hitEnemies.Add(other);
            DealDamage(other);
        }
    }

    private bool IsHeroRelated(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.CompareTag("PlayerAttack");
    }
    
    private bool IsEnemy(Collider2D collider)
    {
        // 排除Hero相关对象
        if (IsHeroRelated(collider))
        {
            return false;
        }
        
        // 检查LayerMask
        if (enemyLayers.value != -1 && (enemyLayers.value & (1 << collider.gameObject.layer)) == 0)
        {
            return false;
        }
        
        // 检查敌人组件
        var enemyComponent = GetEnemyComponent(collider.gameObject);
        return enemyComponent != null;
    }
    
    private Component GetEnemyComponent(GameObject obj)
    {
        var components = obj.GetComponents<Component>();
        
        foreach (var comp in components)
        {
            if (comp == null) continue;
            
            var type = comp.GetType();
            
            // 检查敌人特征
            bool hasTakeDamage = type.GetMethod("TakeDamage") != null;
            bool hasIsDead = type.GetProperty("IsDead") != null;
            
            if (hasTakeDamage && hasIsDead)
            {
                // 检查是否还活着
                var isDeadProperty = type.GetProperty("IsDead");
                bool isDead = (bool)isDeadProperty.GetValue(comp);
                
                return !isDead ? comp : null;
            }
        }
        
        return null;
    }
    
    private void DealDamage(Collider2D enemy)
    {
        var enemyComponent = GetEnemyComponent(enemy.gameObject);
        if (enemyComponent != null)
        {
            // 通过反射调用TakeDamage
            var takeDamageMethod = enemyComponent.GetType().GetMethod("TakeDamage");
            takeDamageMethod?.Invoke(enemyComponent, new object[] { damage });
        }
    }

    
    public void SetHitboxActive(bool active)
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = active;
        }
        
        if (active)
        {
            hitEnemies.Clear();
        }
    }
    
    public void ResetAttackHitbox()
    {
        hitEnemies.Clear();
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }
    
    // 设置攻击伤害
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    // 获取攻击类型
    public bool IsSpecialAttack => isSpecialAttack;
    public bool IsBasicAttack => isBasicAttack;
    public bool IsUpAttack => isUpAttack;
    public bool IsDownAttack => isDownAttack;
}
