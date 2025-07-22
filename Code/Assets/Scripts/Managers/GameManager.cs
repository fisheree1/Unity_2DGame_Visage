using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private bool pauseOnPlayerDeath = true;
    [SerializeField] private float gameOverDelay = 2f;
    
    [Header("References")]
    [SerializeField] private HeroLife heroLife;
    [SerializeField] private HeroLifeUI heroLifeUI;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugKeys = true;
    
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    
    // Game state
    private bool gameIsPaused = false;
    private bool gameIsOver = false;
    
    // Events
    public System.Action OnGameStart;
    public System.Action OnGamePause;
    public System.Action OnGameResume;
    public System.Action OnGameOver;
    public System.Action OnGameRestart;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // 自动查找组件
        if (heroLife == null)
        {
            heroLife = FindObjectOfType<HeroLife>();
        }
        
        if (heroLifeUI == null)
        {
            heroLifeUI = FindObjectOfType<HeroLifeUI>();
        }
        
        // 订阅事件
        if (heroLife != null)
        {
            heroLife.OnDeath += HandlePlayerDeath;
            heroLife.OnRespawn += HandlePlayerRespawn;
        }
        
        // 触发游戏开始事件
        OnGameStart?.Invoke();
        
        Debug.Log("Game Manager initialized");
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        if (heroLife != null)
        {
            heroLife.OnDeath -= HandlePlayerDeath;
            heroLife.OnRespawn -= HandlePlayerRespawn;
        }
    }
    
    private void HandleInput()
    {
        if (!enableDebugKeys) return;

        // 暂停/恢复游戏
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (gameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
                    
        // 调试：解锁/锁定二段跳技能
        if (Input.GetKeyDown(KeyCode.U))
        {
            HeroMovement heroMovement = FindObjectOfType<HeroMovement>();
            if (heroMovement != null)
            {
                if (heroMovement.IsDoubleJumpUnlocked)
                {
                    heroMovement.LockDoubleJump();
                    Debug.Log("Debug: Double Jump locked");
                }
                else
                {
                    heroMovement.UnlockDoubleJump();
                    Debug.Log("Debug: Double Jump unlocked");
                }
            }
        }
        
        // 重启游戏
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        
        // 退出游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
        
        // 调试：直接杀死玩家
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (heroLife != null && !heroLife.IsDead)
            {
                heroLife.TakeDamage(heroLife.MaxHealth);
                Debug.Log("Debug: Player killed");
            }
        }
        
        // 调试：治疗玩家
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (heroLife != null && !heroLife.IsDead)
            {
                heroLife.Heal(1);
                Debug.Log("Debug: Player healed");
            }
        }
    }
    
    private void HandlePlayerDeath()
    {
        Debug.Log("Game Manager: Player died");
        
        if (pauseOnPlayerDeath)
        {
            Invoke(nameof(GameOver), gameOverDelay);
        }
        
        OnGameOver?.Invoke();
    }
    
    private void HandlePlayerRespawn()
    {
        Debug.Log("Game Manager: Player respawned");
        
        if (gameIsOver)
        {
            gameIsOver = false;
            ResumeGame();
        }
    }
    
    public void PauseGame()
    {
        if (gameIsPaused) return;
        
        gameIsPaused = true;
        Time.timeScale = 0f;
        OnGamePause?.Invoke();
        
        Debug.Log("Game paused");
    }
    
    public void ResumeGame()
    {
        if (!gameIsPaused) return;
        
        gameIsPaused = false;
        Time.timeScale = 1f;
        OnGameResume?.Invoke();
        
        Debug.Log("Game resumed");
    }
    
    public void GameOver()
    {
        gameIsOver = true;
        Debug.Log("Game Over");
        
        // 可以在这里添加游戏结束逻辑
        // 比如显示最终分数、保存记录等
    }
    
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        
        // 重置时间缩放
        Time.timeScale = 1f;
        
        // 触发重启事件
        OnGameRestart?.Invoke();
        
        // 重新加载当前场景
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        
        // 重置时间缩放
        Time.timeScale = 1f;
        
        SceneManager.LoadScene(sceneName);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    // 公共属性
    public bool GameIsPaused => gameIsPaused;
    public bool GameIsOver => gameIsOver;
    public HeroLife HeroLife => heroLife;
    public HeroLifeUI HeroLifeUI => heroLifeUI;
}
