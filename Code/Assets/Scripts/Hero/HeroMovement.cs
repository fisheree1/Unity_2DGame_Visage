using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;  // 增加跳跃力度配合新的重力系统
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float ladderDetectionDistance = 0.01f;

    [SerializeField] private GameObject stone;
    [SerializeField] private GameObject stone2;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Double Jump Settings")]
    [SerializeField] private float doubleJumpForce = 10f;  
    [SerializeField] private bool doubleJumpUnlocked = false;

    [Header("Slide Settings")]
    [SerializeField] private float slideSpeed = 16f;
    [SerializeField] private float slideDuration = 1f;
    [SerializeField] private bool slideUnlocked = false;

    [Header("Capsule Collider Settings")]
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private Vector2 slideSize = new Vector2(0.13f, 0.2f);
    [SerializeField] private Vector2 slideOffset = new Vector2(0f, -0.1f);

    [Header("Better Jump Settings")]
    [SerializeField] private float fallMultiplier = 4f;  // 下落时重力倍数
    [SerializeField] private float lowJumpMultiplier = 2f;  // 松开跳跃键时重力倍数
    [SerializeField] private float upwardJumpMultiplier = 1.2f;  // 向上跳跃时重力倍数（轻盈感）
    [SerializeField] private float maxFallSpeed = -25f;  // 最大下落速度
    [SerializeField] private float apexThreshold = 2f;  // 接近跳跃顶点的速度阈值

    [Header("Coyote Time & Jump Buffer")]
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.15f;


    [SerializeField] private float jumpCutMultiplier = 0.3f;  // 更强的跳跃切断效果

    


    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private GameObject detectedLadder;  // 检测到的梯子对象

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private HeroAttackController attackController;
    private HeroLife heroLife;
    private HeroStamina heroStamina;

    private float dirx = 0f;
    private bool isOnLadder = false;
    private bool isClimbing = false;
    private bool isJumping = false;
    private bool hasDoubleJumped = false;
    private bool isSliding = false;
    private MovementState lastState = MovementState.HeroIdle;
    private bool controlsEnabled = true;  // 控制输入是否启用

    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    
    // 外部速度修正系统
    private float externalSpeedMultiplier = 1f;

    [field: SerializeField] public bool isFacingRight { get; private set; } = true;
    [field: SerializeField] private GameObject _cameraFollowObject;

    private enum MovementState
    {
        HeroIdle,
        HeroRun,
        PlayerJump,
        PlayerFall,
        HeroLadder
    }

    public void EnableControls()
    {
        controlsEnabled = true;
        Debug.Log("HeroMovement controls enabled");
    }

    public void DisableControls()
    {
        controlsEnabled = false;
        
        // 立即停止水平移动
        dirx = 0f;
        rb.velocity = new Vector2(0f, rb.velocity.y);
        
        // 重置爬梯状态
        if (isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = 1f;
        }
        
        // 重置滑铲状态
        if (isSliding)
        {
            StopAllCoroutines(); // 停止滑铲协程
            isSliding = false;
            capsuleCollider.size = originalColliderSize;
            capsuleCollider.offset = originalColliderOffset;
            rb.gravityScale = 1f;
        }
        
        Debug.Log("HeroMovement controls disabled - character stopped");
    }

    private float _fallSpeedYDampingChangeThreshold;

    private SpriteRenderer spriteRender;
    private bool isInHideArea = false;
    private bool isHiding = false;
    public bool IsHiding => isHiding;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();
        attackController = GetComponent<HeroAttackController>();
        heroLife = GetComponent<HeroLife>();
        heroStamina = GetComponent<HeroStamina>();
        originalColliderSize = capsuleCollider.size;
        originalColliderOffset = capsuleCollider.offset;

        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
    }

    void Update()
    {
        if (heroLife != null && heroLife.IsDead)
            return;

        // 处理输入（只有在控制启用时）
        if (controlsEnabled)
        {
            // 使用SettingMenu获取自定义按键绑定
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(SettingMenu.GetKeyBinding("MoveLeft"))) horizontal -= 1f;
            if (Input.GetKey(SettingMenu.GetKeyBinding("MoveRight"))) horizontal += 1f;
            if (Input.GetKey(SettingMenu.GetKeyBinding("MoveDown"))) vertical -= 1f;
            if (Input.GetKey(SettingMenu.GetKeyBinding("MoveUp"))) vertical += 1f;

            dirx = horizontal;
            float diry = vertical;

            // 跳跃输入处理
            if (Input.GetKeyUp(SettingMenu.GetKeyBinding("Jump")) && rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }

            UpdateCoyoteTime();
            UpdateJumpBuffer();

            HandleClimbing(diry);
            HandleHorizontalMovement();
            HandleJump();
            HandleSliding();
            HandleFacingDirection();
        }
        else
        {
            // 控制禁用时，保持角色静止
            dirx = 0f;

            // 确保水平速度为零（但保持垂直速度用于重力）
            rb.velocity = new Vector2(0f, rb.velocity.y);

            // 仍然需要更新基本状态
            UpdateCoyoteTime();
        }

        // 这些方法无论控制是否启用都需要执行
        UpdateAnimationState();
        BetterJump();
        HandleHide();
    }

    private void HandleHide()
    {
        if (isInHideArea)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                isHiding = true;
                spriteRender.color = new Color(1, 1, 1, 0.2f);
                stone.GetComponent<Collider2D>().enabled = false;
                stone2.GetComponent<Collider2D>().enabled = false;
                Debug.Log("隐身");

            }
        }
        else if (isHiding)
        {
            isHiding = false;
            spriteRender.color = new Color(1, 1, 1, 1.0f);
            stone.GetComponent<Collider2D>().enabled = true;
            stone2.GetComponent<Collider2D>().enabled = true;
            Debug.Log("No隐身");
        }
    }

     private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HideArea"))
        {
            isInHideArea = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("HideArea"))
        {
            isInHideArea = false;
        }
    }

    private void UpdateCoyoteTime()
    {
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
            // 着地时重置双跳状态
            if (hasDoubleJumped)
            {
                hasDoubleJumped = false;
                Debug.Log("Double jump reset - landed on ground");
            }
        }
        else
            coyoteTimeCounter -= Time.deltaTime;
    }

    private void UpdateJumpBuffer()
    {
        if (Input.GetKeyDown(SettingMenu.GetKeyBinding("Jump")))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleHorizontalMovement()
    {
        // 如果控制被禁用，不处理水平移动
        if (!controlsEnabled)
            return;
            
        bool canMove = attackController == null || attackController.CanMove();
        float speedMultiplier = attackController != null ? attackController.GetMovementSpeedMultiplier() : 1f;
        
        // 应用外部速度修正（如水中减速等）
        speedMultiplier *= externalSpeedMultiplier;

        if (!isClimbing && canMove)
            rb.velocity = new Vector2(dirx * moveSpeed * speedMultiplier, rb.velocity.y);
    }

    private void HandleJump()
    {
        bool canJump = attackController == null || attackController.CanJump();

        if (jumpBufferCounter > 0 && (coyoteTimeCounter > 0 || (doubleJumpUnlocked && !hasDoubleJumped)) && !isClimbing && canJump)
        {
            // 检查是否是双跳
            if (!IsGrounded() && !hasDoubleJumped && doubleJumpUnlocked)
            {
                // 检查双跳体力
                if (heroStamina != null && !heroStamina.CanPerformDoubleJump())
                {
                    Debug.Log("Not enough stamina for double jump!");
                    return;
                }
                
                // 消耗双跳体力
                if (heroStamina != null)
                {
                    heroStamina.ConsumeDoubleJumpStamina();
                }
                
                hasDoubleJumped = true;
                rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
                Debug.Log("Double jump triggered!");
            }
            else
            {
                // 检查普通跳跃体力
                if (heroStamina != null && !heroStamina.CanPerformJump())
                {
                    Debug.Log("Not enough stamina for jump!");
                    return;
                }
                
                // 消耗跳跃体力
                if (heroStamina != null)
                {
                    heroStamina.ConsumeJumpStamina();
                }
                
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                Debug.Log("Normal jump triggered!");
            }
            
            isJumping = true;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
        else if (jumpBufferCounter > 0 && isClimbing)
        {
            isClimbing = false;
            isOnLadder = false;
            rb.gravityScale = 1f;
            rb.velocity = new Vector2(dirx * moveSpeed, jumpForce * 0.7f);
            isJumping = true;
            hasDoubleJumped = false;

            jumpBufferCounter = 0;
            Debug.Log("Jumped off ladder!");
        }
    }

    private void HandleClimbing(float verticalInput)
    {
        bool canClimb = attackController == null || attackController.CanClimb();
        CheckLadderProximity();

        if (isOnLadder && Mathf.Abs(verticalInput) > 0 && canClimb)
        {
            isClimbing = true;
            rb.gravityScale = 0f;  // 爬梯时无重力
            rb.velocity = new Vector2(0, verticalInput * climbSpeed);
            // transform.position = new Vector2(detectedLadder.transform.position.x, transform.position.y);
        }
        else if (isClimbing)
        {
            if (!isOnLadder || !canClimb)
            {
                isClimbing = false;
                rb.gravityScale = 1f;  // 重置为基础重力，BetterJump会接管
            }
            else if (Mathf.Abs(verticalInput) == 0)
                rb.velocity = new Vector2(0, 0);
        }

    }

    private void HandleSliding()
    {
        bool slideInput = Input.GetKeyDown(SettingMenu.GetKeyBinding("Slide"));
        
        if (slideUnlocked && slideInput && !isSliding && IsGrounded())
        {
            // 检查滑铲体力
            if (heroStamina != null && !heroStamina.CanPerformSlide())
            {
                Debug.Log("Not enough stamina for slide!");
                return;
            }
            
            StartCoroutine(Slide());
        }
    }

    private void HandleFacingDirection()
    {
        if (Mathf.Abs(dirx) > 0.1f)
        {
            bool stateChange = transform.rotation.eulerAngles.y == 180f && dirx > 0f
                             || transform.rotation.eulerAngles.y == 0f && dirx < 0f;

            if (dirx < 0f)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                isFacingRight = false;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                isFacingRight = true;
            }

            if (stateChange)
            {
                Debug.Log($"Hero facing direction changed: {isFacingRight}");
                _cameraFollowObject.GetComponent<CameraFollowObject>()?.CallTurn();
            }
        }
    }

    private void CheckLadderProximity()
    {
        Vector2 heroCenter = transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(heroCenter, ladderDetectionDistance);

        detectedLadder = null;  // 每次检测前重置
        
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Ladder"))
            {
                detectedLadder = col.gameObject;
                isOnLadder = true;
                return;
            }
        }
        
        isOnLadder = false;
        if (isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = 1f;
        }
    }

    private void UpdateAnimationState()
    {
        if (heroLife != null && heroLife.IsDead)
            return;

        MovementState state;

        if (isSliding)
        {
            state = MovementState.HeroRun;
            return;
        }
        else if (isClimbing)
            state = MovementState.HeroLadder;
        else if (IsGrounded())
            state = Mathf.Abs(dirx) > 0.1f ? MovementState.HeroRun : MovementState.HeroIdle;
        else
            state = rb.velocity.y > 0.1f ? MovementState.PlayerJump : MovementState.PlayerFall;

        anim.SetInteger("Movements", (int)state);

        if (state != lastState)
        {
            Debug.Log($"Movement State Changed: {lastState} → {state} (IsGrounded: {IsGrounded()}, Velocity.y: {rb.velocity.y:F2})");
            lastState = state;
        }
    }

    private void BetterJump()
    {
        // 空洞骑士风格的跳跃物理
        if (rb.velocity.y < -apexThreshold)
        {
            // 快速下落
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.velocity.y > apexThreshold)
        {
            // 向上跳跃时 - 检查是否松开跳跃键
            if (Input.GetButton("Jump"))
            {
                // 持续按住跳跃键 - 轻盈上升
                rb.gravityScale = upwardJumpMultiplier;
            }
            else
            {
                // 松开跳跃键 - 快速到达顶点
                rb.gravityScale = lowJumpMultiplier;
            }
        }
        else
        {
            // 在跳跃顶点附近 - 短暂的漂浮感
            rb.gravityScale = upwardJumpMultiplier * 0.8f;
        }

        // 限制最大下落速度
        if (rb.velocity.y < maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
    }

    private IEnumerator Slide()
    {
        // 消耗滑铲体力
        if (heroStamina != null)
        {
            heroStamina.ConsumeSlideStamina();
        }
        
        isSliding = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        capsuleCollider.size = slideSize;
        capsuleCollider.offset = slideOffset;

        anim.SetTrigger("Slide");

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            rb.velocity = new Vector2(isFacingRight ? slideSpeed : -slideSpeed, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        capsuleCollider.size = originalColliderSize;
        capsuleCollider.offset = originalColliderOffset;
        rb.gravityScale = originalGravity;
        isSliding = false;
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    public bool IsClimbing => isClimbing;
    public bool IsJumping => isJumping;
    public float GetVelocityY() => rb.velocity.y;
    public float GetVelocityX() => rb.velocity.x;
    public bool HasDoubleJumped => hasDoubleJumped;
    public bool IsDoubleJumpUnlocked => doubleJumpUnlocked;

    public bool getIsSliding() => isSliding;    

    public void UnlockDoubleJump() => doubleJumpUnlocked = true;
    public void LockDoubleJump() { doubleJumpUnlocked = false; hasDoubleJumped = false; }
    public void UnlockSlide() => slideUnlocked = true;
    
    // 外部速度修正方法
    public void SetExternalSpeedMultiplier(float multiplier)
    {
        externalSpeedMultiplier = multiplier;
        Debug.Log($"External speed multiplier set to: {multiplier}");
    }
    
    public void ResetExternalSpeedMultiplier()
    {
        externalSpeedMultiplier = 1f;
        Debug.Log("External speed multiplier reset to 1.0");
    }
    
    public float GetExternalSpeedMultiplier()
    {
        return externalSpeedMultiplier;
    }
}
