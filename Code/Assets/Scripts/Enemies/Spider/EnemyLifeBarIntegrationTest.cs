using UnityEngine;

/// <summary>
/// 集成测试脚本，用于验证 EnemyLifeBar 和 EnemyLife 的集成是否正常工作
/// </summary>
public class EnemyLifeBarIntegrationTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private float testInterval = 1f;
    
    private EnemyLife enemyLife;
    private EnemyLifeBar enemyLifeBar;
    private float lastTestTime;
    private int testPhase = 0;
    
    void Start()
    {
        if (!runTestOnStart) return;
        
        // 查找组件
        enemyLife = GetComponent<EnemyLife>();
        enemyLifeBar = GetComponent<EnemyLifeBar>();
        
        if (enemyLife == null)
        {
            Debug.LogError("EnemyLife component not found!");
            return;
        }
        
        if (enemyLifeBar == null)
        {
            Debug.LogError("EnemyLifeBar component not found!");
            return;
        }
        
        Debug.Log("=== EnemyLifeBar Integration Test Started ===");
        Debug.Log($"Initial Health: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth}");
        Debug.Log("Will run automatic tests every " + testInterval + " seconds");
        
        lastTestTime = Time.time;
    }
    
    void Update()
    {
        if (!runTestOnStart || enemyLife == null || enemyLifeBar == null) return;
        
        // 自动测试
        if (Time.time - lastTestTime >= testInterval)
        {
            RunNextTest();
            lastTestTime = Time.time;
        }
    }
    
    private void RunNextTest()
    {
        switch (testPhase)
        {
            case 0:
                TestDamage();
                break;
            case 1:
                TestMoreDamage();
                break;
            case 2:
                TestHealing();
                break;
            case 3:
                TestReset();
                break;
            case 4:
                TestKill();
                break;
            default:
                testPhase = 0;
                TestReset();
                break;
        }
        
        testPhase++;
    }
    
    private void TestDamage()
    {
        Debug.Log("--- Test Phase 1: Damage ---");
        enemyLife.TakeDamage(5);
        Debug.Log($"Applied 5 damage. Health: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth} ({enemyLife.HealthPercentage:P1})");
        Debug.Log($"Expected bar width: {enemyLife.HealthPercentage:P1}");
    }
    
    private void TestMoreDamage()
    {
        Debug.Log("--- Test Phase 2: More Damage ---");
        enemyLife.TakeDamage(8);
        Debug.Log($"Applied 8 damage. Health: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth} ({enemyLife.HealthPercentage:P1})");
        Debug.Log($"Expected bar width: {enemyLife.HealthPercentage:P1}");
    }
    
    private void TestHealing()
    {
        Debug.Log("--- Test Phase 3: Healing ---");
        enemyLife.Heal(3);
        Debug.Log($"Applied 3 healing. Health: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth} ({enemyLife.HealthPercentage:P1})");
        Debug.Log($"Expected bar width: {enemyLife.HealthPercentage:P1}");
    }
    
    private void TestReset()
    {
        Debug.Log("--- Test Phase 4: Reset to Full Health ---");
        enemyLife.RestoreToFullHealth();
        Debug.Log($"Restored to full health. Health: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth} ({enemyLife.HealthPercentage:P1})");
        Debug.Log($"Expected bar width: {enemyLife.HealthPercentage:P1}");
    }
    
    private void TestKill()
    {
        Debug.Log("--- Test Phase 5: Kill Enemy ---");
        enemyLife.TakeDamage(enemyLife.CurrentHealth);
        Debug.Log($"Applied fatal damage. Health: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth} ({enemyLife.HealthPercentage:P1})");
        Debug.Log($"Expected: Bar should be hidden, IsDead: {enemyLife.IsDead}");
    }
    
    [ContextMenu("Run Manual Test")]
    public void RunManualTest()
    {
        if (enemyLife == null || enemyLifeBar == null)
        {
            Debug.LogError("Components not found for manual test!");
            return;
        }
        
        Debug.Log("=== Manual Test ===");
        Debug.Log($"Current Health: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth}");
        Debug.Log($"Health Percentage: {enemyLife.HealthPercentage:P1}");
        Debug.Log($"Is Dead: {enemyLife.IsDead}");
        Debug.Log($"Expected bar width: {enemyLife.HealthPercentage:P1}");
    }
    
    [ContextMenu("Reset Enemy")]
    public void ResetEnemy()
    {
        if (enemyLife != null)
        {
            enemyLife.RestoreToFullHealth();
            Debug.Log("Enemy reset to full health");
        }
    }
    
    [ContextMenu("Damage Enemy")]
    public void DamageEnemy()
    {
        if (enemyLife != null)
        {
            enemyLife.TakeDamage(5);
            Debug.Log("Applied 5 damage to enemy");
        }
    }
}