using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;

public class SlimeDialogu : MonoBehaviour
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

        Debug.Log("Started dialogue with monster chief");
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
            else if (currentLine.speaker == "Monster Chief")
            {
                speakerNameText.color = Color.magenta; // Monster Chief text in magenta
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
            
            new DialogueLine { speaker = "Monster Chief", text = "Great Emissary. Your presence brings honor to our humble tribe.\n神使大人，你的到来让我们部落蓬荜生辉。", isPlayer = false },
            
            new DialogueLine { speaker = "Designer", text = "(You stare. This is their chief? A Slime?)\n（这是首领？一只史莱姆？）", isPlayer = false },
            
            // Player response
            new DialogueLine { speaker = "You", text = "Enough with the pleasantries. Let's get to the point. Where is the artifact?\n客气话就别说了，开门见山，神器在哪儿？", isPlayer = true },
            
            new DialogueLine { speaker = "Monster Chief", text = "The artifact? Oh, but Emissary... the artifact you seek has always been with the \"monster\" chief.\n神器？您想找的神器一直在\"怪物\"首领那里啊，", isPlayer = false },
            
            new DialogueLine { speaker = "Monster Chief", text = "For years, we have suffered under their tyranny, so deeply that they even stole our very faces.\n这些年我们受怪物迫害太久也太深，连面容也被夺走了。", isPlayer = false },
            
            new DialogueLine { speaker = "Monster Chief", text = "They used the power of your artifact to slaughter our people. We beg you, bring us justice!\n他们就用你给的神器将我们大部分族人屠戮殆尽，您一定要为我们做主啊！", isPlayer = false },
            
            new DialogueLine { speaker = "Designer", text = "(You watch the Slime, saying nothing. Its gelatinous eyes keep darting to the side.)\n（你看着这只史莱姆时不时眼神左瞟，默不作声）", isPlayer = false },
            
            new DialogueLine { speaker = "Monster Chief", text = "Emissary, please! I ask you one last time.\n神使大人！！最后求您一次，", isPlayer = false },
            
            new DialogueLine { speaker = "Monster Chief", text = "If you won't help us reclaim our dignity, then at least help us reclaim our forms!\n就算不能帮我们讨回公道，能不能帮我们夺回我们本来的面容？", isPlayer = false },
            
            new DialogueLine { speaker = "Monster Chief", text = "Just lend us the artifact for a moment. As you know, even we cannot use its true power without your consent.\n您就把神器暂借给我们一下，您也知道，我们要使用神器也得经过你的允许。", isPlayer = false },
            
            // Player response
            new DialogueLine { speaker = "You", text = "And why should I help you? What's in it for me?\n为什么我要帮助你们呢，我能得到什么好处？", isPlayer = true },
            
            new DialogueLine { speaker = "Monster Chief", text = "(Bowing as low as a slime can) Our entire tribe will pledge eternal devotion to you.\n我们会全部落全心全意供奉于您，", isPlayer = false },
            
            new DialogueLine { speaker = "Monster Chief", text = "Our wealth, our thoughts, our very faith—all of it will be yours.\n将所有财富与思想投入对于您的信仰之中。", isPlayer = false },
            
            new DialogueLine { speaker = "Monster Chief", text = "It may be little to one such as you, but it is everything we have!\n可能对于神使而言微不足道，但这已经是我们的全部了！", isPlayer = false },
            
            new DialogueLine { speaker = "Monster Chief", text = "I understand you have much to consider. If you agree, we shall march on the \"monster chief\" together!\n神使大人要考虑的因果太多，如果神使大人同意了，我们就一同前往讨伐\"怪物首领\"吧！", isPlayer = false },
            
            new DialogueLine { speaker = "Monster Chief", text = "You can proceed to the right to begin. I will prepare on the left and join you shortly—\n神使可以先往右行，我去左边准备一下就过来——", isPlayer = false },
            
            new DialogueLine { speaker = "Designer", text = "(Paths open to both your left and right.)\n（左右皆可通行）", isPlayer = false },
        };
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
