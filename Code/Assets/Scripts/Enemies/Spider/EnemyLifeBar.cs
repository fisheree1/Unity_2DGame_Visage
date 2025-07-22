using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyLifeBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    
    [Header("Health Bar Settings")]
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] private bool enableSmoothAnimation = true;
    
    [Header("Auto-Find EnemyLife")]
    [SerializeField] private bool autoFindEnemyLife = true;
    [SerializeField] private EnemyLife enemyLife;
    
    private float fullWidth;
    private float TargetWidth => enemyLife != null ? enemyLife.HealthPercentage * fullWidth : 0f;
    
    private Coroutine adjustBarWidthCoroutine;
    
    private void Start()
    {
        InitializeComponents();
        InitializeHealthBar();
    }
    
    private void InitializeComponents()
    {
        // 自动查找 EnemyLife 组件
        if (autoFindEnemyLife && enemyLife == null)
        {
            enemyLife = GetComponent<EnemyLife>();
            if (enemyLife == null)
            {
                enemyLife = GetComponentInParent<EnemyLife>();
            }
            
            if (enemyLife == null)
            {
                Debug.LogWarning($"EnemyLifeBar on {gameObject.name} couldn't find EnemyLife component!");
                return;
            }
        }
        
        // 验证必要组件
        if (topBar == null || bottomBar == null)
        {
            Debug.LogError($"EnemyLifeBar on {gameObject.name} missing topBar or bottomBar references!");
            return;
        }
        
        // 获取血条的完整宽度
        fullWidth = topBar.rect.width;
        
        // 订阅事件
        SubscribeToEnemyLifeEvents();
    }
    
    private void SubscribeToEnemyLifeEvents()
    {
        if (enemyLife != null)
        {
            enemyLife.OnHealthChanged += OnHealthChanged;
            enemyLife.OnDamageTaken += OnDamageTaken;
            enemyLife.OnHealed += OnHealed;
            enemyLife.OnDeath += OnDeath;
        }
    }
    
    private void UnsubscribeFromEnemyLifeEvents()
    {
        if (enemyLife != null)
        {
            enemyLife.OnHealthChanged -= OnHealthChanged;
            enemyLife.OnDamageTaken -= OnDamageTaken;
            enemyLife.OnHealed -= OnHealed;
            enemyLife.OnDeath -= OnDeath;
        }
    }
    
    private void InitializeHealthBar()
    {
        if (enemyLife != null)
        {
            // 设置初始血条宽度
            float initialWidth = TargetWidth;
            topBar.SetWidth(initialWidth);
            bottomBar.SetWidth(initialWidth);
            
            Debug.Log($"EnemyLifeBar initialized for {gameObject.name}: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth}");
        }
    }
    
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        Debug.Log($"Enemy {gameObject.name} health changed: {currentHealth}/{maxHealth} ({healthPercent:P1})");
        
        // 更新血条宽度
        UpdateHealthBarWidth();
    }
    
    private void OnDamageTaken(int damage)
    {
        Debug.Log($"Enemy {gameObject.name} took {damage} damage");
        // 伤害时的额外效果可以在这里添加
    }
    
    private void OnHealed(int healAmount)
    {
        Debug.Log($"Enemy {gameObject.name} healed {healAmount} HP");
        // 治疗时的额外效果可以在这里添加
    }
    
    private void OnDeath()
    {
        Debug.Log($"Enemy {gameObject.name} died - hiding health bar");
        // 死亡时隐藏血条或播放死亡动画
        gameObject.SetActive(false);
    }
    
    private void UpdateHealthBarWidth()
    {
        if (enemyLife == null || topBar == null || bottomBar == null) return;
        
        if (adjustBarWidthCoroutine != null)
        {
            StopCoroutine(adjustBarWidthCoroutine);
        }
        
        if (enableSmoothAnimation)
        {
            adjustBarWidthCoroutine = StartCoroutine(AdjustBarWidthCoroutine());
        }
        else
        {
            // 立即更新
            float targetWidth = TargetWidth;
            topBar.SetWidth(targetWidth);
            bottomBar.SetWidth(targetWidth);
        }
    }
    
    private IEnumerator AdjustBarWidthCoroutine()
    {
        float targetWidth = TargetWidth;
        
        // 立即更新上层血条（快速响应）
        topBar.SetWidth(targetWidth);
        
        // 平滑更新下层血条（延迟动画）
        while (Mathf.Abs(bottomBar.rect.width - targetWidth) > 0.1f)
        {
            float newWidth = Mathf.Lerp(bottomBar.rect.width, targetWidth, Time.deltaTime * animationSpeed);
            bottomBar.SetWidth(newWidth);
            yield return null;
        }
        
        // 确保最终宽度准确
        bottomBar.SetWidth(targetWidth);
        adjustBarWidthCoroutine = null;
    }
    
    // 公共方法
    public void SetEnemyLife(EnemyLife newEnemyLife)
    {
        if (enemyLife != null)
        {
            UnsubscribeFromEnemyLifeEvents();
        }
        
        enemyLife = newEnemyLife;
        
        if (enemyLife != null)
        {
            SubscribeToEnemyLifeEvents();
            InitializeHealthBar();
        }
    }
    
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Max(0.1f, speed);
    }
    
    public void SetSmoothAnimation(bool enabled)
    {
        enableSmoothAnimation = enabled;
    }
    
    // 用于测试的方法（可以删除）
    private void Update()
    {
        
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEnemyLifeEvents();
    }
    
    // 调试方法
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        // 编辑器中的验证
        if (autoFindEnemyLife && enemyLife == null)
        {
            enemyLife = GetComponent<EnemyLife>();
            if (enemyLife == null)
            {
                enemyLife = GetComponentInParent<EnemyLife>();
            }
        }
    }
}
