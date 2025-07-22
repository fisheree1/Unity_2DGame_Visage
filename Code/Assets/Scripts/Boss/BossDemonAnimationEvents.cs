using UnityEngine;
using System.Collections;
using Cinemachine;

/// <summary>
/// BossDemon动画事件处理器
/// 处理动画事件，提供精确的时机控制
/// </summary>
public class BossDemonAnimationEvents : MonoBehaviour
{
    [Header("事件调试")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showEventNotifications = true;
    
    [Header("音效设置")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip fireBreathSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip shockwaveSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("特效设置")]
    [SerializeField] private ParticleSystem attackEffect;
    [SerializeField] private ParticleSystem fireBreathEffect;
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem shockwaveEffect;
    [SerializeField] private ParticleSystem hurtEffect;
    [SerializeField] private ParticleSystem deathEffect;
    
    [Header("相机震动")]
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private float attackShakeIntensity = 0.3f;
    [SerializeField] private float shockwaveShakeIntensity = 0.8f;
    [SerializeField] private float shakeDuration = 0.2f;
    
    [Header("火焰吐息近战攻击设置")]
    [SerializeField] private float fireBreathMeleeRange = 3f;
    [SerializeField] private float fireBreathDamageInterval = 0.5f;
    [SerializeField] private LayerMask playerLayer = -1;
    [SerializeField] private GameObject fireBreathHitbox; // 火焰吐息攻击的伤害判定范围
    
    private BossDemon bossDemon;
    private Animator animator;
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    
    // 事件计数器（用于调试）
    private int eventCounter = 0;
    
    // 火焰吐息持续伤害相关
    private bool isFireBreathActive = false;
    private float lastFireBreathDamageTime = 0f;
    private Coroutine fireBreathDamageCoroutine;
    
    void Start()
    {
        InitializeComponents();
        InitializeReferences();
    }
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        bossDemon = GetComponent<BossDemon>();
        animator = GetComponent<Animator>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalCameraPosition = mainCamera.transform.position;
            }
        }
        
        // 设置默认玩家层级
        if (playerLayer == -1)
        {
            playerLayer = LayerMask.GetMask("Player");
        }
    }
    
    /// <summary>
    /// 初始化引用
    /// </summary>
    private void InitializeReferences()
    {
        if (bossDemon == null)
        {
            Debug.LogError("BossDemon组件未找到！");
        }
        
        if (animator == null)
        {
            Debug.LogError("Animator组件未找到！");
        }
    }
    
    #region 基础攻击事件
    
    /// <summary>
    /// 攻击开始事件
    /// </summary>
    public void OnAttackStart()
    {
        LogEvent("攻击开始");
        
        PlaySound(attackSound);
        PlayEffect(attackEffect);
        
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(attackShakeIntensity, shakeDuration));
        }
    }
    
    /// <summary>
    /// 攻击命中事件
    /// </summary>
    public void OnAttackHit()
    {
        LogEvent("攻击命中时机");
        
        // 这里可以添加攻击判定逻辑
        if (bossDemon != null)
        {
            // 检查攻击范围内是否有玩家
            // bossDemon.CheckMeleeAttackHit();
        }
    }
    
    /// <summary>
    /// 攻击结束事件
    /// </summary>
    public void OnAttackEnd()
    {
        LogEvent("攻击结束");
        
        // 重置攻击状态
        if (bossDemon != null)
        {
            // bossDemon.OnAttackAnimationEnd();
        }
    }
    
    #endregion
    
    #region 火焰吐息事件
    
    /// <summary>
    /// 火焰吐息开始事件
    /// </summary>
    public void OnFireBreathStart()
    {
        LogEvent("火焰吐息开始");
        
        PlaySound(fireBreathSound);
        PlayEffect(fireBreathEffect);
        
        // 开始火焰吐息持续伤害
        isFireBreathActive = true;
        lastFireBreathDamageTime = Time.time;
        
        // 激活火焰吐息hitbox
        if (fireBreathHitbox != null)
        {
            fireBreathHitbox.SetActive(true);
            
            // 设置hitbox伤害值
            var hitboxComponent = fireBreathHitbox.GetComponent<AttackHitbox>();
            if (hitboxComponent != null)
            {
                float damage = GetFireBreathDamage();
                hitboxComponent.damage = Mathf.RoundToInt(damage);
                Debug.Log($"激活火焰吐息hitbox，伤害: {damage}");
            }
        }
        
        if (fireBreathDamageCoroutine != null)
        {
            StopCoroutine(fireBreathDamageCoroutine);
        }
        fireBreathDamageCoroutine = StartCoroutine(FireBreathMeleeDamageCoroutine());
        
        if (bossDemon != null)
        {
            // 开始火焰吐息效果
            Debug.Log("火焰吐息持续伤害开始");
        }
    }
    
    /// <summary>
    /// 火焰吐息持续事件
    /// </summary>
    public void OnFireBreathContinue()
    {
        LogEvent("火焰吐息持续");
        
        // 继续造成伤害（在协程中处理）
        // 伤害判定现在由hitbox和协程处理
        if (isFireBreathActive)
        {
            Debug.Log("火焰吐息持续中");
        }
    }
    
    /// <summary>
    /// 火焰吐息结束事件
    /// </summary>
    public void OnFireBreathEnd()
    {
        LogEvent("火焰吐息结束");
        
        StopEffect(fireBreathEffect);
        
        // 停用火焰吐息hitbox
        if (fireBreathHitbox != null)
        {
            fireBreathHitbox.SetActive(false);
            Debug.Log("停用火焰吐息hitbox");
        }
        
        // 停止火焰吐息持续伤害
        isFireBreathActive = false;
        
        if (fireBreathDamageCoroutine != null)
        {
            StopCoroutine(fireBreathDamageCoroutine);
            fireBreathDamageCoroutine = null;
        }
        
        if (bossDemon != null)
        {
            Debug.Log("火焰吐息持续伤害结束");
        }
    }
    
    /// <summary>
    /// 火焰吐息近战持续伤害协程
    /// </summary>
    private IEnumerator FireBreathMeleeDamageCoroutine()
    {
        while (isFireBreathActive)
        {
            // 每隔指定间隔重新激活hitbox（用于持续伤害）
            if (Time.time - lastFireBreathDamageTime >= fireBreathDamageInterval)
            {
                // 使用hitbox进行伤害判定
                if (fireBreathHitbox != null && fireBreathHitbox.activeInHierarchy)
                {
                    // hitbox已经激活，伤害由AttackHitbox组件自动处理
                    // 这里只需要更新hitbox的伤害值（如果需要的话）
                    var hitboxComponent = fireBreathHitbox.GetComponent<AttackHitbox>();
                    if (hitboxComponent != null)
                    {
                        float damage = GetFireBreathDamage();
                        hitboxComponent.damage = Mathf.RoundToInt(damage);
                    }
                    
                    Debug.Log("火焰吐息hitbox持续伤害更新");
                }
                else
                {
                    // 备用方案：使用原有的圆形检测
                    DealFireBreathMeleeDamageBackup();
                }
                
                lastFireBreathDamageTime = Time.time;
            }
            
            yield return new WaitForSeconds(0.1f); // 检查频率
        }
    }
    
    /// <summary>
    /// 火焰吐息近战伤害判定（备用方案）
    /// </summary>
    private void DealFireBreathMeleeDamageBackup()
    {
        if (bossDemon == null) return;
        
        Debug.Log("使用备用火焰吐息伤害判定");
        
        // 计算火焰吐息攻击位置（Boss前方）
        Vector3 attackPosition = transform.position;
        
        // 根据Boss朝向调整攻击位置
        bool isFacingRight = transform.localScale.x > 0;
        Vector3 offset = isFacingRight ? Vector3.right : Vector3.left;
        attackPosition += offset * (3f * 0.5f); // 使用固定范围
        
        // 检测范围内的玩家
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPosition, 3f, playerLayer);
        
        foreach (Collider2D target in targets)
        {
            if (target.CompareTag("Player"))
            {
                // 对玩家造成火焰吐息伤害
                HeroLife playerLife = target.GetComponent<HeroLife>();
                if (playerLife != null)
                {
                    // 从BossDemon获取火焰吐息伤害值
                    float damage = GetFireBreathDamage();
                    playerLife.TakeDamage(Mathf.RoundToInt(damage));
                    
                    Debug.Log($"火焰吐息近战攻击对玩家造成 {damage} 点伤害");
                    
                    // 添加燃烧效果
                    AddBurnEffect(target.gameObject);
                    
                    // 轻微的相机震动表示持续伤害
                    if (enableCameraShake)
                    {
                        StartCoroutine(CameraShake(0.1f, 0.1f));
                    }
                }
                break; // 只对一个玩家造成伤害
            }
        }
        
        // 绘制调试信息
        if (enableDebugLogs)
        {
            DebugExtensions.DrawCircle(attackPosition, 3f, Color.red, fireBreathDamageInterval);
        }
    }
    
    /// <summary>
    /// 获取火焰吐息伤害值
    /// </summary>
    private float GetFireBreathDamage()
    {
        // 通过反射获取BossDemon的火焰吐息伤害值
        if (bossDemon != null)
        {
            var field = typeof(BossDemon).GetField("fireBreathDamage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (float)field.GetValue(bossDemon);
            }
        }
        
        // 默认伤害值
        return 15f;
    }
    
    /// <summary>
    /// 添加燃烧效果
    /// </summary>
    private void AddBurnEffect(GameObject target)
    {
        // 检查是否已有燃烧效果
        BurnEffect existingBurn = target.GetComponent<BurnEffect>();
        if (existingBurn == null)
        {
            // 添加燃烧效果组件
            BurnEffect burnEffect = target.AddComponent<BurnEffect>();
            if (burnEffect != null)
            {
                Debug.Log("为玩家添加燃烧效果");
            }
        }
        else
        {
            // 重置燃烧效果持续时间
            existingBurn.ResetBurnDuration();
            Debug.Log("重置玩家燃烧效果持续时间");
        }
    }
    
    #endregion
    
    #region 冲刺攻击事件
    
    /// <summary>
    /// 冲刺开始事件
    /// </summary>
    public void OnDashStart()
    {
        LogEvent("冲刺开始");
        
        PlaySound(dashSound);
        PlayEffect(dashEffect);
        
        // 通知BossDemon冲刺开始
        if (bossDemon != null)
        {
            Debug.Log("动画事件：冲刺开始");
        }
    }
    
    /// <summary>
    /// 冲刺移动事件
    /// </summary>
    public void OnDashMove()
    {
        LogEvent("冲刺移动");
        
        // 冲刺移动逻辑由主系统处理
        if (bossDemon != null)
        {
            Debug.Log("动画事件：冲刺移动");
        }
    }
    
    /// <summary>
    /// 冲刺结束事件
    /// </summary>
    public void OnDashEnd()
    {
        LogEvent("冲刺结束");
        
        StopEffect(dashEffect);
        
        // 通知BossDemon冲刺结束
        if (bossDemon != null)
        {
            Debug.Log("动画事件：冲刺结束");
        }
    }
    
    /// <summary>
    /// 冲刺到达目标事件（用于触发后续逻辑）
    /// </summary>
    public void OnDashReachTarget()
    {
        LogEvent("冲刺到达目标");
        
        // 这个事件可以用来触发距离检查和后续攻击
        if (bossDemon != null)
        {
            Debug.Log("动画事件：冲刺到达目标，准备检查ShakeWave条件");
            // 可以调用BossDemon的公共方法来处理后续逻辑
        }
    }
    
    #endregion
    
    #region 冲击波事件
    
    /// <summary>
    /// 冲击波开始事件
    /// </summary>
    public void OnShockwaveStart()
    {
        LogEvent("冲击波开始");
        
        PlaySound(shockwaveSound);
        
        // 开始冲击波准备阶段
        if (bossDemon != null)
        {
            Debug.Log("动画事件：冲击波开始");
        }
    }
    
    /// <summary>
    /// 冲击波释放事件
    /// </summary>
    public void OnShockwaveRelease()
    {
        LogEvent("冲击波释放");
        
        PlaySound(shockwaveSound);
        PlayEffect(shockwaveEffect);
        
        // 通过CamaraShakeManager触发屏幕震动
        if (enableCameraShake)
        {
            var impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource != null && CamaraShakeManager.Instance != null)
            {
                // 使用现有的震动系统
                CamaraShakeManager.Instance.CamaraShake(impulseSource);
                Debug.Log("触发冲击波屏幕震动");
            }
            else
            {
                // 备用方案：使用内置相机震动
                StartCoroutine(CameraShake(shockwaveShakeIntensity, shakeDuration));
                Debug.Log("使用备用屏幕震动");
            }
        }
        
        // 执行冲击波伤害判定
        if (bossDemon != null)
        {
            Debug.Log("动画事件：冲击波释放，执行伤害判定");
            ExecuteShockwaveDamage();
        }
    }
    
    /// <summary>
    /// 冲击波结束事件
    /// </summary>
    public void OnShockwaveEnd()
    {
        LogEvent("冲击波结束");
        
        StopEffect(shockwaveEffect);
        
        // 通知BossDemon冲击波结束
        if (bossDemon != null)
        {
            Debug.Log("动画事件：冲击波结束");
        }
    }
    
    /// <summary>
    /// 执行冲击波伤害判定
    /// </summary>
    private void ExecuteShockwaveDamage()
    {
        if (bossDemon == null) return;
        
        // 获取冲击波伤害值和范围
        float shockwaveDamage = GetShockwaveDamage();
        float shockwaveRadius = GetShockwaveRadius();
        
        Debug.Log($"执行冲击波伤害判定 - 伤害：{shockwaveDamage}，范围：{shockwaveRadius}");
        
        // 检测范围内的目标
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius, playerLayer);
        
        foreach (var target in hitTargets)
        {
            if (target.CompareTag("Player"))
            {
                HeroLife playerLife = target.GetComponent<HeroLife>();
                if (playerLife != null)
                {
                    playerLife.TakeDamage(Mathf.RoundToInt(shockwaveDamage));
                    Debug.Log($"冲击波对玩家造成 {shockwaveDamage} 点伤害");
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// 获取冲击波伤害值
    /// </summary>
    private float GetShockwaveDamage()
    {
        if (bossDemon != null)
        {
            var field = typeof(BossDemon).GetField("shockwaveDamage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (float)field.GetValue(bossDemon);
            }
        }
        return 30f; // 默认值
    }
    
    /// <summary>
    /// 获取冲击波范围
    /// </summary>
    private float GetShockwaveRadius()
    {
        if (bossDemon != null)
        {
            var field = typeof(BossDemon).GetField("shockwaveRadius", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (float)field.GetValue(bossDemon);
            }
        }
        return 8f; // 默认值
    }
    
    #endregion
    
    #region 状态变化事件
    
    /// <summary>
    /// 受伤开始事件
    /// </summary>
    public void OnHurtStart()
    {
        LogEvent("受伤开始");
        
        PlaySound(hurtSound);
        PlayEffect(hurtEffect);
        
        // 受伤逻辑
        if (bossDemon != null)
        {
            // bossDemon.OnHurtAnimationStart();
        }
    }
    
    /// <summary>
    /// 受伤结束事件
    /// </summary>
    public void OnHurtEnd()
    {
        LogEvent("受伤结束");
        
        StopEffect(hurtEffect);
        
        // 恢复逻辑
        if (bossDemon != null)
        {
            // bossDemon.OnHurtAnimationEnd();
        }
    }
    
    /// <summary>
    /// 死亡开始事件
    /// </summary>
    public void OnDeathStart()
    {
        LogEvent("死亡开始");
        
        PlaySound(deathSound);
        PlayEffect(deathEffect);
        
        // 确保停止所有持续伤害
        StopAllContinuousEffects();
        
        // 死亡逻辑
        if (bossDemon != null)
        {
            // bossDemon.OnDeathAnimationStart();
        }
    }
    
    /// <summary>
    /// 死亡结束事件
    /// </summary>
    public void OnDeathEnd()
    {
        LogEvent("死亡结束");
        
        // 最终清理
        if (bossDemon != null)
        {
            // bossDemon.OnDeathAnimationEnd();
        }
    }
    
    #endregion
    
    #region 阶段转换事件
    
    /// <summary>
    /// 进入第二阶段事件
    /// </summary>
    public void OnEnterPhase2()
    {
        LogEvent("进入第二阶段");
        
        // 阶段转换效果
        if (bossDemon != null)
        {
            // bossDemon.OnPhase2AnimationTrigger();
        }
        
        // 特殊效果
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(0.5f, 1f));
        }
    }
    
    /// <summary>
    /// 阶段转换完成事件
    /// </summary>
    public void OnPhase2TransitionComplete()
    {
        LogEvent("第二阶段转换完成");
        
        // 完成阶段转换
        if (bossDemon != null)
        {
            // bossDemon.OnPhase2TransitionComplete();
        }
    }
    
    #endregion
    
    #region 出生状态事件
    
    /// <summary>
    /// 出生开始事件
    /// </summary>
    public void OnSpawnStart()
    {
        LogEvent("出生开始");
        
        // 播放出生音效
        PlaySound(attackSound); // 临时使用攻击音效
        
        // 播放出生特效
        PlayEffect(hurtEffect); // 临时使用受伤特效
        
        // 触发出生震动
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(0.5f, 0.3f));
        }
        
        Debug.Log("Boss出生动画开始");
    }
    
    /// <summary>
    /// 出生特效事件
    /// </summary>
    public void OnSpawnEffect()
    {
        LogEvent("出生特效");
        
        // 播放出生特效
        PlayEffect(shockwaveEffect); // 临时使用冲击波特效
        
        // 轻微震动
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(0.3f, 0.2f));
        }
        
        Debug.Log("Boss出生特效播放");
    }
    
    /// <summary>
    /// 出生完成事件
    /// </summary>
    public void OnSpawnComplete()
    {
        LogEvent("出生完成");
        
        // 停止出生特效
        StopEffect(hurtEffect);
        StopEffect(shockwaveEffect);
        
        // 通知BossDemon出生完成
        if (bossDemon != null)
        {
            Debug.Log("动画事件：Boss出生完成，准备进入战斗");
            // 这里可以触发BossDemon的出生完成方法
            // bossDemon.OnSpawnAnimationComplete();
        }
    }
    
    /// <summary>
    /// 出生咆哮事件
    /// </summary>
    public void OnSpawnRoar()
    {
        LogEvent("出生咆哮");
        
        // 播放咆哮音效
        PlaySound(dashSound); // 临时使用冲刺音效
        
        // 强烈震动
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(0.8f, 0.5f));
        }
        
        Debug.Log("Boss出生咆哮");
    }
    
    #endregion
    
    #region 工具方法
    
    /// <summary>
    /// 播放音效
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// 播放特效
    /// </summary>
    private void PlayEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Play();
        }
    }
    
    /// <summary>
    /// 停止特效
    /// </summary>
    private void StopEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Stop();
        }
    }
    
    /// <summary>
    /// 停止所有持续效果
    /// </summary>
    private void StopAllContinuousEffects()
    {
        // 停止火焰吐息持续伤害
        isFireBreathActive = false;
        if (fireBreathDamageCoroutine != null)
        {
            StopCoroutine(fireBreathDamageCoroutine);
            fireBreathDamageCoroutine = null;
        }
    }
    
    /// <summary>
    /// 记录事件
    /// </summary>
    private void LogEvent(string eventName)
    {
        eventCounter++;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[动画事件 #{eventCounter}] {eventName} - 时间: {Time.time:F2}");
        }
        
        if (showEventNotifications)
        {
            ShowEventNotification(eventName);
        }
    }
    
    /// <summary>
    /// 显示事件通知
    /// </summary>
    private void ShowEventNotification(string eventName)
    {
        // 这里可以添加UI通知显示
        // 例如：UIManager.Instance.ShowNotification(eventName);
    }
    
    /// <summary>
    /// 相机震动协程
    /// </summary>
    private IEnumerator CameraShake(float intensity, float duration)
    {
        if (mainCamera == null) yield break;
        
        Vector3 originalPosition = mainCamera.transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity, intensity);
            
            mainCamera.transform.position = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.position = originalPosition;
    }
    
    #endregion
    
    #region 调试方法
    
    /// <summary>
    /// 重置事件计数器
    /// </summary>
    [ContextMenu("重置事件计数器")]
    public void ResetEventCounter()
    {
        eventCounter = 0;
        Debug.Log("事件计数器已重置");
    }
    
    /// <summary>
    /// 显示事件统计
    /// </summary>
    [ContextMenu("显示事件统计")]
    public void ShowEventStatistics()
    {
        Debug.Log($"动画事件统计: 总计 {eventCounter} 个事件");
    }
    
    /// <summary>
    /// 测试Dash动画流程
    /// </summary>
    [ContextMenu("测试Dash动画流程")]
    public void TestDashAnimationFlow()
    {
        if (animator == null)
        {
            Debug.LogError("Animator组件未找到，无法测试Dash动画");
            return;
        }
        
        StartCoroutine(TestDashSequence());
    }
    
    /// <summary>
    /// 测试Dash动画序列
    /// </summary>
    private IEnumerator TestDashSequence()
    {
        Debug.Log("=== 开始测试Dash动画流程 ===");
        
        // 1. 设置Dash状态
        Debug.Log("步骤1: 设置State = 2 (Dash)");
        animator.SetInteger("State", 2);
        
        yield return new WaitForSeconds(0.5f);
        
        // 2. 检查当前动画状态
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"当前动画状态信息：");
        Debug.Log($"- shortNameHash: {stateInfo.shortNameHash}");
        Debug.Log($"- normalizedTime: {stateInfo.normalizedTime:F3}");
        Debug.Log($"- State参数值: {animator.GetInteger("State")}");
        
        yield return new WaitForSeconds(1f);
        
        // 3. 触发ShakeWave
        Debug.Log("步骤2: 触发ShakeWave");
        animator.SetTrigger("ShakeWave");
        
        yield return new WaitForSeconds(0.2f);
        
        // 4. 检查ShakeWave动画
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"ShakeWave触发后动画状态：");
        Debug.Log($"- shortNameHash: {stateInfo.shortNameHash}");
        Debug.Log($"- normalizedTime: {stateInfo.normalizedTime:F3}");
        
        yield return new WaitForSeconds(2f);
        
        // 5. 返回Idle
        Debug.Log("步骤3: 返回Idle状态");
        animator.SetInteger("State", 0);
        
        Debug.Log("=== Dash动画流程测试完成 ===");
    }
    
    /// <summary>
    /// 检查动画器参数配置
    /// </summary>
    [ContextMenu("检查动画器参数配置")]
    public void CheckAnimatorParameters()
    {
        if (animator == null)
        {
            Debug.LogError("Animator组件未找到");
            return;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator Controller未配置");
            return;
        }
        
        Debug.Log("=== 动画器参数检查 ===");
        
        var parameters = animator.parameters;
        string[] requiredParams = { "State", "IsPhase2", "IsAttack" };
        string[] requiredTriggers = { "Dead", "IsHit", "Attack", "IsCouterAttack", "ShakeWave" };
        
        // 检查必需参数
        foreach (string param in requiredParams)
        {
            bool found = false;
            foreach (var p in parameters)
            {
                if (p.name == param)
                {
                    found = true;
                    Debug.Log($"? 参数 {param} 已配置 (类型: {p.type})");
                    break;
                }
            }
            
            if (!found)
            {
                Debug.LogError($"? 缺少参数: {param}");
            }
        }
        
        // 检查必需触发器
        foreach (string trigger in requiredTriggers)
        {
            bool found = false;
            foreach (var p in parameters)
            {
                if (p.name == trigger && p.type == AnimatorControllerParameterType.Trigger)
                {
                    found = true;
                    Debug.Log($"? 触发器 {trigger} 已配置");
                    break;
                }
            }
            
            if (!found)
            {
                Debug.LogError($"? 缺少触发器: {trigger}");
            }
        }
        
        // 显示当前参数值
        Debug.Log("=== 当前参数值 ===");
        Debug.Log($"State: {animator.GetInteger("State")}");
        Debug.Log($"IsPhase2: {animator.GetBool("IsPhase2")}");
        Debug.Log($"IsAttack: {animator.GetBool("IsAttack")}");
        
        Debug.Log("=== 动画器参数检查完成 ===");
    }
    
    /// <summary>
    /// 强制触发ShakeWave测试
    /// </summary>
    [ContextMenu("强制触发ShakeWave测试")]
    public void ForceTriggerShakeWave()
    {
        if (animator == null)
        {
            Debug.LogError("Animator组件未找到");
            return;
        }
        
        Debug.Log("强制触发ShakeWave");
        animator.SetTrigger("ShakeWave");
        
        // 启动监控协程
        StartCoroutine(MonitorShakeWaveAnimation());
    }
    
    /// <summary>
    /// 监控ShakeWave动画播放
    /// </summary>
    private IEnumerator MonitorShakeWaveAnimation()
    {
        float startTime = Time.time;
        float maxWaitTime = 5f; // 最大等待时间
        
        Debug.Log("开始监控ShakeWave动画播放...");
        
        while (Time.time - startTime < maxWaitTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // 检查是否进入ShakeWave状态（需要根据实际动画名称调整）
            if (stateInfo.IsName("ShakeWave") || stateInfo.IsName("Boss_Shockwave"))
            {
                Debug.Log($"? ShakeWave动画正在播放 - 进度: {stateInfo.normalizedTime:F3}");
                
                // 如果动画播放完成
                if (stateInfo.normalizedTime >= 0.95f)
                {
                    Debug.Log("? ShakeWave动画播放完成");
                    break;
                }
            }
            else
            {
                Debug.Log($"当前动画状态: {stateInfo.shortNameHash} (等待ShakeWave)");
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        if (Time.time - startTime >= maxWaitTime)
        {
            Debug.LogWarning("? ShakeWave动画监控超时，可能动画配置有问题");
        }
        
        Debug.Log("ShakeWave动画监控结束");
    }
    
    /// <summary>
    /// 测试所有音效
    /// </summary>
    [ContextMenu("测试所有音效")]
    public void TestAllSounds()
    {
        StartCoroutine(TestSoundsSequence());
    }
    
    /// <summary>
    /// 测试音效序列
    /// </summary>
    private IEnumerator TestSoundsSequence()
    {
        AudioClip[] sounds = { attackSound, fireBreathSound, dashSound, shockwaveSound, hurtSound, deathSound };
        string[] soundNames = { "攻击", "火焰吐息", "冲刺", "冲击波", "受伤", "死亡" };
        
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i] != null)
            {
                Debug.Log($"测试音效: {soundNames[i]}");
                PlaySound(sounds[i]);
                yield return new WaitForSeconds(1f);
            }
        }
        
        Debug.Log("音效测试完成");
    }
    
    /// <summary>
    /// 测试所有特效
    /// </summary>
    [ContextMenu("测试所有特效")]
    public void TestAllEffects()
    {
        StartCoroutine(TestEffectsSequence());
    }
    
    /// <summary>
    /// 测试特效序列
    /// </summary>
    private IEnumerator TestEffectsSequence()
    {
        ParticleSystem[] effects = { attackEffect, fireBreathEffect, dashEffect, shockwaveEffect, hurtEffect, deathEffect };
        string[] effectNames = { "攻击", "火焰吐息", "冲刺", "冲击波", "受伤", "死亡" };
        
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i] != null)
            {
                Debug.Log($"测试特效: {effectNames[i]}");
                PlayEffect(effects[i]);
                yield return new WaitForSeconds(2f);
                StopEffect(effects[i]);
            }
        }
        
        Debug.Log("特效测试完成");
    }
    
    /// <summary>
    /// 测试火焰吐息近战攻击
    /// </summary>
    [ContextMenu("测试火焰吐息近战攻击")]
    public void TestFireBreathMeleeAttack()
    {
        OnFireBreathStart();
        StartCoroutine(TestFireBreathSequence());
    }
    
    /// <summary>
    /// 测试火焰吐息序列
    /// </summary>
    private IEnumerator TestFireBreathSequence()
    {
        yield return new WaitForSeconds(3f); // 持续3秒
        OnFireBreathEnd();
        Debug.Log("火焰吐息近战攻击测试完成");
    }
    
    #endregion
    
    #region Unity事件
    
    void OnValidate()
    {
        // 在Inspector中修改参数时自动更新引用
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    void OnDisable()
    {
        // 组件禁用时停止所有特效和持续效果
        StopAllContinuousEffects();
        
        if (attackEffect != null) StopEffect(attackEffect);
        if (fireBreathEffect != null) StopEffect(fireBreathEffect);
        if (dashEffect != null) StopEffect(dashEffect);
        if (shockwaveEffect != null) StopEffect(shockwaveEffect);
        if (hurtEffect != null) StopEffect(hurtEffect);
        if (deathEffect != null) StopEffect(deathEffect);
    }
    
    /// <summary>
    /// 绘制调试信息
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 绘制火焰吐息攻击范围
        if (isFireBreathActive || Application.isEditor)
        {
            Gizmos.color = Color.red;
            
            // 计算攻击位置
            Vector3 attackPosition = transform.position;
            bool isFacingRight = transform.localScale.x > 0;
            Vector3 offset = isFacingRight ? Vector3.right : Vector3.left;
            attackPosition += offset * (fireBreathMeleeRange * 0.5f);
            
            // 绘制攻击范围
            Gizmos.DrawWireSphere(attackPosition, fireBreathMeleeRange);
            
            // 绘制攻击方向
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, offset * fireBreathMeleeRange);
        }
    }
    
    #endregion
}

/// <summary>
/// Debug扩展方法
/// </summary>
public static class DebugExtensions
{
    /// <summary>
    /// 绘制圆形调试线
    /// </summary>
    public static void DrawCircle(Vector3 center, float radius, Color color, float duration = 0f)
    {
        int segments = 36;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
            
            Debug.DrawLine(point1, point2, color, duration);
        }
    }
}