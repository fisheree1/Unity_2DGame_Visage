using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Stone����״̬����ö��
/// ����Stone���˵����п���״̬
/// </summary>
public enum StoneStateType
{
    Idle,       // ����״̬�����˾�ֹ�������ȴ���ҽ�����Ұ
    Walk,       // ����״̬�����˻����ƶ�Ѳ��
    React,      // ��Ӧ״̬����⵽��Һ��׼���׶Σ�0.3�룩
    Attack,     // ����״̬���ͷ�һ�������Ĺ�������
    Defence,    // ����״̬���ܵ�3���˺�����룬���������˺�2��
    Dead        // ����״̬�����˱�����
}

/// <summary>
/// Stone���˲���������
/// ����Stone���˵����пɵ��ڲ�����״̬����
/// </summary>
[System.Serializable]
public class StoneParameter
{
    [Header("Movement Settings - �ƶ�����")]
    [Tooltip("Stone�������ٶ�")]
    public float walkSpeed = 2f;
    [Tooltip("Stone��Ѳ�ߵ����飬���Ϊ����ʹ���������Ѳ��")]
    public Transform[] patrolPoints;
    [Tooltip("��Ѳ�ߵ�ͣ����ʱ�䣨�룩")]
    public float idleAtPatrolPointTime = 2f;
    
    [Header("Sight Settings - ��Ұ����")]
    [Tooltip("���ڼ����ҵ�������sight")]
    public Transform sightObject;
    
    [Header("React Settings - ��Ӧ����")]
    [Tooltip("��⵽��Һ�ķ�Ӧʱ�䣨�룩")]
    public float reactDuration = 0.3f;
    
    [Header("Attack Settings - ��������")]
    [Tooltip("������������ʼ��")]
    public Transform attackPoint;
    [Tooltip("����Ŀ���ͼ��")]
    public LayerMask targetLayer;
    [Tooltip("������������")]
    public float beamMaxDistance = 10f;
    [Tooltip("�����Ŀ��")]
    public float beamWidth = 0.5f;
    [Tooltip("������ʾ����Ч����LineRenderer")]
    public LineRenderer beamRenderer;
    [Tooltip("��������Ч��Ԥ����")]
    public GameObject beamImpactEffect;
    
    [Header("Defence Settings - ��������")]
    [Tooltip("�������״̬������ܻ�����")]
    public int hitsToDefence = 3;
    [Tooltip("����״̬����ʱ�䣨�룩")]
    public float defenceDuration = 2f;
    
    [Header("Animation - ��������")]
    [Tooltip("����������")]
    public Animator animator;
    
    [Header("State Tracking - ״̬����")]
    [Tooltip("��ǰĿ�꣨��ң�")]
    public Transform target;
    [Tooltip("�Ƿ����ڷ�Ӧ��")]
    public bool isReacting = false;
    [Tooltip("�Ƿ񱻻���")]
    public bool isHit = false;
    [Tooltip("��ǰ�ܻ�����")]
    public int hitCount = 0;
    [Tooltip("�Ƿ��ڷ���״̬")]
    public bool isInDefence = false;
    
    [Header("Audio - ��Ƶ����")]
    [Tooltip("��ƵԴ���")]
    public AudioSource audioSource;
    [Tooltip("����������Ч")]
    public AudioClip beamAttackSound;
    [Tooltip("����״̬��Ч")]
    public AudioClip defenceSound;
}

/// <summary>
/// Stone������������
/// ����Stone���˵�״̬�����������Ϊ�߼�
/// 
/// Stone�����ص㣺
/// - ��������sight���ڼ�����
/// - ��⵽���ʱ����React״̬��0.3��׼��ʱ�䣩
/// - ����Ϊһ�������Ĺ�������
/// - �ܵ�3���˺������Defence״̬�����������˺�2��
/// - ֧��Animator���������ж�������
/// </summary>
public class StoneP : MonoBehaviour
{
    [Header("Components - �������")]
    [Tooltip("Stone���˵����в�������")]
    public StoneParameter parameter;
    
    // ״̬�����
    private IState currentState;
    private Dictionary<StoneStateType, IState> states = new Dictionary<StoneStateType, IState>();
    
    // �������
    private Rigidbody2D rb;
    private Collider2D col;
    private EnemyLife enemyLife;
    private CinemachineImpulseSource impulseSource;
    
    // ���Է�����
    public int CurrentHealth => enemyLife?.CurrentHealth ?? 0;
    public int MaxHealth => enemyLife?.MaxHealth ?? 0;
    public bool IsDead => enemyLife?.IsDead ?? false;

    /// <summary>
    /// ��ʼ��Stone����
    /// </summary>
    void Start()
    {
        InitializeComponents();
        InitializeStates();
        
        // Ĭ�Ͻ���Idle״̬
        TransitionState(StoneStateType.Idle);
        
        Debug.Log("Stone���˳�ʼ����ɣ�");
    }

    /// <summary>
    /// ��ʼ�����б�Ҫ�����
    /// </summary>
    private void InitializeComponents()
    {
        // ��ȡ�������
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        enemyLife = GetComponent<EnemyLife>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        
        // ��ʼ������
        if (parameter.audioSource == null)
            parameter.audioSource = GetComponent<AudioSource>();
        
        if (parameter.animator == null)
            parameter.animator = GetComponent<Animator>();
        
        // ����Ҫ���
        if (parameter.sightObject == null)
            Debug.LogWarning("Stone: ȱ��sight�����壡����Inspector��ָ��sightObject��");
        
        if (parameter.attackPoint == null)
            Debug.LogWarning("Stone: ȱ�ٹ����㣡����Inspector��ָ��attackPoint��");
        
        if (parameter.beamRenderer == null)
            Debug.LogWarning("Stone: ȱ��LineRenderer���������Inspector��ָ��beamRenderer��");
    }

    /// <summary>
    /// ��ʼ��״̬��
    /// </summary>
    private void InitializeStates()
    {
        states[StoneStateType.Idle] = new StoneIdleState(this, parameter);
        states[StoneStateType.Walk] = new StoneWalkState(this, parameter);
        states[StoneStateType.React] = new StoneReactState(this, parameter);
        states[StoneStateType.Attack] = new StoneAttackState(this, parameter);
        states[StoneStateType.Defence] = new StoneDefenceState(this, parameter);
        states[StoneStateType.Dead] = new StoneDeadState(this, parameter);
    }

    /// <summary>
    /// ÿ֡����
    /// </summary>
    void Update()
    {
        // �����������ֹͣ���и���
        if (IsDead) return;
        
        // ���µ�ǰ״̬
        currentState?.OnUpdate();
    }

    /// <summary>
    /// ״̬ת������
    /// </summary>
    /// <param name="type">Ŀ��״̬����</param>
    public void TransitionState(StoneStateType type)
    {
        // �����������ֻ����ת��������״̬
        if (IsDead && type != StoneStateType.Dead)
        {
            Debug.LogWarning($"Stone: ���Դ�����״̬ת���� {type}�����������ԡ�");
            return;
        }
        
        // �˳���ǰ״̬
        currentState?.OnExit();
        
        // ������״̬
        currentState = states[type];
        currentState?.OnEnter();
        
        Debug.Log($"Stone״̬ת��: �� {type}");
    }

    /// <summary>
    /// ��Stone����ָ��Ŀ��
    /// </summary>
    /// <param name="target">Ŀ��Transform</param>
    public void FlipTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("FlipTo: Ŀ��Ϊ�գ�");
            return;
        }

        Vector3 direction = target.position - transform.position;
        float newScaleX = Mathf.Sign(direction.x) < 0 ? -1f : 1f;
        transform.localScale = new Vector3(newScaleX, 1f, 1f);

        // ����������
        Debug.DrawRay(transform.position, direction, Color.red, 0.1f);
    }

    #region ��ײ���ϵͳ
    /// <summary>
    /// ������ײ�����¼�
    /// ��Ҫ����sight�����������
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ������������������κδ������¼�
        if (IsDead) return;
        
        HandleSightTrigger(other);
    }

    /// <summary>
    /// ����sight����߼�
    /// </summary>
    private void HandleSightTrigger(Collider2D other)
    {
        GameObject hero = GameObject.FindGameObjectWithTag("Player");
        
        // �����ҽ�����Ұ
        if (other.CompareTag("Player"))
        {
            parameter.target = other.transform;
            
            // ���ݵ�ǰ״̬����ת��
            if (currentState is StoneIdleState || currentState is StoneWalkState)
            {
                TransitionState(StoneStateType.React);
            }
            
            Debug.Log("Stone��⵽��ң����뷴Ӧ״̬��");
        }
        
        // �����ҹ���
        if (other.CompareTag("PlayerAttack"))
        {
            AttackHitbox attackHitbox = other.GetComponent<AttackHitbox>();
            if (attackHitbox != null)
            {
                int damage = attackHitbox.damage;
                TakeDamage(damage);
                
                // �������Ч��
                if (hero != null)
                {
                    Vector2 knockbackDirection = hero.transform.localRotation.y == 0 ? Vector2.right : Vector2.left;
                    GetComponent<Enemy>()?.GetHit(knockbackDirection);
                }
            }
            else
            {
                Debug.LogWarning("��⵽PlayerAttack��û��AttackHitbox�����");
            }
        }
    }

    /// <summary>
    /// ������ײ�˳��¼�
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        // ������������������κδ������¼�
        if (IsDead) return;
        
        if (other.CompareTag("Player"))
        {
            // ����뿪��Ұ��Ĵ���
            parameter.target = null;
            
            // ������ڷ�Ӧ�򹥻�״̬���ص�Idle
            if (currentState is StoneReactState || currentState is StoneAttackState)
            {
                TransitionState(StoneStateType.Idle);
            }
            
            Debug.Log("����뿪Stone��Ұ");
        }
    }
    #endregion

    #region �˺�ϵͳ
    /// <summary>
    /// �ܵ��˺�����
    /// Stone������ƣ��ܵ�3���˺���������״̬
    /// </summary>
    /// <param name="damage">�˺�ֵ</param>
    public void TakeDamage(int damage)
    {
        // ���������״̬�в����˺�
        if (IsDead || parameter.isInDefence) 
        {
            if (parameter.isInDefence)
                Debug.Log("Stone���ڷ���״̬�������˺���");
            return;
        }
        
        parameter.isHit = true;
        parameter.hitCount++;
        
        // ������Ļ��
        if (impulseSource != null)
        {
            CamaraShakeManager.Instance.CamaraShake(impulseSource);
        }
        
        // ʹ��EnemyLife��������˺�
        enemyLife?.TakeDamage(damage);
        
        Debug.Log($"Stone�ܵ� {damage} ���˺�����ǰ�ܻ�����: {parameter.hitCount}/{parameter.hitsToDefence}");
        
        // ����Ƿ�����
        if (IsDead)
        {
            enabled = false; // ���ø��·�ֹ״̬����
            TransitionState(StoneStateType.Dead);
            return;
        }
        
        // ����Ƿ�ﵽ��������
        if (parameter.hitCount >= parameter.hitsToDefence)
        {
            TransitionState(StoneStateType.Defence);
        }
    }
    #endregion

    #region ����ϵͳ
    /// <summary>
    /// ִ�й�������
    /// Stone�����Ƽ��ܣ�һ�������Ĺ�������
    /// </summary>
    public void FireBeamAttack()
    {
        if (parameter.attackPoint == null || parameter.target == null) return;
        
        Vector2 attackDirection = (parameter.target.position - parameter.attackPoint.position).normalized;
        
        // ���Ź�����Ч
        if (parameter.audioSource != null && parameter.beamAttackSound != null)
        {
            parameter.audioSource.PlayOneShot(parameter.beamAttackSound);
        }
        
        // �������߼��
        RaycastHit2D hit = Physics2D.Raycast(
            parameter.attackPoint.position,
            attackDirection,
            parameter.beamMaxDistance,
            parameter.targetLayer
        );
        
        // ��ʾ����Ч��
        StartCoroutine(ShowBeamEffect(attackDirection, hit));
        
        // ��������˺�
        if (hit.collider != null)
        {
            // ����Ƿ�������
            if (hit.collider.CompareTag("Player"))
            {
                // ���Զ��ֿ��ܵ��������ֵ���
                var heroLife = hit.collider.GetComponent<HeroLife>();
                var enemyLife = hit.collider.GetComponent<EnemyLife>();
                
                if (heroLife != null)
                {
                    // ʹ��HeroLife���
                    heroLife.TakeDamage(9999); // Stone�Ĺ���������һ������
                    Debug.Log("Stone��������������ң�һ��������");
                }
                else if (enemyLife != null)
                {
                    // ������ʹ��EnemyLife���
                    enemyLife.TakeDamage(9999);
                    Debug.Log("Stone��������������ң�һ��������");
                }
                else
                {
                    // ͨ���˺�������
                    var damageReceiver = hit.collider.GetComponent<MonoBehaviour>();
                    if (damageReceiver != null)
                    {
                        // ����ͨ���������TakeDamage����
                        var method = damageReceiver.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(int) });
                        if (method != null)
                        {
                            method.Invoke(damageReceiver, new object[] { 9999 });
                            Debug.Log("Stone��������������ң�һ��������");
                        }
                        else
                        {
                            Debug.LogWarning("Stone: �޷����������˺���δ�ҵ����ʵ�����ֵ�����TakeDamage����");
                        }
                    }
                }
            }
        }
        
        Debug.Log("Stone�ͷŹ���������");
    }

    /// <summary>
    /// ��ʾ�����Ӿ�Ч��
    /// </summary>
    private IEnumerator ShowBeamEffect(Vector2 direction, RaycastHit2D hit)
    {
        if (parameter.beamRenderer == null) yield break;
        
        // ��������յ�
        Vector3 startPoint = parameter.attackPoint.position;
        Vector3 endPoint;
        
        if (hit.collider != null)
        {
            endPoint = hit.point;
            
            // �ڻ��е�������Ч
            if (parameter.beamImpactEffect != null)
            {
                GameObject effect = Instantiate(parameter.beamImpactEffect, hit.point, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        else
        {
            endPoint = startPoint + (Vector3)(direction * parameter.beamMaxDistance);
        }
        
        // ����LineRenderer
        parameter.beamRenderer.enabled = true;
        parameter.beamRenderer.startWidth = parameter.beamWidth;
        parameter.beamRenderer.endWidth = parameter.beamWidth;
        parameter.beamRenderer.positionCount = 2;
        parameter.beamRenderer.SetPosition(0, startPoint);
        parameter.beamRenderer.SetPosition(1, endPoint);
        
        // ��������ʱ��
        yield return new WaitForSeconds(0.2f);
        
        // �رչ���
        parameter.beamRenderer.enabled = false;
    }
    #endregion

    #region ���ԺͿ��ӻ�
    /// <summary>
    /// ���Ƶ�����Ϣ
    /// </summary>
    private void OnDrawGizmos()
    {
        if (parameter == null) return;
        
        // ���ƹ�����
        if (parameter.attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(parameter.attackPoint.position, 0.3f);
            
            // ���ƹ�������
            if (parameter.target != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 direction = (parameter.target.position - parameter.attackPoint.position).normalized;
                Gizmos.DrawRay(parameter.attackPoint.position, direction * parameter.beamMaxDistance);
            }
        }
        
        // ��ʾ״̬��Ϣ
        if (Application.isPlaying)
        {
            Gizmos.color = parameter.isInDefence ? Color.blue : Color.white;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2, Vector3.one * 0.5f);
        }
    }
    #endregion

    /// <summary>
    /// ����ʱ��������
    /// </summary>
    private void OnDestroy()
    {
        Debug.Log("Stone��������");
    }
}
