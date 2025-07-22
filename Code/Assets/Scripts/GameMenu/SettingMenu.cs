using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class SettingMenu : MonoBehaviour
{
    [Header("Key Binding UI")]
    public Button moveUpButton;
    public Button moveDownButton;
    public Button moveLeftButton;
    public Button moveRightButton;
    public Button jumpButton;
    public Button attackButton;
    public Button specialAttackButton;
    public Button slideButton;
    
    [Header("Key Display Text")]
    public TextMeshProUGUI moveUpText;
    public TextMeshProUGUI moveDownText;
    public TextMeshProUGUI moveLeftText;
    public TextMeshProUGUI moveRightText;
    public TextMeshProUGUI jumpText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI specialAttackText;
    public TextMeshProUGUI slideText;
    
    [Header("UI Elements")]
    public GameObject waitingForInputPanel;
    public TextMeshProUGUI waitingForInputText;
    public Button resetToDefaultButton;
    public Button backButton;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    
    private bool isWaitingForInput = false;
    private string currentKeyBeingSet = "";
    private KeyCode tempKeyCode;
    
    // 默认按键设置
    private Dictionary<string, KeyCode> defaultKeys = new Dictionary<string, KeyCode>
    {
        {"MoveUp", KeyCode.W},
        {"MoveDown", KeyCode.S},
        {"MoveLeft", KeyCode.A},
        {"MoveRight", KeyCode.D},
        {"Jump", KeyCode.Space},
        {"Attack", KeyCode.J},
        {"SpecialAttack", KeyCode.U},
        {"Slide", KeyCode.K}
    };
    
    void Awake()
    {
        Debug.Log("SettingMenu Awake() called");
    }
    
    void OnEnable()
    {
        Debug.Log("SettingMenu OnEnable() called");
        // 每次激活时重新设置按钮监听器
        if (Application.isPlaying)
        {
            SetupButtonListeners();
            UpdateKeyDisplays();
        }
    }
    
    void Start()
    {
        Debug.Log("SettingMenu Start() called");
        
        LoadKeyBindings();
        UpdateKeyDisplays();
        SetupButtonListeners();
        
        if (waitingForInputPanel != null)
            waitingForInputPanel.SetActive(false);
            
        Debug.Log("SettingMenu initialization complete");
    }
    
    void Update()
    {
        if (isWaitingForInput)
        {
            HandleKeyInput();
        }
    }
    
    private void SetupButtonListeners()
    {
        Debug.Log("Setting up button listeners...");
        
        // 清理旧的监听器
        if (moveUpButton != null) 
        {
            moveUpButton.onClick.RemoveAllListeners();
            moveUpButton.onClick.AddListener(() => {
                Debug.Log("MoveUp button clicked via listener!");
                StartKeyBinding("MoveUp");
            });
            Debug.Log("MoveUp button listener added");
        }
        else Debug.Log("MoveUp button is null!");
        
        if (moveDownButton != null) 
        {
            moveDownButton.onClick.RemoveAllListeners();
            moveDownButton.onClick.AddListener(() => {
                Debug.Log("MoveDown button clicked via listener!");
                StartKeyBinding("MoveDown");
            });
            Debug.Log("MoveDown button listener added");
        }
        else Debug.Log("MoveDown button is null!");
        
        if (moveLeftButton != null) 
        {
            moveLeftButton.onClick.RemoveAllListeners();
            moveLeftButton.onClick.AddListener(() => {
                Debug.Log("MoveLeft button clicked via listener!");
                StartKeyBinding("MoveLeft");
            });
            Debug.Log("MoveLeft button listener added");
        }
        else Debug.Log("MoveLeft button is null!");
        
        if (moveRightButton != null) 
        {
            moveRightButton.onClick.RemoveAllListeners();
            moveRightButton.onClick.AddListener(() => {
                Debug.Log("MoveRight button clicked via listener!");
                StartKeyBinding("MoveRight");
            });
            Debug.Log("MoveRight button listener added");
        }
        else Debug.Log("MoveRight button is null!");
        
        if (jumpButton != null) 
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(() => {
                Debug.Log("Jump button clicked via listener!");
                StartKeyBinding("Jump");
            });
            Debug.Log("Jump button listener added");
        }
        else Debug.Log("Jump button is null!");
        
        if (attackButton != null) 
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(() => {
                Debug.Log("Attack button clicked via listener!");
                StartKeyBinding("Attack");
            });
            Debug.Log("Attack button listener added");
        }
        else Debug.Log("Attack button is null!");
        
        if (specialAttackButton != null) 
        {
            specialAttackButton.onClick.RemoveAllListeners();
            specialAttackButton.onClick.AddListener(() => {
                Debug.Log("SpecialAttack button clicked via listener!");
                StartKeyBinding("SpecialAttack");
            });
            Debug.Log("SpecialAttack button listener added");
        }
        else Debug.Log("SpecialAttack button is null!");
        
        if (slideButton != null) 
        {
            slideButton.onClick.RemoveAllListeners();
            slideButton.onClick.AddListener(() => {
                Debug.Log("Slide button clicked via listener!");
                StartKeyBinding("Slide");
            });
            Debug.Log("Slide button listener added");
        }
        else Debug.Log("Slide button is null!");
        
        if (resetToDefaultButton != null) 
        {
            resetToDefaultButton.onClick.RemoveAllListeners();
            resetToDefaultButton.onClick.AddListener(() => {
                Debug.Log("Reset button clicked via listener!");
                ResetToDefault();
            });
            Debug.Log("Reset button listener added");
        }
        else Debug.Log("Reset button is null!");
        
        if (backButton != null) 
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => {
                Debug.Log("Back button clicked via listener!");
                BackToPauseMenu();
            });
            Debug.Log("Back button listener added");
        }
        else Debug.Log("Back button is null!");
        
        Debug.Log("Button listeners setup complete");
    }
    
    private void StartKeyBinding(string keyName)
    {
        Debug.Log($"StartKeyBinding called for: {keyName}");
        PlayClickSound();
        isWaitingForInput = true;
        currentKeyBeingSet = keyName;
        
        if (waitingForInputPanel != null)
        {
            waitingForInputPanel.SetActive(true);
            Debug.Log("WaitingForInputPanel activated");
        }
        else
        {
            Debug.Log("WaitingForInputPanel is null!");
        }
            
        if (waitingForInputText != null)
        {
            waitingForInputText.text = $"Press a key for {GetKeyDisplayName(keyName)}...\nPress ESC to cancel";
            Debug.Log($"WaitingForInputText updated: {waitingForInputText.text}");
        }
        else
        {
            Debug.Log("WaitingForInputText is null!");
        }
    }
    
    private void HandleKeyInput()
    {
        // 取消绑定
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelKeyBinding();
            return;
        }
        
        // 检测按键输入
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                // 排除一些不适合的按键
                if (IsValidKey(key))
                {
                    SetKeyBinding(currentKeyBeingSet, key);
                    CompleteKeyBinding();
                }
                break;
            }
        }
    }
    
    private bool IsValidKey(KeyCode key)
    {
        // 排除不适合绑定的按键
        KeyCode[] invalidKeys = {
            KeyCode.Escape, KeyCode.Return, KeyCode.Tab,
            KeyCode.LeftShift, KeyCode.RightShift,
            KeyCode.LeftControl, KeyCode.RightControl,
            KeyCode.LeftAlt, KeyCode.RightAlt,
            KeyCode.LeftCommand, KeyCode.RightCommand,
            KeyCode.LeftWindows, KeyCode.RightWindows
        };
        
        foreach (KeyCode invalid in invalidKeys)
        {
            if (key == invalid) return false;
        }
        
        return true;
    }
    
    private void SetKeyBinding(string keyName, KeyCode keyCode)
    {
        // 检查是否与其他按键冲突
        foreach (var kvp in GetCurrentKeyBindings())
        {
            if (kvp.Value == keyCode && kvp.Key != keyName)
            {
                // 如果有冲突，可以选择交换或者提示用户
                Debug.Log($"Key {keyCode} is already bound to {kvp.Key}");
                break;
            }
        }
        
        // 保存按键绑定
        PlayerPrefs.SetString($"KeyBinding_{keyName}", keyCode.ToString());
        PlayerPrefs.Save();
        
        tempKeyCode = keyCode;
    }
    
    private void CompleteKeyBinding()
    {
        PlayClickSound();
        isWaitingForInput = false;
        currentKeyBeingSet = "";
        
        if (waitingForInputPanel != null)
            waitingForInputPanel.SetActive(false);
            
        UpdateKeyDisplays();
        
        Debug.Log($"Key binding updated: {currentKeyBeingSet} = {tempKeyCode}");
    }
    
    private void CancelKeyBinding()
    {
        PlayClickSound();
        isWaitingForInput = false;
        currentKeyBeingSet = "";
        
        if (waitingForInputPanel != null)
            waitingForInputPanel.SetActive(false);
    }
    
    private void LoadKeyBindings()
    {
        // 如果没有保存的按键绑定，使用默认值
        foreach (var kvp in defaultKeys)
        {
            if (!PlayerPrefs.HasKey($"KeyBinding_{kvp.Key}"))
            {
                PlayerPrefs.SetString($"KeyBinding_{kvp.Key}", kvp.Value.ToString());
            }
        }
        PlayerPrefs.Save();
    }
    
    private Dictionary<string, KeyCode> GetCurrentKeyBindings()
    {
        Dictionary<string, KeyCode> currentBindings = new Dictionary<string, KeyCode>();
        
        foreach (var kvp in defaultKeys)
        {
            string savedKey = PlayerPrefs.GetString($"KeyBinding_{kvp.Key}", kvp.Value.ToString());
            if (Enum.TryParse(savedKey, out KeyCode keyCode))
            {
                currentBindings[kvp.Key] = keyCode;
            }
            else
            {
                currentBindings[kvp.Key] = kvp.Value;
            }
        }
        
        return currentBindings;
    }
    
    private void UpdateKeyDisplays()
    {
        var currentBindings = GetCurrentKeyBindings();
        
        if (moveUpText != null && currentBindings.ContainsKey("MoveUp"))
            moveUpText.text = GetKeyDisplayString(currentBindings["MoveUp"]);
        if (moveDownText != null && currentBindings.ContainsKey("MoveDown"))
            moveDownText.text = GetKeyDisplayString(currentBindings["MoveDown"]);
        if (moveLeftText != null && currentBindings.ContainsKey("MoveLeft"))
            moveLeftText.text = GetKeyDisplayString(currentBindings["MoveLeft"]);
        if (moveRightText != null && currentBindings.ContainsKey("MoveRight"))
            moveRightText.text = GetKeyDisplayString(currentBindings["MoveRight"]);
        if (jumpText != null && currentBindings.ContainsKey("Jump"))
            jumpText.text = GetKeyDisplayString(currentBindings["Jump"]);
        if (attackText != null && currentBindings.ContainsKey("Attack"))
            attackText.text = GetKeyDisplayString(currentBindings["Attack"]);
        if (specialAttackText != null && currentBindings.ContainsKey("SpecialAttack"))
            specialAttackText.text = GetKeyDisplayString(currentBindings["SpecialAttack"]);
        if (slideText != null && currentBindings.ContainsKey("Slide"))
            slideText.text = GetKeyDisplayString(currentBindings["Slide"]);
    }
    
    private string GetKeyDisplayString(KeyCode keyCode)
    {
        // 将按键代码转换为更友好的显示文本
        switch (keyCode)
        {
            case KeyCode.Space: return "Space";
            case KeyCode.LeftShift: return "L-Shift";
            case KeyCode.RightShift: return "R-Shift";
            case KeyCode.LeftControl: return "L-Ctrl";
            case KeyCode.RightControl: return "R-Ctrl";
            case KeyCode.Mouse0: return "LMB";
            case KeyCode.Mouse1: return "RMB";
            case KeyCode.Mouse2: return "MMB";
            default: return keyCode.ToString();
        }
    }
    
    private string GetKeyDisplayName(string keyName)
    {
        switch (keyName)
        {
            case "MoveUp": return "Move Up";
            case "MoveDown": return "Move Down";
            case "MoveLeft": return "Move Left";
            case "MoveRight": return "Move Right";
            case "Jump": return "Jump";
            case "Attack": return "Attack";
            case "SpecialAttack": return "Special Attack";
            case "Slide": return "Slide";
            default: return keyName;
        }
    }
    
    private void ResetToDefault()
    {
        PlayClickSound();
        
        // 重置所有按键到默认值
        foreach (var kvp in defaultKeys)
        {
            PlayerPrefs.SetString($"KeyBinding_{kvp.Key}", kvp.Value.ToString());
        }
        PlayerPrefs.Save();
        
        UpdateKeyDisplays();
        Debug.Log("Key bindings reset to default");
    }
    
    private void BackToPauseMenu()
    {
        PlayClickSound();
        
        // 返回暂停菜单
        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
        if (pauseMenu != null)
        {
            pauseMenu.BackToMainPause();
        }
    }
    
    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
    
    // 公共方法供其他脚本调用获取按键绑定
    public static KeyCode GetKeyBinding(string keyName)
    {
        string savedKey = PlayerPrefs.GetString($"KeyBinding_{keyName}");
        if (Enum.TryParse(savedKey, out KeyCode keyCode))
        {
            return keyCode;
        }
        
        // 如果没有找到，返回默认值
        Dictionary<string, KeyCode> defaults = new Dictionary<string, KeyCode>
        {
            {"MoveUp", KeyCode.W},
            {"MoveDown", KeyCode.S},
            {"MoveLeft", KeyCode.A},
            {"MoveRight", KeyCode.D},
            {"Jump", KeyCode.Space},
            {"Attack", KeyCode.J},
            {"SpecialAttack", KeyCode.U},
            {"Slide", KeyCode.K}
        };
        
        return defaults.ContainsKey(keyName) ? defaults[keyName] : KeyCode.None;
    }
    
    // 测试方法 - 可以在Inspector中调用
    [ContextMenu("Test MoveUp Button")]
    public void TestMoveUpButton()
    {
        Debug.Log("Test button clicked!");
        StartKeyBinding("MoveUp");
    }
    
    // 强制重新设置按钮监听器
    [ContextMenu("Force Setup Button Listeners")]
    public void ForceSetupButtonListeners()
    {
        Debug.Log("Force setting up button listeners...");
        SetupButtonListeners();
    }
    
    // 检查按钮遮挡问题
    [ContextMenu("Check Button Occlusion")]
    public void CheckButtonOcclusion()
    {
        Debug.Log("=== 按钮遮挡检查 ===");
        
        // 首先检查EventSystem
        if (!CheckEventSystem())
        {
            return;
        }
        
        Button[] buttons = { moveUpButton, moveDownButton, moveLeftButton, moveRightButton,
                           jumpButton, attackButton, specialAttackButton, slideButton };
        string[] buttonNames = { "MoveUp", "MoveDown", "MoveLeft", "MoveRight",
                               "Jump", "Attack", "SpecialAttack", "Slide" };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                CheckSingleButtonOcclusion(buttons[i], buttonNames[i]);
            }
        }
    }
    
    // 检查并确保EventSystem存在
    private bool CheckEventSystem()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("场景中没有EventSystem，尝试查找或创建...");
            
            // 尝试查找EventSystem
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogWarning("未找到EventSystem，建议手动添加EventSystem到场景中");
                Debug.LogWarning("可以通过: GameObject -> UI -> Event System 来添加");
                return false;
            }
            else
            {
                Debug.Log($"找到EventSystem: {eventSystem.name}");
                return true;
            }
        }
        
        Debug.Log($"EventSystem正常: {EventSystem.current.name}");
        return true;
    }
    
    private void CheckSingleButtonOcclusion(Button button, string buttonName)
    {
        Debug.Log($"\n--- 检查 {buttonName} 按钮 ---");
        
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        
        // 检查按钮基本状态
        Debug.Log($"按钮激活状态: {button.gameObject.activeInHierarchy}");
        Debug.Log($"按钮可交互: {button.interactable}");
        Debug.Log($"按钮启用: {button.enabled}");
        
        // 检查按钮位置和大小
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Vector3 center = (corners[0] + corners[2]) / 2f;
        Vector2 size = corners[2] - corners[0];
        
        Debug.Log($"按钮世界坐标中心: {center}");
        Debug.Log($"按钮世界坐标大小: {size}");
        Debug.Log($"按钮屏幕坐标: {RectTransformUtility.WorldToScreenPoint(null, center)}");
        
        // 检查CanvasGroup影响
        CanvasGroup[] canvasGroups = button.GetComponentsInParent<CanvasGroup>();
        foreach (CanvasGroup cg in canvasGroups)
        {
            if (!cg.interactable || !cg.blocksRaycasts)
            {
                Debug.LogWarning($"发现阻挡的CanvasGroup: {cg.name} - Interactable: {cg.interactable}, BlocksRaycasts: {cg.blocksRaycasts}");
            }
        }
        
        // 检查父物体的Layout组件
        Transform parent = button.transform.parent;
        while (parent != null)
        {
            if (parent.GetComponent<LayoutGroup>() != null)
            {
                Debug.Log($"父物体有Layout组件: {parent.name} - {parent.GetComponent<LayoutGroup>().GetType().Name}");
            }
            parent = parent.parent;
        }
        
        // 进行Raycast测试
        PerformRaycastTest(button, buttonName, center);
    }
    
    private void PerformRaycastTest(Button button, string buttonName, Vector3 worldCenter)
    {
        // 检查EventSystem是否存在
        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem不存在! 无法进行Raycast测试。请确保场景中有EventSystem。");
            return;
        }
        
        // 创建PointerEventData
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
        
        // 执行Raycast
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        Debug.Log($"{buttonName} 按钮Raycast结果 (共{results.Count}个对象):");
        
        bool buttonFound = false;
        for (int i = 0; i < results.Count; i++)
        {
            GameObject hitObject = results[i].gameObject;
            string objectInfo = $"  {i}: {hitObject.name}";
            
            // 添加组件信息
            if (hitObject.GetComponent<Image>() != null)
                objectInfo += " [Image]";
            if (hitObject.GetComponent<Button>() != null)
                objectInfo += " [Button]";
            if (hitObject.GetComponent<TextMeshProUGUI>() != null)
                objectInfo += " [TextMeshPro]";
            
            // 添加层级信息
            objectInfo += $" (层级: {hitObject.transform.GetSiblingIndex()})";
            
            // 检查是否是目标按钮
            if (hitObject == button.gameObject)
            {
                objectInfo += " ⭐ 这是目标按钮!";
                buttonFound = true;
            }
            else if (i == 0)
            {
                objectInfo += " ❌ 这个对象遮挡了按钮!";
                
                // 详细分析遮挡对象
                AnalyzeBlockingObject(hitObject, button.gameObject);
            }
            
            Debug.Log(objectInfo);
        }
        
        if (!buttonFound)
        {
            Debug.LogError($"{buttonName} 按钮未出现在Raycast结果中，完全被遮挡!");
        }
    }
    
    private void AnalyzeBlockingObject(GameObject blockingObject, GameObject targetButton)
    {
        Debug.Log($"\n🔍 详细分析遮挡对象: {blockingObject.name}");
        
        // 检查是否是同一个Canvas
        Canvas blockingCanvas = blockingObject.GetComponentInParent<Canvas>();
        Canvas buttonCanvas = targetButton.GetComponentInParent<Canvas>();
        
        if (blockingCanvas != buttonCanvas)
        {
            Debug.Log($"不同Canvas - 遮挡对象Canvas: {blockingCanvas?.name}, 按钮Canvas: {buttonCanvas?.name}");
            if (blockingCanvas != null && buttonCanvas != null)
            {
                Debug.Log($"Canvas排序层级 - 遮挡: {blockingCanvas.sortingOrder}, 按钮: {buttonCanvas.sortingOrder}");
            }
        }
        else
        {
            Debug.Log("同一Canvas内的遮挡");
        }
        
        // 检查层级关系
        Transform blockingTransform = blockingObject.transform;
        Transform buttonTransform = targetButton.transform;
        
        // 查找共同父物体
        Transform commonParent = FindCommonParent(blockingTransform, buttonTransform);
        if (commonParent != null)
        {
            Debug.Log($"共同父物体: {commonParent.name}");
            
            // 比较在共同父物体下的层级
            Transform blockingAncestor = GetChildInParent(blockingTransform, commonParent);
            Transform buttonAncestor = GetChildInParent(buttonTransform, commonParent);
            
            if (blockingAncestor != null && buttonAncestor != null)
            {
                int blockingIndex = blockingAncestor.GetSiblingIndex();
                int buttonIndex = buttonAncestor.GetSiblingIndex();
                
                Debug.Log($"层级索引 - 遮挡对象: {blockingIndex}, 目标按钮: {buttonIndex}");
                
                if (blockingIndex > buttonIndex)
                {
                    Debug.LogWarning("遮挡对象的渲染层级高于目标按钮!");
                }
            }
        }
        
        // 检查Image组件的Raycast Target
        Image blockingImage = blockingObject.GetComponent<Image>();
        if (blockingImage != null)
        {
            Debug.Log($"遮挡对象Image设置 - Raycast Target: {blockingImage.raycastTarget}, Color: {blockingImage.color}");
            
            if (blockingImage.raycastTarget && blockingImage.color.a > 0.01f)
            {
                Debug.LogWarning("遮挡对象的Image组件启用了Raycast Target且不透明!");
            }
        }
        
        // 检查是否是布局组件
        if (blockingObject.GetComponent<LayoutGroup>() != null)
        {
            Debug.Log("遮挡对象是Layout组件");
        }
        
        // 建议解决方案
        Debug.Log("\n💡 建议解决方案:");
        if (blockingImage != null && blockingImage.raycastTarget)
        {
            Debug.Log("1. 将遮挡对象的Image.raycastTarget设为false");
        }
        Debug.Log("2. 调整UI层级，将目标按钮移到更高层级");
        Debug.Log("3. 检查是否有不必要的背景Panel遮挡");
    }
    
    private Transform FindCommonParent(Transform a, Transform b)
    {
        Transform current = a;
        while (current != null)
        {
            if (IsAncestorOf(current, b))
            {
                return current;
            }
            current = current.parent;
        }
        return null;
    }
    
    private bool IsAncestorOf(Transform ancestor, Transform descendant)
    {
        Transform current = descendant;
        while (current != null)
        {
            if (current == ancestor)
                return true;
            current = current.parent;
        }
        return false;
    }
    
    private Transform GetChildInParent(Transform descendant, Transform ancestor)
    {
        Transform current = descendant;
        while (current != null && current.parent != ancestor)
        {
            current = current.parent;
        }
        return current;
    }
    
    // 修复中间按钮被遮挡的问题
    [ContextMenu("Fix Middle Button Occlusion")]
    public void FixMiddleButtonOcclusion()
    {
        Debug.Log("=== 尝试修复中间按钮遮挡问题 ===");
        
        // 1. 检查并修复父物体的CanvasGroup设置
        CanvasGroup[] canvasGroups = GetComponentsInParent<CanvasGroup>();
        foreach (CanvasGroup cg in canvasGroups)
        {
            if (!cg.interactable || !cg.blocksRaycasts)
            {
                Debug.Log($"修复CanvasGroup: {cg.name}");
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
        
        // 2. 确保按钮在正确的层级
        Button[] middleButtons = { moveLeftButton, moveRightButton, jumpButton, attackButton };
        foreach (Button btn in middleButtons)
        {
            if (btn != null)
            {
                // 将按钮移到父物体的最后（最高层级）
                btn.transform.SetAsLastSibling();
                
                // 确保按钮的设置正确
                btn.interactable = true;
                if (btn.targetGraphic != null)
                {
                    btn.targetGraphic.raycastTarget = true;
                }
            }
        }
        
        // 3. 检查是否有不必要的遮挡Panel
        FixBackgroundPanelBlocking();
        
        Debug.Log("中间按钮遮挡问题修复完成");
    }
    
    // 修复Layout组件遮挡问题
    [ContextMenu("Fix Layout Group Blocking")]
    public void FixLayoutGroupBlocking()
    {
        Debug.Log("=== 修复Layout组件遮挡问题 ===");
        
        // 查找所有按钮的父物体中的Layout组件
        Button[] buttons = { moveUpButton, moveDownButton, moveLeftButton, moveRightButton,
                           jumpButton, attackButton, specialAttackButton, slideButton };
        string[] buttonNames = { "MoveUp", "MoveDown", "MoveLeft", "MoveRight",
                               "Jump", "Attack", "SpecialAttack", "Slide" };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                FixButtonLayoutBlocking(buttons[i], buttonNames[i]);
            }
        }
        
        Debug.Log("Layout组件遮挡修复完成！");
    }
    
    private void FixButtonLayoutBlocking(Button button, string buttonName)
    {
        Debug.Log($"修复 {buttonName} 按钮的Layout遮挡...");
        
        Transform parent = button.transform.parent;
        while (parent != null)
        {
            // 检查HorizontalLayoutGroup
            HorizontalLayoutGroup hlg = parent.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                Debug.Log($"找到HorizontalLayoutGroup: {parent.name}");
                
                // 检查父物体是否有Image且启用了raycastTarget
                Image parentImage = parent.GetComponent<Image>();
                if (parentImage != null && parentImage.raycastTarget)
                {
                    Debug.LogWarning($"Layout容器 {parent.name} 的Image.raycastTarget=true，这会遮挡按钮!");
                    Debug.Log($"将 {parent.name} 的Image.raycastTarget设为false");
                    parentImage.raycastTarget = false;
                }
                
                // 确保按钮在Layout中的设置正确
                LayoutElement layoutElement = button.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = button.gameObject.AddComponent<LayoutElement>();
                    Debug.Log($"为 {buttonName} 添加了LayoutElement组件");
                }
                
                // 确保按钮不被Layout忽略
                layoutElement.ignoreLayout = false;
            }
            
            // 检查VerticalLayoutGroup
            VerticalLayoutGroup vlg = parent.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                Debug.Log($"找到VerticalLayoutGroup: {parent.name}");
                
                Image parentImage = parent.GetComponent<Image>();
                if (parentImage != null && parentImage.raycastTarget)
                {
                    Debug.LogWarning($"Layout容器 {parent.name} 的Image.raycastTarget=true，这会遮挡按钮!");
                    Debug.Log($"将 {parent.name} 的Image.raycastTarget设为false");
                    parentImage.raycastTarget = false;
                }
            }
            
            // 检查GridLayoutGroup
            GridLayoutGroup glg = parent.GetComponent<GridLayoutGroup>();
            if (glg != null)
            {
                Debug.Log($"找到GridLayoutGroup: {parent.name}");
                
                Image parentImage = parent.GetComponent<Image>();
                if (parentImage != null && parentImage.raycastTarget)
                {
                    Debug.LogWarning($"Layout容器 {parent.name} 的Image.raycastTarget=true，这会遮挡按钮!");
                    Debug.Log($"将 {parent.name} 的Image.raycastTarget设为false");
                    parentImage.raycastTarget = false;
                }
            }
            
            parent = parent.parent;
        }
        
        // 确保按钮本身的设置正确
        button.interactable = true;
        if (button.targetGraphic != null)
        {
            button.targetGraphic.raycastTarget = true;
        }
        
        Debug.Log($"{buttonName} 按钮Layout遮挡修复完成");
    }
    
    // 一键修复所有常见的UI遮挡问题
    [ContextMenu("Fix All UI Blocking Issues")]
    public void FixAllUIBlockingIssues()
    {
        Debug.Log("=== 一键修复所有UI遮挡问题 ===");
        
        // 1. 修复Layout组件遮挡
        FixLayoutGroupBlocking();
        
        // 2. 修复背景Panel遮挡
        FixBackgroundPanelBlocking();
        
        // 3. 确保所有按钮设置正确
        EnsureButtonSettings();
        
        // 4. 重新设置按钮监听器
        SetupButtonListeners();
        
        Debug.Log("=== 所有UI遮挡问题修复完成 ===");
        Debug.Log("请测试按钮是否现在可以正常点击！");
    }
    
    private void FixBackgroundPanelBlocking()
    {
        Debug.Log("修复背景Panel遮挡...");
        
        // 查找所有可能遮挡的Image组件
        Image[] allImages = GetComponentsInChildren<Image>();
        
        foreach (Image img in allImages)
        {
            // 跳过按钮的Image
            if (img.GetComponent<Button>() != null)
                continue;
                
            // 检查是否是可能遮挡的背景Panel
            if (img.raycastTarget)
            {
                GameObject obj = img.gameObject;
                
                // 检查是否是Layout容器
                bool isLayoutContainer = obj.GetComponent<LayoutGroup>() != null;
                
                // 检查是否是大面积背景
                RectTransform rect = img.GetComponent<RectTransform>();
                bool isLargeBackground = rect.sizeDelta.magnitude > 300;
                
                // 检查名称是否包含背景相关关键词
                string name = obj.name.ToLower();
                bool isBackground = name.Contains("background") || name.Contains("panel") || 
                                  name.Contains("container") || name.Contains("row");
                
                if (isLayoutContainer || isLargeBackground || isBackground)
                {
                    Debug.Log($"修复遮挡Image: {obj.name} - 设置raycastTarget=false");
                    img.raycastTarget = false;
                }
            }
        }
    }
    
    private void EnsureButtonSettings()
    {
        Debug.Log("确保所有按钮设置正确...");
        
        Button[] buttons = { moveUpButton, moveDownButton, moveLeftButton, moveRightButton,
                           jumpButton, attackButton, specialAttackButton, slideButton };
        string[] buttonNames = { "MoveUp", "MoveDown", "MoveLeft", "MoveRight",
                               "Jump", "Attack", "SpecialAttack", "Slide" };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                Button btn = buttons[i];
                
                // 确保按钮激活和可交互
                btn.gameObject.SetActive(true);
                btn.interactable = true;
                btn.enabled = true;
                
                // 确保按钮的Image可以接收Raycast
                if (btn.targetGraphic != null)
                {
                    btn.targetGraphic.raycastTarget = true;
                }
                
                // 确保按钮在正确的层级
                btn.transform.SetAsLastSibling();
                
                Debug.Log($"{buttonNames[i]} 按钮设置已确保正确");
            }
        }
    }
    
    // 实时监控鼠标点击位置的Raycast结果
    [ContextMenu("Start Click Monitoring")]
    public void StartClickMonitoring()
    {
        StartCoroutine(MonitorMouseClicks());
    }
    
    private IEnumerator MonitorMouseClicks()
    {
        Debug.Log("开始监控鼠标点击...");
        
        // 检查EventSystem是否存在
        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem不存在! 无法监控鼠标点击。");
            yield break;
        }
        
        while (gameObject.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                Debug.Log($"\n🖱️ 鼠标点击位置: {mousePos}");
                
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = mousePos;
                
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                
                Debug.Log($"点击位置的Raycast结果 (共{results.Count}个):");
                for (int i = 0; i < results.Count; i++)
                {
                    GameObject obj = results[i].gameObject;
                    string info = $"  {i}: {obj.name}";
                    
                    if (obj.GetComponent<Button>() != null)
                        info += " [Button] ⭐";
                    if (obj.GetComponent<Image>() != null)
                        info += " [Image]";
                    if (obj.GetComponent<TextMeshProUGUI>() != null)
                        info += " [Text]";
                        
                    Debug.Log(info);
                    
                    if (i == 0 && obj.GetComponent<Button>() == null)
                    {
                        Debug.LogWarning($"❌ 点击被 {obj.name} 拦截了!");
                    }
                }
            }
            
            yield return null;
        }
    }
    
    // 创建EventSystem的方法
    [ContextMenu("Create EventSystem")]
    public void CreateEventSystem()
    {
        if (EventSystem.current != null)
        {
            Debug.Log($"EventSystem已存在: {EventSystem.current.name}");
            return;
        }
        
        // 创建EventSystem GameObject
        GameObject eventSystemGO = new GameObject("EventSystem");
        EventSystem eventSystem = eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();
        
        Debug.Log("已创建新的EventSystem");
    }
    
    // 完整的UI系统检查
    [ContextMenu("Check UI System")]
    public void CheckUISystem()
    {
        Debug.Log("=== UI系统检查 ===");
        
        // 1. 检查EventSystem
        CheckEventSystem();
        
        // 2. 检查Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("未找到Canvas! SettingMenu必须是Canvas的子物体");
        }
        else
        {
            Debug.Log($"找到Canvas: {canvas.name}");
            
            // 检查Canvas组件
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                Debug.LogWarning("Canvas缺少GraphicRaycaster组件");
            }
            else
            {
                Debug.Log("Canvas具有GraphicRaycaster组件");
            }
        }
        
        // 3. 检查按钮状态
        Debug.Log("\n--- 按钮状态检查 ---");
        Button[] buttons = { moveUpButton, moveDownButton, moveLeftButton, moveRightButton,
                           jumpButton, attackButton, specialAttackButton, slideButton };
        string[] buttonNames = { "MoveUp", "MoveDown", "MoveLeft", "MoveRight",
                               "Jump", "Attack", "SpecialAttack", "Slide" };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
            {
                Debug.LogWarning($"{buttonNames[i]} 按钮未分配!");
            }
            else
            {
                Debug.Log($"{buttonNames[i]}: 激活={buttons[i].gameObject.activeInHierarchy}, 可交互={buttons[i].interactable}");
            }
        }
    }
}
