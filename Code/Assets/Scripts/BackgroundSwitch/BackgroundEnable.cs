using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BackgroundEnable : MonoBehaviour
{

    [SerializeField] private float leftBound;
    [SerializeField] private float rightBound;

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject hero;
    
    private HeroLife heroLife;
    void Start()
    {
        // 如果没有手动指定hero，尝试自动查找
        if (hero == null)
        {
            hero = GameObject.FindGameObjectWithTag("Player");
        }
        
        // 获取HeroLife组件
        if (hero != null)
        {
            heroLife = hero.GetComponent<HeroLife>();
            if (heroLife != null)
            {
                // 订阅复活事件
                heroLife.OnRespawn += OnHeroRespawn;
                
                // 初始检查背景状态
                UpdateBackgroundVisibility();
            }
            else
            {
                Debug.LogWarning("HeroLife component not found on hero GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("Hero GameObject not found. Please assign the hero GameObject in the inspector or make sure it has the 'Player' tag.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 现在Update方法为空，因为我们只在复活时更新背景
        // 如果需要其他持续的逻辑，可以在这里添加
    }
    
    /// <summary>
    /// 当角色复活时调用的方法
    /// </summary>
    private void OnHeroRespawn()
    {
        Debug.Log("Hero respawned, updating background visibility.");
        UpdateBackgroundVisibility();
    }
    
    /// <summary>
    /// 更新背景可见性的方法
    /// </summary>
    private void UpdateBackgroundVisibility()
    {
        if (hero != null && background != null)
        {
            bool shouldBeActive = !(hero.transform.position.x < leftBound || hero.transform.position.x > rightBound);
            
            if (background.activeSelf != shouldBeActive)
            {
                background.SetActive(shouldBeActive);
                Debug.Log($"Background set to: {shouldBeActive} (Hero position: {hero.transform.position.x}, Bounds: {leftBound} to {rightBound})");
            }
        }
    }
    
    /// <summary>
    /// 清理事件订阅
    /// </summary>
    private void OnDestroy()
    {
        if (heroLife != null)
        {
            heroLife.OnRespawn -= OnHeroRespawn;
        }
    }
}
