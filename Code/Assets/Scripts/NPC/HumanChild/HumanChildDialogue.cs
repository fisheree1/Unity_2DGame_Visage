using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;

public class HumanChildDialogue : MonoBehaviour
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
            else if (currentLine.speaker == "Little Boy")
            {
                speakerNameText.color = Color.green; // Little Boy text in green
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
            
            new DialogueLine { speaker = "Designer", text = "(A sharp whisper. You freeze.)\n(一声尖锐的低语。你愣住了。)", isPlayer = false },
            
            new DialogueLine { speaker = "Little Boy", text = "Emissary! Over here!\n神使！神使！（小声）", isPlayer = false },
            
            new DialogueLine { speaker = "Designer", text = "(You spin around. A kid, clearly roughed up, is waving you over from a dark corner.)\n（你闻声望去）", isPlayer = false },
            
            new DialogueLine { speaker = "Little Boy", text = "I knew you'd come! I prayed and prayed.\n神使你终于来救我了，我每天晚上都在这里祈祷，祈祷你来救我。", isPlayer = false },
            
            new DialogueLine { speaker = "Little Boy", text = "You gotta get me out of here. The food they give me... I wouldn't feed it to a dog.\n你知道吗，他们每天给我的饭比屎都难吃！！", isPlayer = false },
            
            new DialogueLine { speaker = "Designer", text = "(Your mind races. He recognizes you. But who is he? A human child, or a monster in disguise?)\n（他认识我？怪物夺走了人类的面容，这究竟是人类还是怪物？）", isPlayer = false },
            
            // Player response
            new DialogueLine { speaker = "You", text = "I'm looking for an artifact. Seen it?\n我是来寻找神器的，你知道神器在哪儿吗？", isPlayer = true },
            
            new DialogueLine { speaker = "Little Boy", text = "The artifact? You mean your other half?\n神器？神使是说另一半神器吗？", isPlayer = false },
            
            new DialogueLine { speaker = "Little Boy", text = "The guards talk. Their chief found a piece, but he has no clue how to use it.\n听这里的怪物说\"怪物\"首领获得了一半神器，但是不知道有什么作用。", isPlayer = false },
            
            new DialogueLine { speaker = "Little Boy", text = "Don't worry, your half is still safe with us, back in the village!\n您原来的那一部分还暂借在我们村落呢！", isPlayer = false },
            
            new DialogueLine { speaker = "Designer", text = "(You look at him—so earnest, so broken. You can't just leave him here. You make a decision.)\n（你看着眼前真诚，身受重伤，满目疮痍的小孩，于心不忍，决定帮助他）", isPlayer = false },
            
            // Player decision to help
            new DialogueLine { speaker = "You", text = "I have something that might get you out of a tight spot. Take it.\n我这里有一个物品可以在关键时刻救你一命，你把这个拿去吧，我暂时也不需要。", isPlayer = true },
            
            new DialogueLine { speaker = "You", text = "I'll draw their eyes away. When you get an opening, you run. Don't look back.\n我可以帮你吸引一下眼线，你抓紧时间逃跑吧。", isPlayer = true },
            
            new DialogueLine { speaker = "Little Boy", text = "I will! Thank you!\n谢谢神使！！", isPlayer = false },
            
            new DialogueLine { speaker = "Little Boy", text = "By the way, that magic trick you showed me? I got it down!\n还有，你上次教我的法术我已经学会了！", isPlayer = false },
            
            // Player's stunned silence
            new DialogueLine { speaker = "You", text = "...\n...", isPlayer = true },
            
            new DialogueLine { speaker = "Little Boy", text = "See you, Emissary!\n神使再见！", isPlayer = false },
            
            new DialogueLine { speaker = "Designer", text = "(With a final, grateful look, he slips away, melting into the shadows.)\n（带着最后一次感激的目光，他消失在阴影中。）", isPlayer = false },
        };
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
