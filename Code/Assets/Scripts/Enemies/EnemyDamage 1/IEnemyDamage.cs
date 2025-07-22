using UnityEngine;

/// <summary>
/// 敌人伤害接口，用于统一管理不同敌人的伤害值
/// </summary>
public interface IEnemyDamage
{
    /// <summary>
    /// 获取敌人造成的伤害值
    /// </summary>
    /// <returns>伤害值</returns>
    int GetDamage();
    
    /// <summary>
    /// 获取敌人类型名称（可选，用于调试）
    /// </summary>
    /// <returns>敌人类型名称</returns>
    string GetEnemyType();
}
