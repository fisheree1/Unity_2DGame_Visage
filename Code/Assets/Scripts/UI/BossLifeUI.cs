using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossLifeUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject bossHealthBarPanel; // 整个Boss血条面板
    [SerializeField] private Image healthBorder;
    [SerializeField] private Image healthFill;
    
    [Header("Health Bar Settings")]
    [SerializeField] private float healthBarSmoothTime = 0.3f;
    
    [Header("Visual Effects")]
    [SerializeField] private Color normalHealthColor = Color.red;
    [SerializeField] private Color lowHealthColor = new Color(0.8f, 0.2f, 0.2f); // 深红色
    [SerializeField] private Color criticalHealthColor = Color.magenta;
    [SerializeField] private float lowHealthThreshold = 0.5f;
    [SerializeField] private float criticalHealthThreshold = 0.2f;
    [SerializeField] private bool enableWarningFlash = true;
    [SerializeField] private float flashSpeed = 2f;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 15f; // Boss检测范围
    [SerializeField] private LayerMask bossLayerMask = -1; // Boss所在的层
    [SerializeField] private string bossTag = "Boss"; // Boss的标签
    
    // References
    private BossLife currentBoss;
    private Transform player;
    
    // Runtime values
    private float currentDisplayHealth = 1f;
    private float targetHealthFill = 1f;
    private bool isVisible = false;
    private bool isFlashing = false;
    
    void Start()
    {
        InitializeUI();
        FindPlayerReference();
        
        // 初始时隐藏Boss血条
        SetUIVisibility(false);
    }
    
    void Update()
    {
        CheckForBossInRange();
        
        if (isVisible && currentBoss != null)
        {
            UpdateHealthDisplay();
            UpdateVisualEffects();
        }
    }
    
    private void InitializeUI()
    {
        // 如果没有手动赋值，尝试自动查找UI组件
        if (bossHealthBarPanel == null) 
            bossHealthBarPanel = transform.Find("BossHealthPanel")?.gameObject ?? gameObject;
            
        if (healthBorder == null) 
            healthBorder = transform.Find("BossHealthPanel/HealthBar/Border")?.GetComponent<Image>() 
                        ?? transform.Find("HealthBar/Border")?.GetComponent<Image>();
                        
        if (healthFill == null) 
            healthFill = transform.Find("BossHealthPanel/HealthBar/Fill")?.GetComponent<Image>()
                       ?? transform.Find("HealthBar/Fill")?.GetComponent<Image>();
        
        LoadUISprites();
    }
    
    private void FindPlayerReference()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("BossLifeUI: No Player found with 'Player' tag!");
        }
    }
    
    private void CheckForBossInRange()
    {
        if (player == null) return;
        
        // 首先通过LayerMask过滤，提高性能
        Collider2D[] layerFilteredColliders = Physics2D.OverlapCircleAll(player.position, detectionRange, bossLayerMask);
        
        BossLife nearestBoss = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var collider in layerFilteredColliders)
        {
            // 双重验证：Layer + Tag 都必须匹配
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
        
        // 如果找到了新的Boss或者当前Boss死亡/消失
        if (nearestBoss != currentBoss)
        {
            if (currentBoss != null)
            {
                // 取消订阅旧Boss的事件
                UnsubscribeFromBoss(currentBoss);
            }
            
            currentBoss = nearestBoss;
            
            if (currentBoss != null)
            {
                // 订阅新Boss的事件
                SubscribeToBoss(currentBoss);
                SetUIVisibility(true);
                UpdateBossInfo();
            }
            else
            {
                SetUIVisibility(false);
            }
        }
    }
    
    private void SubscribeToBoss(BossLife boss)
    {
        if (boss != null)
        {
            boss.OnHealthChanged += OnBossHealthChanged;
            boss.OnDeath += OnBossDeath;
            
            // 初始化显示值
            currentDisplayHealth = (float)boss.CurrentHealth / boss.MaxHealth;
            targetHealthFill = currentDisplayHealth;
        }
    }
    
    private void UnsubscribeFromBoss(BossLife boss)
    {
        if (boss != null)
        {
            boss.OnHealthChanged -= OnBossHealthChanged;
            boss.OnDeath -= OnBossDeath;
        }
    }
    
    private void UpdateBossInfo()
    {
        if (currentBoss == null) return;
        
        // 仅需要血条，移除名称和血量文字相关代码
    }
    
    private void UpdateHealthDisplay()
    {
        if (currentBoss == null || healthFill == null) return;
        
        // 计算目标血量百分比
        targetHealthFill = (float)currentBoss.CurrentHealth / currentBoss.MaxHealth;
        
        // 平滑过渡到目标值
        currentDisplayHealth = Mathf.Lerp(currentDisplayHealth, targetHealthFill, Time.deltaTime / healthBarSmoothTime);
        
        // 更新血条填充
        healthFill.fillAmount = currentDisplayHealth;
        
        // 更新血条颜色
        UpdateHealthBarColor();
    }
    
    private void UpdateHealthBarColor()
    {
        if (healthFill == null) return;
        
        Color targetColor;
        
        if (targetHealthFill <= criticalHealthThreshold)
        {
            targetColor = criticalHealthColor;
        }
        else if (targetHealthFill <= lowHealthThreshold)
        {
            targetColor = lowHealthColor;
        }
        else
        {
            targetColor = normalHealthColor;
        }
        
        healthFill.color = targetColor;
    }
    
    private void UpdateVisualEffects()
    {
        if (!enableWarningFlash || healthFill == null || currentBoss == null) return;
        
        // 在危险血量时闪烁
        if (targetHealthFill <= criticalHealthThreshold)
        {
            if (!isFlashing)
            {
                StartCoroutine(FlashHealthBar());
            }
        }
        else
        {
            isFlashing = false;
        }
    }
    
    private IEnumerator FlashHealthBar()
    {
        isFlashing = true;
        Color originalColor = healthFill.color;
        
        while (isFlashing && targetHealthFill <= criticalHealthThreshold)
        {
            // 闪烁效果
            float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
            Color flashColor = Color.Lerp(originalColor, Color.white, alpha * 0.5f);
            healthFill.color = flashColor;
            
            yield return null;
        }
        
        // 恢复原色
        if (healthFill != null)
        {
            UpdateHealthBarColor();
        }
    }
    
    private void SetUIVisibility(bool visible)
    {
        isVisible = visible;
        
        if (bossHealthBarPanel != null)
        {
            bossHealthBarPanel.SetActive(visible);
        }
        
        if (visible)
        {
            Debug.Log("Boss health bar shown");
        }
        else
        {
            Debug.Log("Boss health bar hidden");
        }
    }
    
    private void LoadUISprites()
    {
        // 从Resources加载图片
        Sprite borderSprite = Resources.Load<Sprite>("Art/border");
        Sprite fillSprite = Resources.Load<Sprite>("Art/fill");
        
        // 应用血条图片
        if (healthBorder != null && borderSprite != null)
        {
            healthBorder.sprite = borderSprite;
            healthBorder.type = Image.Type.Sliced;
        }
        
        if (healthFill != null && fillSprite != null)
        {
            healthFill.sprite = fillSprite;
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
        }
    }
    
    // Boss事件处理
    private void OnBossHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"Boss health changed: {currentHealth}/{maxHealth}");
        // 血量变化在Update中处理
    }
    
    private void OnBossDeath()
    {
        Debug.Log("Boss died - hiding health bar");
        
        // Boss死亡后延迟隐藏血条
        StartCoroutine(HideHealthBarAfterDelay(2f));
    }
    
    private IEnumerator HideHealthBarAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentBoss != null)
        {
            UnsubscribeFromBoss(currentBoss);
            currentBoss = null;
        }
        
        SetUIVisibility(false);
    }
    
    private void OnDestroy()
    {
        // 清理事件订阅
        if (currentBoss != null)
        {
            UnsubscribeFromBoss(currentBoss);
        }
    }
    
    // 可视化调试
    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, detectionRange);
        }
    }
}
