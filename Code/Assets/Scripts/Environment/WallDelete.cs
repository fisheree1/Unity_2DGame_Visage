using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDelete : MonoBehaviour
{
    [Header("Boss Reference")]
    [SerializeField] private BossLife bossLife;
    
    [Header("Settings")]
    [SerializeField] private bool destroyWall = true; // true: 摧毁墙体, false: 设置为trigger
    [SerializeField] private bool autoFindBoss = true; // 是否自动查找Boss
    [SerializeField] private float destroyDelay = 0.5f; // 摧毁延迟时间
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject destructionEffect; // 摧毁特效
    [SerializeField] private AudioClip destructionSound; // 摧毁音效
    
    private Collider2D wallCollider;
    private AudioSource audioSource;
    private bool hasProcessedBossDeath = false;
    
    void Start()
    {
        // 获取组件
        wallCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        // 如果启用自动查找且没有手动指定BossLife，尝试自动查找
        if (autoFindBoss && bossLife == null)
        {
            bossLife = FindObjectOfType<BossLife>();
        }
        
        // 订阅Boss死亡事件
        SubscribeToBossEvents();
    }

    void Update()
    {
        // 如果没有BossLife引用，尝试重新查找
        if (bossLife == null && autoFindBoss)
        {
            bossLife = FindObjectOfType<BossLife>();
            if (bossLife != null)
            {
                SubscribeToBossEvents();
            }
        }
    }
    
    private void SubscribeToBossEvents()
    {
        if (bossLife != null)
        {
            // 订阅Boss死亡事件
            bossLife.OnDeath += OnBossDeath;
            
            // 检查Boss是否已经死亡
            if (bossLife.IsDead && !hasProcessedBossDeath)
            {
                OnBossDeath();
            }
            
            Debug.Log($"FinalWall: Successfully connected to Boss '{bossLife.name}'");
        }
        else
        {
            Debug.LogWarning("FinalWall: No BossLife component found! Please assign it in the inspector or ensure a Boss exists in the scene.");
        }
    }
    
    private void OnBossDeath()
    {
        if (hasProcessedBossDeath) return;
        
        hasProcessedBossDeath = true;
        
        Debug.Log("FinalWall: Boss has died, processing wall state change.");
        
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
        // 播放摧毁特效
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, Quaternion.identity);
        }
        
        // 播放摧毁音效
        if (audioSource != null && destructionSound != null)
        {
            audioSource.PlayOneShot(destructionSound);
        }
        
        Debug.Log("FinalWall: Wall will be destroyed.");
        
        // 延迟摧毁以确保特效和音效能够播放
        StartCoroutine(DestroyWallCoroutine());
    }
    
    private IEnumerator DestroyWallCoroutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
    
    private void SetWallAsTrigger()
    {
        if (wallCollider != null)
        {
            wallCollider.isTrigger = true;
            Debug.Log("FinalWall: Wall has been set as trigger.");
        }
        else
        {
            Debug.LogWarning("FinalWall: No collider found to set as trigger!");
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        // 可以在这里添加玩家离开墙体碰撞的逻辑
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player exited wall collision");
        }
    }
    
    // 当墙体设置为trigger时，可以处理trigger事件
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered wall trigger area");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited wall trigger area");
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (bossLife != null)
        {
            bossLife.OnDeath -= OnBossDeath;
        }
    }
    
    // 在编辑器中可视化墙体区域
    private void OnDrawGizmosSelected()
    {
        if (wallCollider != null)
        {
            Gizmos.color = hasProcessedBossDeath ? Color.red : Color.blue;
            Gizmos.DrawWireCube(transform.position, wallCollider.bounds.size);
        }
    }
}