using System.Collections;
using UnityEngine;

public class KeyWallDestroyer : MonoBehaviour
{
    [Header("Target Key")]
    [SerializeField] private LevelOneKey targetKey; // ָ����Կ��
    [SerializeField] private bool autoFindKey = true; // �Ƿ��Զ�Ѱ��Կ��
    
    [Header("Wall Settings")]
    [SerializeField] private GameObject wallToDestroy; // Ҫ�ݻٵ�ǽ��
    [SerializeField] private bool destroyWall = true; // true: �ݻ�ǽ��, false: ����Ϊtrigger
    [SerializeField] private float destroyDelay = 0.5f; // �ݻ��ӳ�ʱ��
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject destructionEffect; // �ݻ���Ч
    [SerializeField] private AudioClip destructionSound; // �ݻ���Ч
    
    private Collider2D wallCollider;
    private AudioSource audioSource;
    private bool hasProcessedKeyCollection = false;
    
    void Start()
    {
        // ��ȡ��Դ
        audioSource = GetComponent<AudioSource>();
        
        // ����Ҫ�ݻٵ�ǽ��
        if (wallToDestroy == null)
        {
            wallToDestroy = gameObject; // ���û��ָ����Ĭ�ϴݻ��Լ�
        }
        
        wallCollider = wallToDestroy.GetComponent<Collider2D>();
        
        // Ѱ��Ŀ��Կ��
        FindTargetKey();
        
        // ����Կ���ռ��¼�
        SubscribeToKeyEvents();
        
        Debug.Log("KeyWallDestroyer initialized");
    }
    
    void Update()
    {
        // ���û���ҵ�Կ�ף��������Բ���
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
            // �ڳ�����Ѱ��LevelOneKey
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
        
        // ����Կ���ռ��¼�
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
        
        // ���Ŵݻ���Ч
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, wallToDestroy.transform.position, Quaternion.identity);
        }
        
        // ���Ŵݻ���Ч
        if (audioSource != null && destructionSound != null)
        {
            audioSource.PlayOneShot(destructionSound);
        }
        
        Debug.Log($"KeyWallDestroyer: Wall '{wallToDestroy.name}' will be destroyed.");
        
        // �ӳٴݻ���ȷ����Ч����Ч�ܹ�����
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
        // ȡ�������¼�����ֹ�ڴ�й©
        if (targetKey != null)
        {
            targetKey.OnKeyCollected -= OnKeyCollected;
        }
    }
    
    // �ڱ༭���п��ӻ�
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
            
            // �������ߴ�Կ�׵�ǽ��
            if (wallToDestroy != null)
            {
                Gizmos.DrawLine(targetKey.transform.position, wallToDestroy.transform.position);
            }
        }
    }
}