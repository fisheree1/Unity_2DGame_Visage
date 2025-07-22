using System.Collections;
using UnityEngine;
using TMPro;

public class SkillNotification : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeSpeed = 1f;
    
    private CanvasGroup canvasGroup;
    private Coroutine currentNotification;
    
    // 单例模式
    public static SkillNotification Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            canvasGroup = GetComponent<CanvasGroup>();
            
            // 初始化时隐藏通知
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 显示技能学习通知
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <param name="description">技能描述</param>
    public void ShowSkillLearned(string skillName, string description)
    {
        // 如果有正在显示的通知，先停止它
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }
        
        currentNotification = StartCoroutine(DisplayNotification(skillName, description));
    }
    
    private IEnumerator DisplayNotification(string skillName, string description)
    {
        // 设置文本
        if (skillNameText != null)
        {
            skillNameText.text = skillName;
        }
        
        if (skillDescriptionText != null)
        {
            skillDescriptionText.text = description;
        }
        
        // 显示面板
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }
        
        // 淡入效果
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeIn());
        }
        
        // 等待显示时间
        yield return new WaitForSeconds(displayDuration);
        
        // 淡出效果
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // 隐藏面板
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
        
        currentNotification = null;
    }
    
    private IEnumerator FadeIn()
    {
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
