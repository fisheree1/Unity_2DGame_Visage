using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

/// <summary>
/// BossDemon Animator�Զ����ù���
/// �����Զ�����������Animator Controller
/// </summary>
public class BossDemonAnimatorSetup : MonoBehaviour
{
    [Header("Animator����")]
    [SerializeField] private string controllerPath = "Assets/Animations/BossController.controller";
    [SerializeField] private bool autoCreateController = true;
    [SerializeField] private bool autoSetupParameters = true;
    [SerializeField] private bool autoSetupStates = true;
    
    [Header("����Ƭ��·��")]
    [SerializeField] private string animationClipsPath = "Assets/Animations/Boss/";
    
    [Header("����Ƭ��")]
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
    /// ����Animator Controller
    /// </summary>
    [ContextMenu("����Animator Controller")]
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
        Debug.LogWarning("Animator Controller�������ܽ��ڱ༭���п���");
#endif
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// ����Animator Controller
    /// </summary>
    private void CreateAnimatorController()
    {
        // ����Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        if (controller == null)
        {
            Debug.LogError("�޷�����Animator Controller");
            return;
        }
        
        // ���ò���
        if (autoSetupParameters)
        {
            SetupAnimatorParameters(controller);
        }
        
        // ����״̬
        if (autoSetupStates)
        {
            SetupAnimatorStates(controller);
        }
        
        // Ӧ�õ����
        animator.runtimeAnimatorController = controller;
        
        Debug.Log("Animator Controller������ɣ�" + controllerPath);
    }
    
    /// <summary>
    /// ����Animator����
    /// </summary>
    private void SetupAnimatorParameters(AnimatorController controller)
    {
        // �����������
        controller.AddParameter("State", AnimatorControllerParameterType.Int);
        
        // ��Ӳ�������
        controller.AddParameter("IsInCombat", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsPhase2", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsHurt", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
        
        // ��Ӹ������
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("AttackSpeed", AnimatorControllerParameterType.Float);
        
        // ��Ӵ���������
        controller.AddParameter("TakeDamage", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("EnterPhase2", AnimatorControllerParameterType.Trigger);
        
        Debug.Log("Animator�����������");
    }
    
    /// <summary>
    /// ����Animator״̬
    /// </summary>
    private void SetupAnimatorStates(AnimatorController controller)
    {
        var rootStateMachine = controller.layers[0].stateMachine;
        
        // ����״̬
        var idleState = rootStateMachine.AddState("Idle");
        var walkState = rootStateMachine.AddState("Walk");
        var attackState = rootStateMachine.AddState("Attack");
        var hurtState = rootStateMachine.AddState("Hurt");
        var deathState = rootStateMachine.AddState("Death");
        var fireBreathState = rootStateMachine.AddState("FireBreath");
        var dashState = rootStateMachine.AddState("Dash");
        var shockwaveState = rootStateMachine.AddState("Shockwave");
        
        // ����Ĭ��״̬
        rootStateMachine.defaultState = idleState;
        
        // ���䶯��Ƭ��
        idleState.motion = idleClip;
        walkState.motion = walkClip;
        attackState.motion = attackClip;
        hurtState.motion = hurtClip;
        deathState.motion = deathClip;
        fireBreathState.motion = fireBreathClip;
        dashState.motion = dashClip;
        shockwaveState.motion = shockwaveClip;
        
        // ����״̬ת��
        SetupStateTransitions(rootStateMachine, idleState, walkState, attackState, 
                              hurtState, deathState, fireBreathState, dashState, shockwaveState);
        
        Debug.Log("Animator״̬�������");
    }
    
    /// <summary>
    /// ����״̬ת��
    /// </summary>
    private void SetupStateTransitions(AnimatorStateMachine stateMachine,
        AnimatorState idleState, AnimatorState walkState, AnimatorState attackState,
        AnimatorState hurtState, AnimatorState deathState, AnimatorState fireBreathState,
        AnimatorState dashState, AnimatorState shockwaveState)
    {
        // ������״̬������״̬
        var anyStateToDeathTransition = stateMachine.AddAnyStateTransition(deathState);
        anyStateToDeathTransition.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
        anyStateToDeathTransition.duration = 0.1f;
        anyStateToDeathTransition.canTransitionToSelf = false;
        
        // ������״̬������״̬
        var anyStateToHurtTransition = stateMachine.AddAnyStateTransition(hurtState);
        anyStateToHurtTransition.AddCondition(AnimatorConditionMode.If, 0, "IsHurt");
        anyStateToHurtTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsDead");
        anyStateToHurtTransition.duration = 0.1f;
        anyStateToHurtTransition.canTransitionToSelf = false;
        
        // Idle״̬��ת��
        SetupIdleTransitions(idleState, walkState, attackState, fireBreathState, dashState);
        
        // Walk״̬��ת��
        SetupWalkTransitions(walkState, idleState, attackState);
        
        // Attack״̬��ת��
        SetupAttackTransitions(attackState, idleState);
        
        // FireBreath״̬��ת��
        SetupFireBreathTransitions(fireBreathState, idleState);
        
        // Dash״̬��ת��
        SetupDashTransitions(dashState, idleState, shockwaveState);
        
        // Shockwave״̬��ת��
        SetupShockwaveTransitions(shockwaveState, idleState);
        
        // Hurt״̬��ת��
        SetupHurtTransitions(hurtState, idleState);
    }
    
    /// <summary>
    /// ����Idle״̬ת��
    /// </summary>
    private void SetupIdleTransitions(AnimatorState idleState, AnimatorState walkState, 
        AnimatorState attackState, AnimatorState fireBreathState, AnimatorState dashState)
    {
        // Idle �� Walk
        var idleToWalkTransition = idleState.AddTransition(walkState);
        idleToWalkTransition.AddCondition(AnimatorConditionMode.Equals, 1, "State");
        idleToWalkTransition.duration = 0.2f;
        idleToWalkTransition.hasExitTime = false;
        
        // Idle �� Attack
        var idleToAttackTransition = idleState.AddTransition(attackState);
        idleToAttackTransition.AddCondition(AnimatorConditionMode.Equals, 2, "State");
        idleToAttackTransition.duration = 0.1f;
        idleToAttackTransition.hasExitTime = false;
        
        // Idle �� FireBreath
        var idleToFireBreathTransition = idleState.AddTransition(fireBreathState);
        idleToFireBreathTransition.AddCondition(AnimatorConditionMode.Equals, 5, "State");
        idleToFireBreathTransition.duration = 0.1f;
        idleToFireBreathTransition.hasExitTime = false;
        
        // Idle �� Dash
        var idleToDashTransition = idleState.AddTransition(dashState);
        idleToDashTransition.AddCondition(AnimatorConditionMode.Equals, 6, "State");
        idleToDashTransition.duration = 0.1f;
        idleToDashTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// ����Walk״̬ת��
    /// </summary>
    private void SetupWalkTransitions(AnimatorState walkState, AnimatorState idleState, AnimatorState attackState)
    {
        // Walk �� Idle
        var walkToIdleTransition = walkState.AddTransition(idleState);
        walkToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        walkToIdleTransition.duration = 0.2f;
        walkToIdleTransition.hasExitTime = false;
        
        // Walk �� Attack
        var walkToAttackTransition = walkState.AddTransition(attackState);
        walkToAttackTransition.AddCondition(AnimatorConditionMode.Equals, 2, "State");
        walkToAttackTransition.duration = 0.1f;
        walkToAttackTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// ����Attack״̬ת��
    /// </summary>
    private void SetupAttackTransitions(AnimatorState attackState, AnimatorState idleState)
    {
        // Attack �� Idle
        var attackToIdleTransition = attackState.AddTransition(idleState);
        attackToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        attackToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        attackToIdleTransition.duration = 0.2f;
        attackToIdleTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// ����FireBreath״̬ת��
    /// </summary>
    private void SetupFireBreathTransitions(AnimatorState fireBreathState, AnimatorState idleState)
    {
        // FireBreath �� Idle
        var fireBreathToIdleTransition = fireBreathState.AddTransition(idleState);
        fireBreathToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        fireBreathToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        fireBreathToIdleTransition.duration = 0.2f;
        fireBreathToIdleTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// ����Dash״̬ת��
    /// </summary>
    private void SetupDashTransitions(AnimatorState dashState, AnimatorState idleState, AnimatorState shockwaveState)
    {
        // Dash �� Shockwave
        var dashToShockwaveTransition = dashState.AddTransition(shockwaveState);
        dashToShockwaveTransition.AddCondition(AnimatorConditionMode.Equals, 7, "State");
        dashToShockwaveTransition.duration = 0.1f;
        dashToShockwaveTransition.hasExitTime = false;
        
        // Dash �� Idle
        var dashToIdleTransition = dashState.AddTransition(idleState);
        dashToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        dashToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        dashToIdleTransition.duration = 0.2f;
        dashToIdleTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// ����Shockwave״̬ת��
    /// </summary>
    private void SetupShockwaveTransitions(AnimatorState shockwaveState, AnimatorState idleState)
    {
        // Shockwave �� Idle
        var shockwaveToIdleTransition = shockwaveState.AddTransition(idleState);
        shockwaveToIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        shockwaveToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        shockwaveToIdleTransition.duration = 0.2f;
        shockwaveToIdleTransition.hasExitTime = false;
    }
    
    /// <summary>
    /// ����Hurt״̬ת��
    /// </summary>
    private void SetupHurtTransitions(AnimatorState hurtState, AnimatorState idleState)
    {
        // Hurt �� Idle
        var hurtToIdleTransition = hurtState.AddTransition(idleState);
        hurtToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsHurt");
        hurtToIdleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsDead");
        hurtToIdleTransition.duration = 0.2f;
        hurtToIdleTransition.hasExitTime = true;
        hurtToIdleTransition.exitTime = 0.8f;
    }
#endif
    
    /// <summary>
    /// �����򵥵Ķ���Ƭ��
    /// </summary>
    [ContextMenu("����Ĭ�϶���Ƭ��")]
    public void CreateDefaultAnimationClips()
    {
#if UNITY_EDITOR
        // ��������Ƭ��Ŀ¼
        string clipPath = animationClipsPath;
        if (!AssetDatabase.IsValidFolder(clipPath))
        {
            System.IO.Directory.CreateDirectory(clipPath);
            AssetDatabase.Refresh();
        }
        
        // ������������Ƭ��
        CreateAnimationClip("Boss_Idle", 2f, true);
        CreateAnimationClip("Boss_Walk", 1f, true);
        CreateAnimationClip("Boss_Attack", 1.5f, false);
        CreateAnimationClip("Boss_Hurt", 0.5f, false);
        CreateAnimationClip("Boss_Death", 3f, false);
        CreateAnimationClip("Boss_FireBreath", 3f, true);
        CreateAnimationClip("Boss_Dash", 0.5f, false);
        CreateAnimationClip("Boss_Shockwave", 1f, false);
        
        Debug.Log("Ĭ�϶���Ƭ�δ������");
#else
        Debug.LogWarning("����Ƭ�δ������ܽ��ڱ༭���п���");
#endif
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// ��������Ƭ��
    /// </summary>
    private void CreateAnimationClip(string clipName, float duration, bool isLoop)
    {
        AnimationClip clip = new AnimationClip();
        clip.name = clipName;
        
        // ����ѭ��
        if (isLoop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }
        
        // �����򵥵�λ�ö����ؼ�֡
        AnimationCurve curve = AnimationCurve.Linear(0f, 0f, duration, 0f);
        clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
        
        // ���涯��Ƭ��
        string path = $"{animationClipsPath}{clipName}.anim";
        AssetDatabase.CreateAsset(clip, path);
        
        Debug.Log($"��������Ƭ��: {path}");
    }
#endif
    
    /// <summary>
    /// ��֤Animator����
    /// </summary>
    [ContextMenu("��֤Animator����")]
    public void ValidateAnimatorSetup()
    {
        if (animator == null)
        {
            Debug.LogError("δ�ҵ�Animator���");
            return;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("δ����Animator Controller");
            return;
        }
        
        // ������
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
                Debug.LogError($"ȱ�ٲ���: {param}");
            }
            else
            {
                Debug.Log($"������֤ͨ��: {param}");
            }
        }
        
        Debug.Log("Animator������֤���");
    }
    
    /// <summary>
    /// ���Զ���״̬�л�
    /// </summary>
    [ContextMenu("���Զ���״̬")]
    public void TestAnimationStates()
    {
        if (animator == null)
        {
            Debug.LogError("δ�ҵ�Animator���");
            return;
        }
        
        StartCoroutine(TestAnimationSequence());
    }
    
    /// <summary>
    /// ���Զ�������
    /// </summary>
    private System.Collections.IEnumerator TestAnimationSequence()
    {
        Debug.Log("��ʼ���Զ�������");
        
        // ���Ը���״̬
        int[] testStates = { 0, 1, 2, 5, 6, 7, 0 }; // Idle, Walk, Attack, FireBreath, Dash, Shockwave, Idle
        string[] stateNames = { "Idle", "Walk", "Attack", "FireBreath", "Dash", "Shockwave", "Idle" };
        
        for (int i = 0; i < testStates.Length; i++)
        {
            animator.SetInteger("State", testStates[i]);
            Debug.Log($"�л���״̬: {stateNames[i]}");
            yield return new UnityEngine.WaitForSeconds(2f);
        }
        
        Debug.Log("�������в������");
    }
}