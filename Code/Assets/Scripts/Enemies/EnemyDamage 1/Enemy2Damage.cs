using UnityEngine;

/// <summary>
/// 敌人类型2的伤害组件
/// 适用于强力敌人，造成较高伤害
/// </summary>
public class Enemy2Damage : MonoBehaviour, IEnemyDamage
{
    [Header("Enemy2 Settings")]
    [SerializeField] private int baseDamage = 2;
    [SerializeField] private bool hasChargeAttack = true;
    [SerializeField] private int chargeAttackDamage = 3;
    [SerializeField] private float chargeAttackCooldown = 5f;
    
    private float lastChargeTime = -999f;
    private bool isChargeAttackReady = true;

    public int GetDamage()
    {
        // 检查是否可以使用冲锋攻击
        if (hasChargeAttack && isChargeAttackReady && Time.time - lastChargeTime >= chargeAttackCooldown)
        {
            lastChargeTime = Time.time;
            isChargeAttackReady = false;
            Invoke(nameof(ResetChargeAttack), chargeAttackCooldown);
            
            Debug.Log($"Enemy2 charge attack! Damage: {chargeAttackDamage}");
            return chargeAttackDamage;
        }
        
        return baseDamage;
    }

    public string GetEnemyType()
    {
        return "Enemy2 (Strong)";
    }

    private void ResetChargeAttack()
    {
        isChargeAttackReady = true;
    }

    public void SetBaseDamage(int newDamage)
    {
        baseDamage = newDamage;
    }

    public void SetChargeAttackDamage(int newDamage)
    {
        chargeAttackDamage = newDamage;
    }
}
