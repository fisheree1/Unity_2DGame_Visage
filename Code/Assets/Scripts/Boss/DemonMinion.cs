using System.Collections;
using UnityEngine;

public class DemonMinion : MonoBehaviour
{
    [Header("С������")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float detectionRange = 8f;
    
    [Header("��������")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float fanAngle = 45f;
    [SerializeField] private int projectilesPerAttack = 3;
    
    [Header("Ͷ��������")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private Transform firePoint;
    
    [Header("�Ӿ�Ч��")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject attackEffect;
    
    // ���
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    
    // ����
    private BossDemon masterBoss;
    private Transform player;
    private HeroLife playerLife;
    
    // ״̬
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isFacingRight = true;
    private float nextAttackTime = 0f;
    
    // AI States
    private enum MinionState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Death
    }
    
    private MinionState currentState = MinionState.Idle;
    private float stateTimer = 0f;
    
    public void Initialize(BossDemon boss, Transform playerTarget)
    {
        masterBoss = boss;
        player = playerTarget;
        
        if (player != null)
        {
            playerLife = player.GetComponent<HeroLife>();
        }
        
        currentHealth = maxHealth;
        
        // ��ʼ�����
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        // ���ȱʧ�����
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        if (anim == null) anim = gameObject.AddComponent<Animator>();
        
        // ���ø���
        rb.gravityScale = 1f;
        rb.freezeRotation = true;
        
        // ���ÿ����
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            firePoint = firePointObj.transform;
        }
        
        // ���û�з��侫�飬ʹ�ü򵥵Ĳ�ɫ����
        if (spriteRenderer.sprite == null)
        {
            CreateSimpleSprite();
        }
        
        SetState(MinionState.Patrol);
    }
    
    void Update()
    {
        if (isDead) return;
        
        stateTimer += Time.deltaTime;
        
        UpdateAI();
        UpdateAnimation();
        UpdateFacingDirection();
    }
    
    /// <summary>
    /// ����AI��Ϊ
    /// </summary>
    private void UpdateAI()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case MinionState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    SetState(MinionState.Chase);
                }
                else if (stateTimer > 2f)
                {
                    SetState(MinionState.Patrol);
                }
                break;
                
            case MinionState.Patrol:
                PatrolBehavior();
                if (distanceToPlayer <= detectionRange)
                {
                    SetState(MinionState.Chase);
                }
                break;
                
            case MinionState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    SetState(MinionState.Attack);
                }
                else if (distanceToPlayer > detectionRange)
                {
                    SetState(MinionState.Idle);
                }
                else
                {
                    ChasePlayer();
                }
                break;
                
            case MinionState.Attack:
                if (distanceToPlayer > attackRange)
                {
                    SetState(MinionState.Chase);
                }
                else
                {
                    AttackPlayer();
                }
                break;
        }
    }
    
    /// <summary>
    /// ����״̬
    /// </summary>
    /// <param name="newState">��״̬</param>
    private void SetState(MinionState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        
        switch (newState)
        {
            case MinionState.Idle:
                rb.velocity = Vector2.zero;
                break;
            case MinionState.Attack:
                rb.velocity = Vector2.zero;
                break;
        }
    }
    
    /// <summary>
    /// Ѳ����Ϊ
    /// </summary>
    private void PatrolBehavior()
    {
        // ��Ѳ�ߣ�Χ�����ɵ���С��ΧԲ���ƶ�
        float patrolSpeed = moveSpeed * 0.5f;
        
        Vector2 patrolDirection = new Vector2(
            Mathf.Cos(stateTimer * patrolSpeed),
            Mathf.Sin(stateTimer * patrolSpeed)
        );
        
        rb.velocity = patrolDirection * patrolSpeed;
    }
    
    /// <summary>
    /// ׷�����
    /// </summary>
    private void ChasePlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(directionToPlayer.x * moveSpeed, rb.velocity.y);
    }
    
    /// <summary>
    /// �������
    /// </summary>
    private void AttackPlayer()
    {
        if (Time.time < nextAttackTime || isAttacking) return;
        
        StartCoroutine(PerformFanAttack());
    }
    
    /// <summary>
    /// ִ�����ι���
    /// </summary>
    private IEnumerator PerformFanAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        
        // �����ڼ�ֹͣ�ƶ�
        rb.velocity = Vector2.zero;
        
        // ��ʾ����Ч��
        if (attackEffect != null)
        {
            GameObject effect = Instantiate(attackEffect, firePoint.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        // ����
        yield return new WaitForSeconds(0.3f);
        
        // ��������Ͷ����
        FireFanProjectiles();
        
        // �ָ�
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
    }
    
    /// <summary>
    /// ��������Ͷ����
    /// </summary>
    private void FireFanProjectiles()
    {
        Vector2 baseDirection = (player.position - firePoint.position).normalized;
        float angleStep = fanAngle / (projectilesPerAttack - 1);
        float startAngle = -fanAngle / 2f;
        
        for (int i = 0; i < projectilesPerAttack; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 projectileDirection = RotateVector2(baseDirection, angle);
            
            CreateProjectile(firePoint.position, projectileDirection);
        }
    }
    
    /// <summary>
    /// ����Ͷ����
    /// </summary>
    /// <param name="position">λ��</param>
    /// <param name="direction">����</param>
    private void CreateProjectile(Vector3 position, Vector2 direction)
    {
        GameObject projectile = null;
        
        if (projectilePrefab != null)
        {
            projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
        }
        else
        {
            // �����򵥵�Ͷ����
            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = position;
            projectile.transform.localScale = Vector3.one * 0.2f;
            
            // ������
            Rigidbody2D projectileRigidbody = projectile.AddComponent<Rigidbody2D>();
            projectileRigidbody.gravityScale = 0f;
            
            // ����Ϊ��ɫ
            Renderer renderer = projectile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
        }
        
        // ����Ͷ��������
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * projectileSpeed;
        }
        
        // ����˺����
        DemonProjectile projectileScript = projectile.GetComponent<DemonProjectile>();
        if (projectileScript == null)
        {
            projectileScript = projectile.AddComponent<DemonProjectile>();
        }
        projectileScript.Initialize(attackDamage, false);
        
        // �Զ�����Ͷ����
        Destroy(projectile, 3f);
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
    /// ���¶���
    /// </summary>
    private void UpdateAnimation()
    {
        if (anim != null)
        {
            anim.SetInteger("State", (int)currentState);
            anim.SetBool("IsAttacking", isAttacking);
            anim.SetFloat("Speed", rb.velocity.magnitude);
        }
    }
    
    /// <summary>
    /// ���³���
    /// </summary>
    private void UpdateFacingDirection()
    {
        if (player != null && !isAttacking)
        {
            bool shouldFaceRight = player.position.x > transform.position.x;
            if (shouldFaceRight != isFacingRight)
            {
                Flip();
            }
        }
    }
    
    /// <summary>
    /// ��ת����
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
        
        // ���¿����
        if (firePoint != null)
        {
            Vector3 localPos = firePoint.localPosition;
            localPos.x = -localPos.x;
            firePoint.localPosition = localPos;
        }
    }
    
    /// <summary>
    /// �����򵥾���
    /// </summary>
    private void CreateSimpleSprite()
    {
        // �����򵥵ĺ�ɫ���龫��
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.red;
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = sprite;
    }
    
    /// <summary>
    /// �ܵ��˺�
    /// </summary>
    /// <param name="damage">�˺�ֵ</param>
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // ����ʱ��˸
            StartCoroutine(DamageFlash());
        }
    }
    
    /// <summary>
    /// ������˸Ч��
    /// </summary>
    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }
    
    /// <summary>
    /// ����
    /// </summary>
    private void Die()
    {
        isDead = true;
        SetState(MinionState.Death);
        
        // ��������Ч��
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // ������ײ��
        if (col != null)
        {
            col.enabled = false;
        }
        
        // ֹͣ�ƶ�
        rb.velocity = Vector2.zero;
        
        // ����Boss��С���б����Ƴ�
        if (masterBoss != null)
        {
            // Boss�ᴦ��������
        }
        
        // �ӳ�����
        Destroy(gameObject, 2f);
    }
    
    /// <summary>
    /// ��������ײ���
    /// </summary>
    /// <param name="other">��ײ����</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // �ܵ���ҹ����˺�
        if (other.CompareTag("Player"))
        {
            AttackHitbox playerAttack = other.GetComponent<AttackHitbox>();
            if (playerAttack != null)
            {
                // ��鹥�����Ƿ񼤻�
                if (playerAttack.gameObject.activeInHierarchy && other.enabled)
                {
                    // ʹ��Ĭ���˺�ֵ����Ϊ�޷�����Damage����
                    TakeDamage(15); // Ĭ����ҹ����˺�
                }
            }
        }
    }
    
    /// <summary>
    /// �ڱ༭���л��Ƶ�����Ϣ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // ��ⷶΧ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // ������Χ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // ������
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(firePoint.position, (isFacingRight ? Vector2.right : Vector2.left) * 2f);
        }
    }
}