using UnityEngine;

/// <summary>
/// 远程攻击敌人系统使用示例和说明
/// 展示如何设置和使用远程攻击敌人
/// </summary>
public class RangedEnemyExample : MonoBehaviour
{
    [Header("系统组件说明")]
    [SerializeField] private string[] systemComponents = {
        "RangedEnemyP.cs - 主要敌人控制器",
        "RangedEnemyIdleState.cs - 待机状态",
        "RangedEnemyChargeState.cs - 蓄力状态", 
        "RangedEnemyAttackState.cs - 攻击状态",
        "TrackingProjectile.cs - 追踪弹射物",
        "RangedProjectile.cs - 普通弹射物"
    };
    
    [Header("状态机流程")]
    [SerializeField] private string[] stateFlow = {
        "1. Idle - 待机，寻找目标",
        "2. Charge - 发现目标后开始蓄力",
        "3. Attack - 蓄力完成后发射弹幕",
        "4. 返回Idle - 攻击完成，进入冷却"
    };
    
    [Header("攻击模式")]
    [SerializeField] private string[] attackModes = {
        "追踪弹 - 单发，会追踪玩家",
        "扇形弹幕 - 多发，扇形分布",
        "两种模式交替使用"
    };
    
    [Header("设置示例")]
    [SerializeField] private GameObject rangedEnemyPrefab;
    [SerializeField] private GameObject trackingProjectilePrefab;
    [SerializeField] private GameObject normalProjectilePrefab;
    
    void Start()
    {
        Debug.Log("=== 远程攻击敌人系统 ===");
        Debug.Log("系统特点：");
        Debug.Log("- 三状态设计：待机、蓄力、攻击");
        Debug.Log("- 双攻击模式：追踪弹 + 扇形弹幕");
        Debug.Log("- 智能目标检测和攻击范围控制");
        Debug.Log("- 完整的碰撞检测和伤害系统");
        Debug.Log("========================");
        
        // 演示如何设置远程敌人
        DemonstrateRangedEnemySetup();
    }
    
    private void DemonstrateRangedEnemySetup()
    {
        Debug.Log("=== 远程敌人设置指南 ===");
        
        // 查找场景中的远程敌人
        RangedEnemyP[] rangedEnemies = FindObjectsOfType<RangedEnemyP>();
        
        if (rangedEnemies.Length > 0)
        {
            Debug.Log($"找到 {rangedEnemies.Length} 个远程敌人");
            
            foreach (RangedEnemyP enemy in rangedEnemies)
            {
                Debug.Log($"- 远程敌人: {enemy.name}");
                CheckEnemyConfiguration(enemy);
            }
        }
        else
        {
            Debug.Log("未找到远程敌人，显示创建指南：");
            ShowCreationGuide();
        }
    }
    
    private void CheckEnemyConfiguration(RangedEnemyP enemy)
    {
        Debug.Log($"检查 {enemy.name} 的配置：");
        
        // 检查EnemyLife组件
        EnemyLife enemyLife = enemy.GetComponent<EnemyLife>();
        if (enemyLife != null)
        {
            Debug.Log($"? EnemyLife组件存在，健康值: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth}");
        }
        else
        {
            Debug.Log("? 缺少EnemyLife组件");
        }
        
        // 检查参数配置
        if (enemy.parameter != null)
        {
            Debug.Log($"? 参数配置存在");
            Debug.Log($"  - 检测范围: {enemy.parameter.detectionRange}");
            Debug.Log($"  - 攻击范围: {enemy.parameter.attackRange}");
            Debug.Log($"  - 攻击冷却: {enemy.parameter.attackCooldown}");
            Debug.Log($"  - 蓄力时间: {enemy.parameter.chargeDuration}");
            Debug.Log($"  - 弹射物伤害: {enemy.parameter.damage}");
            
            // 检查弹射物预制体
            if (enemy.parameter.projectilePrefab != null)
            {
                Debug.Log($"? 普通弹射物预制体: {enemy.parameter.projectilePrefab.name}");
            }
            else
            {
                Debug.Log("? 缺少普通弹射物预制体");
            }
            
            if (enemy.parameter.trackingProjectilePrefab != null)
            {
                Debug.Log($"? 追踪弹射物预制体: {enemy.parameter.trackingProjectilePrefab.name}");
            }
            else
            {
                Debug.Log("? 缺少追踪弹射物预制体");
            }
        }
        else
        {
            Debug.Log("? 缺少参数配置");
        }
    }
    
    private void ShowCreationGuide()
    {
        Debug.Log("=== 远程敌人创建指南 ===");
        Debug.Log("1. 创建空GameObject，命名为'RangedEnemy'");
        Debug.Log("2. 添加以下组件：");
        Debug.Log("   - RangedEnemyP (主脚本)");
        Debug.Log("   - EnemyLife (健康系统)");
        Debug.Log("   - Rigidbody2D (物理)");
        Debug.Log("   - Collider2D (碰撞，设为Trigger)");
        Debug.Log("   - SpriteRenderer (渲染)");
        Debug.Log("   - Animator (动画)");
        Debug.Log("3. 设置参数：");
        Debug.Log("   - Detection Range: 8");
        Debug.Log("   - Attack Range: 6");
        Debug.Log("   - Charge Duration: 1");
        Debug.Log("   - Attack Cooldown: 2");
        Debug.Log("   - Damage: 15");
        Debug.Log("4. 分配弹射物预制体");
        Debug.Log("5. 设置目标层级 (Player)");
        Debug.Log("========================");
    }
    
    [ContextMenu("创建示例远程敌人")]
    public void CreateExampleRangedEnemy()
    {
        // 创建远程敌人GameObject
        GameObject enemyObj = new GameObject("RangedEnemy_Example");
        enemyObj.transform.position = transform.position + Vector3.right * 3f;
        
        // 添加基本组件
        RangedEnemyP rangedEnemy = enemyObj.AddComponent<RangedEnemyP>();
        EnemyLife enemyLife = enemyObj.AddComponent<EnemyLife>();
        Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
        CapsuleCollider2D col = enemyObj.AddComponent<CapsuleCollider2D>();
        SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
        
        // 配置物理组件
        rb.freezeRotation = true;
        rb.gravityScale = 1f;
        col.isTrigger = true;
        
        // 设置敌人标签
        enemyObj.tag = "Enemy";
        
        // 配置参数
        if (rangedEnemy.parameter == null)
        {
            rangedEnemy.parameter = new RangedEnemyParameter();
        }
        
        rangedEnemy.parameter.detectionRange = 8f;
        rangedEnemy.parameter.attackRange = 6f;
        rangedEnemy.parameter.chargeDuration = 1f;
        rangedEnemy.parameter.attackCooldown = 2f;
        rangedEnemy.parameter.damage = 15;
        rangedEnemy.parameter.projectileSpeed = 5f;
        rangedEnemy.parameter.fanProjectileCount = 5;
        rangedEnemy.parameter.fanAngle = 45f;
        rangedEnemy.parameter.targetLayer = LayerMask.GetMask("Player");
        
        // 分配弹射物预制体（如果有的话）
        if (trackingProjectilePrefab != null)
        {
            rangedEnemy.parameter.trackingProjectilePrefab = trackingProjectilePrefab;
        }
        
        if (normalProjectilePrefab != null)
        {
            rangedEnemy.parameter.projectilePrefab = normalProjectilePrefab;
        }
        
        Debug.Log("示例远程敌人创建完成！");
    }
    
    [ContextMenu("测试远程敌人攻击")]
    public void TestRangedEnemyAttack()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        
        foreach (RangedEnemyP enemy in enemies)
        {
            if (enemy.parameter != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    enemy.parameter.target = player.transform;
                    enemy.TransitionState(RangedEnemyStateType.Charge);
                    Debug.Log($"触发 {enemy.name} 的攻击测试");
                }
            }
        }
    }
} 