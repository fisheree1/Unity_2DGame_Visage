using UnityEngine;
using System.Collections;
using Cinemachine;

/// <summary>
/// BossDemon�����¼�������
/// �������¼����ṩ��ȷ��ʱ������
/// </summary>
public class BossDemonAnimationEvents : MonoBehaviour
{
    [Header("�¼�����")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showEventNotifications = true;
    
    [Header("��Ч����")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip fireBreathSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip shockwaveSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("��Ч����")]
    [SerializeField] private ParticleSystem attackEffect;
    [SerializeField] private ParticleSystem fireBreathEffect;
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem shockwaveEffect;
    [SerializeField] private ParticleSystem hurtEffect;
    [SerializeField] private ParticleSystem deathEffect;
    
    [Header("�����")]
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private float attackShakeIntensity = 0.3f;
    [SerializeField] private float shockwaveShakeIntensity = 0.8f;
    [SerializeField] private float shakeDuration = 0.2f;
    
    [Header("������Ϣ��ս��������")]
    [SerializeField] private float fireBreathMeleeRange = 3f;
    [SerializeField] private float fireBreathDamageInterval = 0.5f;
    [SerializeField] private LayerMask playerLayer = -1;
    [SerializeField] private GameObject fireBreathHitbox; // ������Ϣ�������˺��ж���Χ
    
    private BossDemon bossDemon;
    private Animator animator;
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    
    // �¼������������ڵ��ԣ�
    private int eventCounter = 0;
    
    // ������Ϣ�����˺����
    private bool isFireBreathActive = false;
    private float lastFireBreathDamageTime = 0f;
    private Coroutine fireBreathDamageCoroutine;
    
    void Start()
    {
        InitializeComponents();
        InitializeReferences();
    }
    
    /// <summary>
    /// ��ʼ�����
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
        
        // ����Ĭ����Ҳ㼶
        if (playerLayer == -1)
        {
            playerLayer = LayerMask.GetMask("Player");
        }
    }
    
    /// <summary>
    /// ��ʼ������
    /// </summary>
    private void InitializeReferences()
    {
        if (bossDemon == null)
        {
            Debug.LogError("BossDemon���δ�ҵ���");
        }
        
        if (animator == null)
        {
            Debug.LogError("Animator���δ�ҵ���");
        }
    }
    
    #region ���������¼�
    
    /// <summary>
    /// ������ʼ�¼�
    /// </summary>
    public void OnAttackStart()
    {
        LogEvent("������ʼ");
        
        PlaySound(attackSound);
        PlayEffect(attackEffect);
        
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(attackShakeIntensity, shakeDuration));
        }
    }
    
    /// <summary>
    /// ���������¼�
    /// </summary>
    public void OnAttackHit()
    {
        LogEvent("��������ʱ��");
        
        // ���������ӹ����ж��߼�
        if (bossDemon != null)
        {
            // ��鹥����Χ���Ƿ������
            // bossDemon.CheckMeleeAttackHit();
        }
    }
    
    /// <summary>
    /// ���������¼�
    /// </summary>
    public void OnAttackEnd()
    {
        LogEvent("��������");
        
        // ���ù���״̬
        if (bossDemon != null)
        {
            // bossDemon.OnAttackAnimationEnd();
        }
    }
    
    #endregion
    
    #region ������Ϣ�¼�
    
    /// <summary>
    /// ������Ϣ��ʼ�¼�
    /// </summary>
    public void OnFireBreathStart()
    {
        LogEvent("������Ϣ��ʼ");
        
        PlaySound(fireBreathSound);
        PlayEffect(fireBreathEffect);
        
        // ��ʼ������Ϣ�����˺�
        isFireBreathActive = true;
        lastFireBreathDamageTime = Time.time;
        
        // ���������Ϣhitbox
        if (fireBreathHitbox != null)
        {
            fireBreathHitbox.SetActive(true);
            
            // ����hitbox�˺�ֵ
            var hitboxComponent = fireBreathHitbox.GetComponent<AttackHitbox>();
            if (hitboxComponent != null)
            {
                float damage = GetFireBreathDamage();
                hitboxComponent.damage = Mathf.RoundToInt(damage);
                Debug.Log($"���������Ϣhitbox���˺�: {damage}");
            }
        }
        
        if (fireBreathDamageCoroutine != null)
        {
            StopCoroutine(fireBreathDamageCoroutine);
        }
        fireBreathDamageCoroutine = StartCoroutine(FireBreathMeleeDamageCoroutine());
        
        if (bossDemon != null)
        {
            // ��ʼ������ϢЧ��
            Debug.Log("������Ϣ�����˺���ʼ");
        }
    }
    
    /// <summary>
    /// ������Ϣ�����¼�
    /// </summary>
    public void OnFireBreathContinue()
    {
        LogEvent("������Ϣ����");
        
        // ��������˺�����Э���д���
        // �˺��ж�������hitbox��Э�̴���
        if (isFireBreathActive)
        {
            Debug.Log("������Ϣ������");
        }
    }
    
    /// <summary>
    /// ������Ϣ�����¼�
    /// </summary>
    public void OnFireBreathEnd()
    {
        LogEvent("������Ϣ����");
        
        StopEffect(fireBreathEffect);
        
        // ͣ�û�����Ϣhitbox
        if (fireBreathHitbox != null)
        {
            fireBreathHitbox.SetActive(false);
            Debug.Log("ͣ�û�����Ϣhitbox");
        }
        
        // ֹͣ������Ϣ�����˺�
        isFireBreathActive = false;
        
        if (fireBreathDamageCoroutine != null)
        {
            StopCoroutine(fireBreathDamageCoroutine);
            fireBreathDamageCoroutine = null;
        }
        
        if (bossDemon != null)
        {
            Debug.Log("������Ϣ�����˺�����");
        }
    }
    
    /// <summary>
    /// ������Ϣ��ս�����˺�Э��
    /// </summary>
    private IEnumerator FireBreathMeleeDamageCoroutine()
    {
        while (isFireBreathActive)
        {
            // ÿ��ָ��������¼���hitbox�����ڳ����˺���
            if (Time.time - lastFireBreathDamageTime >= fireBreathDamageInterval)
            {
                // ʹ��hitbox�����˺��ж�
                if (fireBreathHitbox != null && fireBreathHitbox.activeInHierarchy)
                {
                    // hitbox�Ѿ�����˺���AttackHitbox����Զ�����
                    // ����ֻ��Ҫ����hitbox���˺�ֵ�������Ҫ�Ļ���
                    var hitboxComponent = fireBreathHitbox.GetComponent<AttackHitbox>();
                    if (hitboxComponent != null)
                    {
                        float damage = GetFireBreathDamage();
                        hitboxComponent.damage = Mathf.RoundToInt(damage);
                    }
                    
                    Debug.Log("������Ϣhitbox�����˺�����");
                }
                else
                {
                    // ���÷�����ʹ��ԭ�е�Բ�μ��
                    DealFireBreathMeleeDamageBackup();
                }
                
                lastFireBreathDamageTime = Time.time;
            }
            
            yield return new WaitForSeconds(0.1f); // ���Ƶ��
        }
    }
    
    /// <summary>
    /// ������Ϣ��ս�˺��ж������÷�����
    /// </summary>
    private void DealFireBreathMeleeDamageBackup()
    {
        if (bossDemon == null) return;
        
        Debug.Log("ʹ�ñ��û�����Ϣ�˺��ж�");
        
        // ���������Ϣ����λ�ã�Bossǰ����
        Vector3 attackPosition = transform.position;
        
        // ����Boss�����������λ��
        bool isFacingRight = transform.localScale.x > 0;
        Vector3 offset = isFacingRight ? Vector3.right : Vector3.left;
        attackPosition += offset * (3f * 0.5f); // ʹ�ù̶���Χ
        
        // ��ⷶΧ�ڵ����
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPosition, 3f, playerLayer);
        
        foreach (Collider2D target in targets)
        {
            if (target.CompareTag("Player"))
            {
                // �������ɻ�����Ϣ�˺�
                HeroLife playerLife = target.GetComponent<HeroLife>();
                if (playerLife != null)
                {
                    // ��BossDemon��ȡ������Ϣ�˺�ֵ
                    float damage = GetFireBreathDamage();
                    playerLife.TakeDamage(Mathf.RoundToInt(damage));
                    
                    Debug.Log($"������Ϣ��ս������������ {damage} ���˺�");
                    
                    // ���ȼ��Ч��
                    AddBurnEffect(target.gameObject);
                    
                    // ��΢������𶯱�ʾ�����˺�
                    if (enableCameraShake)
                    {
                        StartCoroutine(CameraShake(0.1f, 0.1f));
                    }
                }
                break; // ֻ��һ���������˺�
            }
        }
        
        // ���Ƶ�����Ϣ
        if (enableDebugLogs)
        {
            DebugExtensions.DrawCircle(attackPosition, 3f, Color.red, fireBreathDamageInterval);
        }
    }
    
    /// <summary>
    /// ��ȡ������Ϣ�˺�ֵ
    /// </summary>
    private float GetFireBreathDamage()
    {
        // ͨ�������ȡBossDemon�Ļ�����Ϣ�˺�ֵ
        if (bossDemon != null)
        {
            var field = typeof(BossDemon).GetField("fireBreathDamage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (float)field.GetValue(bossDemon);
            }
        }
        
        // Ĭ���˺�ֵ
        return 15f;
    }
    
    /// <summary>
    /// ���ȼ��Ч��
    /// </summary>
    private void AddBurnEffect(GameObject target)
    {
        // ����Ƿ�����ȼ��Ч��
        BurnEffect existingBurn = target.GetComponent<BurnEffect>();
        if (existingBurn == null)
        {
            // ���ȼ��Ч�����
            BurnEffect burnEffect = target.AddComponent<BurnEffect>();
            if (burnEffect != null)
            {
                Debug.Log("Ϊ������ȼ��Ч��");
            }
        }
        else
        {
            // ����ȼ��Ч������ʱ��
            existingBurn.ResetBurnDuration();
            Debug.Log("�������ȼ��Ч������ʱ��");
        }
    }
    
    #endregion
    
    #region ��̹����¼�
    
    /// <summary>
    /// ��̿�ʼ�¼�
    /// </summary>
    public void OnDashStart()
    {
        LogEvent("��̿�ʼ");
        
        PlaySound(dashSound);
        PlayEffect(dashEffect);
        
        // ֪ͨBossDemon��̿�ʼ
        if (bossDemon != null)
        {
            Debug.Log("�����¼�����̿�ʼ");
        }
    }
    
    /// <summary>
    /// ����ƶ��¼�
    /// </summary>
    public void OnDashMove()
    {
        LogEvent("����ƶ�");
        
        // ����ƶ��߼�����ϵͳ����
        if (bossDemon != null)
        {
            Debug.Log("�����¼�������ƶ�");
        }
    }
    
    /// <summary>
    /// ��̽����¼�
    /// </summary>
    public void OnDashEnd()
    {
        LogEvent("��̽���");
        
        StopEffect(dashEffect);
        
        // ֪ͨBossDemon��̽���
        if (bossDemon != null)
        {
            Debug.Log("�����¼�����̽���");
        }
    }
    
    /// <summary>
    /// ��̵���Ŀ���¼������ڴ��������߼���
    /// </summary>
    public void OnDashReachTarget()
    {
        LogEvent("��̵���Ŀ��");
        
        // ����¼�������������������ͺ�������
        if (bossDemon != null)
        {
            Debug.Log("�����¼�����̵���Ŀ�꣬׼�����ShakeWave����");
            // ���Ե���BossDemon�Ĺ�����������������߼�
        }
    }
    
    #endregion
    
    #region ������¼�
    
    /// <summary>
    /// �������ʼ�¼�
    /// </summary>
    public void OnShockwaveStart()
    {
        LogEvent("�������ʼ");
        
        PlaySound(shockwaveSound);
        
        // ��ʼ�����׼���׶�
        if (bossDemon != null)
        {
            Debug.Log("�����¼����������ʼ");
        }
    }
    
    /// <summary>
    /// ������ͷ��¼�
    /// </summary>
    public void OnShockwaveRelease()
    {
        LogEvent("������ͷ�");
        
        PlaySound(shockwaveSound);
        PlayEffect(shockwaveEffect);
        
        // ͨ��CamaraShakeManager������Ļ��
        if (enableCameraShake)
        {
            var impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource != null && CamaraShakeManager.Instance != null)
            {
                // ʹ�����е���ϵͳ
                CamaraShakeManager.Instance.CamaraShake(impulseSource);
                Debug.Log("�����������Ļ��");
            }
            else
            {
                // ���÷�����ʹ�����������
                StartCoroutine(CameraShake(shockwaveShakeIntensity, shakeDuration));
                Debug.Log("ʹ�ñ�����Ļ��");
            }
        }
        
        // ִ�г�����˺��ж�
        if (bossDemon != null)
        {
            Debug.Log("�����¼���������ͷţ�ִ���˺��ж�");
            ExecuteShockwaveDamage();
        }
    }
    
    /// <summary>
    /// ����������¼�
    /// </summary>
    public void OnShockwaveEnd()
    {
        LogEvent("���������");
        
        StopEffect(shockwaveEffect);
        
        // ֪ͨBossDemon���������
        if (bossDemon != null)
        {
            Debug.Log("�����¼������������");
        }
    }
    
    /// <summary>
    /// ִ�г�����˺��ж�
    /// </summary>
    private void ExecuteShockwaveDamage()
    {
        if (bossDemon == null) return;
        
        // ��ȡ������˺�ֵ�ͷ�Χ
        float shockwaveDamage = GetShockwaveDamage();
        float shockwaveRadius = GetShockwaveRadius();
        
        Debug.Log($"ִ�г�����˺��ж� - �˺���{shockwaveDamage}����Χ��{shockwaveRadius}");
        
        // ��ⷶΧ�ڵ�Ŀ��
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius, playerLayer);
        
        foreach (var target in hitTargets)
        {
            if (target.CompareTag("Player"))
            {
                HeroLife playerLife = target.GetComponent<HeroLife>();
                if (playerLife != null)
                {
                    playerLife.TakeDamage(Mathf.RoundToInt(shockwaveDamage));
                    Debug.Log($"������������� {shockwaveDamage} ���˺�");
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// ��ȡ������˺�ֵ
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
        return 30f; // Ĭ��ֵ
    }
    
    /// <summary>
    /// ��ȡ�������Χ
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
        return 8f; // Ĭ��ֵ
    }
    
    #endregion
    
    #region ״̬�仯�¼�
    
    /// <summary>
    /// ���˿�ʼ�¼�
    /// </summary>
    public void OnHurtStart()
    {
        LogEvent("���˿�ʼ");
        
        PlaySound(hurtSound);
        PlayEffect(hurtEffect);
        
        // �����߼�
        if (bossDemon != null)
        {
            // bossDemon.OnHurtAnimationStart();
        }
    }
    
    /// <summary>
    /// ���˽����¼�
    /// </summary>
    public void OnHurtEnd()
    {
        LogEvent("���˽���");
        
        StopEffect(hurtEffect);
        
        // �ָ��߼�
        if (bossDemon != null)
        {
            // bossDemon.OnHurtAnimationEnd();
        }
    }
    
    /// <summary>
    /// ������ʼ�¼�
    /// </summary>
    public void OnDeathStart()
    {
        LogEvent("������ʼ");
        
        PlaySound(deathSound);
        PlayEffect(deathEffect);
        
        // ȷ��ֹͣ���г����˺�
        StopAllContinuousEffects();
        
        // �����߼�
        if (bossDemon != null)
        {
            // bossDemon.OnDeathAnimationStart();
        }
    }
    
    /// <summary>
    /// ���������¼�
    /// </summary>
    public void OnDeathEnd()
    {
        LogEvent("��������");
        
        // ��������
        if (bossDemon != null)
        {
            // bossDemon.OnDeathAnimationEnd();
        }
    }
    
    #endregion
    
    #region �׶�ת���¼�
    
    /// <summary>
    /// ����ڶ��׶��¼�
    /// </summary>
    public void OnEnterPhase2()
    {
        LogEvent("����ڶ��׶�");
        
        // �׶�ת��Ч��
        if (bossDemon != null)
        {
            // bossDemon.OnPhase2AnimationTrigger();
        }
        
        // ����Ч��
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(0.5f, 1f));
        }
    }
    
    /// <summary>
    /// �׶�ת������¼�
    /// </summary>
    public void OnPhase2TransitionComplete()
    {
        LogEvent("�ڶ��׶�ת�����");
        
        // ��ɽ׶�ת��
        if (bossDemon != null)
        {
            // bossDemon.OnPhase2TransitionComplete();
        }
    }
    
    #endregion
    
    #region ����״̬�¼�
    
    /// <summary>
    /// ������ʼ�¼�
    /// </summary>
    public void OnSpawnStart()
    {
        LogEvent("������ʼ");
        
        // ���ų�����Ч
        PlaySound(attackSound); // ��ʱʹ�ù�����Ч
        
        // ���ų�����Ч
        PlayEffect(hurtEffect); // ��ʱʹ��������Ч
        
        // ����������
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(0.5f, 0.3f));
        }
        
        Debug.Log("Boss����������ʼ");
    }
    
    /// <summary>
    /// ������Ч�¼�
    /// </summary>
    public void OnSpawnEffect()
    {
        LogEvent("������Ч");
        
        // ���ų�����Ч
        PlayEffect(shockwaveEffect); // ��ʱʹ�ó������Ч
        
        // ��΢��
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(0.3f, 0.2f));
        }
        
        Debug.Log("Boss������Ч����");
    }
    
    /// <summary>
    /// ��������¼�
    /// </summary>
    public void OnSpawnComplete()
    {
        LogEvent("�������");
        
        // ֹͣ������Ч
        StopEffect(hurtEffect);
        StopEffect(shockwaveEffect);
        
        // ֪ͨBossDemon�������
        if (bossDemon != null)
        {
            Debug.Log("�����¼���Boss������ɣ�׼������ս��");
            // ������Դ���BossDemon�ĳ�����ɷ���
            // bossDemon.OnSpawnAnimationComplete();
        }
    }
    
    /// <summary>
    /// ���������¼�
    /// </summary>
    public void OnSpawnRoar()
    {
        LogEvent("��������");
        
        // ����������Ч
        PlaySound(dashSound); // ��ʱʹ�ó����Ч
        
        // ǿ����
        if (enableCameraShake)
        {
            StartCoroutine(CameraShake(0.8f, 0.5f));
        }
        
        Debug.Log("Boss��������");
    }
    
    #endregion
    
    #region ���߷���
    
    /// <summary>
    /// ������Ч
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// ������Ч
    /// </summary>
    private void PlayEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Play();
        }
    }
    
    /// <summary>
    /// ֹͣ��Ч
    /// </summary>
    private void StopEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Stop();
        }
    }
    
    /// <summary>
    /// ֹͣ���г���Ч��
    /// </summary>
    private void StopAllContinuousEffects()
    {
        // ֹͣ������Ϣ�����˺�
        isFireBreathActive = false;
        if (fireBreathDamageCoroutine != null)
        {
            StopCoroutine(fireBreathDamageCoroutine);
            fireBreathDamageCoroutine = null;
        }
    }
    
    /// <summary>
    /// ��¼�¼�
    /// </summary>
    private void LogEvent(string eventName)
    {
        eventCounter++;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[�����¼� #{eventCounter}] {eventName} - ʱ��: {Time.time:F2}");
        }
        
        if (showEventNotifications)
        {
            ShowEventNotification(eventName);
        }
    }
    
    /// <summary>
    /// ��ʾ�¼�֪ͨ
    /// </summary>
    private void ShowEventNotification(string eventName)
    {
        // ����������UI֪ͨ��ʾ
        // ���磺UIManager.Instance.ShowNotification(eventName);
    }
    
    /// <summary>
    /// �����Э��
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
    
    #region ���Է���
    
    /// <summary>
    /// �����¼�������
    /// </summary>
    [ContextMenu("�����¼�������")]
    public void ResetEventCounter()
    {
        eventCounter = 0;
        Debug.Log("�¼�������������");
    }
    
    /// <summary>
    /// ��ʾ�¼�ͳ��
    /// </summary>
    [ContextMenu("��ʾ�¼�ͳ��")]
    public void ShowEventStatistics()
    {
        Debug.Log($"�����¼�ͳ��: �ܼ� {eventCounter} ���¼�");
    }
    
    /// <summary>
    /// ����Dash��������
    /// </summary>
    [ContextMenu("����Dash��������")]
    public void TestDashAnimationFlow()
    {
        if (animator == null)
        {
            Debug.LogError("Animator���δ�ҵ����޷�����Dash����");
            return;
        }
        
        StartCoroutine(TestDashSequence());
    }
    
    /// <summary>
    /// ����Dash��������
    /// </summary>
    private IEnumerator TestDashSequence()
    {
        Debug.Log("=== ��ʼ����Dash�������� ===");
        
        // 1. ����Dash״̬
        Debug.Log("����1: ����State = 2 (Dash)");
        animator.SetInteger("State", 2);
        
        yield return new WaitForSeconds(0.5f);
        
        // 2. ��鵱ǰ����״̬
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"��ǰ����״̬��Ϣ��");
        Debug.Log($"- shortNameHash: {stateInfo.shortNameHash}");
        Debug.Log($"- normalizedTime: {stateInfo.normalizedTime:F3}");
        Debug.Log($"- State����ֵ: {animator.GetInteger("State")}");
        
        yield return new WaitForSeconds(1f);
        
        // 3. ����ShakeWave
        Debug.Log("����2: ����ShakeWave");
        animator.SetTrigger("ShakeWave");
        
        yield return new WaitForSeconds(0.2f);
        
        // 4. ���ShakeWave����
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"ShakeWave�����󶯻�״̬��");
        Debug.Log($"- shortNameHash: {stateInfo.shortNameHash}");
        Debug.Log($"- normalizedTime: {stateInfo.normalizedTime:F3}");
        
        yield return new WaitForSeconds(2f);
        
        // 5. ����Idle
        Debug.Log("����3: ����Idle״̬");
        animator.SetInteger("State", 0);
        
        Debug.Log("=== Dash�������̲������ ===");
    }
    
    /// <summary>
    /// ��鶯������������
    /// </summary>
    [ContextMenu("��鶯������������")]
    public void CheckAnimatorParameters()
    {
        if (animator == null)
        {
            Debug.LogError("Animator���δ�ҵ�");
            return;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator Controllerδ����");
            return;
        }
        
        Debug.Log("=== ������������� ===");
        
        var parameters = animator.parameters;
        string[] requiredParams = { "State", "IsPhase2", "IsAttack" };
        string[] requiredTriggers = { "Dead", "IsHit", "Attack", "IsCouterAttack", "ShakeWave" };
        
        // ���������
        foreach (string param in requiredParams)
        {
            bool found = false;
            foreach (var p in parameters)
            {
                if (p.name == param)
                {
                    found = true;
                    Debug.Log($"? ���� {param} ������ (����: {p.type})");
                    break;
                }
            }
            
            if (!found)
            {
                Debug.LogError($"? ȱ�ٲ���: {param}");
            }
        }
        
        // �����败����
        foreach (string trigger in requiredTriggers)
        {
            bool found = false;
            foreach (var p in parameters)
            {
                if (p.name == trigger && p.type == AnimatorControllerParameterType.Trigger)
                {
                    found = true;
                    Debug.Log($"? ������ {trigger} ������");
                    break;
                }
            }
            
            if (!found)
            {
                Debug.LogError($"? ȱ�ٴ�����: {trigger}");
            }
        }
        
        // ��ʾ��ǰ����ֵ
        Debug.Log("=== ��ǰ����ֵ ===");
        Debug.Log($"State: {animator.GetInteger("State")}");
        Debug.Log($"IsPhase2: {animator.GetBool("IsPhase2")}");
        Debug.Log($"IsAttack: {animator.GetBool("IsAttack")}");
        
        Debug.Log("=== ���������������� ===");
    }
    
    /// <summary>
    /// ǿ�ƴ���ShakeWave����
    /// </summary>
    [ContextMenu("ǿ�ƴ���ShakeWave����")]
    public void ForceTriggerShakeWave()
    {
        if (animator == null)
        {
            Debug.LogError("Animator���δ�ҵ�");
            return;
        }
        
        Debug.Log("ǿ�ƴ���ShakeWave");
        animator.SetTrigger("ShakeWave");
        
        // �������Э��
        StartCoroutine(MonitorShakeWaveAnimation());
    }
    
    /// <summary>
    /// ���ShakeWave��������
    /// </summary>
    private IEnumerator MonitorShakeWaveAnimation()
    {
        float startTime = Time.time;
        float maxWaitTime = 5f; // ���ȴ�ʱ��
        
        Debug.Log("��ʼ���ShakeWave��������...");
        
        while (Time.time - startTime < maxWaitTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // ����Ƿ����ShakeWave״̬����Ҫ����ʵ�ʶ������Ƶ�����
            if (stateInfo.IsName("ShakeWave") || stateInfo.IsName("Boss_Shockwave"))
            {
                Debug.Log($"? ShakeWave�������ڲ��� - ����: {stateInfo.normalizedTime:F3}");
                
                // ��������������
                if (stateInfo.normalizedTime >= 0.95f)
                {
                    Debug.Log("? ShakeWave�����������");
                    break;
                }
            }
            else
            {
                Debug.Log($"��ǰ����״̬: {stateInfo.shortNameHash} (�ȴ�ShakeWave)");
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        if (Time.time - startTime >= maxWaitTime)
        {
            Debug.LogWarning("? ShakeWave������س�ʱ�����ܶ�������������");
        }
        
        Debug.Log("ShakeWave������ؽ���");
    }
    
    /// <summary>
    /// ����������Ч
    /// </summary>
    [ContextMenu("����������Ч")]
    public void TestAllSounds()
    {
        StartCoroutine(TestSoundsSequence());
    }
    
    /// <summary>
    /// ������Ч����
    /// </summary>
    private IEnumerator TestSoundsSequence()
    {
        AudioClip[] sounds = { attackSound, fireBreathSound, dashSound, shockwaveSound, hurtSound, deathSound };
        string[] soundNames = { "����", "������Ϣ", "���", "�����", "����", "����" };
        
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i] != null)
            {
                Debug.Log($"������Ч: {soundNames[i]}");
                PlaySound(sounds[i]);
                yield return new WaitForSeconds(1f);
            }
        }
        
        Debug.Log("��Ч�������");
    }
    
    /// <summary>
    /// ����������Ч
    /// </summary>
    [ContextMenu("����������Ч")]
    public void TestAllEffects()
    {
        StartCoroutine(TestEffectsSequence());
    }
    
    /// <summary>
    /// ������Ч����
    /// </summary>
    private IEnumerator TestEffectsSequence()
    {
        ParticleSystem[] effects = { attackEffect, fireBreathEffect, dashEffect, shockwaveEffect, hurtEffect, deathEffect };
        string[] effectNames = { "����", "������Ϣ", "���", "�����", "����", "����" };
        
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i] != null)
            {
                Debug.Log($"������Ч: {effectNames[i]}");
                PlayEffect(effects[i]);
                yield return new WaitForSeconds(2f);
                StopEffect(effects[i]);
            }
        }
        
        Debug.Log("��Ч�������");
    }
    
    /// <summary>
    /// ���Ի�����Ϣ��ս����
    /// </summary>
    [ContextMenu("���Ի�����Ϣ��ս����")]
    public void TestFireBreathMeleeAttack()
    {
        OnFireBreathStart();
        StartCoroutine(TestFireBreathSequence());
    }
    
    /// <summary>
    /// ���Ի�����Ϣ����
    /// </summary>
    private IEnumerator TestFireBreathSequence()
    {
        yield return new WaitForSeconds(3f); // ����3��
        OnFireBreathEnd();
        Debug.Log("������Ϣ��ս�����������");
    }
    
    #endregion
    
    #region Unity�¼�
    
    void OnValidate()
    {
        // ��Inspector���޸Ĳ���ʱ�Զ���������
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    void OnDisable()
    {
        // �������ʱֹͣ������Ч�ͳ���Ч��
        StopAllContinuousEffects();
        
        if (attackEffect != null) StopEffect(attackEffect);
        if (fireBreathEffect != null) StopEffect(fireBreathEffect);
        if (dashEffect != null) StopEffect(dashEffect);
        if (shockwaveEffect != null) StopEffect(shockwaveEffect);
        if (hurtEffect != null) StopEffect(hurtEffect);
        if (deathEffect != null) StopEffect(deathEffect);
    }
    
    /// <summary>
    /// ���Ƶ�����Ϣ
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // ���ƻ�����Ϣ������Χ
        if (isFireBreathActive || Application.isEditor)
        {
            Gizmos.color = Color.red;
            
            // ���㹥��λ��
            Vector3 attackPosition = transform.position;
            bool isFacingRight = transform.localScale.x > 0;
            Vector3 offset = isFacingRight ? Vector3.right : Vector3.left;
            attackPosition += offset * (fireBreathMeleeRange * 0.5f);
            
            // ���ƹ�����Χ
            Gizmos.DrawWireSphere(attackPosition, fireBreathMeleeRange);
            
            // ���ƹ�������
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, offset * fireBreathMeleeRange);
        }
    }
    
    #endregion
}

/// <summary>
/// Debug��չ����
/// </summary>
public static class DebugExtensions
{
    /// <summary>
    /// ����Բ�ε�����
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