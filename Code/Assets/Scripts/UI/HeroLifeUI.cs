using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeroLifeUI : MonoBehaviour
{
    [Header("Manual Hero Reference")]
    [SerializeField] private GameObject manualHeroReference;
    
    [Header("UI Components")]
    [SerializeField] private Image healthBorder;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image staminaBorder;
    [SerializeField] private Image staminaFill;
    [SerializeField] private Image heartIcon;
    
    [Header("Health Bar Settings")]
    [SerializeField] private float healthBarSmoothTime = 0.2f;
    [SerializeField] private bool enableHeartBeat = true;
    [SerializeField] private float heartBeatSpeed = 1.5f;
    [SerializeField] private float heartBeatScale = 1.2f;
    
    [Header("Stamina Bar Settings")]
    [SerializeField] private float staminaBarSmoothTime = 0.15f;
    
    [Header("Visual Effects")]
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private Color normalHealthColor = Color.green;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private bool enableWarningFlash = true;
    [SerializeField] private float flashSpeed = 3f;
    
    // References
    private HeroLife heroLife;
    private HeroStamina heroStamina;
    private HeroAttackController attackController;
    
    // Runtime values
    private float currentDisplayHealth = 1f;
    private float currentDisplayStamina = 1f;
    private Vector3 originalHeartScale;
    private bool isFlashing = false;
    
    void Start()
    {
        InitializeUI();
        FindHeroComponents();
        LoadUISprites();
        
        if (heartIcon != null)
        {
            originalHeartScale = heartIcon.transform.localScale;
        }
        
        // 强制更新一次颜色
        Invoke(nameof(ForceUpdateColors), 0.1f);
    }
    
    private void ForceUpdateColors()
    {
        // 移除颜色强制更新，保持原始UI颜色
        Debug.Log("UI initialized - keeping original colors");
    }
    
    void Update()
    {
        UpdateHealthDisplay();
        UpdateStaminaDisplay();
        UpdateVisualEffects();
    }
    
    private void InitializeUI()
    {
        // 如果没有手动赋值，尝试自动查找UI组件
        if (healthBorder == null) healthBorder = transform.Find("HealthBar/Border")?.GetComponent<Image>();
        if (healthFill == null) healthFill = transform.Find("HealthBar/Fill")?.GetComponent<Image>();
        if (staminaBorder == null) staminaBorder = transform.Find("StaminaBar/Border")?.GetComponent<Image>();
        if (staminaFill == null) staminaFill = transform.Find("StaminaBar/Fill")?.GetComponent<Image>();
        if (heartIcon == null) heartIcon = transform.Find("HeartIcon")?.GetComponent<Image>();
    }
    
    private void FindHeroComponents()
    {
        // 首先检查手动引用
        if (manualHeroReference != null)
        {
            heroLife = manualHeroReference.GetComponent<HeroLife>();
            heroStamina = manualHeroReference.GetComponent<HeroStamina>();
            attackController = manualHeroReference.GetComponent<HeroAttackController>();
        }
        else
        {
            // 尝试从当前对象获取组件
            heroLife = GetComponent<HeroLife>();
            heroStamina = GetComponent<HeroStamina>();
            attackController = GetComponent<HeroAttackController>();
            
            // 如果没找到，通过Player标签查找
            if (heroLife == null)
            {
                GameObject hero = GameObject.FindGameObjectWithTag("Player");
                if (hero != null)
                {
                    heroLife = hero.GetComponent<HeroLife>();
                    heroStamina = hero.GetComponent<HeroStamina>();
                    attackController = hero.GetComponent<HeroAttackController>();
                }
            }
        }
        
        // 订阅血量变化事件
        if (heroLife != null)
        {
            heroLife.OnHealthChanged += OnHealthChanged;
            heroLife.OnDeath += OnHeroDeath;
            heroLife.OnRespawn += OnHeroRespawn;
            
            // 初始化显示值
            currentDisplayHealth = (float)heroLife.CurrentHealth / heroLife.MaxHealth;
        }
        
        // 订阅体力变化事件
        if (heroStamina != null)
        {
            heroStamina.OnStaminaChanged.AddListener(OnStaminaChanged);
            heroStamina.OnStaminaEmpty.AddListener(OnStaminaEmpty);
            heroStamina.OnStaminaFull.AddListener(OnStaminaFull);
            heroStamina.OnLowStamina.AddListener(OnLowStamina);
            heroStamina.OnStaminaRecovered.AddListener(OnStaminaRecovered);
            
            // 初始化显示值
            currentDisplayStamina = heroStamina.StaminaPercentage;
        }
    }
    
    private void LoadUISprites()
    {
        // 从Resources加载图片
        Sprite borderSprite = Resources.Load<Sprite>("Art/border");
        Sprite fillSprite = Resources.Load<Sprite>("Art/fill");
        Sprite heartSprite = Resources.Load<Sprite>("Art/heart");
        Sprite expSprite = Resources.Load<Sprite>("Art/Stamina"); // 用作发力条
        
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
        
        // 应用体力条图片
        if (staminaBorder != null && borderSprite != null)
        {
            staminaBorder.sprite = borderSprite;
            staminaBorder.type = Image.Type.Sliced;
        }
        
        if (staminaFill != null && expSprite != null)
        {
            staminaFill.sprite = expSprite;
            staminaFill.type = Image.Type.Filled;
            staminaFill.fillMethod = Image.FillMethod.Horizontal;
            // 不在这里设置颜色，让UpdateStaminaColor()处理
        }
        
        // 应用心脏图标
        if (heartIcon != null && heartSprite != null)
        {
            heartIcon.sprite = heartSprite;
        }
    }
    
    private void UpdateHealthDisplay()
    {
        if (heroLife == null || healthFill == null) return;
        
        // 计算目标血量比例
        float targetHealth = (float)heroLife.CurrentHealth / heroLife.MaxHealth;
        
        // 平滑过渡血量显示
        currentDisplayHealth = Mathf.Lerp(currentDisplayHealth, targetHealth, Time.deltaTime / healthBarSmoothTime);
        healthFill.fillAmount = currentDisplayHealth;
        
        // 更新血条颜色
        UpdateHealthColor();
    }
    
    private void UpdateStaminaDisplay()
    {
        if (heroStamina == null || staminaFill == null) return;
        
        // 计算目标体力比例
        float targetStamina = heroStamina.StaminaPercentage;
        
        // 平滑过渡体力显示
        currentDisplayStamina = Mathf.Lerp(currentDisplayStamina, targetStamina, Time.deltaTime / staminaBarSmoothTime);
        staminaFill.fillAmount = currentDisplayStamina;
        
        // 调试信息
        if (Time.frameCount % 60 == 0) // 每秒输出一次
        {
            Debug.Log($"Stamina: {heroStamina.CurrentStamina:F1}/{heroStamina.MaxStamina} ({heroStamina.StaminaPercentage:P1}) - IsEmpty: {heroStamina.IsStaminaEmpty}, IsLow: {heroStamina.IsLowStamina}");
        }
    }
    
    private void UpdateHealthColor()
    {
        if (healthFill == null) return;
        
        // 如果正在闪烁，不要在这里改变颜色
        if (isFlashing) return;
        
        float healthPercent = currentDisplayHealth;
        
        // 低血量时变红
        if (healthPercent <= lowHealthThreshold)
        {
            // 血量警告闪烁效果
            if (enableWarningFlash && !isFlashing)
            {
                StartCoroutine(HealthWarningFlash());
            }
            healthFill.color = Color.Lerp(lowHealthColor, normalHealthColor, healthPercent / lowHealthThreshold);
        }
        else
        {
            healthFill.color = normalHealthColor;
            isFlashing = false;
        }
    }
    
    private void UpdateVisualEffects()
    {
        // 心跳效果
        if (enableHeartBeat && heartIcon != null && heroLife != null && !heroLife.IsDead)
        {
            float heartBeat = 1f + Mathf.Sin(Time.time * heartBeatSpeed) * 0.1f * heartBeatScale;
            heartIcon.transform.localScale = originalHeartScale * heartBeat;
        }
    }
    
    private IEnumerator HealthWarningFlash()
    {
        isFlashing = true;
        
        while (heroLife != null && (float)heroLife.CurrentHealth / heroLife.MaxHealth <= lowHealthThreshold && !heroLife.IsDead)
        {
            // 闪烁效果
            float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
            Color flashColor = Color.Lerp(lowHealthColor, Color.white, alpha);
            
            if (healthFill != null)
            {
                healthFill.color = flashColor;
            }
            
            yield return null;
        }
        
        isFlashing = false;
    }
    
    // 事件处理
    private void OnHealthChanged(int newHealth)
    {
        // 血量变化时的额外效果可以在这里添加
    }
    
    private void OnHeroDeath()
    {
        // 死亡时隐藏心跳效果
        if (heartIcon != null)
        {
            heartIcon.transform.localScale = originalHeartScale;
        }
    }
    
    private void OnHeroRespawn()
    {
        // 重生时重置UI状态
        isFlashing = false;
        
        // 重置体力显示
        if (heroStamina != null)
        {
            currentDisplayStamina = heroStamina.StaminaPercentage;
        }
    }
    
    // 体力事件处理
    private void OnStaminaChanged()
    {
        // 体力变化时的额外效果可以在这里添加
        Debug.Log($"Stamina changed: {heroStamina.CurrentStamina:F1}/{heroStamina.MaxStamina}");
    }
    
    private void OnStaminaEmpty()
    {
        // 体力耗尽时的效果
        Debug.Log("Stamina is empty!");
    }
    
    private void OnStaminaFull()
    {
        // 体力满时的效果
        Debug.Log("Stamina is full!");
    }
    
    private void OnLowStamina()
    {
        // 低体力警告
        Debug.Log("Warning: Low stamina!");
    }
    
    private void OnStaminaRecovered()
    {
        // 体力恢复
        Debug.Log("Stamina recovered!");
    }
    
    // 体力相关的公共方法
    public float GetCurrentStaminaPercent()
    {
        return heroStamina != null ? heroStamina.StaminaPercentage : 1f;
    }
    
    public bool HasEnoughStamina(float amount)
    {
        return heroStamina != null ? heroStamina.HasStaminaFor(amount) : true;
    }
    
    public void SetStaminaBarVisibility(bool visible)
    {
        if (staminaBorder != null) staminaBorder.gameObject.SetActive(visible);
        if (staminaFill != null) staminaFill.gameObject.SetActive(visible);
    }
    
    void OnDestroy()
    {
        // 取消事件订阅
        if (heroLife != null)
        {
            heroLife.OnHealthChanged -= OnHealthChanged;
            heroLife.OnDeath -= OnHeroDeath;
            heroLife.OnRespawn -= OnHeroRespawn;
        }
        
        // 取消体力事件订阅
        if (heroStamina != null)
        {
            heroStamina.OnStaminaChanged.RemoveListener(OnStaminaChanged);
            heroStamina.OnStaminaEmpty.RemoveListener(OnStaminaEmpty);
            heroStamina.OnStaminaFull.RemoveListener(OnStaminaFull);
            heroStamina.OnLowStamina.RemoveListener(OnLowStamina);
            heroStamina.OnStaminaRecovered.RemoveListener(OnStaminaRecovered);
        }
    }
}
