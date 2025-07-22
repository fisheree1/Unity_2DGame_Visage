using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Skill
{
    public string skillName;
    public bool isUnlocked;
    public string description;
    
    public Skill(string name, bool unlocked = false, string desc = "")
    {
        skillName = name;
        isUnlocked = unlocked;
        description = desc;
    }
}

public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private List<Skill> availableSkills = new List<Skill>();
    
    // 单例模式
    public static SkillManager Instance { get; private set; }
    
    // 技能名称常量
    public const string DOUBLE_JUMP = "DoubleJump";
    
    // 事件
    public System.Action<string> OnSkillUnlocked;
    
    void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSkills();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSkills()
    {
        // 初始化所有技能
        if (availableSkills.Count == 0)
        {
            availableSkills.Add(new Skill(DOUBLE_JUMP, false, "Allows the hero to jump a second time in mid-air"));
        }
        
        Debug.Log("Skill Manager initialized with " + availableSkills.Count + " skills");
    }
    
    /// <summary>
    /// 解锁指定技能
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <returns>是否成功解锁</returns>
    public bool UnlockSkill(string skillName)
    {
        Skill skill = GetSkill(skillName);
        if (skill != null && !skill.isUnlocked)
        {
            skill.isUnlocked = true;
            Debug.Log($"Skill unlocked: {skillName}");
            OnSkillUnlocked?.Invoke(skillName);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 检查技能是否已解锁
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <returns>是否已解锁</returns>
    public bool IsSkillUnlocked(string skillName)
    {
        Skill skill = GetSkill(skillName);
        return skill != null && skill.isUnlocked;
    }
    
    /// <summary>
    /// 获取技能信息
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <returns>技能对象</returns>
    public Skill GetSkill(string skillName)
    {
        return availableSkills.Find(s => s.skillName == skillName);
    }
    
    /// <summary>
    /// 获取所有技能列表
    /// </summary>
    /// <returns>技能列表</returns>
    public List<Skill> GetAllSkills()
    {
        return new List<Skill>(availableSkills);
    }
    
    /// <summary>
    /// 重置所有技能状态（用于调试）
    /// </summary>
    public void ResetAllSkills()
    {
        foreach (var skill in availableSkills)
        {
            skill.isUnlocked = false;
        }
        Debug.Log("All skills have been reset");
    }
    
    /// <summary>
    /// 解锁所有技能（用于调试）
    /// </summary>
    public void UnlockAllSkills()
    {
        foreach (var skill in availableSkills)
        {
            if (!skill.isUnlocked)
            {
                UnlockSkill(skill.skillName);
            }
        }
        Debug.Log("All skills have been unlocked");
    }
}
