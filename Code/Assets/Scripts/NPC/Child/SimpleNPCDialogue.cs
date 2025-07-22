using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;

public class SimpleNPCDialogue : MonoBehaviour
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
            // Child's opening
            new DialogueLine { speaker = "Child", text = "Hey (#`O′)! — \n喂(#`O′)！——", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Hello there, sister! This is the first time I know there are other \"people\" living in this cave!\n你好呀姐姐！我还是第一次知道这个洞穴还有其他“人”住呢！", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Your hometown must be from somewhere else, right? You look different from us \"people\" here.\n你家乡应该在外地吧，和我们这儿的“人”长得都不一样。", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Mom told me there's a crazy person living in the cave. He got traumatized more than ten years ago.\n妈妈跟我说洞穴里住着一个疯子。他十几年前受了刺激。", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "He hurt many \"people\", and said something about not being able to tell the difference.\n他打伤了好多“人”，还说什么自己分不清。", isPlayer = false },

            new DialogueLine { speaker = "Child", text = "Later, our villagers placed him in this cave. No one has visited him for more than ten years, and he doesn't come out either.\n后来我们村里人把他安置在这个洞穴里，十几年了也没人去看过他，他也不出来。", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "I've never seen a crazy person before...\n我还没有见过疯子呢...", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Speaking of which, the high priest recently gave me a book. The book says our village was once very prosperous.\n话说回来，最近大祭司给了我一本书。书上说我们村子曾经非常兴盛。", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "And a kind \"person\" helped us build the village, develop technology, and taught us weapons and magic.\n一个好心“人”帮我们建造村落、发展科技，还教我们兵器和魔法。", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "That \"person\" also had golden hair!\n那个“人”也是金色的头发呢！", isPlayer = false },
            
            // Player response
            new DialogueLine { speaker = "You", text = "What happened next?\n后来呢？", isPlayer = true },
            
            // Child continues
            new DialogueLine { speaker = "Child", text = "Later... I haven't read it yet...\n后来...后来还没看呢...", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Come to think of it, sister, you really look like the person in the book!\n说起来，姐姐你和书上那个人长的真的很像欸！", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Have you been to our village before? Our village is really beautiful!\n你有来过我们村子吗，我们村子真的很好看的！", isPlayer = false },
            
            // Player response
            new DialogueLine { speaker = "You", text = "Not yet, then I'll trouble you, little guide, to show me the way.\n还没有，那就劳烦小导游带路咯。", isPlayer = true },
        };
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
