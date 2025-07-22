using UnityEngine;

/// <summary>
/// 音频配置资源 - 用于配置AudioManager的音频资源和设置
/// </summary>
[CreateAssetMenu(fileName = "AudioConfig", menuName = "Audio/Audio Configuration")]
public class AudioConfiguration : ScriptableObject
{
    [Header("背景音乐")]
    [Tooltip("正常探索时的背景音乐")]
    public AudioClip normalBackgroundMusic;
    
    [Tooltip("普通战斗时的背景音乐")]
    public AudioClip combatBackgroundMusic;
    
    [Tooltip("Boss战斗时的背景音乐")]
    public AudioClip bossBackgroundMusic;
    
    [Header("音量设置")]
    [Range(0f, 1f)]
    [Tooltip("主音量")]
    public float masterVolume = 1f;
    
    [Range(0f, 1f)]
    [Tooltip("音乐音量")]
    public float musicVolume = 0.7f;
    
    [Range(0f, 1f)]
    [Tooltip("音效音量")]
    public float sfxVolume = 0.8f;
    
    [Header("过渡设置")]
    [Tooltip("音乐淡入淡出时间")]
    public float fadeTime = 2f;
    
    [Tooltip("是否启用平滑过渡")]
    public bool smoothTransition = true;
    
    [Header("战斗检测")]
    [Tooltip("战斗检测范围")]
    public float combatDetectionRange = 15f;
    
    [Tooltip("脱离战斗延迟时间")]
    public float combatExitDelay = 3f;
    
    [Header("层级设置")]
    [Tooltip("Boss敌人所在的层级")]
    public LayerMask bossLayerMask = -1;
    
    [Tooltip("普通敌人所在的层级")]
    public LayerMask enemyLayerMask = -1;
    
    [Header("标签设置")]
    [Tooltip("Boss的标签")]
    public string bossTag = "Boss";
    
    [Tooltip("敌人的标签")]
    public string[] enemyTags = { "Enemy" };
    
    [Header("音效资源")]
    [Tooltip("UI音效")]
    public AudioClip[] uiSounds;
    
    [Tooltip("环境音效")]
    public AudioClip[] environmentSounds;
    
    [Tooltip("战斗音效")]
    public AudioClip[] combatSounds;
    
    [Header("调试设置")]
    [Tooltip("启用调试日志")]
    public bool enableDebugLogs = true;
    
    [Tooltip("显示Gizmos")]
    public bool showGizmos = true;
    
    /// <summary>
    /// 验证配置的有效性
    /// </summary>
    public bool IsValid()
    {
        bool isValid = true;
        
        if (normalBackgroundMusic == null)
        {
            Debug.LogWarning("AudioConfiguration: Normal background music is not assigned!");
            isValid = false;
        }
        
        if (combatBackgroundMusic == null)
        {
            Debug.LogWarning("AudioConfiguration: Combat background music is not assigned!");
            isValid = false;
        }
        
        if (bossBackgroundMusic == null)
        {
            Debug.LogWarning("AudioConfiguration: Boss background music is not assigned!");
            isValid = false;
        }
        
        if (fadeTime <= 0f)
        {
            Debug.LogWarning("AudioConfiguration: Fade time should be greater than 0!");
            isValid = false;
        }
        
        if (combatDetectionRange <= 0f)
        {
            Debug.LogWarning("AudioConfiguration: Combat detection range should be greater than 0!");
            isValid = false;
        }
        
        return isValid;
    }
    
    /// <summary>
    /// 应用配置到AudioManager
    /// </summary>
    public void ApplyToAudioManager(AudioManager audioManager)
    {
        if (audioManager == null)
        {
            Debug.LogError("AudioConfiguration: AudioManager is null!");
            return;
        }
        
        audioManager.SetMasterVolume(masterVolume);
        audioManager.SetMusicVolume(musicVolume);
        audioManager.SetSFXVolume(sfxVolume);
    }
    
    /// <summary>
    /// 获取指定类型的音效
    /// </summary>
    public AudioClip GetSoundEffect(SoundType soundType, int index = 0)
    {
        AudioClip[] targetArray = null;
        
        switch (soundType)
        {
            case SoundType.UI:
                targetArray = uiSounds;
                break;
            case SoundType.Environment:
                targetArray = environmentSounds;
                break;
            case SoundType.Combat:
                targetArray = combatSounds;
                break;
        }
        
        if (targetArray != null && index >= 0 && index < targetArray.Length)
        {
            return targetArray[index];
        }
        
        return null;
    }
    
    /// <summary>
    /// 获取随机音效
    /// </summary>
    public AudioClip GetRandomSoundEffect(SoundType soundType)
    {
        AudioClip[] targetArray = null;
        
        switch (soundType)
        {
            case SoundType.UI:
                targetArray = uiSounds;
                break;
            case SoundType.Environment:
                targetArray = environmentSounds;
                break;
            case SoundType.Combat:
                targetArray = combatSounds;
                break;
        }
        
        if (targetArray != null && targetArray.Length > 0)
        {
            int randomIndex = Random.Range(0, targetArray.Length);
            return targetArray[randomIndex];
        }
        
        return null;
    }
}

/// <summary>
/// 音效类型枚举
/// </summary>
public enum SoundType
{
    UI,           // UI界面音效
    Environment,  // 环境音效
    Combat        // 战斗音效
}