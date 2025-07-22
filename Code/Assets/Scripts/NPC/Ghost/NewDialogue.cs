using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;

public class NewDialague : MonoBehaviour
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
    
    // Dialogue content
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
        
        Debug.Log("Ghost Dialogue initialized with " + dialogueLines.Length + " lines");
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
        }
    }
    
    private void CheckPlayerInRange()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
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

        Debug.Log("Started dialogue with High Priest (Ghost)");
        director.Pause();
    }
    
    private void DisplayCurrentLine()
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
            else if (currentLine.speaker == "High Priest")
            {
                speakerNameText.color = Color.magenta; // High Priest text in magenta (mystical)
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
        isTyping = true;
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        typingCoroutine = StartCoroutine(TypeText(text));
    }
    
    private IEnumerator TypeText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
            
            for (int i = 0; i <= text.Length; i++)
            {
                dialogueText.text = text.Substring(0, i);
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        
        isTyping = false;
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
        if (currentLineIndex < dialogueLines.Length && dialogueText != null)
        {
            dialogueText.text = dialogueLines[currentLineIndex].text;
        }
    }
    
    private void ContinueDialogue()
    {
        if (isTyping)
        {
            StopTyping();
            CompleteCurrentLine();
            return;
        }
        
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

        Debug.Log("Dialogue with High Priest ended");
        director.Play();
    }
    
    private void InitializeDialogue()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine { speaker = "Designer", text = "(Press E or SPACE to skip)", isPlayer = false },
            // High Priest's opening
            new DialogueLine { speaker = "High Priest", text = "It's been fifteen years since we last met, hasn't it?\n我们有十五年没见了吧。", isPlayer = false },
            new DialogueLine { speaker = "High Priest", text = "Now I'm old, my appearance has changed. You surely wouldn't recognize me.\n我现在老了，外貌也变了，你肯定认不出我了。", isPlayer = false },

            // High Priest reflects on the past
            new DialogueLine { speaker = "High Priest", text = "I don't know why you betrayed us at the last moment, but I'm still grateful to you.\n我不知道你当初为什么临阵倒戈，但我依旧感谢你。", isPlayer = false },
            new DialogueLine { speaker = "High Priest", text = "Thank you for teaching me magic back then, and for saving me from the monsters' prison.\n谢谢你当初教我魔法，救我离开怪物们的监狱。", isPlayer = false },
            new DialogueLine { speaker = "High Priest", text = "I can say that my current position is entirely built by you.\n可以说，我现在的地位全是你铸就的。", isPlayer = false },
            
            // Recognition of failure
            new DialogueLine { speaker = "High Priest", text = "The fact that you've come here means you have failed.\n你能来到这里，说明你失败了。", isPlayer = false },
            
            // About the artifact and the gift
            new DialogueLine { speaker = "High Priest", text = "Back in prison, you told me your mission was to obtain the divine artifact.\n当初在监狱的时候，你告诉我你的任务是得到神器。", isPlayer = false },
            new DialogueLine { speaker = "High Priest", text = "You also left me something, to be used at the cliff's edge on a full moon night.\n还给我留了一个东西，要在月圆之夜的崖边使用。", isPlayer = false },
            new DialogueLine { speaker = "High Priest", text = "You said it would be useful in a moment of crisis.\n你说它在危机时刻会有用。", isPlayer = false },

            // Returning the gift
            new DialogueLine { speaker = "High Priest", text = "Now I remain safe and sound, so I'll return this to you.\n现在我依旧安然无恙，我把东西还给你吧。", isPlayer = false },
            new DialogueLine { speaker = "High Priest", text = "Remember, use it at the cliff's edge on a full moon night.\n记得，要在月圆之夜的崖边使用", isPlayer = false },
            
            // Final farewell
            new DialogueLine { speaker = "High Priest", text = "This is probably the last time we'll meet, old friend.\n这可能是我们最后一次见面了，老朋友。", isPlayer = false },
            new DialogueLine { speaker = "High Priest", text = "May you find the redemption you seek.\n愿你找到你所追寻的救赎。", isPlayer = false }
        };
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta; // Magenta for the ghostly High Priest
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
