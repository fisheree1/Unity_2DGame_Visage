using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HeroStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float staminaRegenRate = 10f; // 每秒恢复的体力
    [SerializeField] private float regenDelay = 2f; // 消耗体力后多久开始恢复
    
    [Header("Stamina Costs")]
    [SerializeField] private float basicAttackCost = 5f;
    [SerializeField] private float specialAttackCost = 20f;
    [SerializeField] private float slideCost = 15f;
    [SerializeField] private float upAttackCost = 8f;
    [SerializeField] private float downAttackCost = 8f;
    [SerializeField] private float jumpCost = 10f;
    [SerializeField] private float doubleJumpCost = 15f;
    
    [Header("Low Stamina Settings")]
    [SerializeField] private float lowStaminaThreshold = 20f; // 低体力阈值
    [SerializeField] private bool enableLowStaminaEffects = true;
    
    // Events
    [Header("Events")]
    public UnityEvent OnStaminaChanged;
    public UnityEvent OnStaminaEmpty;
    public UnityEvent OnStaminaFull;
    public UnityEvent OnLowStamina;
    public UnityEvent OnStaminaRecovered;
    
    // Private variables
    private float regenTimer = 0f;
    private bool isRegenerating = false;
    private bool wasLowStamina = false;
    
    // Properties
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercentage => currentStamina / maxStamina;
    public bool IsStaminaEmpty => currentStamina <= 0f;
    public bool IsStaminaFull => currentStamina >= maxStamina;
    public bool IsLowStamina => currentStamina <= lowStaminaThreshold;
    
    void Start()
    {
        // 初始化体力为满值
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke();
    }

    void Update()
    {
        HandleStaminaRegeneration();
        CheckLowStaminaState();
    }
    
    private void HandleStaminaRegeneration()
    {
        if (currentStamina < maxStamina)
        {
            regenTimer += Time.deltaTime;
            
            if (regenTimer >= regenDelay)
            {
                if (!isRegenerating)
                {
                    isRegenerating = true;
                }
                
                // 恢复体力
                float regenAmount = staminaRegenRate * Time.deltaTime;
                AddStamina(regenAmount);
            }
        }
        else
        {
            isRegenerating = false;
            regenTimer = 0f;
        }
    }
    
    private void CheckLowStaminaState()
    {
        bool isCurrentlyLowStamina = IsLowStamina && !IsStaminaEmpty;
        
        if (isCurrentlyLowStamina && !wasLowStamina)
        {
            OnLowStamina?.Invoke();
            Debug.Log("Warning: Low Stamina!");
        }
        else if (!isCurrentlyLowStamina && wasLowStamina)
        {
            OnStaminaRecovered?.Invoke();
            Debug.Log("Stamina Recovered!");
        }
        
        wasLowStamina = isCurrentlyLowStamina;
    }
    
    // 公共方法：消耗体力
    public bool ConsumeStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina = Mathf.Max(0f, currentStamina - amount);
            regenTimer = 0f; // 重置恢复计时器
            isRegenerating = false;
            
            OnStaminaChanged?.Invoke();
            
            if (IsStaminaEmpty)
            {
                OnStaminaEmpty?.Invoke();
                Debug.Log("Stamina Empty!");
            }
            return true;
        }
        
        return false;
    }
    
    // 公共方法：恢复体力
    public void AddStamina(float amount)
    {
        float oldStamina = currentStamina;
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        
        if (oldStamina != currentStamina)
        {
            OnStaminaChanged?.Invoke();
            
            if (IsStaminaFull && oldStamina < maxStamina)
            {
                OnStaminaFull?.Invoke();
                Debug.Log("Stamina Full!");
            }
        }
    }
    
    // 专用方法：各种行动的体力消耗
    public bool CanPerformBasicAttack() => currentStamina >= basicAttackCost;
    public bool ConsumeBasicAttackStamina() => ConsumeStamina(basicAttackCost);
    
    public bool CanPerformSpecialAttack() => currentStamina >= specialAttackCost;
    public bool ConsumeSpecialAttackStamina() => ConsumeStamina(specialAttackCost);
    
    public bool CanPerformSlide() => currentStamina >= slideCost;
    public bool ConsumeSlideStamina() => ConsumeStamina(slideCost);
    
    public bool CanPerformUpAttack() => currentStamina >= upAttackCost;
    public bool ConsumeUpAttackStamina() => ConsumeStamina(upAttackCost);
    
    public bool CanPerformDownAttack() => currentStamina >= downAttackCost;
    public bool ConsumeDownAttackStamina() => ConsumeStamina(downAttackCost);
    
    public bool CanPerformJump() => currentStamina >= jumpCost;
    public bool ConsumeJumpStamina() => ConsumeStamina(jumpCost);
    
    public bool CanPerformDoubleJump() => currentStamina >= doubleJumpCost;
    public bool ConsumeDoubleJumpStamina() => ConsumeStamina(doubleJumpCost);
    
    // 设置体力值
    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
        OnStaminaChanged?.Invoke();
    }
    
    public void SetMaxStamina(float value)
    {
        maxStamina = Mathf.Max(1f, value);
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        OnStaminaChanged?.Invoke();
    }
    
    // 完全恢复体力
    public void RestoreFullStamina()
    {
        SetStamina(maxStamina);
        regenTimer = 0f;
        Debug.Log("Stamina fully restored!");
    }
    
    // 检查是否可以执行需要体力的行动
    public bool HasStaminaFor(float cost)
    {
        return currentStamina >= cost;
    }
}
