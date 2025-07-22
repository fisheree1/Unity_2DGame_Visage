using UnityEngine;

public class BossAttackHitBox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackUpward = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float hitboxOffset = 1.5f; // 攻击范围偏移量

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    // Components
    private BossMove bossMove;
    private BossAttack bossAttack;
    private BoxCollider2D hitboxCollider;
    
    // State
    private bool isActive = false;

    void Start()
    {
        // 获取父对象的组件
        bossMove = GetComponentInParent<BossMove>();
        bossAttack = GetComponentInParent<BossAttack>();
        hitboxCollider = GetComponent<BoxCollider2D>();
        
        // 确保碰撞体是Trigger
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
        }
        
        // 设置玩家层
        if (playerLayer == 0)
        {
            playerLayer = LayerMask.GetMask("Player");
        }
        
        // 确保这个GameObject是Boss的子对象
        if (transform.parent == null)
        {
            Debug.LogError("BossAttackHitBox must be a child of the Boss GameObject!");
        }
        
        // 初始时禁用hitbox
        SetActive(false);
        
        Debug.Log($"BossAttackHitBox initialized. Parent: {transform.parent?.name}, Local Position: {transform.localPosition}");
    }

    void Update()
    {
        // 跟随Boss的朝向 - 只在激活时更新位置以提高性能
        if (isActive)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        if (bossMove == null) return;

        // 根据Boss朝向调整hitbox位置
        Vector3 localPos = transform.localPosition;
        float targetX = bossMove.IsFacingRight ? hitboxOffset : -hitboxOffset;
        
        // 更新位置（包括朝向变化）
        if (Mathf.Abs(localPos.x - targetX) > 0.01f)
        {
            transform.localPosition = new Vector3(targetX, localPos.y, localPos.z);
            Debug.Log($"Updated hitbox position to: {transform.localPosition}, Boss facing right: {bossMove.IsFacingRight}");
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = active;
            Debug.Log($"Boss hitbox collider enabled: {active}");
        }
        
        // 可视化调试 - 不要禁用整个GameObject，只控制碰撞体
        // gameObject.SetActive(active);
        
        Debug.Log($"Boss hitbox set active: {active}");
    }

    public void ActivateForDuration(float duration)
    {
        // 激活前更新位置确保正确朝向
        UpdatePosition();
        
        SetActive(true);
        Debug.Log($"Boss attack hitbox activated for {duration} seconds at position: {transform.position}");
        
        // 使用协程而不是Invoke，更可控
        StartCoroutine(DeactivateAfterDelay(duration));
    }

    private System.Collections.IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Deactivate();
    }

    private void Deactivate()
    {
        SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) 
        {
            Debug.Log("Boss hitbox not active, ignoring collision");
            return;
        }

        Debug.Log($"Boss hitbox collision detected with: {other.name}, tag: {other.tag}");

        // 检查是否是玩家 - 使用多种方式检测
        bool isPlayer = other.gameObject.tag == "Player" || 
                       other.gameObject.name.Contains("Hero") || 
                       other.gameObject.name.Contains("Player");

        if (isPlayer)
        {
            Debug.Log("Player detected by Boss hitbox!");
            
            HeroLife heroLife = other.GetComponent<HeroLife>();
            if (heroLife != null)
            {
                if (!heroLife.IsDead)
                {
                    Debug.Log($"Attempting to damage player with {damage} damage");
                    
                    // 造成伤害
                    heroLife.TakeDamage(damage);

                    // 击退效果
                    ApplyKnockback(other);

                    // 生成击中特效
                    SpawnHitEffect(other.transform.position);

                    Debug.Log($"Boss hitbox successfully dealt {damage} damage to player");
                }
                else
                {
                    Debug.Log("Player is already dead, no damage dealt");
                }
            }
            else
            {
                Debug.LogWarning("Player object found but HeroLife component missing!");
                
                // 尝试在父对象中查找HeroLife
                HeroLife parentHeroLife = other.GetComponentInParent<HeroLife>();
                if (parentHeroLife != null)
                {
                    Debug.Log("Found HeroLife in parent, applying damage");
                    parentHeroLife.TakeDamage(damage);
                    ApplyKnockback(other);
                    SpawnHitEffect(other.transform.position);
                }
            }

            // 击中后立即禁用hitbox防止重复伤害
            SetActive(false);
        }
        else
        {
            Debug.Log($"Non-player object detected: {other.name} with tag: {other.tag}");
        }
    }

    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null && bossMove != null)
        {
            Vector2 knockbackDirection = bossMove.IsFacingRight ? Vector2.right : Vector2.left;
            Vector2 knockbackForceVector = knockbackDirection * knockbackForce + Vector2.up * knockbackUpward;
            targetRb.AddForce(knockbackForceVector, ForceMode2D.Impulse);
        }
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, position, Quaternion.identity);
        }
    }

    // 在Scene视图中显示hitbox范围
    private void OnDrawGizmos()
    {
        if (hitboxCollider != null)
        {
            Gizmos.color = isActive ? Color.red : new Color(1, 0, 0, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(hitboxCollider.offset, hitboxCollider.size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hitboxCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(hitboxCollider.offset, hitboxCollider.size);
        }
    }
}
