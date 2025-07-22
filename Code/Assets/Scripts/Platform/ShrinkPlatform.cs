using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkPlatform : MonoBehaviour
{
    [Header("平台控制设置")]
    public float cycleDuration = 4f; // 周期时长（秒）
    public float visibleDuration = 2f; // 可见时长（秒）
    public float timeOffset = 0f; // 时间偏移（秒）
    
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private bool isVisible = true;
    private float timer = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        // 获取平台的 SpriteRenderer 和碰撞器组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
        
        // 如果没有找到SpriteRenderer，输出警告信息
        if (spriteRenderer == null)
        {
            Debug.LogWarning("ShrinkPlatform: 未找到SpriteRenderer组件！");
        }
        
        // 如果没有找到Collider2D，输出警告信息
        if (platformCollider == null)
        {
            Debug.LogWarning("ShrinkPlatform: 未找到Collider2D组件！");
        }
        
        // 确保平台初始状态为可见
        SetPlatformVisibility(true);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        
        // 计算当前应该处于什么状态（加入时间偏移）
        float cycleProgress = (timer + timeOffset) % cycleDuration;
        
        if (cycleProgress < visibleDuration)
        {
            // 应该可见
            if (!isVisible)
            {
                SetPlatformVisibility(true);
            }
        }
        else
        {
            // 应该不可见
            if (isVisible)
            {
                SetPlatformVisibility(false);
            }
        }
    }
    
    /// <summary>
    /// 设置平台的可见性和碰撞
    /// </summary>
    /// <param name="visible">是否可见</param>
    private void SetPlatformVisibility(bool visible)
    {
        isVisible = visible;
        
        // 控制 SpriteRenderer 的可见性
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }
        
        // 控制碰撞器的启用状态
        if (platformCollider != null)
        {
            platformCollider.enabled = visible;
        }
    }
}
