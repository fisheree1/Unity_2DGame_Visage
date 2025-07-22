using UnityEngine;
using System.Collections;

/// <summary>
/// BossSlimeת��BossDemon���ܲ��Խű�
/// ������֤Bossת��ϵͳ����ȷ��
/// </summary>
public class BossTransitionTester : MonoBehaviour
{
    [Header("��������")]
    public BossSlime targetBossSlime;
    public bool enableTestControls = true;
    public bool showTestUI = true;
    
    [Header("���Բ���")]
    public int testDamageAmount = 50;
    public float testHealthPercentage = 0.8f;
    public GameObject testBossPrefab;
    
    private bool isTestingTransition = false;
    private bool transitionCompleted = false;
    private float transitionStartTime;
    
    void Start()
    {
        // �Զ�����BossSlime���û��ָ��
        if (targetBossSlime == null)
        {
            targetBossSlime = FindObjectOfType<BossSlime>();
        }
        
        if (targetBossSlime == null)
        {
            Debug.LogError("BossTransitionTester: δ�ҵ�BossSlimeĿ�꣡");
            return;
        }
        
        Debug.Log("BossSlimeת��BossDemon����ϵͳ������");
        Debug.Log("���Կ���˵����");
        Debug.Log("T - ����Bossת��");
        Debug.Log("K - ���ٻ�ɱBossSlime");
        Debug.Log("R - ���ò���");
        Debug.Log("H - ���ò���Ѫ���ٷֱ�");
        Debug.Log("P - ���ò���Ԥ����");
    }
    
    void Update()
    {
        if (!enableTestControls || targetBossSlime == null) return;
        
        HandleTestControls();
        MonitorTransition();
    }
    
    private void HandleTestControls()
    {
        // T - ����Bossת��
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestBossTransition();
        }
        
        // K - ���ٻ�ɱBossSlime
        if (Input.GetKeyDown(KeyCode.K))
        {
            QuickKillBossSlime();
        }
        
        // R - ���ò���
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTest();
        }
        
        // H - ���ò���Ѫ���ٷֱ�
        if (Input.GetKeyDown(KeyCode.H))
        {
            SetTestHealthPercentage();
        }
        
        // P - ���ò���Ԥ����
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetTestPrefab();
        }
    }
    
    /// <summary>
    /// ����Bossת��
    /// </summary>
    private void TestBossTransition()
    {
        if (targetBossSlime == null || targetBossSlime.IsDead)
        {
            Debug.LogWarning("Ŀ��BossSlime�����ڻ�������");
            return;
        }
        
        if (isTestingTransition)
        {
            Debug.LogWarning("ת���������ڽ�����");
            return;
        }
        
        Debug.Log("=== ��ʼ����Bossת�� ===");
        
        // ���ò��Բ���
        targetBossSlime.parameter.nextBossHealthPercentage = testHealthPercentage;
        targetBossSlime.parameter.enableBossTransition = true;
        
        if (testBossPrefab != null)
        {
            targetBossSlime.parameter.nextBossPrefab = testBossPrefab;
            Debug.Log($"���ò���Ԥ����: {testBossPrefab.name}");
        }
        
        // ��ʼ���ת��
        isTestingTransition = true;
        transitionStartTime = Time.time;
        transitionCompleted = false;
        
        // ��ɱBossSlime
        int currentHealth = Mathf.RoundToInt(targetBossSlime.parameter.currentHealth);
        if (currentHealth > 0)
        {
            Debug.Log($"��BossSlime��������˺�: {currentHealth}");
            targetBossSlime.TakeDamage(currentHealth);
        }
        
        Debug.Log("BossSlime�ѱ���ɱ���ȴ�ת��...");
    }
    
    /// <summary>
    /// ���ٻ�ɱBossSlime
    /// </summary>
    private void QuickKillBossSlime()
    {
        if (targetBossSlime == null || targetBossSlime.IsDead)
        {
            Debug.LogWarning("Ŀ��BossSlime�����ڻ�������");
            return;
        }
        
        Debug.Log("���ٻ�ɱBossSlime");
        int currentHealth = Mathf.RoundToInt(targetBossSlime.parameter.currentHealth);
        targetBossSlime.TakeDamage(currentHealth);
    }
    
    /// <summary>
    /// ���ò���
    /// </summary>
    private void ResetTest()
    {
        Debug.Log("����ת������");
        
        isTestingTransition = false;
        transitionCompleted = false;
        
        // �����µ�BossSlime
        targetBossSlime = FindObjectOfType<BossSlime>();
        
        if (targetBossSlime == null)
        {
            Debug.LogWarning("���ú�δ�ҵ�BossSlime");
        }
        else
        {
            Debug.Log("�ҵ��µ�BossSlimeĿ��");
        }
    }
    
    /// <summary>
    /// ���ò���Ѫ���ٷֱ�
    /// </summary>
    private void SetTestHealthPercentage()
    {
        // ѭ�����ò�ͬ��Ѫ���ٷֱ�
        float[] healthPercentages = { 0.5f, 0.7f, 0.8f, 1.0f };
        
        for (int i = 0; i < healthPercentages.Length; i++)
        {
            if (Mathf.Approximately(testHealthPercentage, healthPercentages[i]))
            {
                testHealthPercentage = healthPercentages[(i + 1) % healthPercentages.Length];
                break;
            }
        }
        
        Debug.Log($"���ò���Ѫ���ٷֱ�: {testHealthPercentage * 100:F0}%");
    }
    
    /// <summary>
    /// ���ò���Ԥ����
    /// </summary>
    private void SetTestPrefab()
    {
        if (testBossPrefab == null)
        {
            Debug.Log("��ǰ�޲���Ԥ���壬��ʹ�ö�̬����ģʽ");
        }
        else
        {
            Debug.Log($"��ǰ����Ԥ����: {testBossPrefab.name}");
        }
        
        Debug.Log("����Inspector������Test Boss Prefab");
    }
    
    /// <summary>
    /// ���ת������
    /// </summary>
    private void MonitorTransition()
    {
        if (!isTestingTransition) return;
        
        float elapsedTime = Time.time - transitionStartTime;
        
        // ����Ƿ��������µ�Boss
        if (!transitionCompleted)
        {
            BossDemon newBossDemon = FindObjectOfType<BossDemon>();
            if (newBossDemon != null)
            {
                transitionCompleted = true;
                Debug.Log($"=== ת�����Գɹ� ===");
                Debug.Log($"ת����ʱ: {elapsedTime:F2}��");
                Debug.Log($"��Boss����: {newBossDemon.GetType().Name}");
                Debug.Log($"��BossѪ��: {newBossDemon.CurrentHealth}/{newBossDemon.MaxHealth}");
                Debug.Log($"Ѫ���ٷֱ�: {newBossDemon.HealthPercentage * 100:F1}%");
                Debug.Log($"λ��: {newBossDemon.transform.position}");
                
                // ��֤Ѫ������
                float expectedPercentage = testHealthPercentage;
                float actualPercentage = newBossDemon.HealthPercentage;
                
                if (Mathf.Abs(expectedPercentage - actualPercentage) < 0.05f)
                {
                    Debug.Log("? Ѫ��������ȷ");
                }
                else
                {
                    Debug.LogWarning($"? Ѫ�����ÿ������� - ����: {expectedPercentage * 100:F1}%, ʵ��: {actualPercentage * 100:F1}%");
                }
            }
        }
        
        // ��ʱ���
        if (elapsedTime > 10f && !transitionCompleted)
        {
            Debug.LogError("=== ת�����Գ�ʱ ===");
            Debug.LogError("���ܵ�ԭ��");
            Debug.LogError("1. enableBossTransition������Ϊfalse");
            Debug.LogError("2. Ԥ������������");
            Debug.LogError("3. �������г����쳣");
            
            isTestingTransition = false;
        }
    }
    
    /// <summary>
    /// ��֤ת������
    /// </summary>
    [ContextMenu("��֤ת������")]
    public void ValidateTransitionConfiguration()
    {
        if (targetBossSlime == null)
        {
            Debug.LogError("δ�ҵ�Ŀ��BossSlime");
            return;
        }
        
        Debug.Log("=== ��֤ת������ ===");
        
        var param = targetBossSlime.parameter;
        
        // ����������
        Debug.Log($"����Bossת��: {param.enableBossTransition}");
        Debug.Log($"Ѫ���ٷֱ�: {param.nextBossHealthPercentage * 100:F1}%");
        Debug.Log($"Ԥ����: {(param.nextBossPrefab != null ? param.nextBossPrefab.name : "δ���ã�����̬������")}");
        
        // ���Ԥ����
        if (param.nextBossPrefab != null)
        {
            BossDemon bossDemonComponent = param.nextBossPrefab.GetComponent<BossDemon>();
            BossLife bossLifeComponent = param.nextBossPrefab.GetComponent<BossLife>();
            
            if (bossDemonComponent != null)
            {
                Debug.Log("? Ԥ�������BossDemon���");
            }
            else if (bossLifeComponent != null)
            {
                Debug.Log("? Ԥ�������BossLife���");
            }
            else
            {
                Debug.LogWarning("? Ԥ����ȱ��BossDemon��BossLife���");
            }
        }
        
        // ����Ҫϵͳ
        if (CamaraShakeManager.Instance != null)
        {
            Debug.Log("? CamaraShakeManagerϵͳ����");
        }
        else
        {
            Debug.LogWarning("? δ�ҵ�CamaraShakeManager");
        }
        
        Debug.Log("=== ������֤��� ===");
    }
    
    /// <summary>
    /// ѹ������
    /// </summary>
    [ContextMenu("ѹ������")]
    public void StressTest()
    {
        StartCoroutine(StressTestCoroutine());
    }
    
    private IEnumerator StressTestCoroutine()
    {
        Debug.Log("��ʼѹ������ - ����ת������");
        
        for (int i = 0; i < 5; i++)
        {
            Debug.Log($"ѹ�����Ե� {i + 1} ��");
            
            // ���ҵ�ǰBoss
            BossSlime currentSlime = FindObjectOfType<BossSlime>();
            if (currentSlime != null)
            {
                int currentHealth = Mathf.RoundToInt(currentSlime.parameter.currentHealth);
                currentSlime.TakeDamage(currentHealth);
            }
            
            // �ȴ�ת�����
            yield return new WaitForSeconds(5f);
            
            // ���������ɵ�Boss
            BossDemon newDemon = FindObjectOfType<BossDemon>();
            if (newDemon != null)
            {
                Debug.Log($"�� {i + 1} ��ת���ɹ�");
                
                // ����׼����һ��
                Destroy(newDemon.gameObject);
                yield return new WaitForSeconds(1f);
                
                // ���´���BossSlime������һ�ֲ���
                // ������Ҫ����ʵ���������
            }
            else
            {
                Debug.LogError($"�� {i + 1} ��ת��ʧ��");
                break;
            }
        }
        
        Debug.Log("ѹ���������");
    }
    
    /// <summary>
    /// ��ʾ����UI
    /// </summary>
    void OnGUI()
    {
        if (!showTestUI) return;
        
        GUILayout.BeginArea(new Rect(10, 200, 350, 400));
        
        GUILayout.Label("=== Bossת������ϵͳ ===");
        
        // ��ʾĿ����Ϣ
        if (targetBossSlime != null)
        {
            GUILayout.Label($"Ŀ��: {targetBossSlime.name}");
            GUILayout.Label($"Ѫ��: {targetBossSlime.parameter.currentHealth:F0}/{targetBossSlime.parameter.maxHealth:F0}");
            GUILayout.Label($"״̬: {(targetBossSlime.IsDead ? "����" : "���")}");
        }
        else
        {
            GUILayout.Label("Ŀ��: δ�ҵ�BossSlime");
        }
        
        GUILayout.Space(10);
        
        // ���Բ���
        GUILayout.Label("=== ���Բ��� ===");
        GUILayout.Label($"Ѫ���ٷֱ�: {testHealthPercentage * 100:F0}%");
        GUILayout.Label($"Ԥ����: {(testBossPrefab != null ? testBossPrefab.name : "��̬����")}");
        
        GUILayout.Space(10);
        
        // ����״̬
        if (isTestingTransition)
        {
            GUILayout.Label("?? ת�����Խ�����...");
            GUILayout.Label($"��ʱ: {Time.time - transitionStartTime:F1}��");
            
            if (transitionCompleted)
            {
                GUILayout.Label("? ת���ɹ���");
            }
        }
        else
        {
            GUILayout.Label("������");
        }
        
        GUILayout.Space(10);
        
        // ����˵��
        GUILayout.Label("=== ����˵�� ===");
        GUILayout.Label("T - ����ת��");
        GUILayout.Label("K - ���ٻ�ɱ");
        GUILayout.Label("R - ���ò���");
        GUILayout.Label("H - �л�Ѫ���ٷֱ�");
        GUILayout.Label("P - ����Ԥ������Ϣ");
        
        GUILayout.EndArea();
    }
}