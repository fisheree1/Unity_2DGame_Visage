using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDecreaseVelocity : MonoBehaviour
{
    [SerializeField]
    private float speedMultiplier = 0.7f; // 移动速度倍率
    
    private bool isPlayerInZone = false; // 玩家是否在区域中
    private HeroMovement heroMovement; // 玩家移动组件引用
    private Rigidbody2D heroRigidbody; // 玩家刚体组件引用
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 移除了持续应用速度限制的代码，改为直接设置HeroMovement的速度倍率
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered speed reduction zone");
            
            // 获取玩家组件
            heroMovement = collision.gameObject.GetComponent<HeroMovement>();
            heroRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
            
            if (heroMovement != null && heroRigidbody != null)
            {
                isPlayerInZone = true;
                // 直接设置HeroMovement的速度倍率
                heroMovement.SetExternalSpeedMultiplier(speedMultiplier);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player left speed reduction zone");
            
            if (heroMovement != null)
            {
                // 恢复正常速度
                heroMovement.ResetExternalSpeedMultiplier();
            }
            
            isPlayerInZone = false;
            
            // 清理引用
            heroMovement = null;
            heroRigidbody = null;
        }
    }
    
    // 公共方法供其他脚本调用
    public bool IsPlayerInZone()
    {
        return isPlayerInZone;
    }
    
    public float GetSpeedMultiplier()
    {
        return speedMultiplier;
    }
}
