using System.Collections;
using UnityEngine;

public class DemonMinion : MonoBehaviour
{
    [Header("小怪属性")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float detectionRange = 8f;
    
    [Header("攻击设置")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float fanAngle = 45f;
    [SerializeField] private int projectilesPerAttack = 3;
    
    [Header("投射物设置")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private Transform firePoint;
    
    [Header("视觉效果")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject attackEffect;
    
    // 组件
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    
    // 引用
    private BossDemon masterBoss;
    private Transform player;
    private HeroLife playerLife;
    
    // 状态
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
        
        // 初始化组件
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        // 添加缺失的组件
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        if (anim == null) anim = gameObject.AddComponent<Animator>();
        
        // 设置刚体
        rb.gravityScale = 1f;
        rb.freezeRotation = true;
        
        // 设置开火点
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            firePoint = firePointObj.transform;
        }
        
        // 如果没有分配精灵，使用简单的彩色精灵
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
    /// 更新AI行为
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
    /// 设置状态
    /// </summary>
    /// <param name="newState">新状态</param>
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
    /// 巡逻行为
    /// </summary>
    private void PatrolBehavior()
    {
        // 简单巡逻：围绕生成点做小范围圆形移动
        float patrolSpeed = moveSpeed * 0.5f;
        
        Vector2 patrolDirection = new Vector2(
            Mathf.Cos(stateTimer * patrolSpeed),
            Mathf.Sin(stateTimer * patrolSpeed)
        );
        
        rb.velocity = patrolDirection * patrolSpeed;
    }
    
    /// <summary>
    /// 追踪玩家
    /// </summary>
    private void ChasePlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(directionToPlayer.x * moveSpeed, rb.velocity.y);
    }
    
    /// <summary>
    /// 攻击玩家
    /// </summary>
    private void AttackPlayer()
    {
        if (Time.time < nextAttackTime || isAttacking) return;
        
        StartCoroutine(PerformFanAttack());
    }
    
    /// <summary>
    /// 执行扇形攻击
    /// </summary>
    private IEnumerator PerformFanAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        
        // 攻击期间停止移动
        rb.velocity = Vector2.zero;
        
        // 显示攻击效果
        if (attackEffect != null)
        {
            GameObject effect = Instantiate(attackEffect, firePoint.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        // 蓄力
        yield return new WaitForSeconds(0.3f);
        
        // 发射扇形投射物
        FireFanProjectiles();
        
        // 恢复
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
    }
    
    /// <summary>
    /// 发射扇形投射物
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
    /// 创建投射物
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="direction">方向</param>
    private void CreateProjectile(Vector3 position, Vector2 direction)
    {
        GameObject projectile = null;
        
        if (projectilePrefab != null)
        {
            projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
        }
        else
        {
            // 创建简单的投射物
            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = position;
            projectile.transform.localScale = Vector3.one * 0.2f;
            
            // 添加组件
            Rigidbody2D projectileRigidbody = projectile.AddComponent<Rigidbody2D>();
            projectileRigidbody.gravityScale = 0f;
            
            // 设置为红色
            Renderer renderer = projectile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
        }
        
        // 设置投射物物理
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * projectileSpeed;
        }
        
        // 添加伤害组件
        DemonProjectile projectileScript = projectile.GetComponent<DemonProjectile>();
        if (projectileScript == null)
        {
            projectileScript = projectile.AddComponent<DemonProjectile>();
        }
        projectileScript.Initialize(attackDamage, false);
        
        // 自动销毁投射物
        Destroy(projectile, 3f);
    }
    
    /// <summary>
    /// 旋转2D向量
    /// </summary>
    /// <param name="vector">向量</param>
    /// <param name="angle">角度</param>
    /// <returns>旋转后的向量</returns>
    private Vector2 RotateVector2(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
    }
    
    /// <summary>
    /// 更新动画
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
    /// 更新朝向
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
    /// 翻转朝向
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
        
        // 更新开火点
        if (firePoint != null)
        {
            Vector3 localPos = firePoint.localPosition;
            localPos.x = -localPos.x;
            firePoint.localPosition = localPos;
        }
    }
    
    /// <summary>
    /// 创建简单精灵
    /// </summary>
    private void CreateSimpleSprite()
    {
        // 创建简单的红色方块精灵
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
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
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
            // 受伤时闪烁
            StartCoroutine(DamageFlash());
        }
    }
    
    /// <summary>
    /// 受伤闪烁效果
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
    /// 死亡
    /// </summary>
    private void Die()
    {
        isDead = true;
        SetState(MinionState.Death);
        
        // 创建死亡效果
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // 禁用碰撞器
        if (col != null)
        {
            col.enabled = false;
        }
        
        // 停止移动
        rb.velocity = Vector2.zero;
        
        // 从主Boss的小怪列表中移除
        if (masterBoss != null)
        {
            // Boss会处理清理工作
        }
        
        // 延迟销毁
        Destroy(gameObject, 2f);
    }
    
    /// <summary>
    /// 触发器碰撞检测
    /// </summary>
    /// <param name="other">碰撞对象</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 受到玩家攻击伤害
        if (other.CompareTag("Player"))
        {
            AttackHitbox playerAttack = other.GetComponent<AttackHitbox>();
            if (playerAttack != null)
            {
                // 检查攻击框是否激活
                if (playerAttack.gameObject.activeInHierarchy && other.enabled)
                {
                    // 使用默认伤害值，因为无法访问Damage属性
                    TakeDamage(15); // 默认玩家攻击伤害
                }
            }
        }
    }
    
    /// <summary>
    /// 在编辑器中绘制调试信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 开火方向
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(firePoint.position, (isFacingRight ? Vector2.right : Vector2.left) * 2f);
        }
    }
}