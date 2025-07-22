using UnityEngine;

/// <summary>
/// ��ħͶ����
/// ����Boss����ĸ���Ͷ������������ӵ���
/// </summary>
public class DemonProjectile : MonoBehaviour
{
    [Header("Ͷ��������")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private bool isContinuousDamage = false;
    [SerializeField] private float continuousDamageInterval = 0.5f;
    [SerializeField] private LayerMask targetLayer = -1;
    
    [Header("�Ӿ�Ч��")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private TrailRenderer trail;
    
    private bool hasHitTarget = false;
    private float lastDamageTime = 0f;
    private Rigidbody2D rb;
    private Collider2D col;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        
        // ȷ��Ͷ�����б�Ҫ�����
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Ͷ���ﲻ������Ӱ��
        }
        
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }
        
        // ����Ĭ��Ŀ���Ϊ���
        if (targetLayer == -1)
        {
            targetLayer = LayerMask.GetMask("Player");
        }
        
        // �Զ������Է�ֹ�ڴ�й©
        Destroy(gameObject, 10f);
    }
    
    /// <summary>
    /// ��ʼ��Ͷ����
    /// </summary>
    /// <param name="damageAmount">�˺�ֵ</param>
    /// <param name="continuous">�Ƿ�Ϊ�����˺�</param>
    public void Initialize(float damageAmount, bool continuous)
    {
        damage = damageAmount;
        isContinuousDamage = continuous;
        
        Debug.Log($"��ħͶ�����ʼ�� - �˺�: {damage}, ����: {continuous}");
    }
    
    void Update()
    {
        // ��������˺�
        if (isContinuousDamage && hasHitTarget)
        {
            if (Time.time - lastDamageTime >= continuousDamageInterval)
            {
                DealDamageToPlayer();
                lastDamageTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// ��������ײ���
    /// </summary>
    /// <param name="other">��ײ����</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"��ħͶ�������: {other.name}, �㼶: {other.gameObject.layer}, ��ǩ: {other.tag}");
        
        // ����Ƿ�������
        if (IsPlayer(other))
        {
            if (!isContinuousDamage)
            {
                // �����˺�
                DealDamageToPlayer(other);
                CreateHitEffect();
                DestroyProjectile();
            }
            else
            {
                // ��ʼ�����˺�
                hasHitTarget = true;
                lastDamageTime = Time.time;
                DealDamageToPlayer(other);
                CreateHitEffect();
                
                // ���ڳ����˺������ŵ��������һ��ʱ��
                transform.SetParent(other.transform);
                
                // �ڳ����˺�����ʱ�������
                Destroy(gameObject, 2f);
            }
        }
        else if (IsEnvironment(other))
        {
            // ���л�����������ըЧ��������
            CreateExplosionEffect();
            DestroyProjectile();
        }
    }
    
    /// <summary>
    /// ������������ײ���
    /// </summary>
    /// <param name="other">��ײ����</param>
    void OnTriggerStay2D(Collider2D other)
    {
        // �ڴ������ڳ���ʱ��������˺�
        if (isContinuousDamage && IsPlayer(other))
        {
            if (Time.time - lastDamageTime >= continuousDamageInterval)
            {
                DealDamageToPlayer(other);
                lastDamageTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// ����Ƿ�Ϊ���
    /// </summary>
    /// <param name="other">��ײ����</param>
    /// <returns>�������ҷ���true</returns>
    private bool IsPlayer(Collider2D other)
    {
        // ����ǩ�Ͳ㼶
        return other.CompareTag("Player") || ((1 << other.gameObject.layer) & targetLayer) != 0;
    }
    
    /// <summary>
    /// ����Ƿ�Ϊ��������
    /// </summary>
    /// <param name="other">��ײ����</param>
    /// <returns>����ǻ������巵��true</returns>
    private bool IsEnvironment(Collider2D other)
    {
        return other.CompareTag("Ground") || 
               other.CompareTag("Wall") || 
               other.CompareTag("Platform") ||
               other.CompareTag("Environment");
    }
    
    /// <summary>
    /// ���������˺�
    /// </summary>
    /// <param name="playerCollider">�����ײ��</param>
    private void DealDamageToPlayer(Collider2D playerCollider = null)
    {
        GameObject player = playerCollider != null ? playerCollider.gameObject : GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            HeroLife playerLife = player.GetComponent<HeroLife>();
            if (playerLife != null)
            {
                playerLife.TakeDamage(Mathf.RoundToInt(damage));
                Debug.Log($"��ħͶ����������� {damage} ���˺�");
            }
        }
    }
    
    /// <summary>
    /// ��������Ч��
    /// </summary>
    private void CreateHitEffect()
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    /// <summary>
    /// ������ըЧ��
    /// </summary>
    private void CreateExplosionEffect()
    {
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    /// <summary>
    /// ����Ͷ����
    /// </summary>
    private void DestroyProjectile()
    {
        // ������βЧ��
        if (trail != null)
        {
            trail.enabled = false;
        }
        
        // ������ײ���Է�ֹ��һ����ײ
        if (col != null)
        {
            col.enabled = false;
        }
        
        // ֹͣ�ƶ�
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // ��������
        Destroy(gameObject);
    }
    
    /// <summary>
    /// ��Ͷ�����Ϊ���ɼ�ʱ����
    /// </summary>
    void OnBecameInvisible()
    {
        // Ͷ�����뿪��Ļʱ����
        if (!isContinuousDamage) // ���Զ����ٳ����˺�Ͷ����
        {
            Destroy(gameObject);
        }
    }
}