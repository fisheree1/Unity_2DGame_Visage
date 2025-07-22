using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Health Pickup Settings")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private float respawnTime = 10f;
    
    [Header("Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private GameObject visualObject;
    
    [Header("Animation")]
    [SerializeField] private bool rotateObject = true;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool bobUpDown = true;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 2f;
    
    private AudioSource audioSource;
    private Vector3 originalPosition;
    private bool isAvailable = true;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        originalPosition = transform.position;
        
        // 确保是触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void Update()
    {
        if (!isAvailable) return;
        
        // 旋转动画
        if (rotateObject && visualObject != null)
        {
            visualObject.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        
        // 上下浮动动画
        if (bobUpDown)
        {
            float newY = originalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAvailable) return;
        
        // 检查是否是玩家
        HeroLife heroLife = other.GetComponent<HeroLife>();
        if (heroLife != null && !heroLife.IsDead)
        {
            // 检查是否需要治疗
            if (heroLife.CurrentHealth < heroLife.MaxHealth)
            {
                // 治疗玩家
                heroLife.Heal(healAmount);
                
                // 播放音效
                if (audioSource != null && pickupSound != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }
                
                // 播放粒子效果
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                Debug.Log($"Health pickup collected! Healed {healAmount} HP");
                
                // 处理道具拾取后的状态
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
                else
                {
                    StartCoroutine(RespawnAfterTime());
                }
            }
            else
            {
                Debug.Log("Health is already full!");
            }
        }
    }
    
    private System.Collections.IEnumerator RespawnAfterTime()
    {
        isAvailable = false;
        
        // 隐藏道具
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        
        // 禁用碰撞器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // 等待重生时间
        yield return new WaitForSeconds(respawnTime);
        
        // 重新启用道具
        isAvailable = true;
        
        if (visualObject != null)
        {
            visualObject.SetActive(true);
        }
        
        if (col != null)
        {
            col.enabled = true;
        }
        
        Debug.Log("Health pickup respawned!");
    }
    
    // 在编辑器中可视化拾取区域
    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
