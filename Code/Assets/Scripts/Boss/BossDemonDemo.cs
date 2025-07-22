using UnityEngine;

/// <summary>
/// 重构后的Boss演示系统
/// 展示新的counterAttack状态和阶段性攻击变化
/// </summary>
public class BossDemonDemo : MonoBehaviour
{
    [Header("演示设置")]
    public bool enableDemoControls = true;
    public bool showDebugInfo = true;
    
    private BossDemon bossDemon;
    private bool isSetup = false;
    
    void Start()
    {
        SetupDemo();
    }
    
    void Update()
    {
        if (!isSetup) return;
        
        HandleDemoControls();
    }
    
    /// <summary>
    /// 设置演示
    /// </summary>
    private void SetupDemo()
    {
        bossDemon = GetComponent<BossDemon>();
        if (bossDemon == null)
        {
            Debug.LogError("BossDemonDemo: 未找到BossDemon组件!");
            return;
        }
        
        isSetup = true;
        Debug.Log("重构后的Boss演示系统已启动!");
        Debug.Log("按键说明:");
        Debug.Log("F1 - 显示Boss状态");
        Debug.Log("F2 - 对Boss造成伤害 (50点)");
        Debug.Log("F3 - 对Boss造成伤害 (100点)");
        Debug.Log("F4 - 触发反击状态");
        Debug.Log("F5 - 强制进入第二阶段");
        Debug.Log("F6 - 模拟英雄滑铲");
        Debug.Log("F7 - 模拟英雄跳跃");
    }
    
    /// <summary>
    /// 处理演示控制
    /// </summary>
    private void HandleDemoControls()
    {
        if (!enableDemoControls) return;
        
        // F1 - 显示Boss状态
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowBossStatus();
        }
        
        // F2 - 造成50点伤害
        if (Input.GetKeyDown(KeyCode.F2))
        {
            DamageBoss(50);
        }
        
        // F3 - 造成100点伤害
        if (Input.GetKeyDown(KeyCode.F3))
        {
            DamageBoss(100);
        }
        
        // F4 - 触发反击状态
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TriggerCounterAttack();
        }
        
        // F5 - 强制进入第二阶段
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ForceEnterPhase2();
        }
        
        // F6 - 模拟英雄滑铲
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SimulateHeroSlide();
        }
        
        // F7 - 模拟英雄跳跃
        if (Input.GetKeyDown(KeyCode.F7))
        {
            SimulateHeroJump();
        }
    }
    
    /// <summary>
    /// 显示Boss状态
    /// </summary>
    private void ShowBossStatus()
    {
        if (bossDemon == null) return;
        
        Debug.Log("=== Boss状态信息 ===");
        Debug.Log($"当前血量: {bossDemon.CurrentHealth}/{bossDemon.MaxHealth} ({bossDemon.HealthPercentage * 100:F1}%)");
        Debug.Log($"当前阶段: {(bossDemon.IsInPhase2 ? "第二阶段" : "第一阶段")}");
        Debug.Log($"是否死亡: {bossDemon.IsDead}");
        
        // 显示阶段特性
        if (bossDemon.IsInPhase2)
        {
            Debug.Log("第二阶段特性:");
            Debug.Log("- 近战攻击变为火焰吐息");
            Debug.Log("- 激活英雄行为检测");
            Debug.Log("- 攻击欲望增强");
        }
        else
        {
            Debug.Log("第一阶段特性:");
            Debug.Log("- 普通近战攻击");
            Debug.Log("- Dash + 冲击波攻击");
        }
        
        Debug.Log("两阶段共有特性:");
        Debug.Log("- 反击状态（受到连续伤害后）");
        Debug.Log("- 小怪召唤（每1/3血量）");
    }
    
    /// <summary>
    /// 对Boss造成伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    private void DamageBoss(int damage)
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        
        Debug.Log($"对Boss造成{damage}点伤害");
        bossDemon.TakeDamage(damage);
        
        // 检查是否触发阶段转换
        if (!bossDemon.IsInPhase2 && bossDemon.HealthPercentage <= 0.5f)
        {
            Debug.Log("Boss进入第二阶段！攻击模式发生变化！");
        }
    }
    
    /// <summary>
    /// 触发反击状态
    /// </summary>
    private void TriggerCounterAttack()
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        
        Debug.Log("模拟连续攻击触发反击状态");
        
        // 连续造成小量伤害以触发反击
        for (int i = 0; i < 3; i++)
        {
            bossDemon.TakeDamage(5);
        }
        
        Debug.Log("反击状态触发：Boss将向后dash并释放扇形弹幕！");
    }
    
    /// <summary>
    /// 强制进入第二阶段
    /// </summary>
    private void ForceEnterPhase2()
    {
        if (bossDemon == null || bossDemon.IsDead || bossDemon.IsInPhase2) return;
        
        Debug.Log("强制进入第二阶段");
        
        // 将血量减少到50%以下
        int targetHealth = Mathf.RoundToInt(bossDemon.MaxHealth * 0.45f);
        int damageNeeded = bossDemon.CurrentHealth - targetHealth;
        
        if (damageNeeded > 0)
        {
            bossDemon.TakeDamage(damageNeeded);
        }
        
        Debug.Log("第二阶段已激活：");
        Debug.Log("- 近战攻击变为火焰吐息");
        Debug.Log("- 英雄行为检测已激活");
        Debug.Log("- 攻击欲望增强");
    }
    
    /// <summary>
    /// 模拟英雄滑铲
    /// </summary>
    private void SimulateHeroSlide()
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        
        if (!bossDemon.IsInPhase2)
        {
            Debug.Log("英雄滑铲模拟 - 但Boss不在第二阶段，不会反应");
            return;
        }
        
        Debug.Log("模拟英雄滑铲行为");
        
        // 通过HeroActionTracker模拟滑铲
        var heroTracker = bossDemon.GetComponentInChildren<HeroActionTracker>();
        if (heroTracker != null)
        {
            // 这里需要调用HeroActionTracker的模拟方法
            Debug.Log("Boss在第二阶段检测到滑铲，可能会执行dash+冲击波反应");
        }
    }
    
    /// <summary>
    /// 模拟英雄跳跃
    /// </summary>
    private void SimulateHeroJump()
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        
        if (!bossDemon.IsInPhase2)
        {
            Debug.Log("英雄跳跃模拟 - 但Boss不在第二阶段，不会反应");
            return;
        }
        
        Debug.Log("模拟英雄跳跃行为");
        
        // 通过HeroActionTracker模拟跳跃
        var heroTracker = bossDemon.GetComponentInChildren<HeroActionTracker>();
        if (heroTracker != null)
        {
            // 这里需要调用HeroActionTracker的模拟方法
            Debug.Log("Boss在第二阶段检测到跳跃，可能会执行扇形弹幕反应");
        }
    }
    
    /// <summary>
    /// 显示GUI信息
    /// </summary>
    void OnGUI()
    {
        if (!showDebugInfo || bossDemon == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        
        GUILayout.Label("=== 重构后的恶魔Boss演示 ===");
        
        // 基本信息
        GUILayout.Label($"血量: {bossDemon.CurrentHealth}/{bossDemon.MaxHealth} ({bossDemon.HealthPercentage * 100:F1}%)");
        GUILayout.Label($"阶段: {(bossDemon.IsInPhase2 ? "第二阶段" : "第一阶段")}");
        GUILayout.Label($"状态: {(bossDemon.IsDead ? "死亡" : "存活")}");
        
        GUILayout.Space(10);
        
        // 阶段特性
        if (bossDemon.IsInPhase2)
        {
            GUILayout.Label("第二阶段特性:");
            GUILayout.Label("? 近战攻击 → 火焰吐息");
            GUILayout.Label("? 英雄行为检测激活");
            GUILayout.Label("? 攻击欲望增强");
        }
        else
        {
            GUILayout.Label("第一阶段特性:");
            GUILayout.Label("? 普通近战攻击");
            GUILayout.Label("? Dash + 冲击波攻击");
        }
        
        GUILayout.Space(10);
        
        // 控制说明
        GUILayout.Label("控制说明:");
        GUILayout.Label("F1 - 显示状态 | F2 - 造成50伤害 | F3 - 造成100伤害");
        GUILayout.Label("F4 - 触发反击 | F5 - 进入第二阶段");
        GUILayout.Label("F6 - 模拟滑铲 | F7 - 模拟跳跃");
        
        GUILayout.EndArea();
    }
}