using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingProjectile : MonoBehaviour
{
    private int damage;
    private Transform target;
    private LayerMask targetLayer;
    private bool hasHitTarget = false;
    
    [Header("Tracking Settings")]
    [SerializeField] private float trackingSpeed = 3f;
    [SerializeField] private float trackingStrength = 2f;
    [SerializeField] private float maxTrackingDistance = 10f;
    
    private Rigidbody2D rb;
    private Vector2 initialVelocity;
    
    public void Initialize(int damage, Transform target, LayerMask targetLayer)
    {
        this.damage = damage;
        this.target = target;
        this.targetLayer = targetLayer;
        
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            initialVelocity = rb.velocity;
        }
        
        Debug.Log($"Tracking projectile initialized - Damage: {damage}, Target: {target?.name}");
    }
    
    private void Update()
    {
        if (hasHitTarget || target == null) return;
        
        // 检查目标距离
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget > maxTrackingDistance)
        {
            // 目标太远，停止追踪
            return;
        }
        
        // 追踪目标
        Vector2 targetDirection = (target.position - transform.position).normalized;
        
        if (rb != null)
        {
            // 逐渐调整速度方向朝向目标
            Vector2 currentVelocity = rb.velocity;
            Vector2 desiredVelocity = targetDirection * trackingSpeed;
            
            // 使用插值来平滑追踪
            Vector2 newVelocity = Vector2.Lerp(currentVelocity, desiredVelocity, trackingStrength * Time.deltaTime);
            rb.velocity = newVelocity;
            
            // 旋转弹射物面向移动方向
            if (newVelocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitTarget) return;
        
        Debug.Log($"Tracking projectile hit: {collision.name}, Layer: {collision.gameObject.layer}, Tag: {collision.tag}");
        
        // 检查是否击中目标
        bool hitTarget = false;
        
        // 检查层级
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            hitTarget = true;
        }
        
        // 检查标签
        if (collision.CompareTag("Player"))
        {
            hitTarget = true;
        }
        
        if (hitTarget)
        {
            // 击中玩家
            HeroLife playerLife = collision.GetComponent<HeroLife>();
            if (playerLife != null)
            {
                playerLife.TakeDamage(damage);
                hasHitTarget = true;
                Debug.Log($"Tracking projectile dealt {damage} damage to player");
                
                // 可以在这里添加击中特效
                CreateHitEffect();
            }
            else
            {
                Debug.LogWarning("Hit player but no HeroLife component found!");
            }
            
            // 销毁弹射物
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            // 击中环境，销毁弹射物
            Debug.Log("Tracking projectile hit environment, destroying");
            Destroy(gameObject);
        }
    }
    
    private void CreateHitEffect()
    {
        // 创建简单的击中特效
        GameObject effect = new GameObject("HitEffect");
        effect.transform.position = transform.position;
        
        // 添加粒子效果或其他视觉效果
        // 这里可以实例化预制的特效
        
        // 简单的销毁特效
        Destroy(effect, 1f);
    }
    
    private void OnBecameInvisible()
    {
        // 当弹射物离开屏幕时销毁
        Destroy(gameObject);
    }
}