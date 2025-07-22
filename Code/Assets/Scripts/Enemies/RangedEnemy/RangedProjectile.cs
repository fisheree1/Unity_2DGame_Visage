using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedProjectile : MonoBehaviour
{
    private int damage;
    private LayerMask targetLayer;
    private bool hasHitTarget = false;
    
    public void Initialize(int damage, LayerMask targetLayer)
    {
        this.damage = damage;
        this.targetLayer = targetLayer;
        
        Debug.Log($"Ranged projectile initialized - Damage: {damage}");
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitTarget) return;
        
        Debug.Log($"Ranged projectile hit: {collision.name}, Layer: {collision.gameObject.layer}, Tag: {collision.tag}");
        
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
                Debug.Log($"Ranged projectile dealt {damage} damage to player");
                
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
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall") || collision.CompareTag("Platform"))
        {
            // 击中环境，销毁弹射物
            Debug.Log("Ranged projectile hit environment, destroying");
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