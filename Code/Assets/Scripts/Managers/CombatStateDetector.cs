using UnityEngine;
using System.Collections;

/// <summary>
/// 战斗状态检测器 - 检测玩家是否处于战斗状态
/// 与AudioManager配合工作，提供精确的战斗检测
/// </summary>
public class CombatStateDetector : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float combatRadius = 12f;
    [SerializeField] private float combatExitDelay = 3f; // 脱离战斗后延迟时间
    [SerializeField] private LayerMask enemyLayers = -1;
    [SerializeField] private LayerMask bossLayers = -1;
    
    [Header("标签设置")]
    [SerializeField] private string[] enemyTags = { "Enemy", "Boss" };
    [SerializeField] private string bossTag = "Boss";
    
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGizmos = true;
    
    // 事件
    public System.Action OnCombatStart;
    public System.Action OnCombatEnd;
    public System.Action OnBossBattleStart;
    public System.Action OnBossBattleEnd;
    
    // 状态
    private bool isInCombat = false;
    private bool isInBossBattle = false;
    private Transform player;
    private Coroutine combatExitCoroutine;
    
    // 检测到的目标
    private BossLife currentBoss;
    private int nearbyEnemyCount = 0;
    
    void Start()
    {
        // 查找玩家引用
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("CombatStateDetector: Player not found!");
        }
    }
    
    void Update()
    {
        if (player != null)
        {
            CheckCombatState();
        }
    }
    
    private void CheckCombatState()
    {
        bool previousCombatState = isInCombat;
        bool previousBossState = isInBossBattle;
        
        // 检测Boss战斗
        BossLife detectedBoss = DetectNearestBoss();
        bool bossDetected = (detectedBoss != null && !detectedBoss.IsDead);
        
        // 检测普通敌人
        int enemyCount = DetectNearbyEnemies();
        bool enemiesDetected = enemyCount > 0;
        
        // 更新Boss战斗状态
        if (bossDetected != isInBossBattle)
        {
            isInBossBattle = bossDetected;
            currentBoss = bossDetected ? detectedBoss : null;
            
            if (isInBossBattle)
            {
                OnBossBattleStart?.Invoke();
                DebugLog($"Boss battle started with {currentBoss.name}");
                
                // Boss战斗时自动进入战斗状态
                if (!isInCombat)
                {
                    isInCombat = true;
                    OnCombatStart?.Invoke();
                    DebugLog("Combat started (Boss detected)");
                }
            }
            else
            {
                OnBossBattleEnd?.Invoke();
                DebugLog("Boss battle ended");
            }
        }
        
        // 更新一般战斗状态
        bool shouldBeInCombat = bossDetected || enemiesDetected;
        
        if (shouldBeInCombat && !isInCombat)
        {
            // 进入战斗
            isInCombat = true;
            nearbyEnemyCount = enemyCount;
            
            // 取消脱离战斗的协程
            if (combatExitCoroutine != null)
            {
                StopCoroutine(combatExitCoroutine);
                combatExitCoroutine = null;
            }
            
            OnCombatStart?.Invoke();
            DebugLog($"Combat started - Enemies: {enemyCount}, Boss: {bossDetected}");
        }
        else if (!shouldBeInCombat && isInCombat)
        {
            // 可能脱离战斗，开始延迟检测
            if (combatExitCoroutine == null)
            {
                combatExitCoroutine = StartCoroutine(DelayedCombatExit());
            }
        }
        else if (shouldBeInCombat && isInCombat)
        {
            // 仍在战斗中，更新敌人数量
            nearbyEnemyCount = enemyCount;
        }
    }
    
    private BossLife DetectNearestBoss()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.position, combatRadius, bossLayers);
        
        BossLife nearestBoss = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag(bossTag))
            {
                BossLife boss = collider.GetComponent<BossLife>();
                if (boss != null && !boss.IsDead)
                {
                    float distance = Vector2.Distance(player.position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestBoss = boss;
                    }
                }
            }
        }
        
        return nearestBoss;
    }
    
    private int DetectNearbyEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.position, combatRadius, enemyLayers);
        int aliveEnemyCount = 0;
        
        foreach (var collider in colliders)
        {
            // 检查标签
            bool isEnemy = false;
            foreach (string tag in enemyTags)
            {
                if (collider.CompareTag(tag))
                {
                    isEnemy = true;
                    break;
                }
            }
            
            if (!isEnemy) continue;
            
            // 检查敌人是否还活着
            if (IsEnemyAlive(collider.gameObject))
            {
                aliveEnemyCount++;
            }
        }
        
        return aliveEnemyCount;
    }
    
    private bool IsEnemyAlive(GameObject enemy)
    {
        // 检查Boss生命组件
        BossLife bossLife = enemy.GetComponent<BossLife>();
        if (bossLife != null)
        {
            return !bossLife.IsDead;
        }
        
        // 检查Enemy组件
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            return true; // Enemy组件存在且活跃
        }
        
        // 检查其他生命组件（可以根据项目扩展）
        var lifeComponents = enemy.GetComponentsInChildren<MonoBehaviour>();
        foreach (var component in lifeComponents)
        {
            // 检查是否有包含"Life"或"Health"的组件名
            string componentName = component.GetType().Name.ToLower();
            if (componentName.Contains("life") || componentName.Contains("health"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private IEnumerator DelayedCombatExit()
    {
        DebugLog($"Combat exit delay started ({combatExitDelay}s)");
        
        yield return new WaitForSeconds(combatExitDelay);
        
        // 再次检查是否真的脱离了战斗
        BossLife boss = DetectNearestBoss();
        int enemies = DetectNearbyEnemies();
        
        if (boss == null && enemies == 0)
        {
            isInCombat = false;
            nearbyEnemyCount = 0;
            
            OnCombatEnd?.Invoke();
            DebugLog("Combat ended");
            
            // 如果之前在Boss战斗中，也结束Boss战斗状态
            if (isInBossBattle)
            {
                isInBossBattle = false;
                currentBoss = null;
                OnBossBattleEnd?.Invoke();
                DebugLog("Boss battle ended (delayed)");
            }
        }
        else
        {
            DebugLog("Still in combat, canceling exit");
        }
        
        combatExitCoroutine = null;
    }
    
    public void ForceEnterCombat()
    {
        if (!isInCombat)
        {
            isInCombat = true;
            OnCombatStart?.Invoke();
            DebugLog("Combat forced to start");
        }
    }
    
    public void ForceExitCombat()
    {
        if (isInCombat)
        {
            if (combatExitCoroutine != null)
            {
                StopCoroutine(combatExitCoroutine);
                combatExitCoroutine = null;
            }
            
            isInCombat = false;
            isInBossBattle = false;
            nearbyEnemyCount = 0;
            currentBoss = null;
            
            OnCombatEnd?.Invoke();
            OnBossBattleEnd?.Invoke();
            DebugLog("Combat forced to end");
        }
    }
    
    private void DebugLog(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[CombatDetector] {message}");
        }
    }
    
    // 可视化调试
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Vector3 center = player != null ? player.position : transform.position;
        
        // 绘制检测范围
        Gizmos.color = isInCombat ? Color.red : Color.green;
        Gizmos.DrawWireSphere(center, combatRadius);
        
        // 绘制Boss战斗指示
        if (isInBossBattle)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(center, combatRadius * 0.8f);
        }
        
        // 绘制敌人位置
        if (Application.isPlaying && player != null)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(player.position, combatRadius, enemyLayers);
            Gizmos.color = Color.yellow;
            foreach (var enemy in enemies)
            {
                if (IsEnemyAlive(enemy.gameObject))
                {
                    Gizmos.DrawWireCube(enemy.transform.position, Vector3.one * 0.5f);
                }
            }
        }
    }
    
    // 公开属性
    public bool IsInCombat => isInCombat;
    public bool IsInBossBattle => isInBossBattle;
    public BossLife CurrentBoss => currentBoss;
    public int NearbyEnemyCount => nearbyEnemyCount;
}