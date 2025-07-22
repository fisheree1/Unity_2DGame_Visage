using UnityEngine;

/// <summary>
/// 敌人伤害组件，实现IEnemyDamage接口
/// 可以添加到任何敌人游戏对象上来定义其伤害值
/// </summary>
public class EnemyDamageComponent : MonoBehaviour, IEnemyDamage
{
    [Header("Enemy Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private string enemyType = "Enemy";
    
    [Header("Optional Settings")]
    [SerializeField] private bool disableAfterHit = false;
    [SerializeField] private float disableDuration = 1f;

    public int GetDamage()
    {
        return damageAmount;
    }

    public string GetEnemyType()
    {
        return enemyType;
    }

    /// <summary>
    /// 设置伤害值（可在运行时动态调整）
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damageAmount = newDamage;
    }

    /// <summary>
    /// 在造成伤害后的处理（可选）
    /// </summary>
    public void OnDamageDealt()
    {
        if (disableAfterHit)
        {
            StartCoroutine(DisableTemporarily());
        }
    }

    private System.Collections.IEnumerator DisableTemporarily()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(disableDuration);
        gameObject.SetActive(true);
    }
}
