using UnityEngine;
using System.Collections;

/// <summary>
/// BossDemon Dash动画调试工具
/// 用于诊断和修复Dash相关的动画问题
/// </summary>
[RequireComponent(typeof(BossDemon))]
[RequireComponent(typeof(Animator))]
public class BossDemonDashDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool enableVisualDebug = true;
    [SerializeField] private bool autoTestOnStart = false;
    
    [Header("测试参数")]
    [SerializeField] private float testDashDuration = 0.5f;
    [SerializeField] private float testShockwaveRadius = 8f;
    
    private BossDemon bossDemon;
    private Animator animator;
    private BossDemonAnimationEvents animationEvents;
    
    // 调试状态
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
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        bossDemon = GetComponent<BossDemon>();
        animator = GetComponent<Animator>();
        animationEvents = GetComponent<BossDemonAnimationEvents>();
        
        if (bossDemon == null)
        {
            Debug.LogError("BossDemonDashDebugger: 未找到BossDemon组件");
        }
        
        if (animator == null)
        {
            Debug.LogError("BossDemonDashDebugger: 未找到Animator组件");
        }
        
        if (animationEvents == null)
        {
            Debug.LogWarning("BossDemonDashDebugger: 未找到BossDemonAnimationEvents组件");
        }
    }
    
    /// <summary>
    /// 延迟自动测试
    /// </summary>
    private IEnumerator DelayedAutoTest()
    {
        yield return new WaitForSeconds(2f); // 等待初始化完成
        TestDashAnimationFlow();
    }
    
    /// <summary>
    /// 测试Dash动画流程
    /// </summary>
    [ContextMenu("测试Dash动画流程")]
    public void TestDashAnimationFlow()
    {
        if (isTestingDash)
        {
            Debug.LogWarning("Dash测试已在进行中，请等待当前测试完成");
            return;
        }
        
        StartCoroutine(ExecuteDashTest());
    }
    
    /// <summary>
    /// 执行Dash测试
    /// </summary>
    private IEnumerator ExecuteDashTest()
    {
        isTestingDash = true;
        testStartTime = Time.time;
        
        LogDebug("=== 开始Dash动画流程测试 ===");
        
        // 步骤1：检查初始状态
        yield return StartCoroutine(CheckInitialState());
        
        // 步骤2：设置Dash状态
        yield return StartCoroutine(TestDashStateTransition());
        
        // 步骤3：模拟Dash动画播放
        yield return StartCoroutine(SimulateDashPlayback());
        
        // 步骤4：测试ShakeWave触发
        yield return StartCoroutine(TestShakeWaveTrigger());
        
        // 步骤5：检查最终状态
        yield return StartCoroutine(CheckFinalState());
        
        LogDebug("=== Dash动画流程测试完成 ===");
        isTestingDash = false;
    }
    
    /// <summary>
    /// 检查初始状态
    /// </summary>
    private IEnumerator CheckInitialState()
    {
        LogDebug("步骤1: 检查初始状态");
        
        if (animator == null)
        {
            LogError("Animator组件缺失");
            yield break;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            LogError("Animator Controller未配置");
            yield break;
        }
        
        // 检查参数配置
        bool hasStateParam = false;
        bool hasShakeWaveTrigger = false;
        
        foreach (var param in animator.parameters)
        {
            if (param.name == "State" && param.type == AnimatorControllerParameterType.Int)
            {
                hasStateParam = true;
                LogDebug($"? State参数已配置，当前值: {animator.GetInteger("State")}");
            }
            else if (param.name == "ShakeWave" && param.type == AnimatorControllerParameterType.Trigger)
            {
                hasShakeWaveTrigger = true;
                LogDebug("? ShakeWave触发器已配置");
            }
        }
        
        if (!hasStateParam)
        {
            LogError("? 缺少State参数配置");
        }
        
        if (!hasShakeWaveTrigger)
        {
            LogError("? 缺少ShakeWave触发器配置");
        }
        
        // 显示当前动画状态
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        LogDebug($"当前动画状态: {currentState.shortNameHash} (normalizedTime: {currentState.normalizedTime:F3})");
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 测试Dash状态转换
    /// </summary>
    private IEnumerator TestDashStateTransition()
    {
        LogDebug("步骤2: 测试Dash状态转换");
        
        // 设置State为2 (Dash)
        LogDebug("设置State = 2 (Dash)");
        animator.SetInteger("State", 2);
        
        // 等待状态转换
        yield return new WaitForSeconds(0.2f);
        
        // 检查转换结果
        int currentStateValue = animator.GetInteger("State");
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        LogDebug($"State参数设置结果: {currentStateValue}");
        LogDebug($"当前动画状态: {stateInfo.shortNameHash}");
        LogDebug($"动画进度: {stateInfo.normalizedTime:F3}");
        
        // 检查是否成功转换到Dash动画
        bool isDashAnimPlaying = CheckIfDashAnimationPlaying(stateInfo);
        
        if (isDashAnimPlaying)
        {
            LogDebug("? 成功转换到Dash动画");
        }
        else
        {
            LogWarning("? 可能未成功转换到Dash动画，请检查Animator Controller配置");
        }
        
        yield return new WaitForSeconds(0.3f);
    }
    
    /// <summary>
    /// 模拟Dash动画播放
    /// </summary>
    private IEnumerator SimulateDashPlayback()
    {
        LogDebug("步骤3: 模拟Dash动画播放");
        
        float dashStartTime = Time.time;
        float maxDashTime = testDashDuration + 1f; // 添加一些缓冲时间
        
        LogDebug($"等待Dash动画播放 (最大等待时间: {maxDashTime}秒)");
        
        while (Time.time - dashStartTime < maxDashTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            if (enableDebugLogs && Time.frameCount % 30 == 0) // 每30帧输出一次
            {
                LogDebug($"Dash动画进度: {stateInfo.normalizedTime:F3}");
            }
            
            // 检查动画是否接近完成
            if (stateInfo.normalizedTime >= 0.8f && CheckIfDashAnimationPlaying(stateInfo))
            {
                LogDebug("? Dash动画即将完成，准备触发ShakeWave");
                break;
            }
            
            yield return null;
        }
        
        if (Time.time - dashStartTime >= maxDashTime)
        {
            LogWarning("? Dash动画播放超时，可能存在配置问题");
        }
    }
    
    /// <summary>
    /// 测试ShakeWave触发
    /// </summary>
    private IEnumerator TestShakeWaveTrigger()
    {
        LogDebug("步骤4: 测试ShakeWave触发");
        
        // 触发ShakeWave
        LogDebug("触发ShakeWave");
        animator.SetTrigger("ShakeWave");
        
        // 等待短暂时间让触发器生效
        yield return new WaitForSeconds(0.1f);
        
        // 监控ShakeWave动画播放
        float shakeWaveStartTime = Time.time;
        float maxShakeWaveTime = 3f;
        bool shakeWaveDetected = false;
        
        LogDebug("监控ShakeWave动画播放...");
        
        while (Time.time - shakeWaveStartTime < maxShakeWaveTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // 检查是否进入ShakeWave动画
            if (CheckIfShakeWaveAnimationPlaying(stateInfo))
            {
                if (!shakeWaveDetected)
                {
                    LogDebug("? ShakeWave动画开始播放");
                    shakeWaveDetected = true;
                }
                
                if (enableDebugLogs && Time.frameCount % 30 == 0)
                {
                    LogDebug($"ShakeWave动画进度: {stateInfo.normalizedTime:F3}");
                }
                
                // 检查动画是否完成
                if (stateInfo.normalizedTime >= 0.95f)
                {
                    LogDebug("? ShakeWave动画播放完成");
                    break;
                }
            }
            
            yield return null;
        }
        
        if (!shakeWaveDetected)
        {
            LogError("? ShakeWave动画未检测到，可能Trigger配置有问题");
        }
        else if (Time.time - shakeWaveStartTime >= maxShakeWaveTime)
        {
            LogWarning("? ShakeWave动画播放超时");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 检查最终状态
    /// </summary>
    private IEnumerator CheckFinalState()
    {
        LogDebug("步骤5: 检查最终状态");
        
        // 重置状态到Idle
        LogDebug("重置State到0 (Idle)");
        animator.SetInteger("State", 0);
        
        yield return new WaitForSeconds(0.3f);
        
        // 检查最终状态
        AnimatorStateInfo finalState = animator.GetCurrentAnimatorStateInfo(0);
        int finalStateValue = animator.GetInteger("State");
        
        LogDebug($"最终State参数值: {finalStateValue}");
        LogDebug($"最终动画状态: {finalState.shortNameHash}");
        
        if (finalStateValue == 0)
        {
            LogDebug("? 成功重置到Idle状态");
        }
        else
        {
            LogWarning($"? State参数未正确重置，当前值: {finalStateValue}");
        }
    }
    
    /// <summary>
    /// 检查是否正在播放Dash动画
    /// </summary>
    private bool CheckIfDashAnimationPlaying(AnimatorStateInfo stateInfo)
    {
        // 可能的Dash动画名称
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
    /// 检查是否正在播放ShakeWave动画
    /// </summary>
    private bool CheckIfShakeWaveAnimationPlaying(AnimatorStateInfo stateInfo)
    {
        // 可能的ShakeWave动画名称
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
    /// 诊断动画器配置
    /// </summary>
    [ContextMenu("诊断动画器配置")]
    public void DiagnoseAnimatorConfiguration()
    {
        LogDebug("=== 动画器配置诊断 ===");
        
        if (animator == null)
        {
            LogError("Animator组件缺失");
            return;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            LogError("Animator Controller未配置");
            return;
        }
        
        LogDebug($"Animator Controller: {animator.runtimeAnimatorController.name}");
        
        // 检查层数
        LogDebug($"动画层数: {animator.layerCount}");
        
        // 检查参数
        LogDebug("=== 参数配置 ===");
        foreach (var param in animator.parameters)
        {
            LogDebug($"参数: {param.name} (类型: {param.type})");
        }
        
        // 检查当前状态
        for (int i = 0; i < animator.layerCount; i++)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
            LogDebug($"层 {i} 当前状态: {stateInfo.shortNameHash} (进度: {stateInfo.normalizedTime:F3})");
        }
        
        LogDebug("=== 诊断完成 ===");
    }
    
    /// <summary>
    /// 强制测试ShakeWave
    /// </summary>
    [ContextMenu("强制测试ShakeWave")]
    public void ForceTestShakeWave()
    {
        StartCoroutine(ForceShakeWaveTest());
    }
    
    /// <summary>
    /// 强制ShakeWave测试协程
    /// </summary>
    private IEnumerator ForceShakeWaveTest()
    {
        LogDebug("=== 强制ShakeWave测试 ===");
        
        LogDebug("触发ShakeWave");
        animator.SetTrigger("ShakeWave");
        
        yield return StartCoroutine(MonitorShakeWaveExecution());
        
        LogDebug("=== ShakeWave测试完成 ===");
    }
    
    /// <summary>
    /// 监控ShakeWave执行
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
                    LogDebug("? ShakeWave动画检测到");
                    detected = true;
                }
                
                LogDebug($"ShakeWave进度: {stateInfo.normalizedTime:F3}");
                
                if (stateInfo.normalizedTime >= 0.95f)
                {
                    LogDebug("? ShakeWave动画完成");
                    break;
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        if (!detected)
        {
            LogError("? ShakeWave动画未检测到，请检查Trigger配置");
        }
    }
    
    /// <summary>
    /// 日志输出（带开关控制）
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[DashDebugger] {message}");
        }
    }
    
    /// <summary>
    /// 警告日志输出
    /// </summary>
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[DashDebugger] {message}");
    }
    
    /// <summary>
    /// 错误日志输出
    /// </summary>
    private void LogError(string message)
    {
        Debug.LogError($"[DashDebugger] {message}");
    }
    
    /// <summary>
    /// 可视化调试信息
    /// </summary>
    void OnDrawGizmos()
    {
        if (!enableVisualDebug) return;
        
        // 绘制测试冲击波范围
        if (isTestingDash)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, testShockwaveRadius);
            
            // 绘制测试状态指示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
        }
    }
}