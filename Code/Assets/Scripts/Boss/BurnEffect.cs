using System.Collections;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    [Header("燃烧设置")]
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float tickInterval = 0.5f;
    
    [Header("视觉效果")]
    [SerializeField] private Color burnColor = Color.red;
    [SerializeField] private float flashIntensity = 0.3f;
    
    private HeroLife heroLife;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float remainingDuration;
    private bool isActive = false;
    
    void Start()
    {
        Initialize(damagePerSecond, duration);
    }
    
    /// <summary>
    /// 初始化燃烧效果
    /// </summary>
    /// <param name="damagePerSec">每秒伤害</param>
    /// <param name="burnDuration">燃烧持续时间</param>
    public void Initialize(float damagePerSec, float burnDuration)
    {
        damagePerSecond = damagePerSec;
        duration = burnDuration;
        remainingDuration = burnDuration;
        
        // 获取组件
        heroLife = GetComponent<HeroLife>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        StartBurnEffect();
    }
    
    /// <summary>
    /// 重置燃烧持续时间
    /// </summary>
    public void ResetBurnDuration()
    {
        remainingDuration = duration;
        Debug.Log($"燃烧效果重置: {remainingDuration} 秒剩余");
    }
    
    /// <summary>
    /// 刷新燃烧效果
    /// </summary>
    /// <param name="newDuration">新的持续时间</param>
    public void RefreshBurn(float newDuration)
    {
        remainingDuration = newDuration;
        Debug.Log($"燃烧效果刷新: {remainingDuration} 秒剩余");
    }
    
    /// <summary>
    /// 开始燃烧效果
    /// </summary>
    private void StartBurnEffect()
    {
        if (isActive) return;
        
        isActive = true;
        StartCoroutine(BurnCoroutine());
        StartCoroutine(VisualEffectCoroutine());
        
        Debug.Log($"燃烧效果开始: {damagePerSecond} 伤害/秒，持续 {duration} 秒");
    }
    
    /// <summary>
    /// 燃烧伤害协程
    /// </summary>
    private IEnumerator BurnCoroutine()
    {
        while (remainingDuration > 0 && heroLife != null && !heroLife.IsDead)
        {
            // 造成伤害
            float damageThisTick = damagePerSecond * tickInterval;
            heroLife.TakeDamage(Mathf.RoundToInt(damageThisTick));
            
            Debug.Log($"燃烧伤害: {damageThisTick}，剩余时间: {remainingDuration}");
            
            // 等待下一次伤害
            yield return new WaitForSeconds(tickInterval);
            remainingDuration -= tickInterval;
        }
        
        EndBurnEffect();
    }
    
    /// <summary>
    /// 视觉效果协程
    /// </summary>
    private IEnumerator VisualEffectCoroutine()
    {
        while (remainingDuration > 0 && spriteRenderer != null)
        {
            // 闪烁红色
            spriteRenderer.color = Color.Lerp(originalColor, burnColor, flashIntensity);
            yield return new WaitForSeconds(0.1f);
            
            // 恢复原色
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
        
        // 确保恢复原色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    /// <summary>
    /// 结束燃烧效果
    /// </summary>
    private void EndBurnEffect()
    {
        isActive = false;
        
        // 恢复原色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        Debug.Log("燃烧效果结束");
        
        // 销毁此组件
        Destroy(this);
    }
    
    void Update()
    {
        // 安全检查 - 如果英雄死亡则结束燃烧
        if (heroLife != null && heroLife.IsDead)
        {
            EndBurnEffect();
        }
    }
    
    void OnDestroy()
    {
        // 销毁时恢复原色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    #region 调试方法
    
    /// <summary>
    /// 测试燃烧效果
    /// </summary>
    [ContextMenu("测试燃烧效果")]
    public void TestBurnEffect()
    {
        Initialize(8f, 3f);
        Debug.Log("燃烧效果测试开始");
    }
    
    /// <summary>
    /// 立即停止燃烧效果
    /// </summary>
    [ContextMenu("停止燃烧效果")]
    public void StopBurnEffect()
    {
        remainingDuration = 0f;
        EndBurnEffect();
        Debug.Log("燃烧效果强制停止");
    }
    
    #endregion
}