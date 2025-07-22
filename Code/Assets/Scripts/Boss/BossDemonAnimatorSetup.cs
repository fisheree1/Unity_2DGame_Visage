using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

/// <summary>
/// BossDemon Animator自动配置工具
/// 用于自动创建和配置Animator Controller
/// </summary>
public class BossDemonAnimatorSetup : MonoBehaviour
{
    [Header("Animator设置")]
    [SerializeField] private string controllerPath = "Assets/Animations/BossController.controller";
    [SerializeField] private bool autoCreateController = true;
    [SerializeField] private bool autoSetupParameters = true;
    [SerializeField] private bool autoSetupStates = true;
    
    [Header("动画片段路径")]
    [SerializeField] private string animationClipsPath = "Assets/Animations/Boss/";
    
    [Header("动画片段")]
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip walkClip;
    [SerializeField] private AnimationClip attackClip;
    [SerializeField] private AnimationClip hurtClip;
    [SerializeField] private AnimationClip deathClip;
    [SerializeField] private AnimationClip fireBreathClip;
    [SerializeField] private AnimationClip dashClip;
    [SerializeField] private AnimationClip shockwaveClip;
    
    private BossDemon bossDemon;
    private Animator animator;
    
    void Start()
    {
        bossDemon = GetComponent<BossDemon>();
        animator = GetComponent<Animator>();
        
        if (autoCreateController)
        {
            SetupAnimatorController();
        }
    }
    
    /// <summary>
    /// 设置Animator Controller
    /// </summary>
    [ContextMenu("设置Animator Controller")]
    public void SetupAnimatorController()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }
        }
        
#if UNITY_EDITOR
        CreateAnimatorController();
#else
        Debug.LogWarning("Animator Controller创建功能仅在编辑器中可用");
#endif
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 创建Animator Controller
    /// </summary>
    private void CreateAnimatorController()
    {
        // 创建Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        if (controller == null)
        {
            Debug.LogError("无法创建Animator Controller");
            return;
        }
        
        // 设置参数
        if (autoSetupParameters)
        {
            SetupAnimatorParameters(controller);
        }
        
        // 设置状态
        if (autoSetupStates)
        {
            SetupAnimatorStates(controller);
        }
        
        // 应用到组件
        animator.runtimeAnimatorController = controller;
        
        Debug.Log("Animator Controller创建完成：" + controllerPath);
    }
    
    /// <summary>
    /// 设置Animator参数
    /// </summary>
    private void SetupAnimatorParameters(AnimatorController controller)
    {
        // 添加整数参数
        controller.AddParameter("State", AnimatorControllerParameterType.Int);
        
        // 添加布尔参数
        controller.AddParameter("IsInCombat", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsPhase2", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsHurt", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
        
        // 添加浮点参数
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("AttackSpeed", AnimatorControllerParameterType.Float);
        
        // 添加触发器参数
        controller.AddParameter("TakeDamage", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("EnterPhase2", AnimatorControllerParameterType.Trigger);
        
        Debug.Log("Animator参数设置完成");
    }
    
    /// <summary>
    /// 设置Animator状态
    /// </summary>
    private void SetupAnimatorStates(AnimatorController controller)
    {
        var rootStateMachine = controller.layers[0].stateMachine;
        
        // 创建状态
        var idleState = rootStateMachine.AddState("Idle");
        var walkState = rootStateMachine.AddState("Walk");
        var attackState = rootStateMachine.AddState("Attack");
        var hurtState = rootStateMachine.AddState("Hurt");
        var deathState = rootStateMachine.AddState("Death");
        var fireBreathState = rootStateMachine.AddState("FireBreath");
        var dashState = rootStateMachine.AddState("Dash");
        var shockwaveState = rootStateMachine.AddState("Shockwave");
        
        // 设置默认状态
        rootStateMachine.defaultState = idleState;
        
        // 分配动画片段
        idleState.motion = idleClip;
        walkState.motion = walkClip;
        attackState.motion = attackClip;
        hurtState.motion = hurtClip;
        deathState.motion = deathClip;
        fireBreathState.motion = fireBreathClip;
        dashState.motion = dashClip;
        shockwaveState.motion = shockwaveClip;
        
        // 设置状态转换
        SetupStateTransitions(rootStateMachine, idleState, walkState, attackState, 
                              hurtState, deathState, fireBreathState, dashState, shockwaveState);
        
        Debug.Log("Animator状态设置完成");
    }
    
    /// <summary>
    /// 设置状态转换
    /// </summary>
    private void SetupStateTransitions(AnimatorStateMachine stateMachine,
        AnimatorState idleState, AnimatorState walkState, AnimatorState attackState,
        AnimatorState hurtState, AnimatorState deathState, AnimatorState fireBreathState,
        AnimatorState dashState, AnimatorState shockwaveState)
    {
        // 从任意状态到死亡状态
        var anyStateToDeathTransition = stateMachine.AddAnyStateTransition(deathState);
        anyStateToDeathTransition.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
        anyStateToDeathTransition.duration = 0.1f;
        anyStateToDeathTransition.canTransitionToSelf = false;
        
        // 从任意状态到受伤状态
        var anyStateToHurtTransition = stateMachine.AddAnyStateTransition(hurtState);
        anyStateToHurtTransition.AddCondition(AnimatorConditionMode.If, 0, "IsHurt");
        anyStateToHurtTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsDead");
        anyStateToHurtTransition.duration = 0.1f;
        anyStateToHurtTransition.canTransitionToSelf = false;
        
        // Idle状态的转换
        SetupIdleTransitions(idleState, walkState, attackState, fireBreathState, dashState);
        
        // Walk状态的转换
        SetupWalkTransitions(walkState, idleState, attackState);
        
        // Attack状态的转换
        SetupAttackTransitions(attackState, idleState);
        
        // FireBreath状态的转换
        SetupFireBreathTransitions(fireBreathState, idleState);
        
        // Dash状态的转换
        SetupDashTransitions(dashState, idleState, shockwaveState);
        
        // Shockwave状态的转换
        SetupShockwaveTransitions(shockwaveState, idleState);
        
        // Hurt状态的转换
        SetupHurtTransitions(hurtState, idleState);
    }
    
    /// <summary>
    /// 设置Idle状态转换
    /// </summary>
    private void SetupIdleTransitions(AnimatorState idleState, AnimatorState walkState, 
        AnimatorState attackState, AnimatorState fireBreathState, AnimatorState dashState)
    {
        // Idle → Walk
        var idleToWalkTransition = idleState.AddTransition(walkState);
        idleToWalkTransition.AddCondition(AnimatorConditionMode.Equals, 1, "State");
        idleToWalkTransition.duration = 0.2f;
        idleToWalkTransition.hasExitTime = false;
        
        // Idle → Attack
        var idleToAttackTransition = idleState.AddTransition(attackState);
        idleToAttackTransition.AddCondition(AnimatorConditionMode.Equals, 2, "State");
        idleToAttackTransition.duration = 0.1f;
        idleToAttackTransition.hasExitTime = false;
        
        // Idle → FireBreath
        var idleToFireBreathTransition = idleState.AddTransition(fireBreathState);
        idleToFireBreathTransition.AddCondition(AnimatorConditionMode.Equals, 5, "State");
        idleToFireBreathTransition.duration = 0.1f;
        idleToFireBreathTransition.hasExitTime = false;
        
        // Idle → Dash
        var idleToDashTransition = idleState.AddTransition(dashState);
        idleToDashTransition.AddCondition(AnimatorConditionMode.Equals, 6, "State");
        idleToDashTransition.duration = 0.1f;
        idleToDashTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// 设置Walk状态转换
    /// </summary>
    private void SetupWalkTransitions(AnimatorState walkState, AnimatorState idleState, AnimatorState attackState)
    {
        // Walk → Idle
        var walkToIdleTransition = walkState.AddTransition(idleState);
        walkToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        walkToIdleTransition.duration = 0.2f;
        walkToIdleTransition.hasExitTime = false;
        
        // Walk → Attack
        var walkToAttackTransition = walkState.AddTransition(attackState);
        walkToAttackTransition.AddCondition(AnimatorConditionMode.Equals, 2, "State");
        walkToAttackTransition.duration = 0.1f;
        walkToAttackTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// 设置Attack状态转换
    /// </summary>
    private void SetupAttackTransitions(AnimatorState attackState, AnimatorState idleState)
    {
        // Attack → Idle
        var attackToIdleTransition = attackState.AddTransition(idleState);
        attackToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        attackToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        attackToIdleTransition.duration = 0.2f;
        attackToIdleTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// 设置FireBreath状态转换
    /// </summary>
    private void SetupFireBreathTransitions(AnimatorState fireBreathState, AnimatorState idleState)
    {
        // FireBreath → Idle
        var fireBreathToIdleTransition = fireBreathState.AddTransition(idleState);
        fireBreathToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        fireBreathToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        fireBreathToIdleTransition.duration = 0.2f;
        fireBreathToIdleTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// 设置Dash状态转换
    /// </summary>
    private void SetupDashTransitions(AnimatorState dashState, AnimatorState idleState, AnimatorState shockwaveState)
    {
        // Dash → Shockwave
        var dashToShockwaveTransition = dashState.AddTransition(shockwaveState);
        dashToShockwaveTransition.AddCondition(AnimatorConditionMode.Equals, 7, "State");
        dashToShockwaveTransition.duration = 0.1f;
        dashToShockwaveTransition.hasExitTime = false;
        
        // Dash → Idle
        var dashToIdleTransition = dashState.AddTransition(idleState);
        dashToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        dashToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        dashToIdleTransition.duration = 0.2f;
        dashToIdleTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// 设置Shockwave状态转换
    /// </summary>
    private void SetupShockwaveTransitions(AnimatorState shockwaveState, AnimatorState idleState)
    {
        // Shockwave → Idle
        var shockwaveToIdleTransition = shockwaveState.AddTransition(idleState);
        shockwaveToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        shockwaveToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        shockwaveToIdleTransition.duration = 0.2f;
        shockwaveToIdleTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// 设置Hurt状态转换
    /// </summary>
    private void SetupHurtTransitions(AnimatorState hurtState, AnimatorState idleState)
    {
        // Hurt → Idle
        var hurtToIdleTransition = hurtState.AddTransition(idleState);
        hurtToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsHurt");
        hurtToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsDead");
        hurtToIdleTransition.duration = 0.2f;
        hurtToIdleTransition.hasExitTime = true;
        hurtToIdleTransition.exitTime = 0.8f;
    }
#endif
    
    /// <summary>
    /// 创建简单的动画片段
    /// </summary>
    [ContextMenu("创建默认动画片段")]
    public void CreateDefaultAnimationClips()
    {
#if UNITY_EDITOR
        // 创建动画片段目录
        string clipPath = animationClipsPath;
        if (!AssetDatabase.IsValidFolder(clipPath))
        {
            System.IO.Directory.CreateDirectory(clipPath);
            AssetDatabase.Refresh();
        }
        
        // 创建基本动画片段
        CreateAnimationClip("Boss_Idle", 2f, true);
        CreateAnimationClip("Boss_Walk", 1f, true);
        CreateAnimationClip("Boss_Attack", 1.5f, false);
        CreateAnimationClip("Boss_Hurt", 0.5f, false);
        CreateAnimationClip("Boss_Death", 3f, false);
        CreateAnimationClip("Boss_FireBreath", 3f, true);
        CreateAnimationClip("Boss_Dash", 0.5f, false);
        CreateAnimationClip("Boss_Shockwave", 1f, false);
        
        Debug.Log("默认动画片段创建完成");
#else
        Debug.LogWarning("动画片段创建功能仅在编辑器中可用");
#endif
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 创建动画片段
    /// </summary>
    private void CreateAnimationClip(string clipName, float duration, bool isLoop)
    {
        AnimationClip clip = new AnimationClip();
        clip.name = clipName;
        
        // 设置循环
        if (isLoop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }
        
        // 创建简单的位置动画关键帧
        AnimationCurve curve = AnimationCurve.Linear(0f, 0f, duration, 0f);
        clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
        
        // 保存动画片段
        string path = $"{animationClipsPath}{clipName}.anim";
        AssetDatabase.CreateAsset(clip, path);
        
        Debug.Log($"创建动画片段: {path}");
    }
#endif
    
    /// <summary>
    /// 验证Animator配置
    /// </summary>
    [ContextMenu("验证Animator配置")]
    public void ValidateAnimatorSetup()
    {
        if (animator == null)
        {
            Debug.LogError("未找到Animator组件");
            return;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("未分配Animator Controller");
            return;
        }
        
        // 检查参数
        var parameters = animator.parameters;
        string[] requiredParams = { "State", "IsInCombat", "IsPhase2", "IsDead", "IsHurt", "IsAttacking" };
        
        foreach (string param in requiredParams)
        {
            bool found = false;
            foreach (var p in parameters)
            {
                if (p.name == param)
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                Debug.LogError($"缺少参数: {param}");
            }
            else
            {
                Debug.Log($"参数验证通过: {param}");
            }
        }
        
        Debug.Log("Animator配置验证完成");
    }
    
    /// <summary>
    /// 测试动画状态切换
    /// </summary>
    [ContextMenu("测试动画状态")]
    public void TestAnimationStates()
    {
        if (animator == null)
        {
            Debug.LogError("未找到Animator组件");
            return;
        }
        
        StartCoroutine(TestAnimationSequence());
    }
    
    /// <summary>
    /// 测试动画序列
    /// </summary>
    private System.Collections.IEnumerator TestAnimationSequence()
    {
        Debug.Log("开始测试动画序列");
        
        // 测试各种状态
        int[] testStates = { 0, 1, 2, 5, 6, 7, 0 }; // Idle, Walk, Attack, FireBreath, Dash, Shockwave, Idle
        string[] stateNames = { "Idle", "Walk", "Attack", "FireBreath", "Dash", "Shockwave", "Idle" };
        
        for (int i = 0; i < testStates.Length; i++)
        {
            animator.SetInteger("State", testStates[i]);
            Debug.Log($"切换到状态: {stateNames[i]}");
            yield return new UnityEngine.WaitForSeconds(2f);
        }
        
        Debug.Log("动画序列测试完成");
    }
}