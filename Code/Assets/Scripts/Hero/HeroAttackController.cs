using System.Collections;
using UnityEngine;

public class HeroAttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float upAttackDuration = 0.8f;
    [SerializeField] private float downAttackDuration = 0.6f;
    [SerializeField] private float specialAttackDuration = 1.2f;
    
    [Header("Attack Cooldowns")]
    [SerializeField] private float attackCooldown = 0.1f;
    [SerializeField] private float upAttackCooldown = 0.8f;
    [SerializeField] private float downAttackCooldown = 0.8f;
    [SerializeField] private float specialAttackCooldown = 0.2f;
    
    [Header("Combo Settings")]
    [SerializeField] private int maxComboCount = 3;
    [SerializeField] private float comboResetTime = 0.5f;
    [SerializeField] private int maxSpecialComboCount = 3;
    
    // Components
    private Animator anim;
    private HeroMovement heroMovement;
    private Rigidbody2D rb;
    private HeroStamina heroStamina;
    
    // Attack Hitboxes
    private GameObject basicAttackHitbox;
    private GameObject specialAttackHitbox;
    private GameObject upAttackHitbox;
    private GameObject downAttackHitbox;
    
    // Attack State
    public enum AttackType { None, Basic, UpAttack, DownAttack, Special }
    private AttackType currentAttackType = AttackType.None;
    private bool isAttacking = false;
    
    // Combo system
    private int comboCount = 0;
    private int specialComboCount = 0;
    private float comboResetTimer = 0f;
    
    // Cooldown timers
    private float attackTimer = 0f;
    private float upAttackTimer = 0f;
    private float downAttackTimer = 0f;
    private float specialAttackTimer = 0f;
    
    // Properties
    public bool IsAttacking => isAttacking;
    public AttackType CurrentAttackType => currentAttackType;
    public int ComboCount => comboCount;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        heroMovement = GetComponent<HeroMovement>();
        rb = GetComponent<Rigidbody2D>();
        heroStamina = GetComponent<HeroStamina>();
        
        // 查找攻击碰撞盒
        basicAttackHitbox = transform.Find("AttackHitbox")?.gameObject;
        specialAttackHitbox = transform.Find("SpecialHitbox")?.gameObject;
        upAttackHitbox = transform.Find("UpAttackHitbox")?.gameObject;
        downAttackHitbox = transform.Find("DownHitbox")?.gameObject;
        
    }
    
    void Update()
    {
        // 更新冷却时间
        UpdateCooldowns();
        
        // 更新连击重置计时器
        UpdateComboTimer();
        
        // 处理攻击输入
        if (!heroMovement.IsClimbing && !isAttacking)
        {
            HandleAttackInput();
        }
    }
    
    private void UpdateCooldowns()
    {
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (upAttackTimer > 0) upAttackTimer -= Time.deltaTime;
        if (downAttackTimer > 0) downAttackTimer -= Time.deltaTime;
        if (specialAttackTimer > 0) specialAttackTimer -= Time.deltaTime;
    }
    
    private void UpdateComboTimer()
    {
        if ((comboCount > 0 || specialComboCount > 0) && !isAttacking)
        {
            comboResetTimer -= Time.deltaTime;
            if (comboResetTimer <= 0)
            {
                ResetCombo();
            }
        }
    }
    
    private void HandleAttackInput()
    {
        // 使用SettingMenu获取自定义按键绑定
        bool wPressed = Input.GetKey(SettingMenu.GetKeyBinding("MoveUp"));
        bool sPressed = Input.GetKey(SettingMenu.GetKeyBinding("MoveDown"));
        bool jPressed = Input.GetKeyDown(SettingMenu.GetKeyBinding("Attack"));
        bool uPressed = Input.GetKeyDown(SettingMenu.GetKeyBinding("SpecialAttack"));
        
        if (!jPressed && !uPressed) return;
        
        // 特殊攻击 - U键
        if (uPressed && specialAttackTimer <= 0)
        {
            TriggerSpecialAttack();
        }
        // 向上攻击 - W+J 在地面或梯子上
        else if (jPressed && wPressed && !heroMovement.IsClimbing && upAttackTimer <= 0)
        {
            TriggerUpAttack();
        }
        // 向下攻击 - S+J 在空中
        else if (jPressed && sPressed && !heroMovement.IsGrounded() && downAttackTimer <= 0)
        {
            TriggerDownAttack();
        }
        // 普通攻击 - 单独按J键
        else if (jPressed && !wPressed && !sPressed && attackTimer <= 0)
        {
            TriggerBasicAttack();
        }
    }
    
    private void TriggerBasicAttack()
    {
        // 检查体力是否足够
        if (heroStamina != null && !heroStamina.CanPerformBasicAttack())
        {
            Debug.Log("Not enough stamina for basic attack!");
            return;
        }
        
        currentAttackType = AttackType.Basic;
        isAttacking = true;
        StopHorizontalMovement();
        attackTimer = attackCooldown;
        
        // 消耗体力
        if (heroStamina != null)
        {
            heroStamina.ConsumeBasicAttackStamina();
        }
        
        // 增加连击数
        comboCount++;
        comboResetTimer = comboResetTime;
        
        // 设置动画参数
        anim.SetTrigger("Attack");
        anim.SetInteger("AttackComboStep", comboCount);
        
        // 激活基础攻击碰撞盒
        if (basicAttackHitbox != null)
        {
            StartCoroutine(ActivateHitboxTemporarily(basicAttackHitbox, 0.1f, 0.3f));
        }
        
        // 限制最大连击数
        if (comboCount >= maxComboCount)
        {
            comboCount = 0;
        }
        
        Debug.Log($"Basic Attack - Combo: {comboCount}");
                
        StartCoroutine(AttackCoroutine(attackDuration));
    }
    
    private void TriggerUpAttack()
    {
        // 检查体力是否足够
        if (heroStamina != null && !heroStamina.CanPerformUpAttack())
        {
            Debug.Log("Not enough stamina for up attack!");
            return;
        }
        
        currentAttackType = AttackType.UpAttack;
        isAttacking = true;
        upAttackTimer = upAttackCooldown;
        
        // 消耗体力
        if (heroStamina != null)
        {
            heroStamina.ConsumeUpAttackStamina();
        }
        
        anim.SetTrigger("UpAttack");
        
        // 使用专门的向上攻击碰撞盒
        if (upAttackHitbox != null)
        {
            StartCoroutine(ActivateHitboxTemporarily(upAttackHitbox, 0.1f, 0.4f));
        }       
        
        StartCoroutine(AttackCoroutine(upAttackDuration));
    }
    
    private void TriggerDownAttack()
    {
        // 检查体力是否足够
        if (heroStamina != null && !heroStamina.CanPerformDownAttack())
        {
            Debug.Log("Not enough stamina for down attack!");
            return;
        }
        
        currentAttackType = AttackType.DownAttack;
        isAttacking = true;
        downAttackTimer = downAttackCooldown;
        
        // 消耗体力
        if (heroStamina != null)
        {
            heroStamina.ConsumeDownAttackStamina();
        }
        
        anim.SetTrigger("DownAttack");
        
        // 使用专门的向下攻击碰撞盒
        if (downAttackHitbox != null)
        {
            StartCoroutine(ActivateHitboxTemporarily(downAttackHitbox, 0.1f, 0.4f));
        }
        
        StartCoroutine(AttackCoroutine(downAttackDuration));
    }
    
    private void TriggerSpecialAttack()
    {
        // 检查体力是否足够
        if (heroStamina != null && !heroStamina.CanPerformSpecialAttack())
        {
            Debug.Log("Not enough stamina for special attack!");
            return;
        }
        
        currentAttackType = AttackType.Special;
        isAttacking = true;
        StopHorizontalMovement();
        specialAttackTimer = specialAttackCooldown;
        
        // 消耗体力
        if (heroStamina != null)
        {
            heroStamina.ConsumeSpecialAttackStamina();
        }
        
        // 增加特殊攻击连击数
        specialComboCount++;
        comboResetTimer = comboResetTime;
        
        // 设置动画参数
        anim.SetTrigger("SpecialAttack");
        anim.SetInteger("AttackComboStep", specialComboCount);
        
        // 激活特殊攻击碰撞盒
        if (specialAttackHitbox != null)
        {
            StartCoroutine(ActivateHitboxTemporarily(specialAttackHitbox, 0.2f, 0.5f));
        }
        
        // 限制最大连击数
        if (specialComboCount >= maxSpecialComboCount)
        {
            specialComboCount = 0;
        }
        
        Debug.Log($"Special Attack - Combo: {specialComboCount}");
        
        StartCoroutine(AttackCoroutine(specialAttackDuration));
    }
    
    // 临时激活攻击碰撞盒的协程
    private IEnumerator ActivateHitboxTemporarily(GameObject hitbox, float startDelay, float duration)
    {
        if (hitbox == null) yield break;
        
        // 等待攻击动画开始
        yield return new WaitForSeconds(startDelay);
        
        // 激活攻击碰撞盒 - 使用AttackHitbox组件
        AttackHitbox attackHitbox = hitbox.GetComponent<AttackHitbox>();
        if (attackHitbox != null)
        {
            attackHitbox.SetHitboxActive(true);
            Debug.Log("Attack hitbox activated via AttackHitbox component");
        }  
        // 等待攻击持续时间
        yield return new WaitForSeconds(duration);
        
        // 关闭攻击碰撞盒
        if (attackHitbox != null)
        {
            attackHitbox.SetHitboxActive(false);
            Debug.Log("Attack hitbox deactivated via AttackHitbox component");
        }

    }

    private void StopHorizontalMovement()
    {
        if (rb != null)
            rb.velocity = new Vector2(0, rb.velocity.y);
    }
    
    private void ResetCombo()
    {
        comboCount = 0;
        specialComboCount = 0;
        comboResetTimer = 0f;
        
        // 重置动画参数
        anim.SetInteger("AttackComboStep", 0);
        
        Debug.Log("Combo reset!");
    }
    
    private IEnumerator AttackCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        isAttacking = false;
        currentAttackType = AttackType.None;
        
        Debug.Log($"Attack finished!");
    }
    
    
    // 供动画事件调用的函数
    public void AttackOver()
    {
        isAttacking = false;
        currentAttackType = AttackType.None;
        Debug.Log("Attack finished via animation event!");
    }
    
    // 公共方法供其他脚本调用
    public bool CanMove()
    {
        return !isAttacking;
    }
    public bool CanJump()
    {
        // 只有在攻击过程中才不能跳跃，攻击结束后立即可以跳跃
        return !isAttacking;
    }
    public bool CanClimb()
    {
        // 攻击时不能攀爬
        return !isAttacking;
    }
    
    public float GetMovementSpeedMultiplier()
    {
        if (!isAttacking) return 1f;
        
        switch (currentAttackType)
        {
            case AttackType.Basic:
                return 0.3f; // 基础攻击时移动速度降低
            case AttackType.UpAttack:
            case AttackType.DownAttack:
            case AttackType.Special:
                return 0f; // 其他攻击时不能移动
            default:
                return 1f;
        }
    }
}
