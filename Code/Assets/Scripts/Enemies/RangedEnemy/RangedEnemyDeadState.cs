using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyDeadState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;
    private float deathTimer;
    private bool hasInitialized;

    public RangedEnemyDeadState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Dead State");
        
        // 设置死亡标记
        parameter.isDead = true;
        parameter.isHit = false;
        parameter.isCharging = false;
        parameter.isAttacking = false;
        hasInitialized = false;
        
        // 重置死亡计时器
        deathTimer = 0f;
        
        // 播放死亡动画
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Death");
        }
        
        // 停止所有移动
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // 设置为运动学模式，不受物理影响
        }
        
        // 禁用碰撞体以防止进一步的交互
        Collider2D col = manager.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // 设置死亡颜色效果
        manager.SetSpriteColor(Color.gray);
        
        // 清除目标
        parameter.target = null;
        
        // 可以在这里添加死亡特效
        CreateDeathEffect();
    }

    public void OnUpdate()
    {
        // 初始化死亡效果（只执行一次）
        if (!hasInitialized)
        {
            InitializeDeathEffects();
            hasInitialized = true;
        }
        
        // 更新死亡计时器
        deathTimer += Time.deltaTime;
        
        // 死亡动画播放完毕后销毁敌人
        if (deathTimer >= 2f) // 2秒后销毁
        {
            DestroyEnemy();
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Dead State");
        // 死亡状态通常不会退出，但保留此方法以防需要
    }

    private void InitializeDeathEffects()
    {
        // 可以在这里添加死亡时的特殊效果
        Debug.Log("RangedEnemy: Initializing death effects");
        
        // 例如：播放死亡音效、创建粒子特效等
        // AudioSource audioSource = manager.GetComponent<AudioSource>();
        // if (audioSource != null && deathSound != null)
        // {
        //     audioSource.PlayOneShot(deathSound);
        // }
    }

    private void CreateDeathEffect()
    {
        // 创建简单的死亡特效
        Debug.Log("RangedEnemy: Creating death effect");
        
        // 可以在这里实例化死亡特效预制体
        // if (deathEffectPrefab != null)
        // {
        //     GameObject effect = Object.Instantiate(deathEffectPrefab, manager.transform.position, Quaternion.identity);
        //     Object.Destroy(effect, 3f);
        // }
    }

    private void DestroyEnemy()
    {
        Debug.Log("RangedEnemy: Destroying enemy");
        
        // 可以在这里添加销毁前的清理工作
        // 例如：掉落物品、经验值、分数等
        
        // 销毁敌人GameObject
        if (manager != null)
        {
            Object.Destroy(manager.gameObject);
        }
    }
}