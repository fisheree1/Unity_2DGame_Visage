using UnityEngine;

/// <summary>
/// 远程攻击敌人系统总结
/// 这个脚本总结了整个远程攻击敌人系统的功能和使用方法
/// </summary>
public class RangedEnemySystemSummary : MonoBehaviour
{
    [Header("系统组件总览")]
    [TextArea(3, 10)]
    public string systemOverview = @"
远程攻击敌人系统 - 完整功能概述:

?? 核心特性:
? 五状态状态机: 待机 → 蓄力 → 攻击 → 受伤 → 死亡
? 双攻击模式: 追踪弹 + 扇形弹幕
? 智能目标检测和范围控制
? 完整的受伤和死亡状态处理

?? 攻击系统:
? 追踪弹: 单发，自动追踪玩家
? 扇形弹幕: 多发，扇形分布
? 智能模式切换: 两种攻击交替使用

?? AI系统:
? 自动目标检测 (8米范围)
? 攻击距离控制 (6米范围)
? 受伤状态中断机制
? 死亡状态完整处理

?? 状态系统:
? Idle: 待机，寻找目标
? Charge: 蓄力准备攻击
? Attack: 发射弹幕
? Hurt: 受伤，击退效果
? Dead: 死亡，清理销毁";

    [Header("快速设置指南")]
    [TextArea(3, 10)]
    public string quickSetupGuide = @"
?? 快速开始:

1. 创建GameObject命名为 'RangedEnemy'
2. 添加必需组件:
   ? RangedEnemyP (主脚本)
   ? EnemyLife (健康系统)
   ? Rigidbody2D (物理)
   ? Collider2D (碰撞检测)
   ? SpriteRenderer (渲染)
   ? Animator (动画)

3. 配置基本参数:
   ? Detection Range: 8
   ? Attack Range: 6
   ? Charge Duration: 1
   ? Attack Cooldown: 2
   ? Damage: 15
   ? Hurt Duration: 0.5
   ? Hurt Knockback Force: 5

4. 创建弹射物预制体并分配给对应字段

5. 设置动画触发器:
   ? Idle, Charge, Attack, Hurt, Death";

    [Header("系统文件结构")]
    [TextArea(3, 10)]
    public string fileStructure = @"
?? 文件结构:

Assets/Scripts/Enemies/RangedEnemy/
├── RangedEnemyP.cs              (主控制器)
├── RangedEnemyIdleState.cs      (待机状态)
├── RangedEnemyChargeState.cs    (蓄力状态)
├── RangedEnemyAttackState.cs    (攻击状态)
├── RangedEnemyHurtState.cs      (受伤状态) ?新增
├── RangedEnemyDeadState.cs      (死亡状态) ?新增
├── TrackingProjectile.cs       (追踪弹射物)
├── RangedProjectile.cs         (普通弹射物)
├── RangedEnemyExample.cs       (使用示例)
├── RangedEnemySystemSummary.cs (本文件)
└── README_RangedEnemy.md       (完整文档)";

    void Start()
    {
        Debug.Log("=== 远程攻击敌人系统已加载 ===");
        Debug.Log("系统功能:");
        Debug.Log("? 五状态状态机 (待机/蓄力/攻击/受伤/死亡)");
        Debug.Log("? 双攻击模式 (追踪弹/扇形弹幕)");
        Debug.Log("? 智能AI和碰撞检测");
        Debug.Log("? 完整的受伤和死亡处理");
        Debug.Log("? 完整的EnemyLife集成");
        Debug.Log("=====================================");
        
        // 检查场景中的远程敌人
        CheckSceneRangedEnemies();
    }

    private void CheckSceneRangedEnemies()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        
        if (enemies.Length > 0)
        {
            Debug.Log($"? 场景中发现 {enemies.Length} 个远程敌人");
            
            foreach (var enemy in enemies)
            {
                string status = enemy.IsDead ? "已死亡" : "存活";
                string hurtStatus = enemy.parameter.isHit ? " (受伤中)" : "";
                Debug.Log($"  - {enemy.name}: {status}{hurtStatus}, 健康: {enemy.CurrentHealth}/{enemy.MaxHealth}");
            }
        }
        else
        {
            Debug.Log("? 场景中没有发现远程敌人");
            Debug.Log("?? 提示: 使用 RangedEnemyExample.cs 中的 '创建示例远程敌人' 功能");
        }
    }

    [ContextMenu("显示系统统计")]
    public void ShowSystemStats()
    {
        Debug.Log("=== 远程攻击敌人系统统计 ===");
        
        // 统计各种组件
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        TrackingProjectile[] trackingProjectiles = FindObjectsOfType<TrackingProjectile>();
        RangedProjectile[] rangedProjectiles = FindObjectsOfType<RangedProjectile>();
        
        Debug.Log($"活跃的远程敌人: {enemies.Length}");
        Debug.Log($"活跃的追踪弹: {trackingProjectiles.Length}");
        Debug.Log($"活跃的普通弹射物: {rangedProjectiles.Length}");
        
        // 统计敌人状态
        int idleCount = 0, chargeCount = 0, attackCount = 0, hurtCount = 0, deadCount = 0;
        
        foreach (var enemy in enemies)
        {
            if (enemy.parameter.isDead)
            {
                deadCount++;
            }
            else if (enemy.parameter.isHit)
            {
                hurtCount++;
            }
            else if (enemy.parameter.isAttacking)
            {
                attackCount++;
            }
            else if (enemy.parameter.isCharging)
            {
                chargeCount++;
            }
            else
            {
                idleCount++;
            }
        }
        
        Debug.Log($"状态分布 - 待机: {idleCount}, 蓄力: {chargeCount}, 攻击: {attackCount}, 受伤: {hurtCount}, 死亡: {deadCount}");
        Debug.Log("==============================");
    }

    [ContextMenu("测试所有远程敌人")]
    public void TestAllRangedEnemies()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogWarning("未找到玩家，无法测试");
            return;
        }
        
        Debug.Log($"开始测试 {enemies.Length} 个远程敌人");
        
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead && !enemy.parameter.isDead)
            {
                // 设置玩家为目标
                enemy.parameter.target = player.transform;
                
                // 强制进入蓄力状态
                enemy.TransitionState(RangedEnemyStateType.Charge);
                
                Debug.Log($"? 触发 {enemy.name} 的攻击测试");
            }
        }
    }

    [ContextMenu("测试受伤状态")]
    public void TestHurtState()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead && !enemy.parameter.isDead)
            {
                // 模拟受伤
                enemy.TakeDamage(1);
                Debug.Log($"? 触发 {enemy.name} 的受伤测试");
            }
        }
    }

    [ContextMenu("重置所有远程敌人")]
    public void ResetAllRangedEnemies()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead && !enemy.parameter.isDead)
            {
                enemy.parameter.target = null;
                enemy.parameter.isCharging = false;
                enemy.parameter.isAttacking = false;
                enemy.parameter.isHit = false;
                enemy.TransitionState(RangedEnemyStateType.Idle);
            }
        }
        
        Debug.Log($"已重置 {enemies.Length} 个远程敌人到待机状态");
    }

    [ContextMenu("清理所有弹射物")]
    public void ClearAllProjectiles()
    {
        TrackingProjectile[] trackingProjectiles = FindObjectsOfType<TrackingProjectile>();
        RangedProjectile[] rangedProjectiles = FindObjectsOfType<RangedProjectile>();
        
        foreach (var projectile in trackingProjectiles)
        {
            DestroyImmediate(projectile.gameObject);
        }
        
        foreach (var projectile in rangedProjectiles)
        {
            DestroyImmediate(projectile.gameObject);
        }
        
        Debug.Log($"已清理 {trackingProjectiles.Length} 个追踪弹和 {rangedProjectiles.Length} 个普通弹射物");
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制系统概览
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // 显示场景中所有远程敌人的连接
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Color lineColor = Color.green;
                if (enemy.parameter.isDead) lineColor = Color.red;
                else if (enemy.parameter.isHit) lineColor = Color.green;
                else if (enemy.parameter.isAttacking) lineColor = Color.yellow;
                else if (enemy.parameter.isCharging) lineColor = Color.blue;
                
                Gizmos.color = lineColor;
                Gizmos.DrawLine(transform.position, enemy.transform.position);
            }
        }
    }
}