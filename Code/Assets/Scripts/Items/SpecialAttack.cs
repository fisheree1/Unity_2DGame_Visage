using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBook : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private string interactionMessage = "Press [E] to pick up Skill Book";
    
    [Header("Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;
    
    // State
    private bool playerInRange = false;
    private bool isCollected = false;
    private Transform player;
    private AudioSource audioSource;
    
    void Start()
    {
        // Find player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        
        // Get audio source
        audioSource = GetComponent<AudioSource>();
        
        // Setup interaction UI
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
            
            // Set interaction text if it has a text component
            var textComponent = interactionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = interactionMessage;
            }
            else
            {
                // Try Unity Text component
                var unityText = interactionUI.GetComponentInChildren<UnityEngine.UI.Text>();
                if (unityText != null)
                {
                    unityText.text = interactionMessage;
                }
            }
        }
        
        Debug.Log("SkillBook initialized");
    }
    
    void Update()
    {
        if (isCollected) return;
        
        CheckPlayerInRange();
        
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            CollectSkillBook();
        }
    }
    
    private void CheckPlayerInRange()
    {
        if (player == null) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        if (playerInRange && !wasInRange)
        {
            ShowInteractionUI();
        }
        else if (!playerInRange && wasInRange)
        {
            HideInteractionUI();
        }
    }
    
    private void ShowInteractionUI()
    {
        if (interactionUI != null && !isCollected)
        {
            interactionUI.SetActive(true);
            Debug.Log("Skill book interaction available");
        }
    }
    
    private void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
    
    private void CollectSkillBook()
    {
        if (isCollected) return;
        
        Debug.Log("Skill book collected!");
        isCollected = true;
        
        // Hide interaction UI
        HideInteractionUI();
        
        // Play pickup sound
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // Spawn pickup effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        
        // TODO: Add skill book functionality here
        // For now, just log that it was collected
        Debug.Log("Skill book functionality will be implemented later");
        
        // Start disappearing
        StartCoroutine(DisappearAfterDelay());
    }
    
    private IEnumerator DisappearAfterDelay()
    {
        // Wait a bit for sound/effects to play
        float delay = 0.5f;
        if (audioSource != null && pickupSound != null)
        {
            delay = Mathf.Max(delay, pickupSound.length);
        }
        
        yield return new WaitForSeconds(delay);
        
        // Fade out effect (optional)
        StartCoroutine(FadeOut());
    }
    
    private IEnumerator FadeOut()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            float fadeTime = 0.5f;
            float elapsedTime = 0f;
            Color originalColor = spriteRenderer.color;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
        
        // Destroy the object
        Destroy(gameObject);
        Debug.Log("Skill book disappeared");
    }
    
    // Optional: Handle collision trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            player = other.transform;
            Debug.Log("Player entered skill book area");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left skill book area");
        }
    }
    
    // Draw interaction range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
