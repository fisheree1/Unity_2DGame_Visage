using UnityEngine;

/// <summary>
/// 英雄动作追踪器
/// 用于监控玩家的动作并通知Boss进行反应
/// </summary>
public class HeroActionTracker : MonoBehaviour
{
    private BossDemon bossDemon;
    private HeroMovement heroMovement;
    private HeroAttackController heroAttack;
    private Transform player;
    
    // 追踪变量
    private bool heroRolledThisFrame = false;
    private bool heroJumpedThisFrame = false;
    private bool heroAttackedThisFrame = false;
    
    // 上一帧状态
    private bool wasGroundedLastFrame = false;
    private bool wasAttackingLastFrame = false;
    private bool wasSlidingLastFrame = false;
    
    // 时间控制
    private float lastRollTime = 0f;
    private float lastJumpTime = 0f;
    private float lastAttackTime = 0f;
    private float actionCooldown = 0.5f; // 防止快速连续检测
    
    /// <summary>
    /// 初始化追踪器
    /// </summary>
    /// <param name="demon">Boss恶魔引用</param>
    public void Initialize(BossDemon demon)
    {
        bossDemon = demon;
        
        // 查找英雄引用
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            heroMovement = playerObj.GetComponent<HeroMovement>();
            heroAttack = playerObj.GetComponent<HeroAttackController>();
        }
        
        if (heroMovement != null)
        {
            wasGroundedLastFrame = heroMovement.IsGrounded();
            wasSlidingLastFrame = heroMovement.getIsSliding();
        }
        
        if (heroAttack != null)
        {
            wasAttackingLastFrame = heroAttack.IsAttacking;
        }
    }
    
    void Update()
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        if (heroMovement == null || heroAttack == null) return;
        
        // 重置帧标志
        heroRolledThisFrame = false;
        heroJumpedThisFrame = false;
        heroAttackedThisFrame = false;
        
        // 检查英雄动作
        CheckForRoll();
        CheckForJump();
        CheckForAttack();
        
        // 更新上一帧状态
        wasGroundedLastFrame = heroMovement.IsGrounded();
        wasAttackingLastFrame = heroAttack.IsAttacking;
        wasSlidingLastFrame = heroMovement.getIsSliding();
    }
    
    /// <summary>
    /// 检查翻滚动作
    /// </summary>
    private void CheckForRoll()
    {
        // 检测英雄开始滑行（翻滚）
        bool isCurrentlySliding = heroMovement.getIsSliding();
        
        if (isCurrentlySliding && !wasSlidingLastFrame)
        {
            // 英雄刚开始滑行/翻滚
            if (Time.time - lastRollTime > actionCooldown)
            {
                heroRolledThisFrame = true;
                lastRollTime = Time.time;
                Debug.Log("检测到英雄动作: 翻滚/滑行");
            }
        }
    }
    
    /// <summary>
    /// 检查跳跃动作
    /// </summary>
    private void CheckForJump()
    {
        // 检测英雄离开地面（跳跃）
        bool isCurrentlyGrounded = heroMovement.IsGrounded();
        
        if (wasGroundedLastFrame && !isCurrentlyGrounded && heroMovement.GetVelocityY() > 0)
        {
            // 英雄刚离开地面并有向上速度（跳跃）
            if (Time.time - lastJumpTime > actionCooldown)
            {
                heroJumpedThisFrame = true;
                lastJumpTime = Time.time;
                Debug.Log("检测到英雄动作: 跳跃");
            }
        }
    }
    
    /// <summary>
    /// 检查攻击动作
    /// </summary>
    private void CheckForAttack()
    {
        // 检测英雄开始攻击
        bool isCurrentlyAttacking = heroAttack.IsAttacking;
        
        if (isCurrentlyAttacking && !wasAttackingLastFrame)
        {
            // 英雄刚开始攻击
            if (Time.time - lastAttackTime > actionCooldown)
            {
                heroAttackedThisFrame = true;
                lastAttackTime = Time.time;
                
                // 通知Boss恶魔进行连续攻击追踪
                if (bossDemon != null)
                {
                    bossDemon.OnHeroAttack();
                }
                
                Debug.Log("检测到英雄动作: 攻击");
            }
        }
    }
    
    /// <summary>
    /// 检查英雄是否在本帧翻滚
    /// </summary>
    /// <returns>如果翻滚返回true</returns>
    public bool HasHeroRolled()
    {
        return heroRolledThisFrame;
    }
    
    /// <summary>
    /// 检查英雄是否在本帧跳跃
    /// </summary>
    /// <returns>如果跳跃返回true</returns>
    public bool HasHeroJumped()
    {
        return heroJumpedThisFrame;
    }
    
    /// <summary>
    /// 检查英雄是否在本帧攻击
    /// </summary>
    /// <returns>如果攻击返回true</returns>
    public bool HasHeroAttacked()
    {
        return heroAttackedThisFrame;
    }
    
    /// <summary>
    /// 获取距离上次翻滚的时间
    /// </summary>
    /// <returns>时间间隔</returns>
    public float TimeSinceLastRoll()
    {
        return Time.time - lastRollTime;
    }
    
    /// <summary>
    /// 获取距离上次跳跃的时间
    /// </summary>
    /// <returns>时间间隔</returns>
    public float TimeSinceLastJump()
    {
        return Time.time - lastJumpTime;
    }
    
    /// <summary>
    /// 获取距离上次攻击的时间
    /// </summary>
    /// <returns>时间间隔</returns>
    public float TimeSinceLastAttack()
    {
        return Time.time - lastAttackTime;
    }
    
    /// <summary>
    /// 检查英雄当前是否正在翻滚
    /// </summary>
    /// <returns>如果正在翻滚返回true</returns>
    public bool IsHeroRolling()
    {
        return heroMovement != null && heroMovement.getIsSliding();
    }
    
    /// <summary>
    /// 检查英雄当前是否正在跳跃
    /// </summary>
    /// <returns>如果正在跳跃返回true</returns>
    public bool IsHeroJumping()
    {
        return heroMovement != null && !heroMovement.IsGrounded() && heroMovement.GetVelocityY() > 0;
    }
    
    /// <summary>
    /// 检查英雄当前是否正在攻击
    /// </summary>
    /// <returns>如果正在攻击返回true</returns>
    public bool IsHeroAttacking()
    {
        return heroAttack != null && heroAttack.IsAttacking;
    }
    
    /// <summary>
    /// 获取当前攻击类型
    /// </summary>
    /// <returns>攻击类型</returns>
    public HeroAttackController.AttackType GetCurrentAttackType()
    {
        return heroAttack != null ? heroAttack.CurrentAttackType : HeroAttackController.AttackType.None;
    }
}