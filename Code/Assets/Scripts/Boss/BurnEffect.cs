using System.Collections;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    [Header("ȼ������")]
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float tickInterval = 0.5f;
    
    [Header("�Ӿ�Ч��")]
    [SerializeField] private Color burnColor = Color.red;
    [SerializeField] private float flashIntensity = 0.3f;
    
    private HeroLife heroLife;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float remainingDuration;
    private bool isActive = false;
    
    void Start()
    {
        Initialize(damagePerSecond, duration);
    }
    
    /// <summary>
    /// ��ʼ��ȼ��Ч��
    /// </summary>
    /// <param name="damagePerSec">ÿ���˺�</param>
    /// <param name="burnDuration">ȼ�ճ���ʱ��</param>
    public void Initialize(float damagePerSec, float burnDuration)
    {
        damagePerSecond = damagePerSec;
        duration = burnDuration;
        remainingDuration = burnDuration;
        
        // ��ȡ���
        heroLife = GetComponent<HeroLife>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        StartBurnEffect();
    }
    
    /// <summary>
    /// ����ȼ�ճ���ʱ��
    /// </summary>
    public void ResetBurnDuration()
    {
        remainingDuration = duration;
        Debug.Log($"ȼ��Ч������: {remainingDuration} ��ʣ��");
    }
    
    /// <summary>
    /// ˢ��ȼ��Ч��
    /// </summary>
    /// <param name="newDuration">�µĳ���ʱ��</param>
    public void RefreshBurn(float newDuration)
    {
        remainingDuration = newDuration;
        Debug.Log($"ȼ��Ч��ˢ��: {remainingDuration} ��ʣ��");
    }
    
    /// <summary>
    /// ��ʼȼ��Ч��
    /// </summary>
    private void StartBurnEffect()
    {
        if (isActive) return;
        
        isActive = true;
        StartCoroutine(BurnCoroutine());
        StartCoroutine(VisualEffectCoroutine());
        
        Debug.Log($"ȼ��Ч����ʼ: {damagePerSecond} �˺�/�룬���� {duration} ��");
    }
    
    /// <summary>
    /// ȼ���˺�Э��
    /// </summary>
    private IEnumerator BurnCoroutine()
    {
        while (remainingDuration > 0 && heroLife != null && !heroLife.IsDead)
        {
            // ����˺�
            float damageThisTick = damagePerSecond * tickInterval;
            heroLife.TakeDamage(Mathf.RoundToInt(damageThisTick));
            
            Debug.Log($"ȼ���˺�: {damageThisTick}��ʣ��ʱ��: {remainingDuration}");
            
            // �ȴ���һ���˺�
            yield return new WaitForSeconds(tickInterval);
            remainingDuration -= tickInterval;
        }
        
        EndBurnEffect();
    }
    
    /// <summary>
    /// �Ӿ�Ч��Э��
    /// </summary>
    private IEnumerator VisualEffectCoroutine()
    {
        while (remainingDuration > 0 && spriteRenderer != null)
        {
            // ��˸��ɫ
            spriteRenderer.color = Color.Lerp(originalColor, burnColor, flashIntensity);
            yield return new WaitForSeconds(0.1f);
            
            // �ָ�ԭɫ
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
        
        // ȷ���ָ�ԭɫ
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    /// <summary>
    /// ����ȼ��Ч��
    /// </summary>
    private void EndBurnEffect()
    {
        isActive = false;
        
        // �ָ�ԭɫ
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        Debug.Log("ȼ��Ч������");
        
        // ���ٴ����
        Destroy(this);
    }
    
    void Update()
    {
        // ��ȫ��� - ���Ӣ�����������ȼ��
        if (heroLife != null && heroLife.IsDead)
        {
            EndBurnEffect();
        }
    }
    
    void OnDestroy()
    {
        // ����ʱ�ָ�ԭɫ
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    #region ���Է���
    
    /// <summary>
    /// ����ȼ��Ч��
    /// </summary>
    [ContextMenu("����ȼ��Ч��")]
    public void TestBurnEffect()
    {
        Initialize(8f, 3f);
        Debug.Log("ȼ��Ч�����Կ�ʼ");
    }
    
    /// <summary>
    /// ����ֹͣȼ��Ч��
    /// </summary>
    [ContextMenu("ֹͣȼ��Ч��")]
    public void StopBurnEffect()
    {
        remainingDuration = 0f;
        EndBurnEffect();
        Debug.Log("ȼ��Ч��ǿ��ֹͣ");
    }
    
    #endregion
}