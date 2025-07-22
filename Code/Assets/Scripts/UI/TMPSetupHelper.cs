using UnityEngine;
using TMPro;

/// <summary>
/// TextMeshPro设置助手
/// 这个脚本可以帮助快速设置GameOverUI中的TextMeshPro组件
/// </summary>
public class TMPSetupHelper : MonoBehaviour
{
    [Header("默认设置")]
    [SerializeField] private TMP_FontAsset defaultFont;
    [SerializeField] private float defaultFontSize = 24f;
    [SerializeField] private Color defaultTextColor = Color.white;
    [SerializeField] private TextAlignmentOptions defaultAlignment = TextAlignmentOptions.Center;
    
    [Header("预设文本")]
    [SerializeField] private string gameOverText = "游戏结束";
    [SerializeField] private string respawnText = "复活 (空格键)";
    [SerializeField] private string restartText = "重新开始 (R键)";
    [SerializeField] private string mainMenuText = "主菜单 (ESC键)";
    [SerializeField] private string quitText = "退出游戏";
    
    [Header("自动设置")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupAllTextMeshPro();
        }
    }
    
    [ContextMenu("设置所有TextMeshPro组件")]
    public void SetupAllTextMeshPro()
    {
        var gameOverUI = GetComponent<GameOverUI>();
        if (gameOverUI == null)
        {
            Debug.LogWarning("TMPSetupHelper: 找不到GameOverUI组件！");
            return;
        }
        
        // 使用反射获取私有字段（仅用于演示，实际项目中建议使用公共属性）
        SetupTextComponent("gameOverText", gameOverText);
        SetupTextComponent("respawnButtonText", respawnText);
        SetupTextComponent("restartButtonText", restartText);
        SetupTextComponent("mainMenuButtonText", mainMenuText);
        SetupTextComponent("quitButtonText", quitText);
        
        Debug.Log("TMPSetupHelper: 所有TextMeshPro组件设置完成！");
    }
    
    private void SetupTextComponent(string fieldName, string text)
    {
        var gameOverUI = GetComponent<GameOverUI>();
        var field = typeof(GameOverUI).GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            var textComponent = field.GetValue(gameOverUI) as TextMeshProUGUI;
            if (textComponent != null)
            {
                SetupSingleTextMeshPro(textComponent, text);
            }
        }
    }
    
    /// <summary>
    /// 设置单个TextMeshPro组件
    /// </summary>
    public void SetupSingleTextMeshPro(TextMeshProUGUI textComponent, string text)
    {
        if (textComponent == null) return;
        
        textComponent.text = text;
        
        if (defaultFont != null)
            textComponent.font = defaultFont;
            
        textComponent.fontSize = defaultFontSize;
        textComponent.color = defaultTextColor;
        textComponent.alignment = defaultAlignment;
        
        // 启用最佳拟合
        textComponent.enableAutoSizing = true;
        textComponent.fontSizeMin = 10f;
        textComponent.fontSizeMax = defaultFontSize;
    }
    
    /// <summary>
    /// 查找并设置场景中所有的TextMeshPro组件
    /// </summary>
    [ContextMenu("设置场景中所有TextMeshPro")]
    public void SetupAllTextMeshProInScene()
    {
        var allTextComponents = FindObjectsOfType<TextMeshProUGUI>();
        
        foreach (var textComponent in allTextComponents)
        {
            // 跳过已经有文本的组件
            if (!string.IsNullOrEmpty(textComponent.text)) continue;
            
            // 根据GameObject名称设置默认文本
            string defaultText = GetDefaultTextByName(textComponent.gameObject.name);
            SetupSingleTextMeshPro(textComponent, defaultText);
        }
        
        Debug.Log($"TMPSetupHelper: 已设置 {allTextComponents.Length} 个TextMeshPro组件");
    }
    
    private string GetDefaultTextByName(string objectName)
    {
        string lowerName = objectName.ToLower();
        
        if (lowerName.Contains("gameover"))
            return gameOverText;
        else if (lowerName.Contains("respawn"))
            return respawnText;
        else if (lowerName.Contains("restart"))
            return restartText;
        else if (lowerName.Contains("mainmenu") || lowerName.Contains("menu"))
            return mainMenuText;
        else if (lowerName.Contains("quit") || lowerName.Contains("exit"))
            return quitText;
        else
            return "文本";
    }
    
    /// <summary>
    /// 创建中文字体资源的提示
    /// </summary>
    [ContextMenu("中文字体设置提示")]
    public void ShowChineseFontSetupTip()
    {
        Debug.Log("中文字体设置步骤：\n" +
                 "1. 在Project窗口中右键 → Create → TextMeshPro → Font Asset\n" +
                 "2. 选择支持中文的字体文件（如思源黑体、微软雅黑等）\n" +
                 "3. 在Character Set中选择Custom Characters\n" +
                 "4. 在Custom Character List中输入你需要的中文字符\n" +
                 "5. 点击Generate Font Atlas\n" +
                 "6. 将生成的Font Asset分配给defaultFont字段");
    }
}
