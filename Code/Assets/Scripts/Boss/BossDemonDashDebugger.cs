using UnityEngine;
using System.Collections;

/// <summary>
/// BossDemon Dash�������Թ���
/// ������Ϻ��޸�Dash��صĶ�������
/// </summary>
[RequireComponent(typeof(BossDemon))]
[RequireComponent(typeof(Animator))]
public class BossDemonDashDebugger : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool enableVisualDebug = true;
    [SerializeField] private bool autoTestOnStart = false;
    
    [Header("���Բ���")]
    [SerializeField] private float testDashDuration = 0.5f;
    [SerializeField] private float testShockwaveRadius = 8f;
    
    private BossDemon bossDemon;
    private Animator animator;
    private BossDemonAnimationEvents animationEvents;
    
    // ����״̬
    private bool isTestingDash = false;
    private float testStartTime;
    
    void Start()
    {
        InitializeComponents();
        
        if (autoTestOnStart)
        {
            StartCoroutine(DelayedAutoTest());
        }
    }
    
    /// <summary>
    /// ��ʼ���������
    /// </summary>
    private void InitializeComponents()
    {
        bossDemon = GetComponent<BossDemon>();
        animator = GetComponent<Animator>();
        animationEvents = GetComponent<BossDemonAnimationEvents>();
        
        if (bossDemon == null)
        {
            Debug.LogError("BossDemonDashDebugger: δ�ҵ�BossDemon���");
        }
        
        if (animator == null)
        {
            Debug.LogError("BossDemonDashDebugger: δ�ҵ�Animator���");
        }
        
        if (animationEvents == null)
        {
            Debug.LogWarning("BossDemonDashDebugger: δ�ҵ�BossDemonAnimationEvents���");
        }
    }
    
    /// <summary>
    /// �ӳ��Զ�����
    /// </summary>
    private IEnumerator DelayedAutoTest()
    {
        yield return new WaitForSeconds(2f); // �ȴ���ʼ�����
        TestDashAnimationFlow();
    }
    
    /// <summary>
    /// ����Dash��������
    /// </summary>
    [ContextMenu("����Dash��������")]
    public void TestDashAnimationFlow()
    {
        if (isTestingDash)
        {
            Debug.LogWarning("Dash�������ڽ����У���ȴ���ǰ�������");
            return;
        }
        
        StartCoroutine(ExecuteDashTest());
    }
    
    /// <summary>
    /// ִ��Dash����
    /// </summary>
    private IEnumerator ExecuteDashTest()
    {
        isTestingDash = true;
        testStartTime = Time.time;
        
        LogDebug("=== ��ʼDash�������̲��� ===");
        
        // ����1������ʼ״̬
        yield return StartCoroutine(CheckInitialState());
        
        // ����2������Dash״̬
        yield return StartCoroutine(TestDashStateTransition());
        
        // ����3��ģ��Dash��������
        yield return StartCoroutine(SimulateDashPlayback());
        
        // ����4������ShakeWave����
        yield return StartCoroutine(TestShakeWaveTrigger());
        
        // ����5���������״̬
        yield return StartCoroutine(CheckFinalState());
        
        LogDebug("=== Dash�������̲������ ===");
        isTestingDash = false;
    }
    
    /// <summary>
    /// ����ʼ״̬
    /// </summary>
    private IEnumerator CheckInitialState()
    {
        LogDebug("����1: ����ʼ״̬");
        
        if (animator == null)
        {
            LogError("Animator���ȱʧ");
            yield break;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            LogError("Animator Controllerδ����");
            yield break;
        }
        
        // ����������
        bool hasStateParam = false;
        bool hasShakeWaveTrigger = false;
        
        foreach (var param in animator.parameters)
        {
            if (param.name == "State" && param.type == AnimatorControllerParameterType.Int)
            {
                hasStateParam = true;
                LogDebug($"? State���������ã���ǰֵ: {animator.GetInteger("State")}");
            }
            else if (param.name == "ShakeWave" && param.type == AnimatorControllerParameterType.Trigger)
            {
                hasShakeWaveTrigger = true;
                LogDebug("? ShakeWave������������");
            }
        }
        
        if (!hasStateParam)
        {
            LogError("? ȱ��State��������");
        }
        
        if (!hasShakeWaveTrigger)
        {
            LogError("? ȱ��ShakeWave����������");
        }
        
        // ��ʾ��ǰ����״̬
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        LogDebug($"��ǰ����״̬: {currentState.shortNameHash} (normalizedTime: {currentState.normalizedTime:F3})");
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// ����Dash״̬ת��
    /// </summary>
    private IEnumerator TestDashStateTransition()
    {
        LogDebug("����2: ����Dash״̬ת��");
        
        // ����StateΪ2 (Dash)
        LogDebug("����State = 2 (Dash)");
        animator.SetInteger("State", 2);
        
        // �ȴ�״̬ת��
        yield return new WaitForSeconds(0.2f);
        
        // ���ת�����
        int currentStateValue = animator.GetInteger("State");
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        LogDebug($"State�������ý��: {currentStateValue}");
        LogDebug($"��ǰ����״̬: {stateInfo.shortNameHash}");
        LogDebug($"��������: {stateInfo.normalizedTime:F3}");
        
        // ����Ƿ�ɹ�ת����Dash����
        bool isDashAnimPlaying = CheckIfDashAnimationPlaying(stateInfo);
        
        if (isDashAnimPlaying)
        {
            LogDebug("? �ɹ�ת����Dash����");
        }
        else
        {
            LogWarning("? ����δ�ɹ�ת����Dash����������Animator Controller����");
        }
        
        yield return new WaitForSeconds(0.3f);
    }
    
    /// <summary>
    /// ģ��Dash��������
    /// </summary>
    private IEnumerator SimulateDashPlayback()
    {
        LogDebug("����3: ģ��Dash��������");
        
        float dashStartTime = Time.time;
        float maxDashTime = testDashDuration + 1f; // ���һЩ����ʱ��
        
        LogDebug($"�ȴ�Dash�������� (���ȴ�ʱ��: {maxDashTime}��)");
        
        while (Time.time - dashStartTime < maxDashTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            if (enableDebugLogs && Time.frameCount % 30 == 0) // ÿ30֡���һ��
            {
                LogDebug($"Dash��������: {stateInfo.normalizedTime:F3}");
            }
            
            // ��鶯���Ƿ�ӽ����
            if (stateInfo.normalizedTime >= 0.8f && CheckIfDashAnimationPlaying(stateInfo))
            {
                LogDebug("? Dash����������ɣ�׼������ShakeWave");
                break;
            }
            
            yield return null;
        }
        
        if (Time.time - dashStartTime >= maxDashTime)
        {
            LogWarning("? Dash�������ų�ʱ�����ܴ�����������");
        }
    }
    
    /// <summary>
    /// ����ShakeWave����
    /// </summary>
    private IEnumerator TestShakeWaveTrigger()
    {
        LogDebug("����4: ����ShakeWave����");
        
        // ����ShakeWave
        LogDebug("����ShakeWave");
        animator.SetTrigger("ShakeWave");
        
        // �ȴ�����ʱ���ô�������Ч
        yield return new WaitForSeconds(0.1f);
        
        // ���ShakeWave��������
        float shakeWaveStartTime = Time.time;
        float maxShakeWaveTime = 3f;
        bool shakeWaveDetected = false;
        
        LogDebug("���ShakeWave��������...");
        
        while (Time.time - shakeWaveStartTime < maxShakeWaveTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // ����Ƿ����ShakeWave����
            if (CheckIfShakeWaveAnimationPlaying(stateInfo))
            {
                if (!shakeWaveDetected)
                {
                    LogDebug("? ShakeWave������ʼ����");
                    shakeWaveDetected = true;
                }
                
                if (enableDebugLogs && Time.frameCount % 30 == 0)
                {
                    LogDebug($"ShakeWave��������: {stateInfo.normalizedTime:F3}");
                }
                
                // ��鶯���Ƿ����
                if (stateInfo.normalizedTime >= 0.95f)
                {
                    LogDebug("? ShakeWave�����������");
                    break;
                }
            }
            
            yield return null;
        }
        
        if (!shakeWaveDetected)
        {
            LogError("? ShakeWave����δ��⵽������Trigger����������");
        }
        else if (Time.time - shakeWaveStartTime >= maxShakeWaveTime)
        {
            LogWarning("? ShakeWave�������ų�ʱ");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// �������״̬
    /// </summary>
    private IEnumerator CheckFinalState()
    {
        LogDebug("����5: �������״̬");
        
        // ����״̬��Idle
        LogDebug("����State��0 (Idle)");
        animator.SetInteger("State", 0);
        
        yield return new WaitForSeconds(0.3f);
        
        // �������״̬
        AnimatorStateInfo finalState = animator.GetCurrentAnimatorStateInfo(0);
        int finalStateValue = animator.GetInteger("State");
        
        LogDebug($"����State����ֵ: {finalStateValue}");
        LogDebug($"���ն���״̬: {finalState.shortNameHash}");
        
        if (finalStateValue == 0)
        {
            LogDebug("? �ɹ����õ�Idle״̬");
        }
        else
        {
            LogWarning($"? State����δ��ȷ���ã���ǰֵ: {finalStateValue}");
        }
    }
    
    /// <summary>
    /// ����Ƿ����ڲ���Dash����
    /// </summary>
    private bool CheckIfDashAnimationPlaying(AnimatorStateInfo stateInfo)
    {
        // ���ܵ�Dash��������
        string[] dashAnimNames = { "Boss_Dash", "Dash", "DashAttack", "BossDemon_Dash" };
        
        foreach (string animName in dashAnimNames)
        {
            if (stateInfo.IsName(animName))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// ����Ƿ����ڲ���ShakeWave����
    /// </summary>
    private bool CheckIfShakeWaveAnimationPlaying(AnimatorStateInfo stateInfo)
    {
        // ���ܵ�ShakeWave��������
        string[] shakeWaveAnimNames = { "Boss_Shockwave", "ShakeWave", "Shockwave", "BossDemon_Shockwave" };
        
        foreach (string animName in shakeWaveAnimNames)
        {
            if (stateInfo.IsName(animName))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// ��϶���������
    /// </summary>
    [ContextMenu("��϶���������")]
    public void DiagnoseAnimatorConfiguration()
    {
        LogDebug("=== ������������� ===");
        
        if (animator == null)
        {
            LogError("Animator���ȱʧ");
            return;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            LogError("Animator Controllerδ����");
            return;
        }
        
        LogDebug($"Animator Controller: {animator.runtimeAnimatorController.name}");
        
        // ������
        LogDebug($"��������: {animator.layerCount}");
        
        // ������
        LogDebug("=== �������� ===");
        foreach (var param in animator.parameters)
        {
            LogDebug($"����: {param.name} (����: {param.type})");
        }
        
        // ��鵱ǰ״̬
        for (int i = 0; i < animator.layerCount; i++)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
            LogDebug($"�� {i} ��ǰ״̬: {stateInfo.shortNameHash} (����: {stateInfo.normalizedTime:F3})");
        }
        
        LogDebug("=== ������ ===");
    }
    
    /// <summary>
    /// ǿ�Ʋ���ShakeWave
    /// </summary>
    [ContextMenu("ǿ�Ʋ���ShakeWave")]
    public void ForceTestShakeWave()
    {
        StartCoroutine(ForceShakeWaveTest());
    }
    
    /// <summary>
    /// ǿ��ShakeWave����Э��
    /// </summary>
    private IEnumerator ForceShakeWaveTest()
    {
        LogDebug("=== ǿ��ShakeWave���� ===");
        
        LogDebug("����ShakeWave");
        animator.SetTrigger("ShakeWave");
        
        yield return StartCoroutine(MonitorShakeWaveExecution());
        
        LogDebug("=== ShakeWave������� ===");
    }
    
    /// <summary>
    /// ���ShakeWaveִ��
    /// </summary>
    private IEnumerator MonitorShakeWaveExecution()
    {
        float startTime = Time.time;
        float maxTime = 5f;
        bool detected = false;
        
        while (Time.time - startTime < maxTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            if (CheckIfShakeWaveAnimationPlaying(stateInfo))
            {
                if (!detected)
                {
                    LogDebug("? ShakeWave������⵽");
                    detected = true;
                }
                
                LogDebug($"ShakeWave����: {stateInfo.normalizedTime:F3}");
                
                if (stateInfo.normalizedTime >= 0.95f)
                {
                    LogDebug("? ShakeWave�������");
                    break;
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        if (!detected)
        {
            LogError("? ShakeWave����δ��⵽������Trigger����");
        }
    }
    
    /// <summary>
    /// ��־����������ؿ��ƣ�
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[DashDebugger] {message}");
        }
    }
    
    /// <summary>
    /// ������־���
    /// </summary>
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[DashDebugger] {message}");
    }
    
    /// <summary>
    /// ������־���
    /// </summary>
    private void LogError(string message)
    {
        Debug.LogError($"[DashDebugger] {message}");
    }
    
    /// <summary>
    /// ���ӻ�������Ϣ
    /// </summary>
    void OnDrawGizmos()
    {
        if (!enableVisualDebug) return;
        
        // ���Ʋ��Գ������Χ
        if (isTestingDash)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, testShockwaveRadius);
            
            // ���Ʋ���״ָ̬ʾ
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
        }
    }
}