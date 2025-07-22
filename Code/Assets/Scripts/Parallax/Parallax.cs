using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("相机对象")]
    public Transform cameraTransform;

    [Tooltip("视差乘数。值越大，背景移动越快。")]
    public float parallaxMultiplierX = 0.5f;
    public float parallaxMultiplierY = 0.5f;

    [Tooltip("是否启用视差效果。")]
    public bool enableXParallax = true;
    public bool enableYParallax = true;

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            // 尝试获取主相机
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogError("ParallaxBackground: 没有找到相机！请手动设置相机对象。");
                enabled = false; // 禁用脚本
                return;
            }
        }
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        // 计算相机移动的位移
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // 根据视差乘数移动背景
        if (enableXParallax)
            transform.position += new Vector3(deltaMovement.x * parallaxMultiplierX, 0, 0);
        if (enableYParallax)
            transform.position += new Vector3(0, deltaMovement.y * parallaxMultiplierY, 0);
        // 更新上一次相机位置
            lastCameraPosition = cameraTransform.position;
    }
}