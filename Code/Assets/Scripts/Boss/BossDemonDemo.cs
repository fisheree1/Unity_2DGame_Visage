using UnityEngine;

/// <summary>
/// �ع����Boss��ʾϵͳ
/// չʾ�µ�counterAttack״̬�ͽ׶��Թ����仯
/// </summary>
public class BossDemonDemo : MonoBehaviour
{
    [Header("��ʾ����")]
    public bool enableDemoControls = true;
    public bool showDebugInfo = true;
    
    private BossDemon bossDemon;
    private bool isSetup = false;
    
    void Start()
    {
        SetupDemo();
    }
    
    void Update()
    {
        if (!isSetup) return;
        
        HandleDemoControls();
    }
    
    /// <summary>
    /// ������ʾ
    /// </summary>
    private void SetupDemo()
    {
        bossDemon = GetComponent<BossDemon>();
        if (bossDemon == null)
        {
            Debug.LogError("BossDemonDemo: δ�ҵ�BossDemon���!");
            return;
        }
        
        isSetup = true;
        Debug.Log("�ع����Boss��ʾϵͳ������!");
        Debug.Log("����˵��:");
        Debug.Log("F1 - ��ʾBoss״̬");
        Debug.Log("F2 - ��Boss����˺� (50��)");
        Debug.Log("F3 - ��Boss����˺� (100��)");
        Debug.Log("F4 - ��������״̬");
        Debug.Log("F5 - ǿ�ƽ���ڶ��׶�");
        Debug.Log("F6 - ģ��Ӣ�ۻ���");
        Debug.Log("F7 - ģ��Ӣ����Ծ");
    }
    
    /// <summary>
    /// ������ʾ����
    /// </summary>
    private void HandleDemoControls()
    {
        if (!enableDemoControls) return;
        
        // F1 - ��ʾBoss״̬
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowBossStatus();
        }
        
        // F2 - ���50���˺�
        if (Input.GetKeyDown(KeyCode.F2))
        {
            DamageBoss(50);
        }
        
        // F3 - ���100���˺�
        if (Input.GetKeyDown(KeyCode.F3))
        {
            DamageBoss(100);
        }
        
        // F4 - ��������״̬
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TriggerCounterAttack();
        }
        
        // F5 - ǿ�ƽ���ڶ��׶�
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ForceEnterPhase2();
        }
        
        // F6 - ģ��Ӣ�ۻ���
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SimulateHeroSlide();
        }
        
        // F7 - ģ��Ӣ����Ծ
        if (Input.GetKeyDown(KeyCode.F7))
        {
            SimulateHeroJump();
        }
    }
    
    /// <summary>
    /// ��ʾBoss״̬
    /// </summary>
    private void ShowBossStatus()
    {
        if (bossDemon == null) return;
        
        Debug.Log("=== Boss״̬��Ϣ ===");
        Debug.Log($"��ǰѪ��: {bossDemon.CurrentHealth}/{bossDemon.MaxHealth} ({bossDemon.HealthPercentage * 100:F1}%)");
        Debug.Log($"��ǰ�׶�: {(bossDemon.IsInPhase2 ? "�ڶ��׶�" : "��һ�׶�")}");
        Debug.Log($"�Ƿ�����: {bossDemon.IsDead}");
        
        // ��ʾ�׶�����
        if (bossDemon.IsInPhase2)
        {
            Debug.Log("�ڶ��׶�����:");
            Debug.Log("- ��ս������Ϊ������Ϣ");
            Debug.Log("- ����Ӣ����Ϊ���");
            Debug.Log("- ����������ǿ");
        }
        else
        {
            Debug.Log("��һ�׶�����:");
            Debug.Log("- ��ͨ��ս����");
            Debug.Log("- Dash + ���������");
        }
        
        Debug.Log("���׶ι�������:");
        Debug.Log("- ����״̬���ܵ������˺���");
        Debug.Log("- С���ٻ���ÿ1/3Ѫ����");
    }
    
    /// <summary>
    /// ��Boss����˺�
    /// </summary>
    /// <param name="damage">�˺�ֵ</param>
    private void DamageBoss(int damage)
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        
        Debug.Log($"��Boss���{damage}���˺�");
        bossDemon.TakeDamage(damage);
        
        // ����Ƿ񴥷��׶�ת��
        if (!bossDemon.IsInPhase2 && bossDemon.HealthPercentage <= 0.5f)
        {
            Debug.Log("Boss����ڶ��׶Σ�����ģʽ�����仯��");
        }
    }
    
    /// <summary>
    /// ��������״̬
    /// </summary>
    private void TriggerCounterAttack()
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        
        Debug.Log("ģ������������������״̬");
        
        // �������С���˺��Դ�������
        for (int i = 0; i < 3; i++)
        {
            bossDemon.TakeDamage(5);
        }
        
        Debug.Log("����״̬������Boss�����dash���ͷ����ε�Ļ��");
    }
    
    /// <summary>
    /// ǿ�ƽ���ڶ��׶�
    /// </summary>
    private void ForceEnterPhase2()
    {
        if (bossDemon == null || bossDemon.IsDead || bossDemon.IsInPhase2) return;
        
        Debug.Log("ǿ�ƽ���ڶ��׶�");
        
        // ��Ѫ�����ٵ�50%����
        int targetHealth = Mathf.RoundToInt(bossDemon.MaxHealth * 0.45f);
        int damageNeeded = bossDemon.CurrentHealth - targetHealth;
        
        if (damageNeeded > 0)
        {
            bossDemon.TakeDamage(damageNeeded);
        }
        
        Debug.Log("�ڶ��׶��Ѽ��");
        Debug.Log("- ��ս������Ϊ������Ϣ");
        Debug.Log("- Ӣ����Ϊ����Ѽ���");
        Debug.Log("- ����������ǿ");
    }
    
    /// <summary>
    /// ģ��Ӣ�ۻ���
    /// </summary>
    private void SimulateHeroSlide()
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        
        if (!bossDemon.IsInPhase2)
        {
            Debug.Log("Ӣ�ۻ���ģ�� - ��Boss���ڵڶ��׶Σ����ᷴӦ");
            return;
        }
        
        Debug.Log("ģ��Ӣ�ۻ�����Ϊ");
        
        // ͨ��HeroActionTrackerģ�⻬��
        var heroTracker = bossDemon.GetComponentInChildren<HeroActionTracker>();
        if (heroTracker != null)
        {
            // ������Ҫ����HeroActionTracker��ģ�ⷽ��
            Debug.Log("Boss�ڵڶ��׶μ�⵽���������ܻ�ִ��dash+�������Ӧ");
        }
    }
    
    /// <summary>
    /// ģ��Ӣ����Ծ
    /// </summary>
    private void SimulateHeroJump()
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        
        if (!bossDemon.IsInPhase2)
        {
            Debug.Log("Ӣ����Ծģ�� - ��Boss���ڵڶ��׶Σ����ᷴӦ");
            return;
        }
        
        Debug.Log("ģ��Ӣ����Ծ��Ϊ");
        
        // ͨ��HeroActionTrackerģ����Ծ
        var heroTracker = bossDemon.GetComponentInChildren<HeroActionTracker>();
        if (heroTracker != null)
        {
            // ������Ҫ����HeroActionTracker��ģ�ⷽ��
            Debug.Log("Boss�ڵڶ��׶μ�⵽��Ծ�����ܻ�ִ�����ε�Ļ��Ӧ");
        }
    }
    
    /// <summary>
    /// ��ʾGUI��Ϣ
    /// </summary>
    void OnGUI()
    {
        if (!showDebugInfo || bossDemon == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        
        GUILayout.Label("=== �ع���Ķ�ħBoss��ʾ ===");
        
        // ������Ϣ
        GUILayout.Label($"Ѫ��: {bossDemon.CurrentHealth}/{bossDemon.MaxHealth} ({bossDemon.HealthPercentage * 100:F1}%)");
        GUILayout.Label($"�׶�: {(bossDemon.IsInPhase2 ? "�ڶ��׶�" : "��һ�׶�")}");
        GUILayout.Label($"״̬: {(bossDemon.IsDead ? "����" : "���")}");
        
        GUILayout.Space(10);
        
        // �׶�����
        if (bossDemon.IsInPhase2)
        {
            GUILayout.Label("�ڶ��׶�����:");
            GUILayout.Label("? ��ս���� �� ������Ϣ");
            GUILayout.Label("? Ӣ����Ϊ��⼤��");
            GUILayout.Label("? ����������ǿ");
        }
        else
        {
            GUILayout.Label("��һ�׶�����:");
            GUILayout.Label("? ��ͨ��ս����");
            GUILayout.Label("? Dash + ���������");
        }
        
        GUILayout.Space(10);
        
        // ����˵��
        GUILayout.Label("����˵��:");
        GUILayout.Label("F1 - ��ʾ״̬ | F2 - ���50�˺� | F3 - ���100�˺�");
        GUILayout.Label("F4 - �������� | F5 - ����ڶ��׶�");
        GUILayout.Label("F6 - ģ�⻬�� | F7 - ģ����Ծ");
        
        GUILayout.EndArea();
    }
}