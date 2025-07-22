using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWallDestroyer : MonoBehaviour
{
    [Header("Target Enemy")]
    [SerializeField] private GameObject targetEnemy; // 指定的敌人
    [SerializeField] private string enemyTag = "Enemy"; // 敌人标签
    [SerializeField] private bool autoFindEnemy = true; // 是否自动查找敌人
    
    [Header("Wall Settings")]
    [SerializeField] private GameObject wallToDestroy; // 要摧毁的墙体
    [SerializeField] private bool destroyWall = true; // true: 摧毁墙体, false: 设置为trigger
    [SerializeField] private float destroyDelay = 0.5f; // 摧毁延迟时间
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject destructionEffect; // 摧毁特效
    [SerializeField] private AudioClip destructionSound; // 摧毁音效
    
    private EnemyLife enemyLife;
    private BossLife bossLife;
    private Collider2D wallCollider;
    private AudioSource audioSource;
    private bool hasProcessedDeath = false;
    
    void Start()
    {
        // 获取组件
        audioSource = GetComponent<AudioSource>();
        
        // 设置要摧毁的墙体
        if (wallToDestroy == null)
        {
            wallToDestroy = gameObject; // 如果没有指定，默认摧毁自己
        }
        
        wallCollider = wallToDestroy.GetComponent<Collider2D>();
        
        // 查找目标敌人
        FindTargetEnemy();
        
        // 订阅死亡事件
        SubscribeToDeathEvents();
    }
    
    void Update()
    {
        // 如果没有找到敌人，尝试重新查找
        if (targetEnemy == null && autoFindEnemy)
        {
            FindTargetEnemy();
            if (targetEnemy != null)
            {
                SubscribeToDeathEvents();
            }
        }
    }
    
    private void FindTargetEnemy()
    {
        if (targetEnemy == null && autoFindEnemy)
        {
            // 通过标签查找敌人
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            if (enemies.Length > 0)
            {
                targetEnemy = enemies[0]; // 选择第一个找到的敌人
                Debug.Log($"EnemyWallDestroyer: Auto-found enemy '{targetEnemy.name}'");
            }
        }
    }
    
    private void SubscribeToDeathEvents()
    {
        if (targetEnemy == null)
        {
            Debug.LogWarning("EnemyWallDestroyer: No target enemy assigned!");
            return;
        }
        
        // 尝试获取 EnemyLife 组件
        enemyLife = targetEnemy.GetComponent<EnemyLife>();
        if (enemyLife != null)
        {
            enemyLife.OnDeath += OnEnemyDeath;
            Debug.Log($"EnemyWallDestroyer: Successfully connected to EnemyLife on '{targetEnemy.name}'");
            return;
        }
        
        // 尝试获取 BossLife 组件
        bossLife = targetEnemy.GetComponent<BossLife>();
        if (bossLife != null)
        {
            bossLife.OnDeath += OnEnemyDeath;
            Debug.Log($"EnemyWallDestroyer: Successfully connected to BossLife on '{targetEnemy.name}'");
            return;
        }
        
        // 如果没有找到生命组件，尝试监听游戏对象的销毁
        StartCoroutine(MonitorEnemyDestruction());
        Debug.Log($"EnemyWallDestroyer: No life component found, monitoring destruction of '{targetEnemy.name}'");
    }
    
    private IEnumerator MonitorEnemyDestruction()
    {
        while (targetEnemy != null && !hasProcessedDeath)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        if (!hasProcessedDeath)
        {
            OnEnemyDeath();
        }
    }
    
    private void OnEnemyDeath()
    {
        if (hasProcessedDeath) return;
        
        hasProcessedDeath = true;
        
        Debug.Log($"EnemyWallDestroyer: Enemy '{targetEnemy?.name}' has died, processing wall destruction.");
        
        if (destroyWall)
        {
            DestroyWall();
        }
        else
        {
            SetWallAsTrigger();
        }
    }
    
    private void DestroyWall()
    {
        if (wallToDestroy == null) return;
        
        // 播放摧毁特效
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, wallToDestroy.transform.position, Quaternion.identity);
        }
        
        // 播放摧毁音效
        if (audioSource != null && destructionSound != null)
        {
            audioSource.PlayOneShot(destructionSound);
        }
        
        Debug.Log($"EnemyWallDestroyer: Wall '{wallToDestroy.name}' will be destroyed.");
        
        // 延迟摧毁以确保特效和音效能够播放
        StartCoroutine(DestroyWallCoroutine());
    }
    
    private IEnumerator DestroyWallCoroutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        
        if (wallToDestroy != null)
        {
            Destroy(wallToDestroy);
        }
    }
    
    private void SetWallAsTrigger()
    {
        if (wallCollider != null)
        {
            wallCollider.isTrigger = true;
            Debug.Log($"EnemyWallDestroyer: Wall '{wallToDestroy.name}' has been set as trigger.");
        }
        else
        {
            Debug.LogWarning("EnemyWallDestroyer: No collider found to set as trigger!");
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (enemyLife != null)
        {
            enemyLife.OnDeath -= OnEnemyDeath;
        }
        
        if (bossLife != null)
        {
            bossLife.OnDeath -= OnEnemyDeath;
        }
    }
    
    // 在编辑器中可视化
    private void OnDrawGizmosSelected()
    {
        if (wallToDestroy != null)
        {
            Gizmos.color = hasProcessedDeath ? Color.red : Color.green;
            Gizmos.DrawWireCube(wallToDestroy.transform.position, wallToDestroy.transform.localScale);
        }
        
        if (targetEnemy != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetEnemy.transform.position, 1f);
            
            // 画线连接敌人和墙体
            if (wallToDestroy != null)
            {
                Gizmos.DrawLine(targetEnemy.transform.position, wallToDestroy.transform.position);
            }
        }
    }
}