using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// �ع���Ķ�ħBossϵͳ
/// ֧�ֽ׶��Թ����仯��Ӣ����Ϊ���
/// �Ѽ���BossLife�������Ѫ������
/// </summary>
public class BossDemon : MonoBehaviour
{
    [Header("Boss״̬")]
    [SerializeField] private DemonState currentState = DemonState.Idle;
    
    [Header("Boss���� - ��BossLife����")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    
    [Header("�׶�ϵͳ")]
    [SerializeField] private bool isInPhase2 = false; // Ѫ������50%ʱ����ڶ��׶�
    [SerializeField] private float healthPhaseThreshold = 0.5f; // 50%Ѫ����ֵ
    
    [Header("��һ�׶ι�������")]
    [SerializeField] private float meleeAttackDamage = 25f;
    [SerializeField] private float shockwaveDamage = 30f;
    [SerializeField] private float shockwaveRadius = 8f;
    
    [Header("�ڶ��׶ι�������")]
    [SerializeField] private float fireBreathDamage = 15f;
    [SerializeField] private float fireBreathDuration = 3f;
    [SerializeField] private float fireBreathRange = 3f;
    
    [Header("��ս���ϵͳ")]
    [SerializeField] private Transform meleeAttackPoint; // ��ս������
    [SerializeField] private float meleeRangeRadius = 2.5f; // ��ս��Χ�뾶
    [SerializeField] private float meleeDetectionInterval = 2f; // ��ս�������2�룩
    [SerializeField] private LayerMask playerLayer = -1; // ��Ҳ㼶
    
    [Header("��̹���")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDistance = 10f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashHeight = 5f; // ��̸߶�
    [SerializeField] private float landingCheckInterval = 0.1f; // ��ؼ����
    
    [Header("С���ٻ�ϵͳ")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionsPerSpawn = 3;
    [SerializeField] private float minionSpawnRadius = 5f;
    [SerializeField] private float minionHealthThreshold = 0.33f; // ÿ1/3Ѫ���ٻ�С��
    
    [Header("Ͷ����ϵͳ")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] fireBreathPoints;
    [SerializeField] private float fireballSpeed = 8f;
    [SerializeField] private float bulletSpeed = 12f;
    
    [Header("AI��Ӧϵͳ")]
    [SerializeField] private float reactionCooldown = 2f;
    [SerializeField] private HeroActionTracker heroTracker;
    [SerializeField] private float slideReactionChance = 0.6f; // �Ի�����Ӧ�ĸ���
    [SerializeField] private float jumpReactionChance = 0.7f; // ����Ծ��Ӧ�ĸ���
    
    [Header("��Ļ������")]
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private float shockwaveShakeForce = 2f; // �������ǿ��
    [SerializeField] private float attackShakeForce = 0.8f; // ������ǿ��
    [SerializeField] private float phaseTransitionShakeForce = 1.5f; // �׶�ת����ǿ��
    
    [Header("�Ӿ�Ч��")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject hurtEffect;
    [SerializeField] private GameObject fireBreathEffect;
    [SerializeField] private GameObject shockwaveEffect;
    
    [Header("�˺��ж�Hitbox")]
    [SerializeField] private GameObject attackHitbox; // ��ս�������˺��ж���Χ
    [SerializeField] private GameObject fireBreathHitbox; // ������Ϣ�������˺��ж���Χ
    
    [Header("���˶���ϵͳ")]
    [SerializeField] private float hurtAnimationHealthThreshold = 0.2f; // 1/5 HP ��ֵ
    
    // ���
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private BossDemonAnimationEvents animationEvents;
    private CinemachineImpulseSource impulseSource;
    private BossLife bossLife; // Ѫ���������
    
    // ����
    private Transform player;
    private HeroMovement heroMovement;
    private HeroAttackController heroAttack;
    private HeroLife heroLife;
    
    // ״̬׷��
    private bool isDead = false;
    private bool isHurt = false;
    private bool isAttacking = false;
    private bool isFacingRight = true;
    private bool isDashing = false;
    private bool isInCombat = false;
    private bool isWaitingForLanding = false; // �ȴ����״̬
    
    // �׶κ��ٻ�׷��
    private float lastMinionSpawnHealth = 1f;
    private List<GameObject> spawnedMinions = new List<GameObject>();
    
    // AI��Ӧϵͳ
    private float lastReactionTime = 0f;
    private int consecutiveHeroAttacks = 0;
    private float lastHeroAttackTime = 0f;
    
    // ��ʱ��
    private float stateTimer = 0f;
    private float nextAttackTime = 0f;
    private float fireBreathTimer = 0f;
    
    // ���˶���׷�ٱ���
    private float lastHurtAnimationHealth = 1f; // �ϴβ������˶���ʱ��Ѫ���ٷֱ�
    
    // ��ս���ϵͳ����
    private float lastMeleeDetectionTime = 0f; // �ϴν�ս���ʱ��
    private bool isPlayerInMeleeRange = false; // ����Ƿ��ڽ�ս��Χ��
    
    /// <summary>
    /// ��ħBoss״̬ö��
    /// </summary>
    public enum DemonState
    {
        Spawn,          // ����״̬
        Idle,           // ����
        Walk,           // ����
        Attack,         // ��һ�׶���ͨ����
        FireBreath,     // �ڶ��׶λ�����Ϣ����
        Hurt,           // ����
        Death,          // ����
        Dash,           // ���
        Shockwave       // �����
    }
    
    // ���� - ͨ��BossLife�����ȡ
    public bool IsDead => bossLife != null ? bossLife.IsDead : isDead;
    public bool IsInPhase2 => isInPhase2;
    public float HealthPercentage => bossLife != null ? (float)bossLife.CurrentHealth / bossLife.MaxHealth : 0f;
    public int CurrentHealth => bossLife != null ? bossLife.CurrentHealth : 0;
    public int MaxHealth => bossLife != null ? bossLife.MaxHealth : 0;
    public bool IsSpawning => currentState == DemonState.Spawn; // �Ƿ����ڳ���
    public bool IsFacingRight => isFacingRight; // ��ӳ�������
    
    // �¼�
    public System.Action<int, int> OnHealthChanged;
    public System.Action OnDeath;
    public System.Action OnPhase2Enter;
    public System.Action OnSpawnComplete; // ��������¼�
    
    void Start()
    {
        InitializeComponents();
        InitializeReferences();
        InitializeStats();
        InitializeHeroTracker();
        SetupBossLifeIntegration();
        
        // �ӳ���״̬��ʼ
        StartSpawn();
    }
    
    void Update()
    {
        if (IsDead) return;
        
        UpdateTimers();
        UpdatePhaseSystem();
        UpdateMinionSpawning();
        
        // ֻ���ڳ�����ɺ�Ž���AI����
        if (currentState != DemonState.Spawn)
        {
            UpdateAI();
        }
        
        UpdateAnimations();
        UpdateFacingDirection();
        
        if (isInPhase2)
        {
            UpdateReactionSystem();
        }
    }
    
    /// <summary>
    /// ��ʼ�����
    /// </summary>
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        animationEvents = GetComponent<BossDemonAnimationEvents>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        bossLife = GetComponent<BossLife>();
        
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        if (anim == null) anim = gameObject.AddComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        if (animationEvents == null) animationEvents = gameObject.AddComponent<BossDemonAnimationEvents>();
        
        // ��ʼ����Ļ�����
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            Debug.Log("ΪBossDemon���CinemachineImpulseSource���");
        }
        
        // ȷ����BossLife���
        if (bossLife == null)
        {
            bossLife = gameObject.AddComponent<BossLife>();
            Debug.Log("ΪBossDemon���BossLife���");
        }
        
        // ��ʼ������hitbox
        InitializeAttackHitboxes();
        
        rb.gravityScale = 1f;
        rb.freezeRotation = true;
    }
    
    /// <summary>
    /// ��ʼ������hitbox
    /// </summary>
    private void InitializeAttackHitboxes()
    {
        // ������ս����hitbox
        if (attackHitbox == null)
        {
            attackHitbox = CreateAttackHitbox("AttackHitbox", Vector3.right * 1.5f, new Vector2(3f, 2f));
            Debug.Log("������ս����hitbox");
        }
        
        // ����������Ϣhitbox
        if (fireBreathHitbox == null)
        {
            fireBreathHitbox = CreateAttackHitbox("FireBreathHitbox", Vector3.right * 2f, new Vector2(4f, 3f));
            Debug.Log("����������Ϣhitbox");
        }
        
        // ��ʼ����ս������
        if (meleeAttackPoint == null)
        {
            GameObject meleePoint = new GameObject("MeleeAttackPoint");
            meleePoint.transform.SetParent(transform);
            meleePoint.transform.localPosition = new Vector3(1.5f, 0f, 0f);
            meleeAttackPoint = meleePoint.transform;
            Debug.Log("������ս������");
        }
        
        // ͬʱΪBossDemonAnimationEvents����hitbox����
        if (animationEvents != null)
        {
            var field = typeof(BossDemonAnimationEvents).GetField("fireBreathHitbox", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(animationEvents, fireBreathHitbox);
                Debug.Log("ΪBossDemonAnimationEvents���û�����Ϣhitbox����");
            }
        }
    }
    
    /// <summary>
    /// ��������hitbox
    /// </summary>
    /// <param name="name">hitbox����</param>
    /// <param name="localPosition">���λ��</param>
    /// <param name="size">hitbox��С</param>
    /// <returns>������hitbox��Ϸ����</returns>
    private GameObject CreateAttackHitbox(string name, Vector3 localPosition, Vector2 size)
    {
        GameObject hitbox = new GameObject(name);
        hitbox.transform.SetParent(transform);
        hitbox.transform.localPosition = localPosition;
        
        // ���BoxCollider2D
        BoxCollider2D boxCollider = hitbox.AddComponent<BoxCollider2D>();
        boxCollider.size = size;
        boxCollider.isTrigger = true;
        
        // ���BossAttackHitBox�������ȷ�������
        BossAttackHitBox attackHitboxComponent = hitbox.AddComponent<BossAttackHitBox>();
        // ʹ�÷�������˽���ֶε�damageֵ
        var damageField = typeof(BossAttackHitBox).GetField("damage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (damageField != null)
        {
            damageField.SetValue(attackHitboxComponent, (int)meleeAttackDamage);
        }
        
        // ���ñ�ǩ�Ͳ㼶
        hitbox.tag = "EnemyAttack"; // ȷ�����ṥ�������Լ�
        
        // ��ʼʱ����hitbox
        hitbox.SetActive(false);
        
        Debug.Log($"��������hitbox: {name}, ��С: {size}, λ��: {localPosition}");
        
        return hitbox;
    }
    
    /// <summary>
    /// ����BossLife����
    /// </summary>
    private void SetupBossLifeIntegration()
    {
        if (bossLife != null)
        {
            // ����BossLife�¼�
            bossLife.OnHealthChanged += OnBossLifeHealthChanged;
            bossLife.OnDeath += OnBossLifeDeath;
            
            Debug.Log($"BossDemon����BossLife��� - Ѫ��: {bossLife.CurrentHealth}/{bossLife.MaxHealth}");
        }
        else
        {
            Debug.LogError("BossDemon: �޷��ҵ��򴴽�BossLife�����");
        }
    }
    
    /// <summary>
    /// BossLifeѪ���仯�¼�����
    /// </summary>
    private void OnBossLifeHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"BossDemonѪ���仯: {currentHealth}/{maxHealth} ({HealthPercentage * 100:F1}%)");
        
        // ת���¼����ⲿϵͳ
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// BossLife�����¼�����
    /// </summary>
    private void OnBossLifeDeath()
    {
        Debug.Log("BossDemonͨ��BossLife����");
        Die();
    }
    
    /// <summary>
    /// �ܵ��˺� - ����BossLife
    /// </summary>
    /// <param name="damage">�˺�ֵ</param>
    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        
        // �����ڼ䲻���˺�
        if (currentState == DemonState.Spawn)
        {
            Debug.Log("Boss���ڳ������޷��ܵ��˺�");
            return;
        }
        
        Debug.Log($"BossDemon�ܵ��˺�: {damage}");
        
        // ��¼����ǰ��Ѫ���ٷֱ�
        float healthPercentageBeforeDamage = HealthPercentage;
        
        // ͨ��BossLife�����˺�
        if (bossLife != null)
        {
            bossLife.TakeDamage(damage);
        }
        
        // ����Ƿ�����
        if (IsDead) return;
        
        // ���˶��������߼���ֻ�е�Ѫ�����ٳ���1/5ʱ�Ų���
        float currentHealthPercentage = HealthPercentage;
        bool shouldPlayHurtAnimation = CheckShouldPlayHurtAnimation(healthPercentageBeforeDamage, currentHealthPercentage);
        
        if (shouldPlayHurtAnimation)
        {
            Debug.Log($"Ѫ���� {healthPercentageBeforeDamage * 100:F1}% ���� {currentHealthPercentage * 100:F1}%���������˶���");
            StartCoroutine(HurtCoroutine());
            
            // �����ϴβ������˶�����Ѫ��
            lastHurtAnimationHealth = currentHealthPercentage;
        }
        else
        {
            Debug.Log($"Ѫ���� {healthPercentageBeforeDamage * 100:F1}% ���� {currentHealthPercentage * 100:F1}%��δ�ﵽ���˶�����ֵ");
        }
    }
    
    /// <summary>
    /// ����Ƿ�Ӧ�ò������˶���
    /// </summary>
    /// <param name="healthBefore">����ǰѪ���ٷֱ�</param>
    /// <param name="healthAfter">���˺�Ѫ���ٷֱ�</param>
    /// <returns>�Ƿ�Ӧ�ò������˶���</returns>
    private bool CheckShouldPlayHurtAnimation(float healthBefore, float healthAfter)
    {
        // ����Ѫ���½��˶��ٸ�1/5��ֵ
        float healthDropped = healthBefore - healthAfter;
        
        // ����Ƿ��Խ��1/5Ѫ���Ľ���
        float thresholdsPassed = healthDropped / hurtAnimationHealthThreshold;
        
        // ���Ѫ���½�����1/5�����ߴ��ϴ����˶����������ۼ��½�����1/5
        float totalHealthDropSinceLastHurt = lastHurtAnimationHealth - healthAfter;
        
        bool shouldPlay = totalHealthDropSinceLastHurt >= hurtAnimationHealthThreshold;
        
        Debug.Log($"���˶������ - �����½�: {healthDropped * 100:F1}%, �ۼ��½�: {totalHealthDropSinceLastHurt * 100:F1}%, ��ֵ: {hurtAnimationHealthThreshold * 100:F1}%, Ӧ����: {shouldPlay}");
        
        return shouldPlay;
    }
    
    /// <summary>
    /// ����Э�� - ����ԭ�����˶���
    /// </summary>
    private IEnumerator HurtCoroutine()
    {
        isHurt = true;
        if (anim != null) anim.SetTrigger("IsHit");
        
        // ����������Ч
        if (hurtEffect != null)
        {
            Instantiate(hurtEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Boss���˶�������");
        
        yield return new WaitForSeconds(0.5f);
        
        isHurt = false;
        SetAnimationState(1); // �ص�Idle
    }
    
    /// <summary>
    /// ��ʼ������
    /// </summary>
    private void InitializeReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            heroMovement = playerObj.GetComponent<HeroMovement>();
            heroAttack = playerObj.GetComponent<HeroAttackController>();
            heroLife = playerObj.GetComponent<HeroLife>();
        }
        
        if (fireBreathPoints == null || fireBreathPoints.Length == 0)
        {
            InitializeFireBreathPoints();
        }
    }
    
    /// <summary>
    /// ��ʼ��ͳ������
    /// </summary>
    private void InitializeStats()
    {
        lastMinionSpawnHealth = 1f;
        lastHurtAnimationHealth = 1f; // ��ʼ�����˶���ϵͳ
        
        // ��ʼѪ��������BossLife����
        if (bossLife != null)
        {
            OnHealthChanged?.Invoke(bossLife.CurrentHealth, bossLife.MaxHealth);
        }
        
        Debug.Log($"BossDemonͳ�����ݳ�ʼ����� - ���˶�����ֵ: {hurtAnimationHealthThreshold * 100:F1}%");
    }
    
    /// <summary>
    /// ��ʼ��Ӣ�۶���׷����
    /// </summary>
    private void InitializeHeroTracker()
    {
        if (heroTracker == null)
        {
            GameObject trackerObj = new GameObject("HeroActionTracker");
            trackerObj.transform.SetParent(transform);
            heroTracker = trackerObj.AddComponent<HeroActionTracker>();
            heroTracker.Initialize(this);
        }
    }
    
    /// <summary>
    /// ��ʼ��������Ϣ��
    /// </summary>
    private void InitializeFireBreathPoints()
    {
        List<Transform> points = new List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            GameObject point = new GameObject($"FireBreathPoint_{i}");
            point.transform.SetParent(transform);
            point.transform.localPosition = new Vector3(1f + i * 0.5f, 0f, 0f);
            points.Add(point.transform);
        }
        fireBreathPoints = points.ToArray();
    }
    
    /// <summary>
    /// ������Ļ��
    /// </summary>
    /// <param name="shakeForce">��ǿ��</param>
    private void TriggerScreenShake(float shakeForce)
    {
        if (!enableScreenShake) return;
        
        if (impulseSource != null && CamaraShakeManager.Instance != null)
        {
            // ֱ��ʹ��CamaraShakeManagerϵͳ
            CamaraShakeManager.Instance.CamaraShake(impulseSource);
            Debug.Log($"������Ļ�𶯣�ǿ��: {shakeForce}");
        }
        else
        {
            Debug.LogWarning("�޷�������Ļ�𶯣�CinemachineImpulseSource �� CamaraShakeManager δ�ҵ�");
        }
    }
    
    /// <summary>
    /// ���¼�ʱ��
    /// </summary>
    private void UpdateTimers()
    {
        stateTimer += Time.deltaTime;
        
        if (fireBreathTimer > 0)
        {
            fireBreathTimer -= Time.deltaTime;
        }
        
        if (Time.time - lastHeroAttackTime > 1f)
        {
            consecutiveHeroAttacks = 0;
        }
    }
    
    /// <summary>
    /// ���½׶�ϵͳ
    /// </summary>
    private void UpdatePhaseSystem()
    {
        float healthPercentage = HealthPercentage;
        
        // ���ڶ��׶�ת��
        if (!isInPhase2 && healthPercentage <= healthPhaseThreshold)
        {
            EnterPhase2();
        }
    }
    
    /// <summary>
    /// ����С���ٻ�ϵͳ
    /// </summary>
    private void UpdateMinionSpawning()
    {
        float currentHealthPercentage = HealthPercentage;
        
        // ÿ1/3Ѫ���ٻ�С��
        if (currentHealthPercentage <= lastMinionSpawnHealth - minionHealthThreshold)
        {
            SpawnMinions();
            lastMinionSpawnHealth = currentHealthPercentage;
        }
    }
    
    /// <summary>
    /// ����AI��Ϊ
    /// </summary>
    private void UpdateAI()
    {
        // ������ڳ�����������AI����
        if (currentState == DemonState.Spawn)
        {
            return;
        }
        
        if (isHurt || isDashing || isWaitingForLanding) return;
        
        if (player == null || heroLife == null || heroLife.IsDead)
        {
            SetAnimationState(1); // Idle
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // ���½�ս��Χ��⣨ÿ2����һ�Σ�
        UpdateMeleeRangeDetection();
        
        // �������Ƿ��ڼ�ⷶΧ��
        if (distanceToPlayer <= detectionRange)
        {
            isInCombat = true;
            
            // ʹ���µĽ�ս���ϵͳ
            if (isPlayerInMeleeRange)
            {
                // �����빥��
                if (Time.time >= nextAttackTime)
                {
                    if (isInPhase2)
                    {
                        StartFireBreathAttack(); // �ڶ��׶�ʹ�û�����Ϣ
                    }
                    else
                    {
                        StartMeleeAttack(); // ��һ�׶�ʹ����ͨ��ս
                    }
                }
            }
            else
            {
                // ��Զ���� - ���ݽ׶�ѡ�񹥻�
                if (Time.time >= nextAttackTime)
                {
                    ChooseRangedAttack(distanceToPlayer);
                }
                else
                {
                    // ������ƶ�
                    SetAnimationState(2); // Walk
                    MoveTowardsPlayer();
                }
            }
        }
        else
        {
            isInCombat = false;
            SetAnimationState(1); // Idle
        }
    }
    
    /// <summary>
    /// ���½�ս��Χ��⣨ÿ2����һ�Σ�
    /// </summary>
    private void UpdateMeleeRangeDetection()
    {
        // ����Ƿ��˼��ʱ��
        if (Time.time - lastMeleeDetectionTime >= meleeDetectionInterval)
        {
            lastMeleeDetectionTime = Time.time;
            
            // ִ�н�ս��Χ���
            bool wasInRange = isPlayerInMeleeRange;
            isPlayerInMeleeRange = CheckMeleeRange();
            
            // �����Ҳ��ڽ�ս��Χ�ڣ���֮ǰ�ڷ�Χ�ڣ�����dash����
            if (wasInRange && !isPlayerInMeleeRange && isInCombat && !isAttacking && !isDashing)
            {
                Debug.Log("����뿪��ս��Χ��������̹����ͳ����");
                StartDashAttack();
            }
            
            // ������־
            if (wasInRange != isPlayerInMeleeRange)
            {
                Debug.Log($"��ս��Χ���仯: {wasInRange} -> {isPlayerInMeleeRange}");
            }
        }
    }
    
    /// <summary>
    /// �������Ƿ��ڽ�ս��Χ�ڣ��Թ�����ΪԲ�ģ�
    /// </summary>
    /// <returns>����Ƿ��ڽ�ս��Χ��</returns>
    private bool CheckMeleeRange()
    {
        if (meleeAttackPoint == null || player == null) return false;
        
        // ʹ��Physics2D.OverlapCircle���м��
        Collider2D playerCollider = Physics2D.OverlapCircle(
            meleeAttackPoint.position,
            meleeRangeRadius,
            playerLayer
        );
        
        bool inRange = playerCollider != null && playerCollider.CompareTag("Player");
        
        // ������־��ÿ�μ��ʱ�����
        Debug.Log($"��ս��Χ��� - ������: {meleeAttackPoint.position}, �뾶: {meleeRangeRadius}, ���: {inRange}");
        
        return inRange;
    }
    
    /// <summary>
    /// ѡ��Զ�̹���
    /// </summary>
    /// <param name="distance">����ҵľ���</param>
    private void ChooseRangedAttack(float distance)
    {
        // ����ʹ�ó��+��س��������
        StartDashAttack();
    }
    
    /// <summary>
    /// ��һ�׶Σ���ʼ��ͨ��ս����
    /// </summary>
    private void StartMeleeAttack()
    {
        isAttacking = true;
        if (anim != null) anim.SetTrigger("Attack");
        nextAttackTime = Time.time + 2f;
        
        StartCoroutine(MeleeAttackCoroutine());
    }
    
    /// <summary>
    /// ��һ�׶Σ���ͨ��ս����Э��
    /// </summary>
    private IEnumerator MeleeAttackCoroutine()
    {
        Debug.Log("Bossִ�е�һ�׶���ͨ��ս����");
        
        yield return new WaitForSeconds(0.5f); // ����ʱ��
        
        // ����������
        TriggerScreenShake(attackShakeForce);
        
        // ʹ��BossAttackHitBox�����˺��ж�
        if (attackHitbox != null)
        {
            // �����hitbox
            attackHitbox.SetActive(true);
            
            // ��ȡBossAttackHitBox���������
            var hitboxComponent = attackHitbox.GetComponent<BossAttackHitBox>();
            if (hitboxComponent != null)
            {
                // �����˺�ֵ
                var damageField = typeof(BossAttackHitBox).GetField("damage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (damageField != null)
                {
                    damageField.SetValue(hitboxComponent, Mathf.RoundToInt(meleeAttackDamage));
                }
                
                // ����hitbox
                hitboxComponent.ActivateForDuration(0.3f);
                Debug.Log($"�����ս����hitbox���˺�: {meleeAttackDamage}");
            }
            
            // �ȴ��������
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            Debug.LogWarning("AttackHitboxδ���ã�ʹ�ô�ͳ�˺��ж�");
            
            // ���÷�����ʹ��ԭ�еľ����ж�
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance <= attackRange)
                {
                    DamagePlayer(meleeAttackDamage);
                    Debug.Log($"��ͨ��ս�������У����{meleeAttackDamage}���˺�");
                }
            }
        }
        
        yield return new WaitForSeconds(1f); // �ָ�ʱ��
        
        isAttacking = false;
        SetAnimationState(1); // �ص�Idle
    }
    
    /// <summary>
    /// �ڶ��׶Σ���ʼ������Ϣ����
    /// </summary>
    private void StartFireBreathAttack()
    {
        isAttacking = true;
        if (anim != null) anim.SetTrigger("Attack");
        nextAttackTime = Time.time + 4f;
        fireBreathTimer = fireBreathDuration;
        
        Debug.Log("Bossִ�еڶ��׶λ�����Ϣ����");
        
        // ���ö����¼�ϵͳ���������Ϣ
        if (animationEvents != null)
        {
            StartCoroutine(FireBreathAttackCoroutine());
        }
    }
    
    /// <summary>
    /// �ڶ��׶Σ�������Ϣ����Э��
    /// </summary>
    private IEnumerator FireBreathAttackCoroutine()
    {
        Debug.Log("Bossִ�еڶ��׶λ�����Ϣ����");
        
        // ���������Ϣhitbox
        if (fireBreathHitbox != null)
        {
            var hitboxComponent = fireBreathHitbox.GetComponent<BossAttackHitBox>();
            if (hitboxComponent != null)
            {
                // �����˺�ֵ
                var damageField = typeof(BossAttackHitBox).GetField("damage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (damageField != null)
                {
                    damageField.SetValue(hitboxComponent, Mathf.RoundToInt(fireBreathDamage));
                }
                
                // ����hitbox��������������Ϣ�ڼ�
                hitboxComponent.ActivateForDuration(fireBreathDuration);
                Debug.Log($"���������Ϣhitbox���˺�: {fireBreathDamage}������ʱ��: {fireBreathDuration}");
            }
        }
        
        // �����¼�ϵͳ�ᴦ�������˺��߼�
        yield return new WaitForSeconds(fireBreathDuration);
        
        isAttacking = false;
        SetAnimationState(1); // �ص�Idle
    }
    
    /// <summary>
    /// ��ʼ��̹���
    /// </summary>
    private void StartDashAttack()
    {
        Debug.Log("Boss��ʼ��̹���");
        
        // ���ó��״̬
        isDashing = true;
        isAttacking = true;
        
        // ���ö���״̬ΪDash (State = 3)
        SetAnimationState(3);
        
        // ���ù�����ȴ
        nextAttackTime = Time.time + 4f;
        
        // �������Э��
        StartCoroutine(DashAttackCoroutine());
    }
    
    /// <summary>
    /// ��̹���Э�� - �°汾����̺�ȴ����
    /// </summary>
    private IEnumerator DashAttackCoroutine()
    {
        if (player == null) yield break;
        
        Vector2 dashDirection = (player.position - transform.position).normalized;
        Vector3 targetPosition = player.position;
        
        Debug.Log("Boss��ʼ��̹���Э��");
        
        // �ȴ�����ʱ���ö�����ʼ����
        yield return new WaitForSeconds(0.1f);
        
        // ��һ�׶Σ����ϳ��
        Vector2 jumpDirection = new Vector2(dashDirection.x, 1f).normalized;
        rb.velocity = jumpDirection * dashSpeed;
        
        Debug.Log("Boss���ϳ��");
        
        // �ȴ���̳���ʱ��
        yield return new WaitForSeconds(dashDuration);
        
        // �ڶ��׶Σ���Ŀ��λ�ó��
        Vector2 finalDirection = (targetPosition - transform.position).normalized;
        rb.velocity = finalDirection * dashSpeed;
        
        Debug.Log("Boss��Ŀ����");
        
        // �ȴ�����Ŀ�긽��
        yield return new WaitForSeconds(dashDuration * 0.5f);
        
        // �����׶Σ��ȴ����
        isWaitingForLanding = true;
        isDashing = false;
        
        Debug.Log("Boss�ȴ����");
        
        // ��ʼ��ؼ��Э��
        StartCoroutine(WaitForLandingCoroutine());
    }
    
    /// <summary>
    /// �ȴ����Э��
    /// </summary>
    private IEnumerator WaitForLandingCoroutine()
    {
        float landingCheckTimer = 0f;
        
        while (isWaitingForLanding)
        {
            landingCheckTimer += Time.deltaTime;
            
            // ���ڼ���Ƿ����
            if (landingCheckTimer >= landingCheckInterval)
            {
                landingCheckTimer = 0f;
                
                // ����Ƿ�ӽ�������ٶȺ�С
                if (IsNearGround() || Mathf.Abs(rb.velocity.y) < 0.5f)
                {
                    Debug.Log("Boss��أ��ͷų����");
                    
                    // ��أ��ͷų����
                    isWaitingForLanding = false;
                    yield return StartCoroutine(LandingShockwaveAttack());
                    break;
                }
            }
            
            yield return null;
        }
        
        // ����״̬
        isAttacking = false;
        SetAnimationState(1); // �ص�Idle
    }
    
    /// <summary>
    /// ����Ƿ�ӽ�����
    /// </summary>
    private bool IsNearGround()
    {
        // ���·������߼�����
        float rayDistance = 1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, LayerMask.GetMask("Ground"));
        
        return hit.collider != null;
    }
    
    /// <summary>
    /// ��س��������
    /// </summary>
    private IEnumerator LandingShockwaveAttack()
    {
        Debug.Log("Boss�ͷ���س����");
        
        // ȷ��Bossֹͣ�ƶ�
        rb.velocity = Vector2.zero;
        
        // �������������
        if (anim != null) 
        {
            anim.SetTrigger("ShakeWave");
        }
        
        // ���������Ч��
        if (shockwaveEffect != null)
        {
            Instantiate(shockwaveEffect, transform.position, Quaternion.identity);
        }
        
        // ����ǿ�ҵ���Ļ��
        TriggerScreenShake(shockwaveShakeForce);
        
        // �ȴ�һС��ʱ���ö�������
        yield return new WaitForSeconds(0.3f);
        
        // ִ�г�����˺�
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius);
        foreach (var target in hitTargets)
        {
            if (target.CompareTag("Player"))
            {
                DamagePlayer(shockwaveDamage);
                Debug.Log($"��س�������У����{shockwaveDamage}���˺�");
                break;
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// ��ʼ���ε�Ļ����
    /// </summary>
    private void StartFanBarrageAttack()
    {
        StartCoroutine(FanBarrageCoroutine());
    }
    
    /// <summary>
    /// ���ε�Ļ����Э��
    /// </summary>
    private IEnumerator FanBarrageCoroutine()
    {
        Debug.Log("Boss�ͷ����ε�Ļ����");
        
        int bulletCount = isInPhase2 ? 8 : 5;
        float fanAngle = isInPhase2 ? 60f : 45f;
        
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = -fanAngle / 2 + (fanAngle / (bulletCount - 1)) * i;
            Vector2 direction = RotateVector2(isFacingRight ? Vector2.right : Vector2.left, angle);
            
            CreateBullet(transform.position, direction);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    /// <summary>
    /// �ٻ�С��
    /// </summary>
    private void SpawnMinions()
    {
        if (minionPrefab == null) return;
        
        Debug.Log($"Boss�ٻ�{minionsPerSpawn}��С��");
        
        for (int i = 0; i < minionsPerSpawn; i++)
        {
            float angle = (360f / minionsPerSpawn) * i;
            Vector2 spawnPos = (Vector2)transform.position + (Vector2)(Quaternion.Euler(0, 0, angle) * Vector2.up * minionSpawnRadius);
            
            GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            spawnedMinions.Add(minion);
            
            // ��ʼ��С��
            DemonMinion minionScript = minion.GetComponent<DemonMinion>();
            if (minionScript == null)
            {
                minionScript = minion.AddComponent<DemonMinion>();
            }
            minionScript.Initialize(this, player);
        }
    }
    
    /// <summary>
    /// ����ڶ��׶�
    /// </summary>
    private void EnterPhase2()
    {
        isInPhase2 = true;
        moveSpeed *= 1.2f; // �����ƶ��ٶȣ�������������
        OnPhase2Enter?.Invoke();
        
        Debug.Log("Boss����ڶ��׶� - ����������ǿ��");
        
        // �����׶�ת����
        TriggerScreenShake(phaseTransitionShakeForce);
        
        // �׶�ת�����Ӿ�Ч��
        if (hurtEffect != null)
        {
            Instantiate(hurtEffect, transform.position, Quaternion.identity);
        }
    }
    
    /// <summary>
    /// ��ʼ����״̬
    /// </summary>
    private void StartSpawn()
    {
        Debug.Log("BossDemon��ʼ����");
        
        // ���ó���״̬
        currentState = DemonState.Spawn;
        
        // ȷ��Boss�����ƶ�
        rb.velocity = Vector2.zero;
        
        // ����AI����ֱ���������
        isInCombat = false;
        
        // ���ų�������
        if (anim != null)
        {
            anim.SetInteger("State", (int)DemonState.Spawn);
            anim.SetTrigger("Spawn");
            Debug.Log("Boss����������ʼ����");
        }
        
        // ����������Ч
        if (hurtEffect != null) // ��ʱʹ��������Ч
        {
            Instantiate(hurtEffect, transform.position, Quaternion.identity);
        }
        
        // ����������
        TriggerScreenShake(phaseTransitionShakeForce);
        
        // ��ʼ����Э��
        StartCoroutine(SpawnCoroutine());
    }
    
    /// <summary>
    /// ����Э��
    /// </summary>
    private IEnumerator SpawnCoroutine()
    {
        Debug.Log("Boss����Э�̿�ʼ");
        
        // �ȴ����������������
        yield return new WaitForSeconds(2f); // ������������ʱ��
        
        // ��ɳ���
        CompleteSpawn();
    }
    
    /// <summary>
    /// ��ɳ���
    /// </summary>
    private void CompleteSpawn()
    {
        Debug.Log("Boss�������");
        
        // �л���Idle״̬
        currentState = DemonState.Idle;
        
        // ������������¼�
        OnSpawnComplete?.Invoke();
        
        // ���¶���״̬
        if (anim != null)
        {
            anim.SetInteger("State", (int)DemonState.Idle);
            Debug.Log("Boss����������ɣ��л���Idle״̬");
        }
        
        // ����AI����
        isInCombat = false; // ��ʼʱ����ս��״̬
        
        // ���ͳ�ʼѪ���¼�
        if (bossLife != null)
        {
            OnHealthChanged?.Invoke(bossLife.CurrentHealth, bossLife.MaxHealth);
        }
        
        Debug.Log("Boss��׼���ý���ս��");
    }
    
    /// <summary>
    /// ��Ӣ�ۻ����ķ�Ӧ�����ڶ��׶Σ�
    /// </summary>
    private void ReactToHeroSlide()
    {
        lastReactionTime = Time.time;
        
        Debug.Log("Boss��⵽Ӣ�ۻ�����ִ�з�Ӧ");
        
        // �м���dash���ͷų����
        StartDashAttack();
    }
    
    /// <summary>
    /// ��Ӣ����Ծ�ķ�Ӧ�����ڶ��׶Σ�
    /// </summary>
    private void ReactToHeroJump()
    {
        lastReactionTime = Time.time;
        
        Debug.Log("Boss��⵽Ӣ����Ծ���ͷ����ε�Ļ");
        
        // ��Ӣ���ͷ����ε�Ļ����
        StartFanBarrageAttack();
    }
    
    /// <summary>
    /// �����ӵ�
    /// </summary>
    /// <param name="position">λ��</param>
    /// <param name="direction">����</param>
    private void CreateBullet(Vector3 position, Vector2 direction)
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * bulletSpeed;
            }
            
            // ����˺����
            DemonProjectile projectile = bullet.GetComponent<DemonProjectile>();
            if (projectile == null)
            {
                projectile = bullet.AddComponent<DemonProjectile>();
            }
            projectile.Initialize(15f, false); // �����˺�
            
            Destroy(bullet, 5f);
        }
    }
    
    /// <summary>
    /// ��ת2D����
    /// </summary>
    /// <param name="vector">����</param>
    /// <param name="angle">�Ƕ�</param>
    /// <returns>��ת�������</returns>
    private Vector2 RotateVector2(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
    }
    
    /// <summary>
    /// ���������˺�
    /// </summary>
    /// <param name="damage">�˺�ֵ</param>
    private void DamagePlayer(float damage)
    {
        if (player != null && heroLife != null && !heroLife.IsDead)
        {
            heroLife.TakeDamage(Mathf.RoundToInt(damage));
        }
    }
    
    /// <summary>
    /// ����
    /// </summary>
    private void Die()
    {
        if (isDead) return; // ��ֹ�ظ�����
        
        isDead = true;
        if (anim != null) anim.SetTrigger("Dead");
        rb.velocity = Vector2.zero;
        
        Debug.Log("BossDemon����");
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // ������ײ��
        if (col != null)
        {
            col.enabled = false;
        }
        
        // ���������ٻ���С��
        foreach (var minion in spawnedMinions)
        {
            if (minion != null)
            {
                Destroy(minion);
            }
        }
        spawnedMinions.Clear();
        
        OnDeath?.Invoke();
        
        // ���������������٣�BossLife����ᴦ�������
        // Destroy(gameObject, 3f);
    }
    
    /// <summary>
    /// Ӣ�۹�������׷��
    /// </summary>
    public void OnHeroAttack()
    {
        if (Time.time - lastHeroAttackTime < 1f)
        {
            consecutiveHeroAttacks++;
        }
        else
        {
            consecutiveHeroAttacks = 1;
        }
        lastHeroAttackTime = Time.time;
    }
    
    /// <summary>
    /// �ڱ༭���л��Ƶ�����Ϣ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // ��ⷶΧ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // ������Χ��ԭ�е�Բ�η�Χ�������ο���
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // �������Χ
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
        
        // ��ս������Χ���Թ�����ΪԲ�ģ�
        if (meleeAttackPoint != null)
        {
            Gizmos.color = isPlayerInMeleeRange ? Color.red : new Color(1f, 0.5f, 0f); // ��ɫ
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeRangeRadius);
            
            // ���ƹ�������
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(meleeAttackPoint.position, Vector3.one * 0.3f);
        }
        
        // ��ս����hitbox
        if (attackHitbox != null)
        {
            Gizmos.color = Color.red;
            BoxCollider2D attackBox = attackHitbox.GetComponent<BoxCollider2D>();
            if (attackBox != null)
            {
                Vector3 hitboxWorldPos = transform.TransformPoint(attackHitbox.transform.localPosition);
                Vector3 hitboxSize = new Vector3(attackBox.size.x, attackBox.size.y, 0.2f);
                Gizmos.DrawWireCube(hitboxWorldPos, hitboxSize);
                
                // ���hitbox�����ʵ�ķ�����ʾ
                if (attackHitbox.activeInHierarchy)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                    Gizmos.DrawCube(hitboxWorldPos, hitboxSize);
                }
            }
        }
        
        // ������Ϣhitbox
        if (fireBreathHitbox != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f); // ��ɫ
            BoxCollider2D fireBreathBox = fireBreathHitbox.GetComponent<BoxCollider2D>();
            if (fireBreathBox != null)
            {
                Vector3 hitboxWorldPos = transform.TransformPoint(fireBreathHitbox.transform.localPosition);
                Vector3 hitboxSize = new Vector3(fireBreathBox.size.x, fireBreathBox.size.y, 0.2f);
                Gizmos.DrawWireCube(hitboxWorldPos, hitboxSize);
                
                // ���hitbox�����ʵ�ķ�����ʾ
                if (fireBreathHitbox.activeInHierarchy)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                    Gizmos.DrawCube(hitboxWorldPos, hitboxSize);
                }
            }
        }
        
        // �ڶ��׶α��
        if (isInPhase2)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        }
        
        // ��ʾ��ս���״̬
        if (Application.isPlaying && meleeAttackPoint != null)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(meleeAttackPoint.position + Vector3.up * 1f, 
                $"��ս��Χ: {(isPlayerInMeleeRange ? "��" : "��")}");
#endif
        }
    }
    
    /// <summary>
    /// �������ʱ����
    /// </summary>
    private void OnDestroy()
    {
        // ȡ������BossLife�¼�
        if (bossLife != null)
        {
            bossLife.OnHealthChanged -= OnBossLifeHealthChanged;
            bossLife.OnDeath -= OnBossLifeDeath;
        }
    }
    
    #region ���ԺͲ��Է���
    
    /// <summary>
    /// �������˶���ϵͳ
    /// </summary>
    [ContextMenu("�������˶���ϵͳ")]
    public void TestHurtAnimationSystem()
    {
        if (IsDead)
        {
            Debug.LogWarning("Boss���������޷��������˶���ϵͳ");
            return;
        }
        
        Debug.Log("=== ���˶���ϵͳ���Կ�ʼ ===");
        Debug.Log($"��ǰѪ��: {CurrentHealth}/{MaxHealth} ({HealthPercentage * 100:F1}%)");
        Debug.Log($"���˶�����ֵ: {hurtAnimationHealthThreshold * 100:F1}%");
        Debug.Log($"�ϴ����˶���Ѫ��: {lastHurtAnimationHealth * 100:F1}%");
        
        // ������Ҫ�����˺����ܴ�����һ�����˶���
        float healthNeededToTrigger = hurtAnimationHealthThreshold * MaxHealth;
        int damageNeeded = Mathf.CeilToInt(healthNeededToTrigger);
        
        Debug.Log($"��Ҫ {damageNeeded} ���˺�������һ�����˶���");
        
        // ����˺�
        TakeDamage(damageNeeded);
        
        Debug.Log("=== ���˶���ϵͳ������� ===");
    }
    
    /// <summary>
    /// ���Զ��С�˺�
    /// </summary>
    [ContextMenu("���Զ��С�˺�")]
    public void TestMultipleSmallDamage()
    {
        if (IsDead)
        {
            Debug.LogWarning("Boss���������޷�����");
            return;
        }
        
        Debug.Log("=== ���С�˺����Կ�ʼ ===");
        
        StartCoroutine(SmallDamageTestCoroutine());
    }
    
    /// <summary>
    /// С�˺�����Э��
    /// </summary>
    private IEnumerator SmallDamageTestCoroutine()
    {
        int smallDamage = Mathf.RoundToInt(MaxHealth * 0.05f); // 5%��Ѫ��
        
        Debug.Log($"ÿ����� {smallDamage} ���˺� (Լ{(float)smallDamage / MaxHealth * 100:F1}%Ѫ��)");
        
        for (int i = 1; i <= 8; i++)
        {
            if (IsDead) break;
            
            Debug.Log($"--- �� {i} �ι��� ---");
            TakeDamage(smallDamage);
            
            yield return new WaitForSeconds(1f);
        }
        
        Debug.Log("=== ���С�˺�������� ===");
    }
    
    /// <summary>
    /// �������˶���ϵͳ
    /// </summary>
    [ContextMenu("�������˶���ϵͳ")]
    public void ResetHurtAnimationSystem()
    {
        lastHurtAnimationHealth = HealthPercentage;
        Debug.Log($"���˶���ϵͳ�����ã���ǰѪ���ٷֱ�: {HealthPercentage * 100:F1}%");
    }
    
    /// <summary>
    /// ��ʾ���˶���ϵͳ״̬
    /// </summary>
    [ContextMenu("��ʾ���˶���ϵͳ״̬")]
    public void ShowHurtAnimationSystemStatus()
    {
        Debug.Log("=== ���˶���ϵͳ״̬ ===");
        Debug.Log($"��ǰѪ��: {CurrentHealth}/{MaxHealth} ({HealthPercentage * 100:F1}%)");
        Debug.Log($"�ϴ����˶���Ѫ��: {lastHurtAnimationHealth * 100:F1}%");
        Debug.Log($"�����´����˶���: {(lastHurtAnimationHealth - HealthPercentage) * 100:F1}% / {hurtAnimationHealthThreshold * 100:F1}%");
        Debug.Log($"���˶�����ֵ: {hurtAnimationHealthThreshold * 100:F1}% HP");
        
        float progressToNextHurt = (lastHurtAnimationHealth - HealthPercentage) / hurtAnimationHealthThreshold;
        Debug.Log($"�´����˶�������: {progressToNextHurt * 100:F1}%");
        
        if (progressToNextHurt >= 1f)
        {
            Debug.Log("?? �Ѵﵽ���˶�������������");
        }
    }
    
    /// <summary>
    /// ǿ�ƴ������˶���
    /// </summary>
    [ContextMenu("ǿ�ƴ������˶���")]
    public void ForceHurtAnimation()
    {
        if (IsDead)
        {
            Debug.LogWarning("Boss���������޷��������˶���");
            return;
        }
        
        Debug.Log("ǿ�ƴ������˶���");
        StartCoroutine(HurtCoroutine());
        
        // ���¼�¼
        lastHurtAnimationHealth = HealthPercentage;
    }
    
    /// <summary>
    /// ���Խ�ս���ϵͳ
    /// </summary>
    [ContextMenu("���Խ�ս���ϵͳ")]
    public void TestMeleeDetectionSystem()
    {
        Debug.Log("=== ��ս���ϵͳ���� ===");
        Debug.Log($"��ս������λ��: {(meleeAttackPoint != null ? meleeAttackPoint.position : Vector3.zero)}");
        Debug.Log($"��ս���뾶: {meleeRangeRadius}");
        Debug.Log($"�����: {meleeDetectionInterval}��");
        Debug.Log($"��Ҳ㼶: {playerLayer.value}");
        Debug.Log($"��ǰ����ڽ�ս��Χ��: {isPlayerInMeleeRange}");
        Debug.Log($"�����ϴμ��ʱ��: {Time.time - lastMeleeDetectionTime:F2}��");
        
        // �ֶ�ִ��һ�μ��
        bool currentResult = CheckMeleeRange();
        Debug.Log($"�ֶ������: {currentResult}");
        
        if (player != null && meleeAttackPoint != null)
        {
            float actualDistance = Vector2.Distance(meleeAttackPoint.position, player.position);
            Debug.Log($"ʵ�ʾ���: {actualDistance:F2} (���뾶: {meleeRangeRadius})");
        }
    }
    
    /// <summary>
    /// ǿ�ƴ�����̹���
    /// </summary>
    [ContextMenu("ǿ�ƴ�����̹���")]
    public void ForceDashAttack()
    {
        if (IsDead || isAttacking || isDashing)
        {
            Debug.LogWarning("Boss״̬������ִ�г�̹���");
            return;
        }
        
        Debug.Log("ǿ�ƴ�����̹���");
        StartDashAttack();
    }
    
    /// <summary>
    /// ���ý�ս���ϵͳ
    /// </summary>
    [ContextMenu("���ý�ս���ϵͳ")]
    public void ResetMeleeDetectionSystem()
    {
        lastMeleeDetectionTime = 0f;
        isPlayerInMeleeRange = false;
        Debug.Log("��ս���ϵͳ������");
    }
    
    /// <summary>
    /// ��ʾ��ս���״̬
    /// </summary>
    [ContextMenu("��ʾ��ս���״̬")]
    public void ShowMeleeDetectionStatus()
    {
        Debug.Log("=== ��ս���״̬ ===");
        Debug.Log($"����ڽ�ս��Χ��: {isPlayerInMeleeRange}");
        Debug.Log($"�ϴμ��ʱ��: {lastMeleeDetectionTime:F2}");
        Debug.Log($"�����´μ��: {meleeDetectionInterval - (Time.time - lastMeleeDetectionTime):F2}��");
        Debug.Log($"��ǰս��״̬: {isInCombat}");
        Debug.Log($"��ǰ����״̬: {isAttacking}");
        Debug.Log($"��ǰ���״̬: {isDashing}");
        
        if (meleeAttackPoint != null && player != null)
        {
            float distance = Vector2.Distance(meleeAttackPoint.position, player.position);
            Debug.Log($"�����㵽��Ҿ���: {distance:F2}");
            Debug.Log($"�Ƿ��ڼ��뾶��: {distance <= meleeRangeRadius}");
        }
    }
    
    #endregion

    /// <summary>
    /// ���·�Ӧϵͳ�����ڶ��׶Σ�
    /// </summary>
    private void UpdateReactionSystem()
    {
        if (Time.time - lastReactionTime < reactionCooldown) return;
        
        // ���Ӣ�۶�������Ӧ�ط�Ӧ
        if (heroTracker != null)
        {
            if (heroTracker.HasHeroRolled() && Random.value < slideReactionChance)
            {
                ReactToHeroSlide();
            }
            else if (heroTracker.HasHeroJumped() && Random.value < jumpReactionChance)
            {
                ReactToHeroJump();
            }
        }
    }
    
    /// <summary>
    /// ������ƶ�
    /// </summary>
    private void MoveTowardsPlayer()
    {
        if (player == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
    }
    
    /// <summary>
    /// ���ö���״̬
    /// </summary>
    /// <param name="stateValue">״ֵ̬��0=Spawn, 1=Idle, 2=Walk, 3=Dash</param>
    private void SetAnimationState(int stateValue)
    {
        if (anim != null)
        {
            anim.SetInteger("State", stateValue);
        }
    }
    
    /// <summary>
    /// ���¶���
    /// </summary>
    private void UpdateAnimations()
    {
        if (anim != null)
        {
            anim.SetBool("IsPhase2", isInPhase2);
            anim.SetBool("IsAttacking", isAttacking);
            anim.SetBool("IsSpawning", currentState == DemonState.Spawn);
        }
    }
    
    /// <summary>
    /// ���³���
    /// </summary>
    private void UpdateFacingDirection()
    {
        if (player != null && !isAttacking && !isDashing && !isWaitingForLanding)
        {
            bool shouldFaceRight = player.position.x > transform.position.x;
            if (shouldFaceRight != isFacingRight)
            {
                Flip();
            }
        }
    }

    /// <summary>
    /// ��תBoss����
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}