using UnityEngine;

/// <summary>
/// BossDemon��BossLife������֤�ű�
/// ���ڲ��Ժ���֤����ϵͳ����ȷ��
/// </summary>
public class BossDemonBossLifeIntegrationValidator : MonoBehaviour
{
    [Header("��֤����")]
    public BossDemon targetBossDemon;
    public bool enableAutoValidation = true;
    public bool showDebugInfo = true;
    
    [Header("���Բ���")]
    public int testDamageAmount = 50;
    public float validationInterval = 2f;
    
    private BossLife bossLife;
    private bool integrationValid = false;
    private float lastValidationTime = 0f;
    
    void Start()
    {
        if (targetBossDemon == null)
        {
            targetBossDemon = FindObjectOfType<BossDemon>();
        }
        
        if (targetBossDemon == null)
        {
            Debug.LogError("BossDemonBossLifeIntegrationValidator: δ�ҵ�BossDemonĿ�꣡");
            return;
        }
        
        bossLife = targetBossDemon.GetComponent<BossLife>();
        
        ValidateIntegration();
        
        if (enableAutoValidation)
        {
            InvokeRepeating(nameof(ValidateIntegration), validationInterval, validationInterval);
        }
        
        Debug.Log("BossDemon��BossLife������֤ϵͳ������");
        Debug.Log("����˵����");
        Debug.Log("V - �ֶ���֤����");
        Debug.Log("D - �����˺�����");
        Debug.Log("H - ����Ѫ��ͬ��");
        Debug.Log("A - �������˶���");
        Debug.Log("S - ���Գ���״̬");
    }
    
    void Update()
    {
        if (targetBossDemon == null) return;
        
        // V - �ֶ���֤����
        if (Input.GetKeyDown(KeyCode.V))
        {
            ValidateIntegration();
        }
        
        // D - �����˺�����
        if (Input.GetKeyDown(KeyCode.D))
        {
            TestDamageProcessing();
        }
        
        // H - ����Ѫ��ͬ��
        if (Input.GetKeyDown(KeyCode.H))
        {
            TestHealthSync();
        }
        
        // A - �������˶���
        if (Input.GetKeyDown(KeyCode.A))
        {
            TestHurtAnimation();
        }
        
        // S - ���Գ���״̬
        if (Input.GetKeyDown(KeyCode.S))
        {
            TestSpawnState();
        }
    }
    
    /// <summary>
    /// ��֤����������
    /// </summary>
    [ContextMenu("��֤����")]
    public void ValidateIntegration()
    {
        if (targetBossDemon == null)
        {
            Debug.LogError("��֤ʧ�ܣ�δ�ҵ�Ŀ��BossDemon");
            integrationValid = false;
            return;
        }
        
        Debug.Log("=== ��ʼ��֤BossDemon��BossLife���� ===");
        
        bool allChecksPass = true;
        
        // ���1��BossLife�������
        if (bossLife == null)
        {
            bossLife = targetBossDemon.GetComponent<BossLife>();
        }
        
        if (bossLife != null)
        {
            Debug.Log("? BossLife�������");
        }
        else
        {
            Debug.LogError("? BossLife���ȱʧ");
            allChecksPass = false;
        }
        
        // ���2�����Է�����ȷ
        try
        {
            int currentHealth = targetBossDemon.CurrentHealth;
            int maxHealth = targetBossDemon.MaxHealth;
            float healthPercentage = targetBossDemon.HealthPercentage;
            bool isDead = targetBossDemon.IsDead;
            bool isSpawning = targetBossDemon.IsSpawning;
            
            Debug.Log($"? ���Է������� - Ѫ��: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%), ����: {isDead}, ����: {isSpawning}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"? ���Է���ʧ��: {e.Message}");
            allChecksPass = false;
        }
        
        // ���3���¼�ϵͳ
        bool hasHealthChangedEvent = HasEvent(targetBossDemon, "OnHealthChanged");
        bool hasDeathEvent = HasEvent(targetBossDemon, "OnDeath");
        bool hasSpawnCompleteEvent = HasEvent(targetBossDemon, "OnSpawnComplete");
        
        if (hasHealthChangedEvent && hasDeathEvent && hasSpawnCompleteEvent)
        {
            Debug.Log("? �¼�ϵͳ����");
        }
        else
        {
            Debug.LogWarning("? �¼�ϵͳ���ܲ�����");
        }
        
        // ���4��Ѫ��ֵһ����
        if (bossLife != null)
        {
            bool healthConsistent = (targetBossDemon.CurrentHealth == bossLife.CurrentHealth) &&
                                   (targetBossDemon.MaxHealth == bossLife.MaxHealth);
            
            if (healthConsistent)
            {
                Debug.Log("? Ѫ��ֵһ������ȷ");
            }
            else
            {
                Debug.LogError($"? Ѫ��ֵ��һ�� - BossDemon: {targetBossDemon.CurrentHealth}/{targetBossDemon.MaxHealth}, BossLife: {bossLife.CurrentHealth}/{bossLife.MaxHealth}");
                allChecksPass = false;
            }
        }
        
        // ���5������״̬һ����
        if (bossLife != null)
        {
            bool deathConsistent = (targetBossDemon.IsDead == bossLife.IsDead);
            
            if (deathConsistent)
            {
                Debug.Log("? ����״̬һ������ȷ");
            }
            else
            {
                Debug.LogError($"? ����״̬��һ�� - BossDemon: {targetBossDemon.IsDead}, BossLife: {bossLife.IsDead}");
                allChecksPass = false;
            }
        }
        
        // ���6������״̬����
        bool spawnStateValid = !targetBossDemon.IsSpawning || targetBossDemon.IsSpawning;
        
        if (spawnStateValid)
        {
            Debug.Log("? ����״̬��������");
        }
        else
        {
            Debug.LogError("? ����״̬�����쳣");
            allChecksPass = false;
        }
        
        integrationValid = allChecksPass;
        lastValidationTime = Time.time;
        
        if (integrationValid)
        {
            Debug.Log("=== ������֤ͨ�� ===");
        }
        else
        {
            Debug.LogError("=== ������֤ʧ�� ===");
        }
    }
    
    /// <summary>
    /// �����˺�����
    /// </summary>
    private void TestDamageProcessing()
    {
        if (targetBossDemon == null || targetBossDemon.IsDead)
        {
            Debug.LogWarning("�޷������˺�����Ŀ�겻���ڻ�������");
            return;
        }
        
        Debug.Log("=== �����˺����� ===");
        
        int healthBefore = targetBossDemon.CurrentHealth;
        Debug.Log($"�˺�ǰѪ��: {healthBefore}");
        
        targetBossDemon.TakeDamage(testDamageAmount);
        
        // �ȴ�һ֡ȷ���������
        StartCoroutine(ValidateDamageAfterFrame(healthBefore));
    }
    
    /// <summary>
    /// �ȴ�һ֡����֤�˺�
    /// </summary>
    private System.Collections.IEnumerator ValidateDamageAfterFrame(int healthBefore)
    {
        yield return null;
        
        int healthAfter = targetBossDemon.CurrentHealth;
        int expectedHealth = healthBefore - testDamageAmount;
        
        Debug.Log($"�˺���Ѫ��: {healthAfter}");
        Debug.Log($"����Ѫ��: {expectedHealth}");
        
        if (healthAfter == expectedHealth || healthAfter == 0) // ����Ѫ������Ϊ��
        {
            Debug.Log("? �˺�������ȷ");
        }
        else
        {
            Debug.LogError("? �˺������쳣");
        }
    }
    
    /// <summary>
    /// ����Ѫ��ͬ��
    /// </summary>
    private void TestHealthSync()
    {
        if (targetBossDemon == null || bossLife == null)
        {
            Debug.LogWarning("�޷�����Ѫ��ͬ�������ȱʧ");
            return;
        }
        
        Debug.Log("=== ����Ѫ��ͬ�� ===");
        
        Debug.Log($"BossDemonѪ��: {targetBossDemon.CurrentHealth}/{targetBossDemon.MaxHealth}");
        Debug.Log($"BossLifeѪ��: {bossLife.CurrentHealth}/{bossLife.MaxHealth}");
        
        bool syncCorrect = (targetBossDemon.CurrentHealth == bossLife.CurrentHealth) &&
                          (targetBossDemon.MaxHealth == bossLife.MaxHealth) &&
                          (targetBossDemon.IsDead == bossLife.IsDead);
        
        if (syncCorrect)
        {
            Debug.Log("? Ѫ��ͬ����ȷ");
        }
        else
        {
            Debug.LogError("? Ѫ��ͬ���쳣");
        }
    }
    
    /// <summary>
    /// �������˶���
    /// </summary>
    private void TestHurtAnimation()
    {
        if (targetBossDemon == null || targetBossDemon.IsDead)
        {
            Debug.LogWarning("�޷��������˶�����Ŀ�겻���ڻ�������");
            return;
        }
        
        Debug.Log("=== �������˶��� ===");
        
        // ��������˺��Դ������˶���
        int smallDamage = 1;
        targetBossDemon.TakeDamage(smallDamage);
        
        Debug.Log("���˶����Ѵ�������۲�Boss�Ƿ񲥷����˶���");
    }
    
    /// <summary>
    /// ���Գ���״̬
    /// </summary>
    private void TestSpawnState()
    {
        if (targetBossDemon == null)
        {
            Debug.LogWarning("�޷����Գ���״̬��Ŀ�겻����");
            return;
        }
        
        Debug.Log("=== ���Գ���״̬ ===");
        
        bool isSpawning = targetBossDemon.IsSpawning;
        Debug.Log($"��ǰ����״̬: {(isSpawning ? "���ڳ���" : "����ɳ���")}");
        
        if (isSpawning)
        {
            Debug.Log("Boss���ڳ����У���ʱӦ�ã�");
            Debug.Log("1. �����˺�");
            Debug.Log("2. ������AI����");
            Debug.Log("3. ���ų�������");
        }
        else
        {
            Debug.Log("Boss����ɳ�����Ӧ���ܹ�����ս��");
        }
    }
    
    /// <summary>
    /// �������Ƿ���ָ�����¼�
    /// </summary>
    private bool HasEvent(object obj, string eventName)
    {
        if (obj == null) return false;
        
        var eventInfo = obj.GetType().GetEvent(eventName);
        return eventInfo != null;
    }
    
    /// <summary>
    /// ��ʾ��֤UI
    /// </summary>
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 340, 300));
        
        GUILayout.Label("=== ������֤ϵͳ ===");
        
        if (targetBossDemon != null)
        {
            GUILayout.Label($"Ŀ��: {targetBossDemon.name}");
            GUILayout.Label($"Ѫ��: {targetBossDemon.CurrentHealth}/{targetBossDemon.MaxHealth}");
            GUILayout.Label($"״̬: {(targetBossDemon.IsDead ? "����" : "���")}");
            GUILayout.Label($"����״̬: {(targetBossDemon.IsSpawning ? "���ڳ���" : "����ɳ���")}");
            GUILayout.Label($"�ڶ��׶�: {(targetBossDemon.IsInPhase2 ? "��" : "��")}");
        }
        else
        {
            GUILayout.Label("Ŀ��: δ�ҵ�");
        }
        
        GUILayout.Space(10);
        
        // ����״̬
        GUILayout.Label("=== ����״̬ ===");
        GUILayout.Label($"BossLife���: {(bossLife != null ? "?" : "?")}");
        GUILayout.Label($"������֤: {(integrationValid ? "? ͨ��" : "? ʧ��")}");
        GUILayout.Label($"�ϴ���֤: {(lastValidationTime > 0 ? $"{Time.time - lastValidationTime:F1}sǰ" : "δ��֤")}");
        
        GUILayout.Space(10);
        
        // ����˵��
        GUILayout.Label("=== ����˵�� ===");
        GUILayout.Label("V - �ֶ���֤");
        GUILayout.Label("D - �����˺�");
        GUILayout.Label("H - ����Ѫ��ͬ��");
        GUILayout.Label("A - �������˶���");
        GUILayout.Label("S - ���Գ���״̬");
        
        GUILayout.EndArea();
    }
}