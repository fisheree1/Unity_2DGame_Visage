using UnityEngine;
using System.Collections;

/// <summary>
/// BossSlime转换BossDemon功能测试脚本
/// 用于验证Boss转换系统的正确性
/// </summary>
public class BossTransitionTester : MonoBehaviour
{
    [Header("测试设置")]
    public BossSlime targetBossSlime;
    public bool enableTestControls = true;
    public bool showTestUI = true;
    
    [Header("测试参数")]
    public int testDamageAmount = 50;
    public float testHealthPercentage = 0.8f;
    public GameObject testBossPrefab;
    
    private bool isTestingTransition = false;
    private bool transitionCompleted = false;
    private float transitionStartTime;
    
    void Start()
    {
        // 自动查找BossSlime如果没有指定
        if (targetBossSlime == null)
        {
            targetBossSlime = FindObjectOfType<BossSlime>();
        }
        
        if (targetBossSlime == null)
        {
            Debug.LogError("BossTransitionTester: 未找到BossSlime目标！");
            return;
        }
        
        Debug.Log("BossSlime转换BossDemon测试系统已启动");
        Debug.Log("测试控制说明：");
        Debug.Log("T - 测试Boss转换");
        Debug.Log("K - 快速击杀BossSlime");
        Debug.Log("R - 重置测试");
        Debug.Log("H - 设置测试血量百分比");
        Debug.Log("P - 设置测试预制体");
    }
    
    void Update()
    {
        if (!enableTestControls || targetBossSlime == null) return;
        
        HandleTestControls();
        MonitorTransition();
    }
    
    private void HandleTestControls()
    {
        // T - 测试Boss转换
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestBossTransition();
        }
        
        // K - 快速击杀BossSlime
        if (Input.GetKeyDown(KeyCode.K))
        {
            QuickKillBossSlime();
        }
        
        // R - 重置测试
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTest();
        }
        
        // H - 设置测试血量百分比
        if (Input.GetKeyDown(KeyCode.H))
        {
            SetTestHealthPercentage();
        }
        
        // P - 设置测试预制体
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetTestPrefab();
        }
    }
    
    /// <summary>
    /// 测试Boss转换
    /// </summary>
    private void TestBossTransition()
    {
        if (targetBossSlime == null || targetBossSlime.IsDead)
        {
            Debug.LogWarning("目标BossSlime不存在或已死亡");
            return;
        }
        
        if (isTestingTransition)
        {
            Debug.LogWarning("转换测试已在进行中");
            return;
        }
        
        Debug.Log("=== 开始测试Boss转换 ===");
        
        // 配置测试参数
        targetBossSlime.parameter.nextBossHealthPercentage = testHealthPercentage;
        targetBossSlime.parameter.enableBossTransition = true;
        
        if (testBossPrefab != null)
        {
            targetBossSlime.parameter.nextBossPrefab = testBossPrefab;
            Debug.Log($"设置测试预制体: {testBossPrefab.name}");
        }
        
        // 开始监控转换
        isTestingTransition = true;
        transitionStartTime = Time.time;
        transitionCompleted = false;
        
        // 击杀BossSlime
        int currentHealth = Mathf.RoundToInt(targetBossSlime.parameter.currentHealth);
        if (currentHealth > 0)
        {
            Debug.Log($"对BossSlime造成致命伤害: {currentHealth}");
            targetBossSlime.TakeDamage(currentHealth);
        }
        
        Debug.Log("BossSlime已被击杀，等待转换...");
    }
    
    /// <summary>
    /// 快速击杀BossSlime
    /// </summary>
    private void QuickKillBossSlime()
    {
        if (targetBossSlime == null || targetBossSlime.IsDead)
        {
            Debug.LogWarning("目标BossSlime不存在或已死亡");
            return;
        }
        
        Debug.Log("快速击杀BossSlime");
        int currentHealth = Mathf.RoundToInt(targetBossSlime.parameter.currentHealth);
        targetBossSlime.TakeDamage(currentHealth);
    }
    
    /// <summary>
    /// 重置测试
    /// </summary>
    private void ResetTest()
    {
        Debug.Log("重置转换测试");
        
        isTestingTransition = false;
        transitionCompleted = false;
        
        // 查找新的BossSlime
        targetBossSlime = FindObjectOfType<BossSlime>();
        
        if (targetBossSlime == null)
        {
            Debug.LogWarning("重置后未找到BossSlime");
        }
        else
        {
            Debug.Log("找到新的BossSlime目标");
        }
    }
    
    /// <summary>
    /// 设置测试血量百分比
    /// </summary>
    private void SetTestHealthPercentage()
    {
        // 循环设置不同的血量百分比
        float[] healthPercentages = { 0.5f, 0.7f, 0.8f, 1.0f };
        
        for (int i = 0; i < healthPercentages.Length; i++)
        {
            if (Mathf.Approximately(testHealthPercentage, healthPercentages[i]))
            {
                testHealthPercentage = healthPercentages[(i + 1) % healthPercentages.Length];
                break;
            }
        }
        
        Debug.Log($"设置测试血量百分比: {testHealthPercentage * 100:F0}%");
    }
    
    /// <summary>
    /// 设置测试预制体
    /// </summary>
    private void SetTestPrefab()
    {
        if (testBossPrefab == null)
        {
            Debug.Log("当前无测试预制体，将使用动态创建模式");
        }
        else
        {
            Debug.Log($"当前测试预制体: {testBossPrefab.name}");
        }
        
        Debug.Log("请在Inspector中设置Test Boss Prefab");
    }
    
    /// <summary>
    /// 监控转换过程
    /// </summary>
    private void MonitorTransition()
    {
        if (!isTestingTransition) return;
        
        float elapsedTime = Time.time - transitionStartTime;
        
        // 检查是否生成了新的Boss
        if (!transitionCompleted)
        {
            BossDemon newBossDemon = FindObjectOfType<BossDemon>();
            if (newBossDemon != null)
            {
                transitionCompleted = true;
                Debug.Log($"=== 转换测试成功 ===");
                Debug.Log($"转换耗时: {elapsedTime:F2}秒");
                Debug.Log($"新Boss类型: {newBossDemon.GetType().Name}");
                Debug.Log($"新Boss血量: {newBossDemon.CurrentHealth}/{newBossDemon.MaxHealth}");
                Debug.Log($"血量百分比: {newBossDemon.HealthPercentage * 100:F1}%");
                Debug.Log($"位置: {newBossDemon.transform.position}");
                
                // 验证血量设置
                float expectedPercentage = testHealthPercentage;
                float actualPercentage = newBossDemon.HealthPercentage;
                
                if (Mathf.Abs(expectedPercentage - actualPercentage) < 0.05f)
                {
                    Debug.Log("? 血量设置正确");
                }
                else
                {
                    Debug.LogWarning($"? 血量设置可能有误 - 期望: {expectedPercentage * 100:F1}%, 实际: {actualPercentage * 100:F1}%");
                }
            }
        }
        
        // 超时检查
        if (elapsedTime > 10f && !transitionCompleted)
        {
            Debug.LogError("=== 转换测试超时 ===");
            Debug.LogError("可能的原因：");
            Debug.LogError("1. enableBossTransition被设置为false");
            Debug.LogError("2. 预制体配置有误");
            Debug.LogError("3. 死亡序列出现异常");
            
            isTestingTransition = false;
        }
    }
    
    /// <summary>
    /// 验证转换配置
    /// </summary>
    [ContextMenu("验证转换配置")]
    public void ValidateTransitionConfiguration()
    {
        if (targetBossSlime == null)
        {
            Debug.LogError("未找到目标BossSlime");
            return;
        }
        
        Debug.Log("=== 验证转换配置 ===");
        
        var param = targetBossSlime.parameter;
        
        // 检查基本配置
        Debug.Log($"启用Boss转换: {param.enableBossTransition}");
        Debug.Log($"血量百分比: {param.nextBossHealthPercentage * 100:F1}%");
        Debug.Log($"预制体: {(param.nextBossPrefab != null ? param.nextBossPrefab.name : "未设置（将动态创建）")}");
        
        // 检查预制体
        if (param.nextBossPrefab != null)
        {
            BossDemon bossDemonComponent = param.nextBossPrefab.GetComponent<BossDemon>();
            BossLife bossLifeComponent = param.nextBossPrefab.GetComponent<BossLife>();
            
            if (bossDemonComponent != null)
            {
                Debug.Log("? 预制体包含BossDemon组件");
            }
            else if (bossLifeComponent != null)
            {
                Debug.Log("? 预制体包含BossLife组件");
            }
            else
            {
                Debug.LogWarning("? 预制体缺少BossDemon或BossLife组件");
            }
        }
        
        // 检查必要系统
        if (CamaraShakeManager.Instance != null)
        {
            Debug.Log("? CamaraShakeManager系统正常");
        }
        else
        {
            Debug.LogWarning("? 未找到CamaraShakeManager");
        }
        
        Debug.Log("=== 配置验证完成 ===");
    }
    
    /// <summary>
    /// 压力测试
    /// </summary>
    [ContextMenu("压力测试")]
    public void StressTest()
    {
        StartCoroutine(StressTestCoroutine());
    }
    
    private IEnumerator StressTestCoroutine()
    {
        Debug.Log("开始压力测试 - 连续转换测试");
        
        for (int i = 0; i < 5; i++)
        {
            Debug.Log($"压力测试第 {i + 1} 轮");
            
            // 查找当前Boss
            BossSlime currentSlime = FindObjectOfType<BossSlime>();
            if (currentSlime != null)
            {
                int currentHealth = Mathf.RoundToInt(currentSlime.parameter.currentHealth);
                currentSlime.TakeDamage(currentHealth);
            }
            
            // 等待转换完成
            yield return new WaitForSeconds(5f);
            
            // 查找新生成的Boss
            BossDemon newDemon = FindObjectOfType<BossDemon>();
            if (newDemon != null)
            {
                Debug.Log($"第 {i + 1} 轮转换成功");
                
                // 清理，准备下一轮
                Destroy(newDemon.gameObject);
                yield return new WaitForSeconds(1f);
                
                // 重新创建BossSlime进行下一轮测试
                // 这里需要根据实际情况调整
            }
            else
            {
                Debug.LogError($"第 {i + 1} 轮转换失败");
                break;
            }
        }
        
        Debug.Log("压力测试完成");
    }
    
    /// <summary>
    /// 显示测试UI
    /// </summary>
    void OnGUI()
    {
        if (!showTestUI) return;
        
        GUILayout.BeginArea(new Rect(10, 200, 350, 400));
        
        GUILayout.Label("=== Boss转换测试系统 ===");
        
        // 显示目标信息
        if (targetBossSlime != null)
        {
            GUILayout.Label($"目标: {targetBossSlime.name}");
            GUILayout.Label($"血量: {targetBossSlime.parameter.currentHealth:F0}/{targetBossSlime.parameter.maxHealth:F0}");
            GUILayout.Label($"状态: {(targetBossSlime.IsDead ? "死亡" : "存活")}");
        }
        else
        {
            GUILayout.Label("目标: 未找到BossSlime");
        }
        
        GUILayout.Space(10);
        
        // 测试参数
        GUILayout.Label("=== 测试参数 ===");
        GUILayout.Label($"血量百分比: {testHealthPercentage * 100:F0}%");
        GUILayout.Label($"预制体: {(testBossPrefab != null ? testBossPrefab.name : "动态创建")}");
        
        GUILayout.Space(10);
        
        // 测试状态
        if (isTestingTransition)
        {
            GUILayout.Label("?? 转换测试进行中...");
            GUILayout.Label($"耗时: {Time.time - transitionStartTime:F1}秒");
            
            if (transitionCompleted)
            {
                GUILayout.Label("? 转换成功！");
            }
        }
        else
        {
            GUILayout.Label("待机中");
        }
        
        GUILayout.Space(10);
        
        // 控制说明
        GUILayout.Label("=== 控制说明 ===");
        GUILayout.Label("T - 测试转换");
        GUILayout.Label("K - 快速击杀");
        GUILayout.Label("R - 重置测试");
        GUILayout.Label("H - 切换血量百分比");
        GUILayout.Label("P - 设置预制体信息");
        
        GUILayout.EndArea();
    }
}