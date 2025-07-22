using UnityEngine;

[System.Serializable]
public class BossDemonSetup : MonoBehaviour
{
    [Header("自动设置")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createPrefabs = true;
    [SerializeField] private bool setupUI = true;
    
    [Header("Boss配置")]
    [SerializeField] private int bossMaxHealth = 400;
    [SerializeField] private float bossMovementSpeed = 4f;
    [SerializeField] private float bossDetectionRange = 15f;
    
    [Header("预制体创建")]
    [SerializeField] private bool createFireballPrefab = true;
    [SerializeField] private bool createBulletPrefab = true;
    [SerializeField] private bool createMinionPrefab = true;
    
    private BossDemon bossDemon;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupBossDemon();
        }
    }
    
    /// <summary>
    /// 设置恶魔Boss
    /// </summary>
    [ContextMenu("设置恶魔Boss")]
    public void SetupBossDemon()
    {
        Debug.Log("正在设置恶魔Boss...");
        
        // 获取或添加BossDemon组件
        bossDemon = GetComponent<BossDemon>();
        if (bossDemon == null)
        {
            bossDemon = gameObject.AddComponent<BossDemon>();
        }
        
        // 设置必要组件
        SetupComponents();
        
        // 如果需要则创建预制体
        if (createPrefabs)
        {
            CreateProjectilePrefabs();
        }
        
        // 设置UI集成
        if (setupUI)
        {
            SetupUIIntegration();
        }
        
        Debug.Log("恶魔Boss设置完成！");
    }
    
    /// <summary>
    /// 设置必要组件
    /// </summary>
    private void SetupComponents()
    {
        // 确保Boss有必要的组件
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.freezeRotation = true;
        }
        
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 3f);
        }
        
        if (GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBossSprite();
            sr.color = Color.red;
        }
        
        if (GetComponent<Animator>() == null)
        {
            gameObject.AddComponent<Animator>();
        }
        
        // 设置Boss标签
        if (!gameObject.CompareTag("Boss"))
        {
            gameObject.tag = "Boss";
        }
    }
    
    /// <summary>
    /// 创建投射物预制体
    /// </summary>
    private void CreateProjectilePrefabs()
    {
        // 创建火球预制体
        if (createFireballPrefab)
        {
            GameObject fireballPrefab = CreateFireballPrefab();
            AssignFireballPrefab(fireballPrefab);
        }
        
        // 创建子弹预制体
        if (createBulletPrefab)
        {
            GameObject bulletPrefab = CreateBulletPrefab();
            AssignBulletPrefab(bulletPrefab);
        }
        
        // 创建小怪预制体
        if (createMinionPrefab)
        {
            GameObject minionPrefab = CreateMinionPrefab();
            AssignMinionPrefab(minionPrefab);
        }
    }
    
    /// <summary>
    /// 创建火球预制体
    /// </summary>
    /// <returns>火球预制体</returns>
    private GameObject CreateFireballPrefab()
    {
        GameObject fireball = new GameObject("BossFireball");
        fireball.AddComponent<BossFireball>();
        
        // 在实际中，这会在编辑器中创建为预制体资源
        return fireball;
    }
    
    /// <summary>
    /// 创建子弹预制体
    /// </summary>
    /// <returns>子弹预制体</returns>
    private GameObject CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("BossBullet");
        bullet.AddComponent<BossBullet>();
        
        return bullet;
    }
    
    /// <summary>
    /// 创建小怪预制体
    /// </summary>
    /// <returns>小怪预制体</returns>
    private GameObject CreateMinionPrefab()
    {
        GameObject minion = new GameObject("DemonMinion");
        minion.AddComponent<DemonMinion>();
        
        // 添加基础组件
        minion.AddComponent<Rigidbody2D>();
        minion.AddComponent<BoxCollider2D>();
        SpriteRenderer minionSr = minion.AddComponent<SpriteRenderer>();
        minionSr.sprite = CreateMinionSprite();
        minionSr.color = Color.magenta;
        
        return minion;
    }
    
    /// <summary>
    /// 创建Boss精灵
    /// </summary>
    /// <returns>Boss精灵</returns>
    private Sprite CreateBossSprite()
    {
        // 创建一个简单的Boss精灵
        Texture2D texture = new Texture2D(64, 96);
        Color[] colors = new Color[64 * 96];
        
        // 创建一个简单的恶魔形状
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 96; y++)
            {
                float centerX = 32f;
                float centerY = 48f;
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                
                if (distance <= 30f)
                {
                    // 身体
                    colors[y * 64 + x] = Color.red;
                }
                else if (y > 80 && distance <= 35f)
                {
                    // 头部 - 使用深红色
                    colors[y * 64 + x] = new Color(0.5f, 0f, 0f); // 深红色
                }
                else
                {
                    colors[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 96), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// 创建小怪精灵
    /// </summary>
    /// <returns>小怪精灵</returns>
    private Sprite CreateMinionSprite()
    {
        // 创建一个简单的小怪精灵
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        Vector2 center = new Vector2(16, 16);
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 14f)
                {
                    colors[y * 32 + x] = Color.magenta;
                }
                else
                {
                    colors[y * 32 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// 分配火球预制体给Boss
    /// </summary>
    /// <param name="prefab">预制体</param>
    private void AssignFireballPrefab(GameObject prefab)
    {
        // 使用反射将预制体分配给Boss
        var field = typeof(BossDemon).GetField("fireballPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(bossDemon, prefab);
        }
    }
    
    /// <summary>
    /// 分配子弹预制体给Boss
    /// </summary>
    /// <param name="prefab">预制体</param>
    private void AssignBulletPrefab(GameObject prefab)
    {
        var field = typeof(BossDemon).GetField("bulletPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(bossDemon, prefab);
        }
    }
    
    /// <summary>
    /// 分配小怪预制体给Boss
    /// </summary>
    /// <param name="prefab">预制体</param>
    private void AssignMinionPrefab(GameObject prefab)
    {
        var field = typeof(BossDemon).GetField("minionPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(bossDemon, prefab);
        }
    }
    
    /// <summary>
    /// 设置UI集成
    /// </summary>
    private void SetupUIIntegration()
    {
        // 查找现有的Boss UI
        BossLifeUI bossUI = FindObjectOfType<BossLifeUI>();
        if (bossUI != null)
        {
            Debug.Log("找到现有的BossLifeUI - 集成将自动工作");
        }
        else
        {
            Debug.Log("未找到BossLifeUI - Boss血条可能不会显示");
        }
    }
    
    /// <summary>
    /// 测试Boss设置
    /// </summary>
    [ContextMenu("测试Boss设置")]
    public void TestBossSetup()
    {
        if (bossDemon == null)
        {
            bossDemon = GetComponent<BossDemon>();
        }
        
        if (bossDemon != null)
        {
            Debug.Log($"恶魔Boss状态:");
            Debug.Log($"- 生命值: {bossDemon.HealthPercentage * 100}%");
            Debug.Log($"- 第二阶段: {bossDemon.IsInPhase2}");
            Debug.Log($"- 已死亡: {bossDemon.IsDead}");
        }
        else
        {
            Debug.LogError("未找到BossDemon组件！");
        }
    }
}