using UnityEngine;

[System.Serializable]
public class BossDemonSetup : MonoBehaviour
{
    [Header("�Զ�����")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createPrefabs = true;
    [SerializeField] private bool setupUI = true;
    
    [Header("Boss����")]
    [SerializeField] private int bossMaxHealth = 400;
    [SerializeField] private float bossMovementSpeed = 4f;
    [SerializeField] private float bossDetectionRange = 15f;
    
    [Header("Ԥ���崴��")]
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
    /// ���ö�ħBoss
    /// </summary>
    [ContextMenu("���ö�ħBoss")]
    public void SetupBossDemon()
    {
        Debug.Log("�������ö�ħBoss...");
        
        // ��ȡ�����BossDemon���
        bossDemon = GetComponent<BossDemon>();
        if (bossDemon == null)
        {
            bossDemon = gameObject.AddComponent<BossDemon>();
        }
        
        // ���ñ�Ҫ���
        SetupComponents();
        
        // �����Ҫ�򴴽�Ԥ����
        if (createPrefabs)
        {
            CreateProjectilePrefabs();
        }
        
        // ����UI����
        if (setupUI)
        {
            SetupUIIntegration();
        }
        
        Debug.Log("��ħBoss������ɣ�");
    }
    
    /// <summary>
    /// ���ñ�Ҫ���
    /// </summary>
    private void SetupComponents()
    {
        // ȷ��Boss�б�Ҫ�����
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
        
        // ����Boss��ǩ
        if (!gameObject.CompareTag("Boss"))
        {
            gameObject.tag = "Boss";
        }
    }
    
    /// <summary>
    /// ����Ͷ����Ԥ����
    /// </summary>
    private void CreateProjectilePrefabs()
    {
        // ��������Ԥ����
        if (createFireballPrefab)
        {
            GameObject fireballPrefab = CreateFireballPrefab();
            AssignFireballPrefab(fireballPrefab);
        }
        
        // �����ӵ�Ԥ����
        if (createBulletPrefab)
        {
            GameObject bulletPrefab = CreateBulletPrefab();
            AssignBulletPrefab(bulletPrefab);
        }
        
        // ����С��Ԥ����
        if (createMinionPrefab)
        {
            GameObject minionPrefab = CreateMinionPrefab();
            AssignMinionPrefab(minionPrefab);
        }
    }
    
    /// <summary>
    /// ��������Ԥ����
    /// </summary>
    /// <returns>����Ԥ����</returns>
    private GameObject CreateFireballPrefab()
    {
        GameObject fireball = new GameObject("BossFireball");
        fireball.AddComponent<BossFireball>();
        
        // ��ʵ���У�����ڱ༭���д���ΪԤ������Դ
        return fireball;
    }
    
    /// <summary>
    /// �����ӵ�Ԥ����
    /// </summary>
    /// <returns>�ӵ�Ԥ����</returns>
    private GameObject CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("BossBullet");
        bullet.AddComponent<BossBullet>();
        
        return bullet;
    }
    
    /// <summary>
    /// ����С��Ԥ����
    /// </summary>
    /// <returns>С��Ԥ����</returns>
    private GameObject CreateMinionPrefab()
    {
        GameObject minion = new GameObject("DemonMinion");
        minion.AddComponent<DemonMinion>();
        
        // ��ӻ������
        minion.AddComponent<Rigidbody2D>();
        minion.AddComponent<BoxCollider2D>();
        SpriteRenderer minionSr = minion.AddComponent<SpriteRenderer>();
        minionSr.sprite = CreateMinionSprite();
        minionSr.color = Color.magenta;
        
        return minion;
    }
    
    /// <summary>
    /// ����Boss����
    /// </summary>
    /// <returns>Boss����</returns>
    private Sprite CreateBossSprite()
    {
        // ����һ���򵥵�Boss����
        Texture2D texture = new Texture2D(64, 96);
        Color[] colors = new Color[64 * 96];
        
        // ����һ���򵥵Ķ�ħ��״
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 96; y++)
            {
                float centerX = 32f;
                float centerY = 48f;
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                
                if (distance <= 30f)
                {
                    // ����
                    colors[y * 64 + x] = Color.red;
                }
                else if (y > 80 && distance <= 35f)
                {
                    // ͷ�� - ʹ�����ɫ
                    colors[y * 64 + x] = new Color(0.5f, 0f, 0f); // ���ɫ
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
    /// ����С�־���
    /// </summary>
    /// <returns>С�־���</returns>
    private Sprite CreateMinionSprite()
    {
        // ����һ���򵥵�С�־���
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
    /// �������Ԥ�����Boss
    /// </summary>
    /// <param name="prefab">Ԥ����</param>
    private void AssignFireballPrefab(GameObject prefab)
    {
        // ʹ�÷��佫Ԥ��������Boss
        var field = typeof(BossDemon).GetField("fireballPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(bossDemon, prefab);
        }
    }
    
    /// <summary>
    /// �����ӵ�Ԥ�����Boss
    /// </summary>
    /// <param name="prefab">Ԥ����</param>
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
    /// ����С��Ԥ�����Boss
    /// </summary>
    /// <param name="prefab">Ԥ����</param>
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
    /// ����UI����
    /// </summary>
    private void SetupUIIntegration()
    {
        // �������е�Boss UI
        BossLifeUI bossUI = FindObjectOfType<BossLifeUI>();
        if (bossUI != null)
        {
            Debug.Log("�ҵ����е�BossLifeUI - ���ɽ��Զ�����");
        }
        else
        {
            Debug.Log("δ�ҵ�BossLifeUI - BossѪ�����ܲ�����ʾ");
        }
    }
    
    /// <summary>
    /// ����Boss����
    /// </summary>
    [ContextMenu("����Boss����")]
    public void TestBossSetup()
    {
        if (bossDemon == null)
        {
            bossDemon = GetComponent<BossDemon>();
        }
        
        if (bossDemon != null)
        {
            Debug.Log($"��ħBoss״̬:");
            Debug.Log($"- ����ֵ: {bossDemon.HealthPercentage * 100}%");
            Debug.Log($"- �ڶ��׶�: {bossDemon.IsInPhase2}");
            Debug.Log($"- ������: {bossDemon.IsDead}");
        }
        else
        {
            Debug.LogError("δ�ҵ�BossDemon�����");
        }
    }
}