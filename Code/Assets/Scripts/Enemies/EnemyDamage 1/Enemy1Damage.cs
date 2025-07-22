using UnityEngine;

/// <summary>
/// 敌人类型1的伤害组件
/// 适用于基础敌人，造成较低伤害
/// </summary>
public class Enemy1Damage : MonoBehaviour, IEnemyDamage
{
    [Header("Enemy1 Settings")]
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private bool canCritical = false;
    [SerializeField] private float criticalChance = 0.1f;
    [SerializeField] private int criticalMultiplier = 2;

    public int GetDamage()
    {
        int finalDamage = baseDamage;
        
        if (canCritical && Random.value < criticalChance)
        {
            finalDamage *= criticalMultiplier;
            Debug.Log($"Enemy1 critical hit! Damage: {finalDamage}");
        }
        
        return finalDamage;
    }

    public string GetEnemyType()
    {
        return "Enemy1 (Basic)";
    }

    public void SetBaseDamage(int newDamage)
    {
        baseDamage = newDamage;
    }
}
