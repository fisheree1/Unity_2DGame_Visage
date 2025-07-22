using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class yanjiang : MonoBehaviour
{
    [SerializeField]
    private int damagePerTick = 1; // 每次扣血的伤害值
    
    [SerializeField]
    private float damageInterval = 0.5f; // 扣血间隔时间（秒）
    
    private bool isPlayerInTrap = false; // 玩家是否在陷阱中
    private HeroLife heroLife; // 玩家生命组件引用
    private Coroutine damageCoroutine; // 持续伤害协程引用

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered continuous damage trap");
            heroLife = collision.gameObject.GetComponent<HeroLife>();
            
            if (heroLife != null)
            {
                isPlayerInTrap = true;
                // 开始持续伤害协程
                damageCoroutine = StartCoroutine(ContinuousDamage());
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player left continuous damage trap");
            isPlayerInTrap = false;
            
            // 停止持续伤害协程
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
            
            heroLife = null;
        }
    }
    
    private IEnumerator ContinuousDamage()
    {
        while (isPlayerInTrap && heroLife != null)
        {
            // 检查玩家是否无敌
            if (!heroLife.IsInvulnerable)
            {
                heroLife.TakeDamage(damagePerTick);
                Debug.Log($"Continuous damage dealt: {damagePerTick}");
            }
            
            // 等待下一次伤害间隔
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
