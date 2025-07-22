using UnityEngine;

/// <summary>
/// GameOverUI管理器，用于简化GameOverUI的使用
/// 这个脚本可以挂载到GameOverUI预制体上
/// </summary>
public class GameOverUIManager : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnAwake = true;
    
    private GameOverUI gameOverUI;
    
    void Awake()
    {
        if (autoSetupOnAwake)
        {
            SetupGameOverUI();
        }
    }
    
    private void SetupGameOverUI()
    {
        gameOverUI = GetComponent<GameOverUI>();
        if (gameOverUI == null)
        {
            gameOverUI = gameObject.AddComponent<GameOverUI>();
        }
    }
    
    /// <summary>
    /// 显示GameOverUI
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.Show();
        }
        else
        {
            // 备用方案：直接显示GameObject
            gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// 隐藏GameOverUI
    /// </summary>
    public void HideGameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.Hide();
        }
        else
        {
            // 备用方案：直接隐藏GameObject
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 检查GameOverUI是否正在显示
    /// </summary>
    public bool IsShowing()
    {
        if (gameOverUI != null)
        {
            return gameOverUI.IsShowing;
        }
        
        return gameObject.activeInHierarchy;
    }
}
