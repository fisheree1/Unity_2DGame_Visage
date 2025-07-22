using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroLife : MonoBehaviour
{
    [Header("Hero Health Settings")]
    [SerializeField] public int maxHealth = 3;
    [SerializeField] public int currentHealth;

    [Header("Damage Response")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float invulnerabilityTime = 0.5f;

    [Header("Death Settings")]
    [SerializeField] private float deathAnimationDuration = 2.0f;
    [SerializeField] private float fallDeathHeight = -300f;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI;

    private HeroMovement hero;

    // Components
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private HeroMovement heroMovement;
    private HeroAttackController attackController;
    private Rigidbody2D rb;

    // State
    private bool isDead = false;
    private bool isInvulnerable = false;
    private Color originalColor;
    private Vector3 respawnPosition;
    private int savedHPCount = 0; // 保存在checkpoint时的血瓶数量
    private int additionalHPCount = 0; // 激活checkpoint后额外收集的血瓶数量

    // Events
    public System.Action<int> OnHealthChanged;
    public System.Action OnDeath;
    public System.Action OnRespawn;

    public CameraManager cameraManager;
    private void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        heroMovement = GetComponent<HeroMovement>();
        hero = heroMovement; // 引用同一个HeroMovement组件
        attackController = GetComponent<HeroAttackController>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        respawnPosition = transform.position;
        // 初始化时保存当前的血瓶数量
        savedHPCount = HeroHP.HPCount;
        additionalHPCount = 0;

        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Update()
    {
        CheckFallDeath();

        // 测试按键：P键直接造成致命伤害
        if (!isDead && Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(maxHealth);
        }
    }

    private void CheckFallDeath()
    {
        if (!isDead && transform.position.y < fallDeathHeight)
        {
            TakeDamage(maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {   
        if (hero.getIsSliding()) return; // 如果正在滑行，则不受伤害
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageResponse());
        }
    }

    private IEnumerator DamageResponse()
    {
        isInvulnerable = true;
        StartCoroutine(DamageFlash());
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            float elapsedTime = 0f;
            float flashInterval = 0.1f;

            while (elapsedTime < damageFlashDuration)
            {
                spriteRenderer.color = damageFlashColor;
                yield return new WaitForSeconds(flashInterval);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);
                elapsedTime += flashInterval * 2;
            }

            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (heroMovement != null) heroMovement.enabled = false;
        if (attackController != null) attackController.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (anim != null)
        {
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsJumping", false);
            anim.SetBool("IsAttacking", false);
            anim.SetInteger("Movements", 0);
            anim.SetFloat("Speed", 0f);
            anim.SetFloat("VelocityY", 0f);
            anim.SetBool("IsDead", true);
            anim.SetTrigger("Death");
        }

        OnDeath?.Invoke();

        StartCoroutine(ShowGameOverUI());
    }

    private IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSeconds(deathAnimationDuration * 0.5f);
        
        // 总是显示GameOverUI，无论是否有checkpoint
        if (gameOverUI != null)
        {
            // 先尝试激活GameObject
            gameOverUI.SetActive(true);
            
            // 然后调用GameOverUI脚本的显示方法（如果存在）
            GameOverUI gameOverScript = gameOverUI.GetComponent<GameOverUI>();
            if (gameOverScript != null)
            {
                gameOverScript.ShowGameOverUI();
            }
            
            Debug.Log("GameOverUI displayed - Player can choose to respawn or restart");
        }
        else
        {
            Debug.LogWarning("GameOverUI is null! Cannot display game over screen.");
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Respawn()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        isDead = false;
        isInvulnerable = false;
        currentHealth = maxHealth;

        // 恢复checkpoint时保存的血瓶数量
        RestoreHPCount();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = false;
        }

        transform.position = respawnPosition;
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager != null)
        {
            cameraManager.RespawnCamera();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        if (anim != null)
        {
            anim.Play("Idle", 0, 0f);
            anim.SetInteger("Movements", 0);
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsDead", false);
            anim.SetFloat("Speed", 0f);
            anim.SetFloat("VelocityY", 0f);

            for (int i = 0; i < 8; i++)
                yield return null;
        }

        if (heroMovement != null) heroMovement.enabled = true;
        if (attackController != null) attackController.enabled = true;

        // 隐藏GameOverUI - 兼容两种设置方式
        if (gameOverUI != null)
        {
            GameOverUI gameOverScript = gameOverUI.GetComponent<GameOverUI>();
            if (gameOverScript != null)
            {
                gameOverScript.HideGameOverUI();
            }
            else
            {
                gameOverUI.SetActive(false);
            }
        }

        OnRespawn?.Invoke();
        OnHealthChanged?.Invoke(currentHealth);

        if (anim != null)
        {
            anim.SetTrigger("Respawn");
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void IncreaseMaxHealth(int amount)
    {
        if (isDead) return;
        maxHealth += amount;
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void SetRespawnPoint(Vector3 newRespawnPoint)
    {
        respawnPosition = newRespawnPoint;
        
        // 激活checkpoint时回满血量
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
        
        // 在设置checkpoint时保存当前血瓶数量
        SaveCurrentHPCount();
        
        Debug.Log($"Checkpoint activated - Health restored to full ({currentHealth}/{maxHealth})");
    }

    private void SaveCurrentHPCount()
    {
        // 保存当前总的血瓶数量，并重置额外收集的数量
        savedHPCount = HeroHP.HPCount;
        additionalHPCount = 0;
        Debug.Log($"Checkpoint activated - Saved HP count: {savedHPCount}");
    }

    private void RestoreHPCount()
    {
        // 恢复为保存的血瓶数量加上之后收集的额外血瓶
        HeroHP.HPCount = savedHPCount + additionalHPCount;
        Debug.Log($"Respawned - Restored HP count: {savedHPCount} + {additionalHPCount} = {HeroHP.HPCount}");
    }

    // 当玩家收集血瓶时调用此方法来追踪额外收集的数量
    public void OnHPCollected()
    {
        additionalHPCount++;
        Debug.Log($"HP collected - Additional count: {additionalHPCount}");
    }

    public Vector3 GetRespawnPoint() => respawnPosition;

    public bool HasValidRespawnPoint()
    {
        return true;
    }

    // Properties for external access
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int SavedHPCount => savedHPCount;
    public int AdditionalHPCount => additionalHPCount;
}
