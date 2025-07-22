using UnityEngine;

/// <summary>
/// BossDemon与BossLife集成验证脚本
/// 用于测试和验证集成系统的正确性
/// </summary>
public class BossDemonBossLifeIntegrationValidator : MonoBehaviour
{
    [Header("验证设置")]
    public BossDemon targetBossDemon;
    public bool enableAutoValidation = true;
    public bool showDebugInfo = true;
    
    [Header("测试参数")]
    public int testDamageAmount = 50;
    public float validationInterval = 2f;
    
    private BossLife bossLife;
    private bool integrationValid = false;
    private float lastValidationTime = 0f;
    
    void Start()
    {
        if (targetBossDemon == null)
        {
            targetBossDemon = FindObjectOfType<BossDemon>();
        }
        
        if (targetBossDemon == null)
        {
            Debug.LogError("BossDemonBossLifeIntegrationValidator: 未找到BossDemon目标！");
            return;
        }
        
        bossLife = targetBossDemon.GetComponent<BossLife>();
        
        ValidateIntegration();
        
        if (enableAutoValidation)
        {
            InvokeRepeating(nameof(ValidateIntegration), validationInterval, validationInterval);
        }
        
        Debug.Log("BossDemon与BossLife集成验证系统已启动");
        Debug.Log("按键说明：");
        Debug.Log("V - 手动验证集成");
        Debug.Log("D - 测试伤害处理");
        Debug.Log("H - 测试血量同步");
        Debug.Log("A - 测试受伤动画");
        Debug.Log("S - 测试出生状态");
    }
    
    void Update()
    {
        if (targetBossDemon == null) return;
        
        // V - 手动验证集成
        if (Input.GetKeyDown(KeyCode.V))
        {
            ValidateIntegration();
        }
        
        // D - 测试伤害处理
        if (Input.GetKeyDown(KeyCode.D))
        {
            TestDamageProcessing();
        }
        
        // H - 测试血量同步
        if (Input.GetKeyDown(KeyCode.H))
        {
            TestHealthSync();
        }
        
        // A - 测试受伤动画
        if (Input.GetKeyDown(KeyCode.A))
        {
            TestHurtAnimation();
        }
        
        // S - 测试出生状态
        if (Input.GetKeyDown(KeyCode.S))
        {
            TestSpawnState();
        }
    }
    
    /// <summary>
    /// 验证集成完整性
    /// </summary>
    [ContextMenu("验证集成")]
    public void ValidateIntegration()
    {
        if (targetBossDemon == null)
        {
            Debug.LogError("验证失败：未找到目标BossDemon");
            integrationValid = false;
            return;
        }
        
        Debug.Log("=== 开始验证BossDemon与BossLife集成 ===");
        
        bool allChecksPass = true;
        
        // 检查1：BossLife组件存在
        if (bossLife == null)
        {
            bossLife = targetBossDemon.GetComponent<BossLife>();
        }
        
        if (bossLife != null)
        {
            Debug.Log("? BossLife组件存在");
        }
        else
        {
            Debug.LogError("? BossLife组件缺失");
            allChecksPass = false;
        }
        
        // 检查2：属性访问正确
        try
        {
            int currentHealth = targetBossDemon.CurrentHealth;
            int maxHealth = targetBossDemon.MaxHealth;
            float healthPercentage = targetBossDemon.HealthPercentage;
            bool isDead = targetBossDemon.IsDead;
            bool isSpawning = targetBossDemon.IsSpawning;
            
            Debug.Log($"? 属性访问正常 - 血量: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%), 死亡: {isDead}, 出生: {isSpawning}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"? 属性访问失败: {e.Message}");
            allChecksPass = false;
        }
        
        // 检查3：事件系统
        bool hasHealthChangedEvent = HasEvent(targetBossDemon, "OnHealthChanged");
        bool hasDeathEvent = HasEvent(targetBossDemon, "OnDeath");
        bool hasSpawnCompleteEvent = HasEvent(targetBossDemon, "OnSpawnComplete");
        
        if (hasHealthChangedEvent && hasDeathEvent && hasSpawnCompleteEvent)
        {
            Debug.Log("? 事件系统完整");
        }
        else
        {
            Debug.LogWarning("? 事件系统可能不完整");
        }
        
        // 检查4：血量值一致性
        if (bossLife != null)
        {
            bool healthConsistent = (targetBossDemon.CurrentHealth == bossLife.CurrentHealth) &&
                                   (targetBossDemon.MaxHealth == bossLife.MaxHealth);
            
            if (healthConsistent)
            {
                Debug.Log("? 血量值一致性正确");
            }
            else
            {
                Debug.LogError($"? 血量值不一致 - BossDemon: {targetBossDemon.CurrentHealth}/{targetBossDemon.MaxHealth}, BossLife: {bossLife.CurrentHealth}/{bossLife.MaxHealth}");
                allChecksPass = false;
            }
        }
        
        // 检查5：死亡状态一致性
        if (bossLife != null)
        {
            bool deathConsistent = (targetBossDemon.IsDead == bossLife.IsDead);
            
            if (deathConsistent)
            {
                Debug.Log("? 死亡状态一致性正确");
            }
            else
            {
                Debug.LogError($"? 死亡状态不一致 - BossDemon: {targetBossDemon.IsDead}, BossLife: {bossLife.IsDead}");
                allChecksPass = false;
            }
        }
        
        // 检查6：出生状态功能
        bool spawnStateValid = !targetBossDemon.IsSpawning || targetBossDemon.IsSpawning;
        
        if (spawnStateValid)
        {
            Debug.Log("? 出生状态功能正常");
        }
        else
        {
            Debug.LogError("? 出生状态功能异常");
            allChecksPass = false;
        }
        
        integrationValid = allChecksPass;
        lastValidationTime = Time.time;
        
        if (integrationValid)
        {
            Debug.Log("=== 集成验证通过 ===");
        }
        else
        {
            Debug.LogError("=== 集成验证失败 ===");
        }
    }
    
    /// <summary>
    /// 测试伤害处理
    /// </summary>
    private void TestDamageProcessing()
    {
        if (targetBossDemon == null || targetBossDemon.IsDead)
        {
            Debug.LogWarning("无法测试伤害处理：目标不存在或已死亡");
            return;
        }
        
        Debug.Log("=== 测试伤害处理 ===");
        
        int healthBefore = targetBossDemon.CurrentHealth;
        Debug.Log($"伤害前血量: {healthBefore}");
        
        targetBossDemon.TakeDamage(testDamageAmount);
        
        // 等待一帧确保处理完成
        StartCoroutine(ValidateDamageAfterFrame(healthBefore));
    }
    
    /// <summary>
    /// 等待一帧后验证伤害
    /// </summary>
    private System.Collections.IEnumerator ValidateDamageAfterFrame(int healthBefore)
    {
        yield return null;
        
        int healthAfter = targetBossDemon.CurrentHealth;
        int expectedHealth = healthBefore - testDamageAmount;
        
        Debug.Log($"伤害后血量: {healthAfter}");
        Debug.Log($"期望血量: {expectedHealth}");
        
        if (healthAfter == expectedHealth || healthAfter == 0) // 考虑血量不能为负
        {
            Debug.Log("? 伤害处理正确");
        }
        else
        {
            Debug.LogError("? 伤害处理异常");
        }
    }
    
    /// <summary>
    /// 测试血量同步
    /// </summary>
    private void TestHealthSync()
    {
        if (targetBossDemon == null || bossLife == null)
        {
            Debug.LogWarning("无法测试血量同步：组件缺失");
            return;
        }
        
        Debug.Log("=== 测试血量同步 ===");
        
        Debug.Log($"BossDemon血量: {targetBossDemon.CurrentHealth}/{targetBossDemon.MaxHealth}");
        Debug.Log($"BossLife血量: {bossLife.CurrentHealth}/{bossLife.MaxHealth}");
        
        bool syncCorrect = (targetBossDemon.CurrentHealth == bossLife.CurrentHealth) &&
                          (targetBossDemon.MaxHealth == bossLife.MaxHealth) &&
                          (targetBossDemon.IsDead == bossLife.IsDead);
        
        if (syncCorrect)
        {
            Debug.Log("? 血量同步正确");
        }
        else
        {
            Debug.LogError("? 血量同步异常");
        }
    }
    
    /// <summary>
    /// 测试受伤动画
    /// </summary>
    private void TestHurtAnimation()
    {
        if (targetBossDemon == null || targetBossDemon.IsDead)
        {
            Debug.LogWarning("无法测试受伤动画：目标不存在或已死亡");
            return;
        }
        
        Debug.Log("=== 测试受伤动画 ===");
        
        // 造成少量伤害以触发受伤动画
        int smallDamage = 1;
        targetBossDemon.TakeDamage(smallDamage);
        
        Debug.Log("受伤动画已触发，请观察Boss是否播放受伤动画");
    }
    
    /// <summary>
    /// 测试出生状态
    /// </summary>
    private void TestSpawnState()
    {
        if (targetBossDemon == null)
        {
            Debug.LogWarning("无法测试出生状态：目标不存在");
            return;
        }
        
        Debug.Log("=== 测试出生状态 ===");
        
        bool isSpawning = targetBossDemon.IsSpawning;
        Debug.Log($"当前出生状态: {(isSpawning ? "正在出生" : "已完成出生")}");
        
        if (isSpawning)
        {
            Debug.Log("Boss正在出生中，此时应该：");
            Debug.Log("1. 不受伤害");
            Debug.Log("2. 不进行AI攻击");
            Debug.Log("3. 播放出生动画");
        }
        else
        {
            Debug.Log("Boss已完成出生，应该能够正常战斗");
        }
    }
    
    /// <summary>
    /// 检查对象是否有指定的事件
    /// </summary>
    private bool HasEvent(object obj, string eventName)
    {
        if (obj == null) return false;
        
        var eventInfo = obj.GetType().GetEvent(eventName);
        return eventInfo != null;
    }
    
    /// <summary>
    /// 显示验证UI
    /// </summary>
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 340, 300));
        
        GUILayout.Label("=== 集成验证系统 ===");
        
        if (targetBossDemon != null)
        {
            GUILayout.Label($"目标: {targetBossDemon.name}");
            GUILayout.Label($"血量: {targetBossDemon.CurrentHealth}/{targetBossDemon.MaxHealth}");
            GUILayout.Label($"状态: {(targetBossDemon.IsDead ? "死亡" : "存活")}");
            GUILayout.Label($"出生状态: {(targetBossDemon.IsSpawning ? "正在出生" : "已完成出生")}");
            GUILayout.Label($"第二阶段: {(targetBossDemon.IsInPhase2 ? "是" : "否")}");
        }
        else
        {
            GUILayout.Label("目标: 未找到");
        }
        
        GUILayout.Space(10);
        
        // 集成状态
        GUILayout.Label("=== 集成状态 ===");
        GUILayout.Label($"BossLife组件: {(bossLife != null ? "?" : "?")}");
        GUILayout.Label($"集成验证: {(integrationValid ? "? 通过" : "? 失败")}");
        GUILayout.Label($"上次验证: {(lastValidationTime > 0 ? $"{Time.time - lastValidationTime:F1}s前" : "未验证")}");
        
        GUILayout.Space(10);
        
        // 控制说明
        GUILayout.Label("=== 控制说明 ===");
        GUILayout.Label("V - 手动验证");
        GUILayout.Label("D - 测试伤害");
        GUILayout.Label("H - 测试血量同步");
        GUILayout.Label("A - 测试受伤动画");
        GUILayout.Label("S - 测试出生状态");
        
        GUILayout.EndArea();
    }
}