using System.Collections;
using UnityEngine;

public class FinalBossAttackHitBox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int damage = 30;
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackUpward = 2.5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float hitboxOffset = 2f; // 攻击范围偏移量
    
    [Header("Attack Type")]
    [SerializeField] private string attackType = "Unknown"; // "Punch" or "CrouchKick"
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    // Components
    private FinalBossMove finalBossMove;
    private FinalBossAttack finalBossAttack;
    private Collider2D hitBoxCollider;
    
    // State
    private bool isActive = false;

    void Start()
    {
        // 获取父对象的组件
        finalBossMove = GetComponentInParent<FinalBossMove>();
        finalBossAttack = GetComponentInParent<FinalBossAttack>();
        hitBoxCollider = GetComponent<Collider2D>();
        
        // 确保碰撞体是Trigger
        if (hitBoxCollider != null)
        {
            hitBoxCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError("FinalBossAttackHitBox requires a Collider2D component!");
        }
        
        // 设置玩家层
        if (playerLayer == 0)
        {
            playerLayer = LayerMask.GetMask("Player");
        }
        
        // 确保这个GameObject是FinalBoss的子对象
        if (transform.parent == null)
        {
            Debug.LogError("FinalBossAttackHitBox must be a child of the FinalBoss GameObject!");
        }
        
        // 初始时禁用hitbox
        SetHitBoxActive(false);
        
        Debug.Log($"FinalBossAttackHitBox initialized. Parent: {transform.parent?.name}, Attack Type: {attackType}");
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
        if (finalBossMove == null) return;

        // 根据Boss朝向调整hitbox位置
        Vector3 localPos = transform.localPosition;
        float targetX = finalBossMove.IsFacingRight ? hitboxOffset : -hitboxOffset;
        
        // 更新位置（包括朝向变化）
        if (Mathf.Abs(localPos.x - targetX) > 0.01f)
        {
            transform.localPosition = new Vector3(targetX, localPos.y, localPos.z);
            Debug.Log($"Updated Final Boss hitbox position to: {transform.localPosition}, Boss facing right: {finalBossMove.IsFacingRight}");
        }
    }

    public void SetHitBoxActive(bool active)
    {
        isActive = active;
        if (hitBoxCollider != null)
        {
            hitBoxCollider.enabled = active;
            Debug.Log($"Final Boss hitbox ({attackType}) collider enabled: {active}");
        }
        
        Debug.Log($"Final Boss hitbox ({attackType}) set active: {active}");
    }

    public void ActivateForDuration(float duration)
    {
        if (!isActive)
        {
            StartCoroutine(ActivateHitBoxCoroutine(duration));
        }
    }

    private IEnumerator ActivateHitBoxCoroutine(float duration)
    {
        // 激活前更新位置确保正确朝向
        UpdatePosition();
        
        SetHitBoxActive(true);
        Debug.Log($"Final Boss {attackType} hitbox activated for {duration} seconds at position: {transform.position}");
        
        yield return new WaitForSeconds(duration);
        
        SetHitBoxActive(false);
        Debug.Log($"Final Boss {attackType} hitbox deactivated");
    }

    public void SetAttackTypeAndDamage(string type, int damageAmount)
    {
        attackType = type;
        damage = damageAmount;
        Debug.Log($"Final Boss hitbox set to {type} with {damageAmount} damage");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) 
        {
            Debug.Log($"Final Boss {attackType} hitbox not active, ignoring collision");
            return;
        }

        Debug.Log($"Final Boss {attackType} hitbox collision detected with: {other.name}, tag: {other.tag}");

        // 检查是否是玩家 - 使用多种方式检测
        bool isPlayer = other.gameObject.tag == "Player" || 
                       other.gameObject.name.Contains("Hero") || 
                       other.gameObject.name.Contains("Player");

        if (isPlayer)
        {
            Debug.Log($"Player detected by Final Boss {attackType} hitbox!");
            
            HeroLife heroLife = other.GetComponent<HeroLife>();
            if (heroLife != null)
            {
                if (!heroLife.IsDead)
                {
                    Debug.Log($"Attempting to damage player with {damage} damage from {attackType} attack");
                    
                    // 造成伤害
                    heroLife.TakeDamage(damage);

                    // 击退效果
                    ApplyKnockback(other);

                    // 生成击中特效
                    SpawnHitEffect(other.transform.position);

                    Debug.Log($"Final Boss {attackType} hitbox successfully dealt {damage} damage to player");
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
            SetHitBoxActive(false);
        }
        else
        {
            Debug.Log($"Non-player object detected: {other.name} with tag: {other.tag}");
        }
    }

    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null && finalBossMove != null)
        {
            Vector2 knockbackDirection = finalBossMove.IsFacingRight ? Vector2.right : Vector2.left;
            Vector2 knockbackForceVector = knockbackDirection * knockbackForce + Vector2.up * knockbackUpward;
            targetRb.AddForce(knockbackForceVector, ForceMode2D.Impulse);
            
            Debug.Log($"Applied knockback to player: {knockbackForceVector}");
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
        if (hitBoxCollider != null)
        {
            Gizmos.color = isActive ? Color.red : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (hitBoxCollider is BoxCollider2D boxCollider)
            {
                Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
            }
            else if (hitBoxCollider is CircleCollider2D circleCollider)
            {
                Gizmos.DrawWireSphere(circleCollider.offset, circleCollider.radius);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hitBoxCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (hitBoxCollider is BoxCollider2D boxCollider)
            {
                Gizmos.DrawCube(boxCollider.offset, boxCollider.size);
            }
            else if (hitBoxCollider is CircleCollider2D circleCollider)
            {
                Gizmos.DrawSphere(circleCollider.offset, circleCollider.radius);
            }
        }
    }
}
