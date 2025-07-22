using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private GameObject activeIndicator;
    [SerializeField] private GameObject inactiveIndicator;
    
    [Header("Effects")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private ParticleSystem activationEffect;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        UpdateVisuals();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家
        HeroLife heroLife = other.GetComponent<HeroLife>();
        if (heroLife != null && !isActive)
        {
            ActivateCheckpoint(heroLife);
        }
    }
    
    private void ActivateCheckpoint(HeroLife heroLife)
    {
        isActive = true;
        
        // 设置重生点
        heroLife.SetRespawnPoint(transform.position);
        
        // 播放音效
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // 播放粒子效果
        if (activationEffect != null)
        {
            activationEffect.Play();
        }
        
        // 更新视觉效果
        UpdateVisuals();
        
        Debug.Log($"Checkpoint activated at {transform.position}");
        
        // 禁用其他检查点（如果需要）
        DeactivateOtherCheckpoints();
    }
    
    private void DeactivateOtherCheckpoints()
    {
        // 找到所有其他检查点并禁用它们
        Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();
        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            if (checkpoint != this && checkpoint.isActive)
            {
                checkpoint.isActive = false;
                checkpoint.UpdateVisuals();
            }
        }
    }
    
    private void UpdateVisuals()
    {
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(isActive);
        }
        
        if (inactiveIndicator != null)
        {
            inactiveIndicator.SetActive(!isActive);
        }
    }
    
    // 在编辑器中可视化触发区域
    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = isActive ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
