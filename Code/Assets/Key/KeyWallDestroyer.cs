using System.Collections;
using UnityEngine;

public class KeyWallDestroyer : MonoBehaviour
{
    [Header("Target Key")]
    [SerializeField] private LevelOneKey targetKey; // 指定的钥匙
    [SerializeField] private bool autoFindKey = true; // 是否自动寻找钥匙
    
    [Header("Wall Settings")]
    [SerializeField] private GameObject wallToDestroy; // 要摧毁的墙壁
    [SerializeField] private bool destroyWall = true; // true: 摧毁墙壁, false: 设置为trigger
    [SerializeField] private float destroyDelay = 0.5f; // 摧毁延迟时间
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject destructionEffect; // 摧毁特效
    [SerializeField] private AudioClip destructionSound; // 摧毁音效
    
    private Collider2D wallCollider;
    private AudioSource audioSource;
    private bool hasProcessedKeyCollection = false;
    
    void Start()
    {
        // 获取音源
        audioSource = GetComponent<AudioSource>();
        
        // 设置要摧毁的墙壁
        if (wallToDestroy == null)
        {
            wallToDestroy = gameObject; // 如果没有指定，默认摧毁自己
        }
        
        wallCollider = wallToDestroy.GetComponent<Collider2D>();
        
        // 寻找目标钥匙
        FindTargetKey();
        
        // 订阅钥匙收集事件
        SubscribeToKeyEvents();
        
        Debug.Log("KeyWallDestroyer initialized");
    }
    
    void Update()
    {
        // 如果没有找到钥匙，继续尝试查找
        if (targetKey == null && autoFindKey)
        {
            FindTargetKey();
            if (targetKey != null)
            {
                SubscribeToKeyEvents();
            }
        }
    }
    
    private void FindTargetKey()
    {
        if (targetKey == null && autoFindKey)
        {
            // 在场景中寻找LevelOneKey
            LevelOneKey foundKey = FindObjectOfType<LevelOneKey>();
            if (foundKey != null)
            {
                targetKey = foundKey;
                Debug.Log($"KeyWallDestroyer: Auto-found key '{targetKey.name}'");
            }
        }
    }
    
    private void SubscribeToKeyEvents()
    {
        if (targetKey == null)
        {
            Debug.LogWarning("KeyWallDestroyer: No target key assigned!");
            return;
        }
        
        // 订阅钥匙收集事件
        targetKey.OnKeyCollected += OnKeyCollected;
        Debug.Log($"KeyWallDestroyer: Successfully subscribed to key collection event for '{targetKey.name}'");
    }
    
    private void OnKeyCollected()
    {
        if (hasProcessedKeyCollection) return;
        
        hasProcessedKeyCollection = true;
        
        Debug.Log($"KeyWallDestroyer: Key '{targetKey?.name}' has been collected, processing wall destruction.");
        
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
        
        Debug.Log($"KeyWallDestroyer: Wall '{wallToDestroy.name}' will be destroyed.");
        
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
            Debug.Log($"KeyWallDestroyer: Wall '{wallToDestroy.name}' has been set as trigger.");
        }
        else
        {
            Debug.LogWarning("KeyWallDestroyer: No collider found to set as trigger!");
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (targetKey != null)
        {
            targetKey.OnKeyCollected -= OnKeyCollected;
        }
    }
    
    // 在编辑器中可视化
    private void OnDrawGizmosSelected()
    {
        if (wallToDestroy != null)
        {
            Gizmos.color = hasProcessedKeyCollection ? Color.red : Color.green;
            Gizmos.DrawWireCube(wallToDestroy.transform.position, wallToDestroy.transform.localScale);
        }
        
        if (targetKey != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetKey.transform.position, 1f);
            
            // 绘制连线从钥匙到墙壁
            if (wallToDestroy != null)
            {
                Gizmos.DrawLine(targetKey.transform.position, wallToDestroy.transform.position);
            }
        }
    }
}