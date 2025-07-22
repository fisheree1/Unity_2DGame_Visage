using System.Collections;
using UnityEngine;

public class FinalBossAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int punchDamage = 25;
    [SerializeField] private int crouchKickDamage = 30;
    [SerializeField] private float meleeAttackDuration = 0.3f;
    
    [Header("Attack HitBox Objects")]
    [SerializeField] private GameObject punchAttackHitBoxObject;
    [SerializeField] private GameObject crouchKickHitBoxObject;

    [Header("Magic Attack Settings")]
    [SerializeField] private GameObject smallSparkPrefab;
    [SerializeField] private GameObject lightningPrefab; // 雷电攻击预制体
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private float magicProjectileSpeed = 8f;
    [SerializeField] private int smallSparkDamage = 20;
    [SerializeField] private int lightningDamage = 35;
    [SerializeField] private int fireDamage = 30;
    
    [Header("Lightning Attack Settings")]
    [SerializeField] private int lightningStrikeCount = 3;
    [SerializeField] private float lightningRange = 2f;
    [SerializeField] private float lightningWarningTime = 1.5f;
    [SerializeField] private float lightningStrikeDelay = 0.3f;
    
    [Header("Fire Attack Settings")]
    [SerializeField] private int fireCount = 2;
    [SerializeField] private float fireRange = 2f;
    [SerializeField] private float fireWarningTime = 1.2f;
    [SerializeField] private float fireBurnDuration = 2f;
    [SerializeField] private float fireStrikeDelay = 0.4f;
    [SerializeField] private float fireSpawnDepth = 3f; // Fire生成深度
    
    [Header("Meditation Magic Settings")]
    [SerializeField] private float meditationMagicInterval = 1.5f; // 减少间隔时间，让魔法攻击更频繁
    [SerializeField] private int fanProjectileCount = 12; 
    [SerializeField] private float fanAngle = 360f; 
    private bool isMeditationMagicActive = false;
    private Coroutine meditationMagicCoroutine;

    // Components
    private Animator anim;
    private FinalBossMove finalBossMove;

    void Start()
    {
        // 获取组件
        anim = GetComponent<Animator>();
        finalBossMove = GetComponent<FinalBossMove>();
        
        // 检查必要组件
        if (anim == null) Debug.LogError("Final Boss Attack: Animator component missing!");
        if (finalBossMove == null) Debug.LogError("Final Boss Attack: FinalBossMove component missing!");
        
        // 检查攻击碰撞箱
        if (punchAttackHitBoxObject == null)
        {
            Debug.LogWarning("Final Boss Attack: Punch attack hitbox object not assigned!");
            // 尝试通过名称查找
            Transform punchHitbox = transform.Find("PunchAttackHitbox");
            if (punchHitbox != null)
            {
                punchAttackHitBoxObject = punchHitbox.gameObject;
                Debug.Log("Final Boss Attack: Found punch hitbox by name");
            }
        }
        
        if (crouchKickHitBoxObject == null)
        {
            Debug.LogWarning("Final Boss Attack: Crouch kick attack hitbox object not assigned!");
            // 尝试通过名称查找
            Transform crouchKickHitbox = transform.Find("CrouchKickHitBox");
            if (crouchKickHitbox != null)
            {
                crouchKickHitBoxObject = crouchKickHitbox.gameObject;
                Debug.Log("Final Boss Attack: Found crouch kick hitbox by name");
            }
        }
    }

    // 执行拳击攻击
    public void PerformPunchAttack()
    {
        Debug.Log("Final Boss performing Punch Attack");
        
        // 触发拳击动画
        if (anim != null)
        {
            Debug.Log("Setting isPunch trigger");
            anim.SetTrigger("isPunch");
            
            // 检查trigger是否成功设置
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Punch"))
            {
                Debug.Log("isPunch trigger was set successfully");
            }
            else
            {
                Debug.Log("isPunch trigger may not have been set correctly");
            }
        }
        else
        {
            Debug.LogError("Animator is null when trying to trigger isPunch");
        }
        
        // 激活Punch攻击碰撞箱
        if (punchAttackHitBoxObject != null)
        {
            FinalBossAttackHitBox hitBoxComponent = punchAttackHitBoxObject.GetComponent<FinalBossAttackHitBox>();
            if (hitBoxComponent != null)
            {
                hitBoxComponent.SetAttackTypeAndDamage("Punch", punchDamage);
                hitBoxComponent.ActivateForDuration(meleeAttackDuration);
                return;
            }
        }
        
        // 备用方案：直接攻击检测
        PerformDirectMeleeAttack();
    }

    // 执行蹲踢攻击
    public void PerformCrouchKickAttack()
    {
        Debug.Log("Final Boss performing Crouch Kick Attack");
        
        // 触发CrouchKick动画
        if (anim != null)
        {
            Debug.Log("Setting isCrouchKick trigger");
            anim.SetTrigger("isCrouchKick");
            
            // 检查trigger是否成功设置
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("CrouchKick"))
            {
                Debug.Log("isCrouchKick trigger was set successfully");
            }
            else
            {
                Debug.Log("isCrouchKick trigger may not have been set correctly");
            }
        }
        else
        {
            Debug.LogError("Animator is null when trying to trigger isCrouchKick");
        }
        
        // 激活CrouchKick攻击碰撞箱
        if (crouchKickHitBoxObject != null)
        {
            FinalBossAttackHitBox hitBoxComponent = crouchKickHitBoxObject.GetComponent<FinalBossAttackHitBox>();
            if (hitBoxComponent != null)
            {
                hitBoxComponent.SetAttackTypeAndDamage("CrouchKick", crouchKickDamage);
                hitBoxComponent.ActivateForDuration(meleeAttackDuration);
                return;
            }
        }
        
        // 备用方案：直接攻击检测
        PerformDirectMeleeAttack();
    }

    // 备用直接攻击检测（如果没有hitbox子对象）
    private void PerformDirectMeleeAttack()
    {
        Debug.Log("Final Boss performing direct melee attack (fallback)");
        
        if (finalBossMove?.Player == null) return;
        
        float attackRange = 3f;
        float distanceToPlayer = Vector2.Distance(transform.position, finalBossMove.Player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            // 检查玩家是否在攻击方向上
            Vector2 attackDirection = finalBossMove.IsFacingRight ? Vector2.right : Vector2.left;
            Vector2 toPlayer = (finalBossMove.Player.position - transform.position).normalized;
            
            if (Vector2.Dot(attackDirection, toPlayer) > 0.5f) // 45度范围内
            {
                // 尝试对玩家造成伤害
                GameObject playerObj = finalBossMove.Player.gameObject;
                HeroLife heroLife = playerObj.GetComponent<HeroLife>();
                
                if (heroLife != null && !heroLife.IsDead)
                {
                    heroLife.TakeDamage(punchDamage);
                    Debug.Log($"Final Boss direct attack hit player for {punchDamage} damage");
                    

                }
            }
        }
    }

    // 从动画事件调用的方法 - 如果需要在特定帧执行额外逻辑可以在这里扩展
    public void OnPunchAttackHit()
    {
        Debug.Log("Final Boss Punch Attack Hit (Animation Event)");
        // 伤害处理现在由AttackHitBox组件负责
    }

    public void OnCrouchKickAttackHit()
    {
        Debug.Log("Final Boss Crouch Kick Attack Hit (Animation Event)");
        // 伤害处理现在由AttackHitBox组件负责
    }

    // 攻击结束回调
    public void OnAttackComplete()
    {
        Debug.Log("Final Boss Attack Complete");
        // 可以在这里添加攻击完成后的逻辑（如重置状态、播放音效等）
    }

    // Meditation状态下的魔法攻击控制
    public void StartMeditationMagicAttacks()
    {
        if (isMeditationMagicActive) return;
        
        isMeditationMagicActive = true;
        meditationMagicCoroutine = StartCoroutine(MeditationMagicSequence());
    }
    
    public void StopMeditationMagicAttacks()
    {
        isMeditationMagicActive = false;
        if (meditationMagicCoroutine != null)
        {
            StopCoroutine(meditationMagicCoroutine);
            meditationMagicCoroutine = null;
        }
    }
    
    private IEnumerator MeditationMagicSequence()
    {
        Debug.Log("Final Boss starting Meditation Magic Sequence");
        
        // 在冥想状态下持续释放魔法
        while (isMeditationMagicActive)
        {
            // 按顺序释放三种魔法：Lightning -> Fire -> SmallSpark Fan
            
            // 1. Lightning攻击
            if (isMeditationMagicActive)
            {
                Debug.Log("Meditation: Casting Lightning");
                CastLightning();
                yield return new WaitForSeconds(meditationMagicInterval);
            }
            
            // 2. Fire攻击
            if (isMeditationMagicActive)
            {
                Debug.Log("Meditation: Casting Fire");
                CastFire();
                yield return new WaitForSeconds(meditationMagicInterval);
            }
            
            // 3. SmallSpark扇形攻击
            if (isMeditationMagicActive)
            {
                Debug.Log("Meditation: Casting SmallSpark Fan");
                CastSmallSparkFan();
                yield return new WaitForSeconds(meditationMagicInterval);
            }
        }
        
        Debug.Log("Meditation Magic Sequence ended");
    }
    
    // 360度扇形攻击的SmallSpark（冥想状态）
    private void CastSmallSparkFan()
    {
        if (smallSparkPrefab == null)
        {
            Debug.LogError("SmallSpark prefab is null!");
            return;
        }
        
        if (finalBossMove?.Player == null)
        {
            Debug.LogError("Player reference is null!");
            return;
        }
        
        Debug.Log($"Casting SmallSpark Fan with {fanProjectileCount} projectiles");
        
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
        
        // 生成360度扇形攻击的投射物
        for (int i = 0; i < fanProjectileCount; i++)
        {
            float angle = (360f / fanProjectileCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            
            GameObject projectile = Instantiate(smallSparkPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Created SmallSpark projectile {i} at {spawnPosition} with direction {direction}");
            
            SmallSparkProjectile sparkScript = projectile.GetComponent<SmallSparkProjectile>();
            
            if (sparkScript != null)
            {
                sparkScript.SetDirection(direction);
                sparkScript.SetDamage(smallSparkDamage);
                sparkScript.SetSpeed(magicProjectileSpeed);
            }
            else
            {
                Debug.LogError($"SmallSpark prefab doesn't have SmallSparkProjectile script!");
            }
        }
        
        Debug.Log($"Final Boss cast SmallSpark Fan (360 degrees, {fanProjectileCount} projectiles)");
    }
    
    // 从上而下的雷电攻击（冥想状态）
    private void CastLightning()
    {
        if (lightningPrefab == null)
        {
            Debug.LogError("Lightning prefab is null!");
            return;
        }
        
        if (finalBossMove?.Player == null)
        {
            Debug.LogError("Player reference is null!");
            return;
        }
        
        Debug.Log($"Casting {lightningStrikeCount} Lightning Strike(s)");
        
        StartCoroutine(CastLightningSequence());
    }
    
    // 雷电攻击序列
    private IEnumerator CastLightningSequence()
    {
        // 连续雷击
        for (int i = 0; i < lightningStrikeCount; i++)
        {
            // 只追踪Hero的x坐标，使用Boss的y坐标作为基准
            Vector3 targetPosition = new Vector3(finalBossMove.Player.position.x, transform.position.y, finalBossMove.Player.position.z);
            
            // 为多个雷击添加随机偏移
            if (i > 0)
            {
                float randomOffset = Random.Range(-2f, 2f);
                targetPosition.x += randomOffset;
            }
            
            yield return StartCoroutine(CreateLightningStrike(targetPosition));
            
            if (i < lightningStrikeCount - 1)
            {
                yield return new WaitForSeconds(lightningStrikeDelay);
            }
        }
    }
    
    // 创建单次雷电攻击
    private IEnumerator CreateLightningStrike(Vector3 targetPosition)
    {
        // 创建警告指示器
        GameObject warningIndicator = CreateLightningWarning(targetPosition);
        
        // 等待警告时间
        yield return new WaitForSeconds(lightningWarningTime);
        
        // 移除警告指示器
        if (warningIndicator != null)
        {
            Destroy(warningIndicator);
        }
        
        // 创建雷电攻击
        Vector3 lightningPosition = new Vector3(targetPosition.x, targetPosition.y + 10f, targetPosition.z);
        GameObject lightning = null;
        
        if (lightningPrefab != null)
        {
            // 使用预制体雷电攻击
            lightning = Instantiate(lightningPrefab, lightningPosition, Quaternion.identity);
            
            // 尝试获取LightningStrike组件
            LightningStrike lightningScript = lightning.GetComponent<LightningStrike>();
            if (lightningScript != null)
            {
                // 使用LightningStrike的Initialize方法
                LayerMask playerLayer = LayerMask.GetMask("Player");
                lightningScript.Initialize(lightningDamage, targetPosition, lightningRange, playerLayer);
                Debug.Log($"Initialized LightningStrike with damage: {lightningDamage}, range: {lightningRange}");
            }
            else
            {
                Debug.LogError("LightningStrike component not found on lightning prefab!");
            }
        }
        else
        {
            Debug.LogWarning("No lightning prefab assigned!");
        }
        
        Debug.Log($"Lightning strike created at {targetPosition}");
    }
    
    // 创建雷电警告指示器
    private GameObject CreateLightningWarning(Vector3 position)
    {
        // 创建简单的警告指示器
        GameObject warning = new GameObject("LightningWarning");
        warning.transform.position = position;
        
        // 添加视觉组件
        SpriteRenderer spriteRenderer = warning.AddComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1f, 1f, 0f, 0.5f); // 半透明黄色
        
        // 创建简单的圆形精灵
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);
                
                if (distance <= 30f)
                {
                    float alpha = 1f - (distance / 30f);
                    colors[x + y * 64] = new Color(1f, 1f, 0f, alpha * 0.5f);
                }
                else
                {
                    colors[x + y * 64] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite warningSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = warningSprite;
        spriteRenderer.sortingOrder = 10; // 确保警告在其他对象之上
        
        // 添加脉冲动画
        StartCoroutine(PulseWarning(warning.transform));
        
        Debug.Log($"Lightning warning created at {position}");
        return warning;
    }
    
    // 警告指示器脉冲动画
    private IEnumerator PulseWarning(Transform warningTransform)
    {
        if (warningTransform == null) yield break;
        
        Vector3 originalScale = warningTransform.localScale;
        float elapsedTime = 0f;
        
        while (warningTransform != null && elapsedTime < lightningWarningTime)
        {
            float pulseScale = 1f + Mathf.Sin(elapsedTime * 8f) * 0.2f;
            warningTransform.localScale = originalScale * pulseScale;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    // 从地底向上的火焰攻击（冥想状态）
    private void CastFire()
    {
        if (firePrefab == null)
        {
            Debug.LogError("Fire prefab is null!");
            return;
        }
        
        if (finalBossMove?.Player == null)
        {
            Debug.LogError("Player reference is null!");
            return;
        }
        
        Debug.Log($"Casting {fireCount} Fire(s)");

        for (int i = 0; i < fireCount; i++)
        {
            // 只追踪Hero的x坐标，使用Boss的y坐标作为基准
            Vector3 targetPosition = new Vector3(finalBossMove.Player.position.x, transform.position.y, finalBossMove.Player.position.z);
            
            // 为多个火焰攻击添加随机偏移
            if (i > 0)
            {
                float randomOffset = Random.Range(-1.5f, 1.5f);
                targetPosition.x += randomOffset;
            }
            
            // 在目标位置下方生成火焰
            Vector3 spawnPosition = targetPosition + Vector3.down * fireSpawnDepth;
            
            GameObject projectile = Instantiate(firePrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Created Fire projectile at {spawnPosition}");
            
            FireProjectile fireScript = projectile.GetComponent<FireProjectile>();
            
            if (fireScript != null)
            {
                fireScript.SetDamage(fireDamage);
                fireScript.SetRange(fireRange);
                fireScript.SetTargetPosition(targetPosition);
                fireScript.SetWarningTime(fireWarningTime);
                fireScript.SetBurnDuration(fireBurnDuration);
                fireScript.SetFireStrikeCount(1); // 每个Fire只攻击一次
            }
            else
            {
                Debug.LogError($"Fire prefab doesn't have FireProjectile script!");
            }
        }
        
        Debug.Log($"Final Boss cast {fireCount} Fire(s) (Flame from below)");
    }
    
    // 外围区域的蹲下魔法攻击 - 使用SmallSpark扇形攻击
    public void CastOuterZoneCrouchMagic()
    {
        if (smallSparkPrefab == null)
        {
            Debug.LogError("SmallSpark prefab is null!");
            return;
        }
        
        if (finalBossMove?.Player == null)
        {
            Debug.LogError("Player reference is null!");
            return;
        }
        
        Debug.Log("Final Boss casting Outer Zone Crouch Magic - SmallSpark Fan Attack");
        
        // 使用已有的SmallSpark扇形攻击
        CastSmallSparkFan();
    }
}
