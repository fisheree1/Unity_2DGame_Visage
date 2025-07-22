using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class CheckPoint : MonoBehaviour
{
    [Header("CheckPoint Settings")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private float activationRadius = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("Visual Effects")]
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color availableColor = Color.yellow;
    [SerializeField] private float glowIntensity = 0.5f;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TextMeshProUGUI interactionText;
    
    [Header("Camera References")]
    [SerializeField] private CinemachineVirtualCamera associatedCamera;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    // State
    private bool playerInRange = false;
    private HeroLife heroLife;
    private CameraManager cameraManager;

    void Start()
    {
        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = inactiveColor;
        }
        
        // 查找 CameraManager
        cameraManager = CameraManager.instance;
        if (cameraManager == null)
        {
            Debug.LogWarning("CameraManager instance not found!");
        }
        
        // 如果没有手动设置关联相机，尝试自动查找当前激活的相机
        if (associatedCamera == null && cameraManager != null)
        {
            // 获取当前激活的虚拟相机
            CinemachineVirtualCamera[] allCameras = FindObjectsOfType<CinemachineVirtualCamera>();
            foreach (var cam in allCameras)
            {
                if (cam.enabled)
                {
                    associatedCamera = cam;
                    break;
                }
            }
        }
        
        // 隐藏交互UI
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
    }
    
    void Update()
    {
        // 检查玩家是否在范围内
        CheckPlayerInRange();
        
        // 处理交互输入
        if (playerInRange)
        {
            ActivateCheckPoint();
        }
        
        // 更新视觉效果
        UpdateVisualEffects();
    }
    
    private void CheckPlayerInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 2D游戏中忽略Z轴的距离计算
            Vector2 checkPointPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y);
            float distance = Vector2.Distance(checkPointPos, playerPos);
            
            bool wasInRange = playerInRange;
            playerInRange = distance <= activationRadius;
            
            // 进入范围时获取HeroLife组件并显示UI
            if (playerInRange && !wasInRange)
            {
                heroLife = player.GetComponent<HeroLife>();
                OnPlayerEnterRange();
            }
            // 离开范围时隐藏UI
            else if (!playerInRange && wasInRange)
            {
                OnPlayerExitRange();
            }
        }
        else
        {
            if (playerInRange)
            {
                OnPlayerExitRange();
            }
            playerInRange = false;
        }
    }
    
    private void OnPlayerEnterRange()
    {
        // 只在未激活时显示交互提示
        if (interactionUI != null && !isActivated)
        {
            interactionUI.SetActive(true);
        }
    }
    
    private void OnPlayerExitRange()
    {
        // 隐藏交互提示
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
    
    private void ActivateCheckPoint()
    {
        // 检查是否已激活或没有HeroLife组件
        if (isActivated || heroLife == null) return;
        
        // 激活检查点
        isActivated = true;
        
        // 设置复活点（这会自动保存血瓶数量）
        heroLife.SetRespawnPoint(transform.position);
        cameraManager = FindObjectOfType<CameraManager>();
        cameraManager.SetRespawnCamera(cameraManager.GetCurrentCamera());
        
        Debug.Log($"CheckPoint activated at {transform.position} with HP count: {HeroHP.HPCount}");
        
        // 隐藏交互提示
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
    
    private void UpdateVisualEffects()
    {
        if (spriteRenderer == null) return;
        
        if (isActivated)
        {
            // 激活状态：绿色发光
            spriteRenderer.color = Color.Lerp(activeColor, Color.white, 
                Mathf.Sin(Time.time * 2f) * glowIntensity + glowIntensity);
        }
        else if (playerInRange)
        {
            // 可交互状态：黄色发光
            spriteRenderer.color = Color.Lerp(availableColor, Color.white, 
                Mathf.Sin(Time.time * 3f) * glowIntensity + glowIntensity);
        }
        else
        {
            // 未激活状态：灰色
            spriteRenderer.color = inactiveColor;
        }
    }
    
    // 可视化检查点范围（编辑器中显示）
    private void OnDrawGizmosSelected()
    {
        // 绘制激活范围
        Gizmos.color = isActivated ? activeColor : availableColor;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
        
        // 绘制检查点标记
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
    
    // 公共属性
    public bool IsActivated => isActivated;
    public Vector3 GetRespawnPosition() => transform.position;
}
