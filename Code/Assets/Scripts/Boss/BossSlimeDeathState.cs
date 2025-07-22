using System.Collections;
using UnityEngine;
using Cinemachine;

public class BossSlimeDeathState : IState
{
    private BossSlime boss;
    private BossSlimeParameter parameter;
    private bool deathSequenceStarted = false;
    

    public BossSlimeDeathState(BossSlime boss, BossSlimeParameter parameter)
    {
        this.boss = boss;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("=== BossSlimeDeathState.OnEnter() ===");
        Debug.Log("Boss史莱姆进入死亡状态");
        
        // 设置死亡动画
        if (parameter.animator != null)
        {
            parameter.animator.SetBool("isDead", true);
            parameter.animator.SetBool("isIdle", false);
            parameter.animator.SetBool("isAttacking", false);
            parameter.animator.SetBool("isHurt", false);
            parameter.animator.SetTrigger("death");
            Debug.Log("死亡动画触发完成");
        }
        else
        {
            Debug.LogWarning("Animator组件为空，无法播放死亡动画");
        }
        
        // 停止所有移动并设置物理状态
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f; // 将重力设置为0
            Debug.Log("停止Boss移动并将重力设置为0");
        }
        
        // 禁用碰撞器
        Collider2D collider = boss.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            Debug.Log("已禁用Boss碰撞器");
        }
        
        // 如果有多个碰撞器，全部禁用
        Collider2D[] allColliders = boss.GetComponents<Collider2D>();
        if (allColliders != null && allColliders.Length > 0)
        {
            foreach (var col in allColliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
            Debug.Log($"已禁用Boss的所有碰撞器，共{allColliders.Length}个");
        }
        
        // 开始死亡序列
        if (!deathSequenceStarted)
        {
            deathSequenceStarted = true;
            Debug.Log("启动死亡序列协程");
            boss.StartCoroutine(DeathSequence());
        }
        else
        {
            Debug.LogWarning("死亡序列已经启动，跳过重复启动");
        }
    }
    
    public void OnUpdate()
    {
        // 死亡状态不需要更新逻辑
        // 一切都在死亡序列协程中处理
    }
    
    public void OnExit()
    {
        // 死亡状态是最终状态，不需要退出逻辑
    }
    
    private IEnumerator DeathSequence()
    {
        Debug.Log("=== 死亡序列调试 ===");
        Debug.Log("1. Boss史莱姆死亡序列开始");
        
        // 创建死亡特效
        Debug.Log("2. 开始创建死亡特效");
        
        Debug.Log("3. 死亡特效创建完成");
        
        // 等待死亡动画播放
        Debug.Log("4. 等待死亡动画播放（2秒）");
        
        Debug.Log("5. 死亡动画等待完成");
        
        // 处理死亡奖励/掉落
        Debug.Log("6. 处理死亡奖励");
       
        Debug.Log("7. 死亡奖励处理完成");
        
        // 等待更多时间以产生戏剧效果
        Debug.Log("8. 等待戏剧效果（1秒）");
        
        Debug.Log("9. 戏剧效果等待完成");
        
        // 关键步骤：创建BossDemon
        Debug.Log("10. ⭐ 准备召唤BossDemon ⭐");
        Debug.Log($"转换配置检查 - enableBossTransition: {parameter.enableBossTransition}");
        SpawnBossDemon();
        Debug.Log("11. ⭐ BossDemon召唤流程完成 ⭐");
        
        // 淡出并销毁
        Debug.Log("12. 开始淡出并销毁");
        yield return boss.StartCoroutine(FadeOutAndDestroy());
        Debug.Log("13. 死亡序列完全结束");
    }
    
    /// <summary>
    /// 在BossSlime死亡位置生成BossDemon
    /// </summary>
    private void SpawnBossDemon()
    {
        Debug.Log("=== SpawnBossDemon调试 ===");
        
        // 检查是否启用Boss转换
        if (!parameter.enableBossTransition)
        {
            Debug.LogError("❌ Boss转换功能已禁用，跳过生成下一个Boss");
            Debug.LogError("请在Inspector中勾选 'Enable Boss Transition'");
            return;
        }
        
        Debug.Log("✅ Boss转换功能已启用");
        
        try
        {
            Debug.Log("开始生成下一个Boss...");
            
            // 记录BossSlime的位置
            Vector3 spawnPosition = boss.transform.position;
            Debug.Log($"生成位置: {spawnPosition}");
            GameObject nextBoss = null;
            
            // 优先使用配置的预制体
            if (parameter.nextBossPrefab != null)
            {
                Debug.Log($"使用配置的预制体: {parameter.nextBossPrefab.name}");
                nextBoss = Object.Instantiate(parameter.nextBossPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"✅ 使用配置的预制体生成Boss: {parameter.nextBossPrefab.name}");
            }
            else
            {
                Debug.Log("未配置预制体，使用动态创建");
                // 回退方案：动态创建BossDemon
                nextBoss = CreateBossDemonDynamically(spawnPosition);
                Debug.Log("✅ 动态创建BossDemon完成");
            }
            
            if (nextBoss != null)
            {
                Debug.Log("✅ Boss对象创建成功，开始后续配置");
                
                // 设置Boss血量
                Debug.Log("设置Boss血量...");
                SetNextBossHealth(nextBoss);
                
                // 生成召唤特效
                Debug.Log("生成召唤特效...");
                CreateSpawnEffect(spawnPosition);
                
                // 生成召唤冲击波
                Debug.Log("生成召唤冲击波...");
                CreateSpawnShockwave(spawnPosition);
                
                Debug.Log($"✅ 下一个Boss已成功生成，位置: {spawnPosition}");
                
                // 通知Boss的UI
                Debug.Log("通知Boss UI系统...");
                NotifyBossUISystem(nextBoss);
                
                Debug.Log("🎉 BossDemon召唤完全成功！");
            }
            else
            {
                Debug.LogError("❌ Boss对象创建失败");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 生成下一个Boss时失败： {e.Message}");
            Debug.LogError($"错误堆栈: {e.StackTrace}");
            
            // 防错方案：从Resources加载简单的BossDemon
            Debug.Log("尝试使用备用方案...");
            TryLoadBossDemonFromResources();
        }
    }
    
    /// <summary>
    /// 动态创建BossDemon
    /// </summary>
    private GameObject CreateBossDemonDynamically(Vector3 spawnPosition)
    {
        Debug.Log("=== 动态创建BossDemon ===");
        Debug.Log("动态创建BossDemon...");
        
        // 创建BossDemon GameObject
        GameObject bossDemonObj = new GameObject("BossDemon");
        bossDemonObj.transform.position = spawnPosition;
        Debug.Log($"创建BossDemon GameObject，位置: {spawnPosition}");
        
        // 添加BossDemonSetup自动配置BossDemon
        BossDemonSetup setup = bossDemonObj.AddComponent<BossDemonSetup>();
        Debug.Log("添加BossDemonSetup组件");
        
        return bossDemonObj;
    }
    
    /// <summary>
    /// 设置下一个Boss的血量
    /// </summary>
    private void SetNextBossHealth(GameObject nextBoss)
    {
        try
        {
            // 尝试通过BossDemon组件设置血量
            var bossDemon = nextBoss.GetComponent<BossDemon>();
            if (bossDemon != null)
            {
                // 使用反射获取BossDemon的血量属性
                System.Reflection.FieldInfo maxHealthField = typeof(BossDemon).GetField("maxHealth", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                System.Reflection.FieldInfo currentHealthField = typeof(BossDemon).GetField("currentHealth", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (maxHealthField != null && currentHealthField != null)
                {
                    int maxHealth = (int)maxHealthField.GetValue(bossDemon);
                    int spawnHealth = Mathf.RoundToInt(maxHealth * parameter.nextBossHealthPercentage);
                    currentHealthField.SetValue(bossDemon, spawnHealth);
                    Debug.Log($"BossDemon生成完毕，血量: {spawnHealth}/{maxHealth} ({parameter.nextBossHealthPercentage:P0})");
                }
            }
            
            // 尝试通过BossLife组件设置血量
            var bossLife = nextBoss.GetComponent<BossLife>();
            if (bossLife != null)
            {
                int currentHealth = Mathf.RoundToInt(bossLife.MaxHealth * parameter.nextBossHealthPercentage);
                
                // 使用反射设置BossLife的当前血量
                System.Reflection.FieldInfo currentHealthField = typeof(BossLife).GetField("currentHealth", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (currentHealthField != null)
                {
                    currentHealthField.SetValue(bossLife, currentHealth);
                    Debug.Log($"通过BossLife设置血量: {currentHealth}/{bossLife.MaxHealth}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"设置下一个Boss血量时出错： {e.Message}");
        }
    }
    
    private IEnumerator CreateDeathEffects()
    {
        // 创建简单的死亡特效粒子
        for (int i = 0; i < 10; i++)
        {
            CreateDeathParticle();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void CreateDeathParticle()
    {
        GameObject particle = new GameObject("DeathParticle");
        particle.transform.position = boss.transform.position + (Vector3)Random.insideUnitCircle * 2f;
        
        SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
        
        // 创建简单的粒子纹理
        Texture2D texture = new Texture2D(8, 8);
        Color[] colors = new Color[8 * 8];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(1f, 0.5f, 0.2f, 0.8f); // 橙色粒子
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
        sr.sprite = sprite;
        sr.sortingOrder = 10;
        
        // 动画播放粒子
        boss.StartCoroutine(AnimateDeathParticle(particle));
    }
    
    private IEnumerator AnimateDeathParticle(GameObject particle)
    {
        float lifetime = 1f;
        float elapsedTime = 0f;
        Vector3 startPos = particle.transform.position;
        Vector3 endPos = startPos + Vector3.up * Random.Range(2f, 4f);
        
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        
        while (elapsedTime < lifetime)
        {
            float t = elapsedTime / lifetime;
            
            // 向上移动
            particle.transform.position = Vector3.Lerp(startPos, endPos, t);
            
            // 淡出
            if (sr != null)
            {
                Color color = sr.color;
                color.a = 1f - t;
                sr.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Object.Destroy(particle);
    }
    
    private void HandleDeathRewards()
    {
        Debug.Log("Boss史莱姆死亡奖励已处理");
        
        // 这里可以生成物品、给予经验等
        // 现在只是记录死亡
        
        // 可能的实现：
        // - 生成血量/法力药水
        // - 给予玩家经验值
        // - 解锁新区域
        // - 触发剧情事件
    }
    
    private IEnumerator FadeOutAndDestroy()
    {
        SpriteRenderer sr = boss.GetComponent<SpriteRenderer>();
        
        if (sr != null)
        {
            Color startColor = sr.color;
            float fadeTime = 2f;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeTime)
            {
                float t = elapsedTime / fadeTime;
                Color color = startColor;
                color.a = 1f - t;
                sr.color = color;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        Debug.Log("Boss史莱姆已被销毁");
        Object.Destroy(boss.gameObject,0.5f);
    }
    
    /// <summary>
    /// 生成召唤特效
    /// </summary>
    private void CreateSpawnEffect(Vector3 position)
    {
        try
        {
            Debug.Log("创建召唤特效");
            
            // 创建多个召唤粒子效果
            for (int i = 0; i < 20; i++)
            {
                CreateSpawnParticle(position);
            }
            
            // 触发相机震动
            if (CamaraShakeManager.Instance != null)
            {
                var impulseSource = boss.GetComponent<Cinemachine.CinemachineImpulseSource>();
                if (impulseSource != null)
                {
                    CamaraShakeManager.Instance.CamaraShake(impulseSource);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建召唤特效时失败： {e.Message}");
        }
    }
    
    /// <summary>
    /// 创建召唤粒子
    /// </summary>
    private void CreateSpawnParticle(Vector3 centerPosition)
    {
        GameObject particle = new GameObject("SpawnParticle");
        particle.transform.position = centerPosition + (Vector3)Random.insideUnitCircle * 3f;
        
        SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
        
        // 创建强烈的红色粒子
        Texture2D texture = new Texture2D(12, 12);
        Color[] colors = new Color[12 * 12];
        Color particleColor = new Color(0.8f, 0.2f, 0.2f, 1f); // 红色粒子
        
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = particleColor;
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 12, 12), new Vector2(0.5f, 0.5f));
        sr.sprite = sprite;
        sr.sortingOrder = 15;
        
        // 播放动画
        boss.StartCoroutine(AnimateSpawnParticle(particle, centerPosition));
    }
    
    /// <summary>
    /// 召唤粒子动画播放协程
    /// </summary>
    private IEnumerator AnimateSpawnParticle(GameObject particle, Vector3 centerPosition)
    {
        float lifetime = 1.5f;
        float elapsedTime = 0f;
        Vector3 startPos = particle.transform.position;
        Vector3 endPos = centerPosition + Vector3.up * Random.Range(3f, 6f);
        
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        
        while (elapsedTime < lifetime)
        {
            float t = elapsedTime / lifetime;
            
            // 螺旋上升移动
            particle.transform.position = Vector3.Lerp(startPos, endPos, t * t); // 加速上升
            
            // 淡出
            if (sr != null)
            {
                Color color = sr.color;
                color.a = 1f - t;
                sr.color = color;
                
                // 添加放大效果
                float scale = 1f + t * 0.5f;
                particle.transform.localScale = Vector3.one * scale;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Object.Destroy(particle);
    }
    
    /// <summary>
    /// 创建召唤冲击波
    /// </summary>
    private void CreateSpawnShockwave(Vector3 position)
    {
        try
        {
            Debug.Log("创建召唤冲击波");
            
            // 寻找距离位置近的玩家
            Collider2D[] nearbyTargets = Physics2D.OverlapCircleAll(position, 8f);
            
            foreach (var target in nearbyTargets)
            {
                if (target.CompareTag("Player"))
                {
                    // 作为BossDemon为玩家的一种凶猛的感觉
                    HeroLife playerLife = target.GetComponent<HeroLife>();
                    if (playerLife != null)
                    {
                        playerLife.TakeDamage(20); // 中等召唤伤害
                        Debug.Log("BossDemon生成时对玩家造成冲击");
                    }
                    
                    // 添加击退效果
                    Rigidbody2D playerRb = target.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDirection = (target.transform.position - position).normalized;
                        playerRb.AddForce(knockbackDirection * 8f, ForceMode2D.Impulse);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建召唤冲击波时失败： {e.Message}");
        }
    }
    
    /// <summary>
    /// 通知Boss UI系统
    /// </summary>
    private void NotifyBossUISystem(GameObject bossDemon)
    {
        try
        {
            // 确保BossLife组件
            BossLife bossLife = bossDemon.GetComponent<BossLife>();
            if (bossLife == null)
            {
                bossLife = bossDemon.AddComponent<BossLife>();
            }
            
            // 寻找并通知BossLifeUI
            var bossLifeUI = Object.FindObjectOfType<BossLifeUI>();
            if (bossLifeUI != null)
            {
                Debug.Log("通知BossLifeUI新的Boss生成");
                // BossLifeUI应该自动检测新的Boss
            }
            
            Debug.Log("Boss UI系统已更新");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"通知Boss UI系统失败： {e.Message}");
        }
    }
    
    /// <summary>
    /// 备用时从Resources加载简单的BossDemon
    /// </summary>
    private void TryLoadBossDemonFromResources()
    {
        try
        {
            // 尝试从Resources文件夹加载BossDemon Prefab
            GameObject bossDemonPrefab = Resources.Load<GameObject>("BossDemon");
            if (bossDemonPrefab != null)
            {
                Vector3 spawnPosition = boss.transform.position;
                GameObject bossDemon = Object.Instantiate(bossDemonPrefab, spawnPosition, Quaternion.identity);
                Debug.Log("从资源加载BossDemon Prefab");
                
                CreateSpawnEffect(spawnPosition);
                NotifyBossUISystem(bossDemon);
            }
            else
            {
                Debug.LogWarning("在Resources文件夹中未找到BossDemon Prefab");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"从Resources加载BossDemon失败： {e.Message}");
        }
    }
}