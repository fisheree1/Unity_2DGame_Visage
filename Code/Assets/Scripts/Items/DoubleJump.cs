using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoubleJump : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool isCollected = false;
    
    [Header("Visual Effects")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TextMeshProUGUI interactionText;
    
    [Header("Skill Settings")]
    [SerializeField] private string skillToUnlock = "DoubleJump";
    [SerializeField] private string skillDisplayName = "Double Jump";
    [SerializeField] private string skillDescription = "Allows you to jump again in mid-air!";
    
    // Components
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    
    // State
    private bool playerInRange = false;
    private bool isInteracting = false;
    
    void Start()
    {
        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        
        // 设置UI文本
        if (interactionText != null)
        {
            interactionText.text = $"Press <color=yellow>[{interactKey}]</color> to learn {skillDisplayName}";
        }
        
        // 隐藏交互UI
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        // 如果已经收集，隐藏技能书
        if (isCollected)
        {
            gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isCollected || isInteracting) return;
        
        // 检查玩家是否在范围内
        CheckPlayerInRange();
        
        // 处理交互输入
        if (playerInRange)
        {
            StartCoroutine(CollectSkillBook());
        }
        
        // 更新视觉效果
        UpdateVisualEffects();
    }
    
    private void CheckPlayerInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 skillBookPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y);
            float distance = Vector2.Distance(skillBookPos, playerPos);
            
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRadius;
            
            // 进入范围时显示UI
            if (playerInRange && !wasInRange)
            {
                OnPlayerEnterRange();
            }
            // 离开范围时隐藏UI
            else if (!playerInRange && wasInRange)
            {
                OnPlayerExitRange();
            }
        }
        else
        {
            if (playerInRange)
            {
                OnPlayerExitRange();
            }
            playerInRange = false;
        }
    }
    
    private void OnPlayerEnterRange()
    {
        // 显示交互提示
        if (interactionUI != null && !isCollected)
        {
            interactionUI.SetActive(true);
        }
    }
    
    private void OnPlayerExitRange()
    {
        // 隐藏交互提示
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
    
    private void UpdateVisualEffects()
    {
        // 浮动效果
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPosition + Vector3.up * bobOffset;
        
        // 颜色效果
        if (spriteRenderer != null)
        {
            spriteRenderer.color = playerInRange ? highlightColor : normalColor;
        }
    }
    
    private IEnumerator CollectSkillBook()
    {
        isInteracting = true;
        
        // 隐藏交互UI
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        // 播放收集效果
        yield return StartCoroutine(PlayCollectionEffect());
        
        // 解锁技能 - 使用事件通知系统
        HeroMovement heroMovement = FindObjectOfType<HeroMovement>();
        if (heroMovement != null)
        {
            // 直接解锁二段跳技能
            heroMovement.UnlockDoubleJump();
            Debug.Log($"Successfully learned skill: {skillDisplayName}");
            ShowSkillLearnedMessage();
        }
        else
        {
            Debug.LogError("HeroMovement not found!");
        }
        
        // 标记为已收集并隐藏
        isCollected = true;
        gameObject.SetActive(false);
    }
    
    private IEnumerator PlayCollectionEffect()
    {
        // 简单的收集动画：缩小并向上移动
        Vector3 originalScale = transform.localScale;
        Vector3 originalPosition = transform.position;
        
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            
            // 缩小效果
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            
            // 向上移动
            transform.position = originalPosition + Vector3.up * progress * 2f;
            
            // 淡出效果
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f - progress;
                spriteRenderer.color = color;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private void ShowSkillLearnedMessage()
    {
        // 显示技能学习成功的消息
        Debug.Log($"🎉 Skill Learned: {skillDisplayName}\n{skillDescription}");
        
        // 可以在这里添加UI提示系统
        // 例如显示一个弹窗或者屏幕提示
    }
    
    // 可视化交互范围（编辑器中显示）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
    
    // 公共属性
    public bool IsCollected => isCollected;
    public string SkillName => skillToUnlock;
}
