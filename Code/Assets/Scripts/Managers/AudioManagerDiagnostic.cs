using UnityEngine;

/// <summary>
/// AudioManager诊断工具 - 专门针对初始音乐播放问题的增强版本
/// </summary>
public class AudioManagerDiagnostic : MonoBehaviour
{
    [Header("诊断控制")]
    [SerializeField] private bool autoRunDiagnostic = true;
    [SerializeField] private KeyCode diagnosticKey = KeyCode.F2;
    
    [Header("测试控制")]
    [SerializeField] private KeyCode testNormalMusicKey = KeyCode.F3;
    [SerializeField] private KeyCode testCombatMusicKey = KeyCode.F4;
    [SerializeField] private KeyCode testBossMusicKey = KeyCode.F5;
    [SerializeField] private KeyCode forcePlayCurrentKey = KeyCode.F6; // 新增
    
    void Start()
    {
        if (autoRunDiagnostic)
        {
            // 延迟2秒后运行诊断，确保AudioManager完全初始化
            Invoke(nameof(RunFullDiagnostic), 2f);
        }
        
        Debug.Log("=== AudioManager诊断工具 (增强版) ===");
        Debug.Log($"按 {diagnosticKey} 键运行诊断");
        Debug.Log($"按 {testNormalMusicKey} 键测试正常音乐");
        Debug.Log($"按 {testCombatMusicKey} 键测试战斗音乐");
        Debug.Log($"按 {testBossMusicKey} 键测试Boss音乐");
        Debug.Log($"按 {forcePlayCurrentKey} 键强制播放当前音乐");
        Debug.Log("====================================");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(diagnosticKey))
        {
            RunFullDiagnostic();
        }
        
        if (Input.GetKeyDown(testNormalMusicKey))
        {
            TestNormalMusic();
        }
        
        if (Input.GetKeyDown(testCombatMusicKey))
        {
            TestCombatMusic();
        }
        
        if (Input.GetKeyDown(testBossMusicKey))
        {
            TestBossMusic();
        }
        
        if (Input.GetKeyDown(forcePlayCurrentKey))
        {
            ForcePlayCurrentMusic();
        }
    }
    
    [ContextMenu("运行完整诊断")]
    public void RunFullDiagnostic()
    {
        Debug.Log("========== AudioManager 初始音乐问题诊断 ==========");
        
        CheckAudioManagerInstance();
        CheckInitializationStatus();
        CheckAudioConfiguration();
        CheckAudioSources();
        CheckAudioClips();
        CheckVolumeSettings();
        CheckCurrentState();
        CheckInitialMusicPlayback();
        
        Debug.Log("========== 诊断完成 ==========");
    }
    
    private void CheckAudioManagerInstance()
    {
        Debug.Log("--- 检查AudioManager实例 ---");
        
        if (AudioManager.Instance == null)
        {
            Debug.LogError("? AudioManager.Instance 为空!");
            Debug.LogError("解决方案: 确保场景中有AudioManager组件");
            return;
        }
        
        Debug.Log("? AudioManager实例存在");
        
        // 检查GameObject状态
        if (!AudioManager.Instance.gameObject.activeInHierarchy)
        {
            Debug.LogError("? AudioManager GameObject未激活!");
            return;
        }
        
        Debug.Log("? AudioManager GameObject已激活");
    }
    
    private void CheckInitializationStatus()
    {
        Debug.Log("--- 检查初始化状态 ---");
        
        if (AudioManager.Instance == null) return;
        
        bool isInitialized = AudioManager.Instance.IsInitialized;
        bool hasPlayedInitialMusic = AudioManager.Instance.HasPlayedInitialMusic;
        
        Debug.Log($"AudioManager已初始化: {(isInitialized ? "?" : "?")} {isInitialized}");
        Debug.Log($"已播放初始音乐: {(hasPlayedInitialMusic ? "?" : "?")} {hasPlayedInitialMusic}");
        
        if (!isInitialized)
        {
            Debug.LogWarning("?? AudioManager尚未完全初始化，请等待");
        }
        
        if (!hasPlayedInitialMusic)
        {
            Debug.LogError("? 初始音乐尚未播放 - 这是问题所在!");
        }
    }
    
    private void CheckAudioConfiguration()
    {
        Debug.Log("--- 检查音频配置 ---");
        
        if (AudioManager.Instance == null) return;
        
        var config = AudioManager.Instance.Config;
        if (config == null)
        {
            Debug.LogError("? AudioConfiguration 未分配!");
            Debug.LogError("解决方案: 在AudioManager的Inspector中分配AudioConfiguration");
            return;
        }
        
        Debug.Log("? AudioConfiguration已分配");
        
        // 检查配置有效性
        if (!config.IsValid())
        {
            Debug.LogWarning("?? AudioConfiguration配置无效");
        }
        else
        {
            Debug.Log("? AudioConfiguration配置有效");
        }
    }
    
    private void CheckAudioSources()
    {
        Debug.Log("--- 检查音频源 ---");
        
        if (AudioManager.Instance == null) return;
        
        // 检查是否有子对象AudioSource
        var audioSources = AudioManager.Instance.GetComponentsInChildren<AudioSource>();
        
        Debug.Log($"找到 {audioSources.Length} 个AudioSource组件");
        
        foreach (var source in audioSources)
        {
            CheckAudioSource(source.name, source);
        }
        
        if (audioSources.Length == 0)
        {
            Debug.LogError("? 未找到任何AudioSource组件!");
        }
    }
    
    private void CheckAudioSource(string name, AudioSource source)
    {
        if (source == null)
        {
            Debug.LogError($"? {name} 为空");
            return;
        }
        
        string status = source.isPlaying ? "?? 播放中" : "?? 未播放";
        Debug.Log($"{status} {name}:");
        Debug.Log($"   - 音量: {source.volume:F2}");
        Debug.Log($"   - 静音: {(source.mute ? "?" : "?")} {source.mute}");
        Debug.Log($"   - 启用: {(source.enabled ? "?" : "?")} {source.enabled}");
        Debug.Log($"   - 循环: {(source.loop ? "?" : "?")} {source.loop}");
        Debug.Log($"   - 当前剪辑: {(source.clip != null ? source.clip.name : "? 无")}");
        Debug.Log($"   - 正在播放: {(source.isPlaying ? "?" : "?")} {source.isPlaying}");
        
        // 检查常见问题
        if (!source.enabled)
        {
            Debug.LogError($"   ?? 修复建议: 启用 {name} AudioSource");
        }
        
        if (source.mute)
        {
            Debug.LogError($"   ?? 修复建议: 取消 {name} 的静音");
        }
        
        if (source.volume <= 0.01f)
        {
            Debug.LogError($"   ?? 修复建议: 增加 {name} 的音量");
        }
        
        if (source.clip == null)
        {
            Debug.LogError($"   ?? 修复建议: 为 {name} 分配音频剪辑");
        }
    }
    
    private void CheckAudioClips()
    {
        Debug.Log("--- 检查音频剪辑 ---");
        
        if (AudioManager.Instance?.Config == null) return;
        
        var config = AudioManager.Instance.Config;
        
        CheckAudioClip("正常背景音乐", config.normalBackgroundMusic);
        CheckAudioClip("战斗背景音乐", config.combatBackgroundMusic);
        CheckAudioClip("Boss背景音乐", config.bossBackgroundMusic);
    }
    
    private void CheckAudioClip(string name, AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError($"? {name} 未分配");
        }
        else
        {
            Debug.Log($"? {name}: {clip.name} (长度: {clip.length:F1}秒)");
        }
    }
    
    private void CheckVolumeSettings()
    {
        Debug.Log("--- 检查音量设置 ---");
        
        if (AudioManager.Instance == null) return;
        
        float masterVolume = AudioManager.Instance.MasterVolume;
        float musicVolume = AudioManager.Instance.MusicVolume;
        float sfxVolume = AudioManager.Instance.SFXVolume;
        
        Debug.Log($"主音量: {(masterVolume > 0.01f ? "?" : "?")} {masterVolume:F2}");
        Debug.Log($"音乐音量: {(musicVolume > 0.01f ? "?" : "?")} {musicVolume:F2}");
        Debug.Log($"音效音量: {(sfxVolume > 0.01f ? "?" : "?")} {sfxVolume:F2}");
        
        float finalMusicVolume = masterVolume * musicVolume;
        Debug.Log($"最终音乐音量: {(finalMusicVolume > 0.01f ? "?" : "?")} {finalMusicVolume:F2}");
        
        if (finalMusicVolume <= 0.01f)
        {
            Debug.LogError("? 音乐音量过低，无法听到声音!");
            Debug.LogError("?? 修复建议: 增加主音量或音乐音量");
        }
    }
    
    private void CheckCurrentState()
    {
        Debug.Log("--- 检查当前状态 ---");
        
        if (AudioManager.Instance == null) return;
        
        Debug.Log($"当前音乐状态: {AudioManager.Instance.CurrentMusicState}");
        Debug.Log($"是否在战斗中: {AudioManager.Instance.IsInCombat}");
        Debug.Log($"是否在Boss战中: {AudioManager.Instance.IsBossBattle}");
    }
    
    private void CheckInitialMusicPlayback()
    {
        Debug.Log("--- 检查初始音乐播放状态 ---");
        
        if (AudioManager.Instance == null) return;
        
        bool hasPlayed = AudioManager.Instance.HasPlayedInitialMusic;
        
        if (!hasPlayed)
        {
            Debug.LogError("? 关键问题: 初始音乐从未播放!");
            Debug.LogError("?? 可能原因:");
            Debug.LogError("   1. AudioConfiguration未分配");
            Debug.LogError("   2. Normal Background Music未分配");
            Debug.LogError("   3. 音量设置为0");
            Debug.LogError("   4. AudioSource未正确配置");
            Debug.LogError($"?? 立即修复: 按 {forcePlayCurrentKey} 键强制播放音乐");
        }
        else
        {
            Debug.Log("? 初始音乐已播放");
            
            // 检查当前是否还在播放
            var audioSources = AudioManager.Instance.GetComponentsInChildren<AudioSource>();
            bool anyPlaying = false;
            
            foreach (var source in audioSources)
            {
                if (source.isPlaying)
                {
                    anyPlaying = true;
                    Debug.Log($"? 当前播放中: {source.name} - {(source.clip != null ? source.clip.name : "无剪辑")}");
                }
            }
            
            if (!anyPlaying)
            {
                Debug.LogWarning("?? 初始音乐已播放但当前无音频在播放");
                Debug.LogWarning("?? 可能原因: 音乐被意外停止或剪辑太短");
            }
        }
    }
    
    #region 测试方法
    
    [ContextMenu("测试正常音乐")]
    public void TestNormalMusic()
    {
        Debug.Log("?? 测试播放正常音乐...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNormalMusic();
            CheckMusicPlayback("正常音乐");
        }
    }
    
    [ContextMenu("测试战斗音乐")]
    public void TestCombatMusic()
    {
        Debug.Log("?? 测试播放战斗音乐...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCombatMusic();
            CheckMusicPlayback("战斗音乐");
        }
    }
    
    [ContextMenu("测试Boss音乐")]
    public void TestBossMusic()
    {
        Debug.Log("?? 测试播放Boss音乐...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossMusic();
            CheckMusicPlayback("Boss音乐");
        }
    }
    
    [ContextMenu("强制播放当前音乐")]
    public void ForcePlayCurrentMusic()
    {
        Debug.Log("?? 强制播放当前状态音乐...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ForcePlayCurrentMusic();
            CheckMusicPlayback("强制播放");
        }
        else
        {
            Debug.LogError("? AudioManager.Instance 为空，无法强制播放!");
        }
    }
    
    private void CheckMusicPlayback(string musicType)
    {
        // 延迟检查，给音乐一些启动时间
        Invoke(nameof(DelayedPlaybackCheck), 0.5f);
    }
    
    private void DelayedPlaybackCheck()
    {
        if (AudioManager.Instance == null) return;
        
        // 查找正在播放的音频源
        var audioSources = AudioManager.Instance.GetComponentsInChildren<AudioSource>();
        bool anyPlaying = false;
        
        foreach (var source in audioSources)
        {
            if (source.isPlaying)
            {
                Debug.Log($"? 检测到音频正在播放: {source.name}");
                Debug.Log($"   剪辑: {(source.clip != null ? source.clip.name : "无")}");
                Debug.Log($"   音量: {source.volume:F2}");
                anyPlaying = true;
            }
        }
        
        if (!anyPlaying)
        {
            Debug.LogError("? 没有检测到正在播放的音频!");
            Debug.LogError("?? 建议检查:");
            Debug.LogError("   1. 音频剪辑是否已分配");
            Debug.LogError("   2. 音量是否大于0");
            Debug.LogError("   3. AudioSource是否启用");
        }
    }
    
    #endregion
    
    #region 修复建议
    
    [ContextMenu("显示初始音乐问题修复建议")]
    public void ShowInitialMusicFixSuggestions()
    {
        Debug.Log("========== 初始音乐问题修复建议 ==========");
        Debug.Log("?? 问题: 初始状态无音乐播放，但状态切换后能正常播放");
        Debug.Log("");
        Debug.Log("?? 解决步骤:");
        Debug.Log("1. 检查AudioConfiguration:");
        Debug.Log("   - 确保已分配给AudioManager");
        Debug.Log("   - 确保Normal Background Music已分配");
        Debug.Log("");
        Debug.Log("2. 检查音量设置:");
        Debug.Log("   - Master Volume > 0");
        Debug.Log("   - Music Volume > 0");
        Debug.Log("   - 最终音量 = Master × Music > 0.01");
        Debug.Log("");
        Debug.Log("3. 检查AudioSource:");
        Debug.Log("   - 确保已启用且未静音");
        Debug.Log("   - 确保循环设置正确");
        Debug.Log("");
        Debug.Log("4. 强制播放测试:");
        Debug.Log($"   - 按 {forcePlayCurrentKey} 键强制播放当前音乐");
        Debug.Log($"   - 按 {testNormalMusicKey} 键测试正常音乐");
        Debug.Log("");
        Debug.Log("5. 如果仍然无效:");
        Debug.Log("   - 在AudioManager中勾选'Disable Smooth Transition'");
        Debug.Log("   - 使用手动覆盖设置直接分配音频剪辑");
        Debug.Log("   - 重新创建AudioManager和AudioConfiguration");
        Debug.Log("==========================================");
    }
    
    #endregion
}