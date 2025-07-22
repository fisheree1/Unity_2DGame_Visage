using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;

public class MomDialogue : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;
    
    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;

    [SerializeField] private PlayableDirector director;
    
    // State
    private bool playerInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private int currentLineIndex = 0;
    private Transform player;
    private HeroMovement playerMovement;
    
    // Simple dialogue content
    private DialogueLine[] dialogueLines;
    private Coroutine typingCoroutine;
    
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string text;
        public bool isPlayer;
    }
    
    void Start()
    {
        InitializeDialogue();
        
        // Find player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerMovement = playerObject.GetComponent<HeroMovement>();
        }
        
        // Setup UI
        if (interactionUI != null)
            interactionUI.SetActive(false);
            
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (interactionText != null)
            interactionText.text = $"Press [{interactKey}] to talk";
            
        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueDialogue);
            
            // Set button text to English
            TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Continue";
            }
        }
        
        Debug.Log("Simple NPC Dialogue initialized with " + dialogueLines.Length + " lines");
    }
    
    void Update()
    {
        CheckPlayerInRange();
        
        if (playerInRange && Input.GetKeyDown(interactKey) && !isDialogueActive)
        {
            StartDialogue();
        }
        
        if (isDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    // Skip typing animation
                    StopTyping();
                    CompleteCurrentLine();
                }
                else
                {
                    ContinueDialogue();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
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
        if (interactionUI != null && !isDialogueActive)
            interactionUI.SetActive(true);
    }
    
    private void HideInteractionUI()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    public void StartDialogue()
    {
        isDialogueActive = true;
        currentLineIndex = 0;

        HideInteractionUI();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Disable player movement
        if (playerMovement != null)
            playerMovement.enabled = false;

        DisplayCurrentLine();

        Debug.Log("Started dialogue with child");
        director.Pause();
    }
    
    public void DisplayCurrentLine()
    {
        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }
        
        DialogueLine currentLine = dialogueLines[currentLineIndex];
        
        // Set speaker name with appropriate styling
        if (speakerNameText != null)
        {
            speakerNameText.text = currentLine.speaker;
            
            // Change color based on speaker
            if (currentLine.isPlayer)
            {
                speakerNameText.color = Color.cyan; // Player text in cyan
            }
            else if (currentLine.speaker == "Child's Mother")
            {
                speakerNameText.color = Color.red; // Mother text in red (angry)
            }
            else if (currentLine.speaker == "Child")
            {
                speakerNameText.color = Color.yellow; // Child text in yellow
            }
            else
            {
                speakerNameText.color = Color.white; // Default white
            }
        }
        
        // Start typing animation
        StartTyping(currentLine.text);
        
        Debug.Log($"Displaying line {currentLineIndex}: {currentLine.speaker} - {currentLine.text}");
    }
    
    private void StartTyping(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
            
        isTyping = true;
        typingCoroutine = StartCoroutine(TypeText(text));
    }
    
    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }
    
    private void CompleteCurrentLine()
    {
        if (currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex].text;
        }
        isTyping = false;
    }
    
    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }
    
    public void ContinueDialogue()
    {
        if (isTyping) return;
        
        currentLineIndex++;
        DisplayCurrentLine();
    }

    public void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Restore player movement
        if (playerMovement != null)
            playerMovement.enabled = true;

        Debug.Log("Dialogue ended");
        director.Play();
    }
    
    private void InitializeDialogue()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine { speaker = "Designer", text = "(Press E or SPACE to skip)", isPlayer = false },
            // Child calls mother
            new DialogueLine { speaker = "Child", text = "Mom, I found someone who looks just like the person in the book! You gotta see, it's really her!\n妈妈，我找到一个和书上很像的人！你快看看，真的很像的！", isPlayer = false },
            
            // Mother's reaction
            new DialogueLine { speaker = "Child's Mother", text = "！！！", isPlayer = false },
            new DialogueLine { speaker = "Child's Mother", text = "Get over here, fast!\n赶紧过来！", isPlayer = false },
            new DialogueLine { speaker = "Child's Mother", text = "......", isPlayer = false },
            new DialogueLine { speaker = "Child's Mother", text = "Haven't you done enough, helping those monsters kill so many of our people? Now you've found us, are you trying to wipe us out completely?\n你助纣为虐杀了我们这么多族人还不够吗？现在找到我们还想赶尽杀绝？", isPlayer = false },
            new DialogueLine { speaker = "Child's Mother", text = "Yuanyuan, go find your uncle. Tell him they've found us.\n源源，你去找叔叔，就说他们找到我们了。", isPlayer = false },
            
            // Child's confusion
            new DialogueLine { speaker = "Child", text = "Huh? But big sister is a good person.\n啊？可是姐姐是好人啊。", isPlayer = false },
            
            // Mother's insistence
            new DialogueLine { speaker = "Child's Mother", text = "You don't understand anything! Just go, now!\n你什么都不懂，赶紧去！", isPlayer = false },

        };
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
