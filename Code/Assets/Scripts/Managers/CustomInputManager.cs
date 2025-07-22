using UnityEngine;

public class CustomInputManager : MonoBehaviour
{
    public static CustomInputManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 移动输入
    public bool GetMoveUp()
    {
        return Input.GetKey(SettingMenu.GetKeyBinding("MoveUp"));
    }
    
    public bool GetMoveDown()
    {
        return Input.GetKey(SettingMenu.GetKeyBinding("MoveDown"));
    }
    
    public bool GetMoveLeft()
    {
        return Input.GetKey(SettingMenu.GetKeyBinding("MoveLeft"));
    }
    
    public bool GetMoveRight()
    {
        return Input.GetKey(SettingMenu.GetKeyBinding("MoveRight"));
    }
    
    // 动作输入
    public bool GetJumpDown()
    {
        return Input.GetKeyDown(SettingMenu.GetKeyBinding("Jump"));
    }
    
    public bool GetJump()
    {
        return Input.GetKey(SettingMenu.GetKeyBinding("Jump"));
    }
    
    public bool GetJumpUp()
    {
        return Input.GetKeyUp(SettingMenu.GetKeyBinding("Jump"));
    }
    
    public bool GetAttackDown()
    {
        return Input.GetKeyDown(SettingMenu.GetKeyBinding("Attack"));
    }
    
    public bool GetSpecialAttackDown()
    {
        return Input.GetKeyDown(SettingMenu.GetKeyBinding("SpecialAttack"));
    }
    
    public bool GetSlideDown()
    {
        return Input.GetKeyDown(SettingMenu.GetKeyBinding("Slide"));
    }
    
    // 获取水平和垂直输入（用于移动）
    public float GetHorizontalInput()
    {
        float horizontal = 0f;
        if (GetMoveLeft()) horizontal -= 1f;
        if (GetMoveRight()) horizontal += 1f;
        return horizontal;
    }
    
    public float GetVerticalInput()
    {
        float vertical = 0f;
        if (GetMoveDown()) vertical -= 1f;
        if (GetMoveUp()) vertical += 1f;
        return vertical;
    }
    
    // 显示当前按键绑定信息（调试用）
    public void LogCurrentKeyBindings()
    {
        Debug.Log($"Current Key Bindings:");
        Debug.Log($"Move Up: {SettingMenu.GetKeyBinding("MoveUp")}");
        Debug.Log($"Move Down: {SettingMenu.GetKeyBinding("MoveDown")}");
        Debug.Log($"Move Left: {SettingMenu.GetKeyBinding("MoveLeft")}");
        Debug.Log($"Move Right: {SettingMenu.GetKeyBinding("MoveRight")}");
        Debug.Log($"Jump: {SettingMenu.GetKeyBinding("Jump")}");
        Debug.Log($"Attack: {SettingMenu.GetKeyBinding("Attack")}");
        Debug.Log($"Special Attack: {SettingMenu.GetKeyBinding("SpecialAttack")}");
        Debug.Log($"Slide: {SettingMenu.GetKeyBinding("Slide")}");
    }
}
