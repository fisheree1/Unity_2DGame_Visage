using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private string targetSceneName; // 目标场景名称
    [SerializeField] private KeyCode interactKey = KeyCode.E; // 交互按键
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI; // 交互提示UI
    [SerializeField] private TextMeshProUGUI interactionText; // 交互提示文本
    [SerializeField] private string promptText = "Press E to Enter Portal"; // 提示文本内容
    
    [Header("Visual Effects")]
    [SerializeField] private bool hasVisualEffects = true;
    [SerializeField] private float glowIntensity = 0.5f;
    [SerializeField] private Color portalColor = Color.cyan;
    
    [Header("Audio")]
    [SerializeField] private AudioClip portalSound;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Color originalColor;
    
    // State
    private bool playerInRange = false;
    private bool isTransitioning = false;
    
    void Start()
    {
        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // 初始化UI
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        if (interactionText != null)
        {
            interactionText.text = promptText;
        }
        
        // 验证目标场景名称
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"Portal '{gameObject.name}' has no target scene set!");
        }
    }
    
    void Update()
    {
        // 处理交互输入
        if (playerInRange && !isTransitioning)
        {
            EnterPortal();
        }
        
        // 更新视觉效果
        if (hasVisualEffects)
        {
            UpdateVisualEffects();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // 显示交互提示
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
            
            Debug.Log($"Player entered portal range - {gameObject.name}");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // 隐藏交互提示
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
            
            Debug.Log($"Player left portal range - {gameObject.name}");
        }
    }
    
    private void EnterPortal()
    {
        if (isTransitioning || string.IsNullOrEmpty(targetSceneName))
        {
            return;
        }
        
        isTransitioning = true;
        
        // 播放音效
        if (audioSource != null && portalSound != null)
        {
            audioSource.PlayOneShot(portalSound);
        }
        
        // 隐藏交互UI
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        Debug.Log($"Portal activated - Loading scene: {targetSceneName}");
        
        // 开始场景切换协程
        StartCoroutine(TransitionToScene());
    }
    
    private IEnumerator TransitionToScene()
    {
        // 可以在这里添加传送门进入动画或效果
        yield return new WaitForSeconds(0.5f); // 短暂延迟让音效播放
        
        // 加载目标场景
        SceneManager.LoadScene(targetSceneName);
    }
    
    private void UpdateVisualEffects()
    {
        if (spriteRenderer == null) return;
        
        // 创建发光效果
        float glowValue = Mathf.Sin(Time.time * 2f) * glowIntensity + (1f - glowIntensity);
        Color currentColor = Color.Lerp(originalColor, portalColor, glowValue);
        spriteRenderer.color = currentColor;
    }
    
    // 在编辑器中可视化触发区域
    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = portalColor;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
            
            // 绘制传送门标记
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.3f);
        }
    }
    
    // 公共方法供外部调用
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }
    
    public string GetTargetScene()
    {
        return targetSceneName;
    }
    
    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
}
