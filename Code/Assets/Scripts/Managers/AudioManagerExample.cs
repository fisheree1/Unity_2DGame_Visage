using UnityEngine;

/// <summary>
/// AudioManager使用示例 - 展示如何在游戏中集成音频系统
/// </summary>
public class AudioManagerExample : MonoBehaviour
{
    [Header("示例音效")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip itemPickupSound;
    [SerializeField] private AudioClip playerHurtSound;
    
    [Header("测试设置")]
    [SerializeField] private KeyCode testNormalMusicKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode testCombatMusicKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode testBossMusicKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode testSFXKey = KeyCode.Space;
    
    void Update()
    {
        HandleTestInput();
    }
    
    private void HandleTestInput()
    {
        // 测试音乐切换
        if (Input.GetKeyDown(testNormalMusicKey))
        {
            PlayNormalMusic();
        }
        
        if (Input.GetKeyDown(testCombatMusicKey))
        {
            PlayCombatMusic();
        }
        
        if (Input.GetKeyDown(testBossMusicKey))
        {
            PlayBossMusic();
        }
        
        // 测试音效播放
        if (Input.GetKeyDown(testSFXKey))
        {
            PlayTestSFX();
        }
        
        // 音量调节测试
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                AdjustMasterVolume(0.1f);
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                AdjustMasterVolume(-0.1f);
            }
        }
    }
    
    #region 音乐控制示例
    
    public void PlayNormalMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNormalMusic();
            Debug.Log("切换到正常背景音乐");
        }
    }
    
    public void PlayCombatMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCombatMusic();
            Debug.Log("切换到战斗背景音乐");
        }
    }
    
    public void PlayBossMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossMusic();
            Debug.Log("切换到Boss战背景音乐");
        }
    }
    
    #endregion
    
    #region 音效播放示例
    
    public void PlayButtonClickSFX()
    {
        if (AudioManager.Instance != null && buttonClickSound != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSound);
        }
    }
    
    public void PlayItemPickupSFX()
    {
        if (AudioManager.Instance != null && itemPickupSound != null)
        {
            AudioManager.Instance.PlaySFX(itemPickupSound, 0.8f); // 80%音量
        }
    }
    
    public void PlayPlayerHurtSFX()
    {
        if (AudioManager.Instance != null && playerHurtSound != null)
        {
            AudioManager.Instance.PlaySFX(playerHurtSound, 1.2f); // 120%音量
        }
    }
    
    public void PlayTestSFX()
    {
        // 播放随机UI音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRandomSFX(SoundType.UI);
            Debug.Log("播放随机UI音效");
        }
    }
    
    #endregion
    
    #region 音量控制示例
    
    public void AdjustMasterVolume(float delta)
    {
        if (AudioManager.Instance != null)
        {
            float currentVolume = AudioManager.Instance.MasterVolume;
            float newVolume = Mathf.Clamp01(currentVolume + delta);
            AudioManager.Instance.SetMasterVolume(newVolume);
            Debug.Log($"主音量调整为: {newVolume:F2}");
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
            Debug.Log($"音乐音量设置为: {volume:F2}");
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
            Debug.Log($"音效音量设置为: {volume:F2}");
        }
    }
    
    #endregion
    
    #region UI集成示例
    
    /// <summary>
    /// UI按钮点击事件示例
    /// </summary>
    public void OnUIButtonClick()
    {
        PlayButtonClickSFX();
        // 其他按钮逻辑...
    }
    
    /// <summary>
    /// 玩家受伤事件示例
    /// </summary>
    public void OnPlayerTakeDamage()
    {
        PlayPlayerHurtSFX();
        // 其他受伤逻辑...
    }
    
    /// <summary>
    /// 道具拾取事件示例
    /// </summary>
    public void OnItemPickup()
    {
        PlayItemPickupSFX();
        // 其他拾取逻辑...
    }
    
    #endregion
    
    #region 状态查询示例
    
    public void LogCurrentAudioState()
    {
        if (AudioManager.Instance != null)
        {
            Debug.Log("=== 当前音频状态 ===");
            Debug.Log($"是否在战斗中: {AudioManager.Instance.IsInCombat}");
            Debug.Log($"是否在Boss战中: {AudioManager.Instance.IsBossBattle}");
            Debug.Log($"当前音乐状态: {AudioManager.Instance.CurrentMusicState}");
            Debug.Log($"主音量: {AudioManager.Instance.MasterVolume:F2}");
            Debug.Log($"音乐音量: {AudioManager.Instance.MusicVolume:F2}");
            Debug.Log($"音效音量: {AudioManager.Instance.SFXVolume:F2}");
        }
    }
    
    #endregion
    
    #region 事件订阅示例
    
    void Start()
    {
        // 订阅战斗状态变化事件
        CombatStateDetector detector = FindObjectOfType<CombatStateDetector>();
        if (detector != null)
        {
            detector.OnCombatStart += OnCombatStarted;
            detector.OnCombatEnd += OnCombatEnded;
            detector.OnBossBattleStart += OnBossBattleStarted;
            detector.OnBossBattleEnd += OnBossBattleEnded;
        }
        
        // 显示控制提示
        ShowControlsHelp();
    }
    
    void OnDestroy()
    {
        // 取消事件订阅
        CombatStateDetector detector = FindObjectOfType<CombatStateDetector>();
        if (detector != null)
        {
            detector.OnCombatStart -= OnCombatStarted;
            detector.OnCombatEnd -= OnCombatEnded;
            detector.OnBossBattleStart -= OnBossBattleStarted;
            detector.OnBossBattleEnd -= OnBossBattleEnded;
        }
    }
    
    private void OnCombatStarted()
    {
        Debug.Log("?? 战斗开始! 音乐已切换到战斗模式");
        // 可以在这里添加其他战斗开始的逻辑，如UI变化等
    }
    
    private void OnCombatEnded()
    {
        Debug.Log("? 战斗结束! 音乐已恢复正常");
        // 可以在这里添加其他战斗结束的逻辑
    }
    
    private void OnBossBattleStarted()
    {
        Debug.Log("?? Boss战开始! 史诗级音乐已响起");
        // Boss战特殊逻辑，如特殊UI、警告效果等
    }
    
    private void OnBossBattleEnded()
    {
        Debug.Log("?? Boss战结束! 胜利属于你!");
        // Boss战结束特殊逻辑，如奖励音效、胜利画面等
    }
    
    #endregion
    
    private void ShowControlsHelp()
    {
        Debug.Log("=== AudioManager 测试控制 ===");
        Debug.Log($"按 {testNormalMusicKey} - 播放正常音乐");
        Debug.Log($"按 {testCombatMusicKey} - 播放战斗音乐");
        Debug.Log($"按 {testBossMusicKey} - 播放Boss音乐");
        Debug.Log($"按 {testSFXKey} - 播放测试音效");
        Debug.Log("按 Shift + Plus/Minus - 调节主音量");
        Debug.Log("============================");
    }
}