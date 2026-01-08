using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public Image backgroundImage;
    public Image speakerPortrait;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject continueButton;
    public Transform choicesParent;
    public Button choiceButtonPrefab;
    public Button startDialogueButton;
    public DialogueNode startNodeForButton;

    [Header("Typewriter")]
    public Typewriter typewriter;
    public bool skipTypewriterOnClick = true;

    [Header("Audio")]
    public AudioClip dialogueOpenSound;
    public AudioClip dialogueCloseSound;
    public AudioClip choiceSelectSound;
    public AudioClip typewriterSound;

    [Header("Animation")]
    public Animator dialoguePanelAnimator;
    public string openTrigger = "Open";
    public string closeTrigger = "Close";

    private DialogueNode currentNode;
    private int currentLineIndex;
    private bool isShowingChoices;
    private bool isInDialogue;
    private System.Action onDialogueEnd;

    public event System.Action<DialogueNode> OnDialogueStart;
    public event System.Action OnDialogueEnd;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        dialoguePanel.SetActive(false);

        if (speakerPortrait != null)
        {
            speakerPortrait.enabled = true;
        }

        if (startDialogueButton != null)
        {
            startDialogueButton.onClick.AddListener(() =>
            {
                startDialogueButton.gameObject.SetActive(false);
                StartDialogue(startNodeForButton);
            });
        }
    }

    void Update()
    {
        if (!isInDialogue || isShowingChoices) return;

        // Space or Click to continue
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (skipTypewriterOnClick && typewriter.IsTyping)
            {
                typewriter.Complete(dialogueText, currentNode.lines[currentLineIndex]);
            }
            else if (!typewriter.IsTyping)
            {
                NextLine();
            }
        }
    }

    public void StartDialogue_Button()
    {
        if (startNodeForButton == null)
        {
            Debug.LogWarning("DialogueManager: startNodeForButton null — Inspector'da bir DialogueNode atayın.");
            return;
        }

        StartDialogue(startNodeForButton);
    }

    public void StartDialogue(DialogueNode startNode, System.Action callback = null)
    {
        if (isInDialogue) return;

        currentNode = startNode;
        currentLineIndex = 0;
        isShowingChoices = false;
        isInDialogue = true;
        onDialogueEnd = callback;

        // Show panel
        dialoguePanel.SetActive(true);

        // Play animation
        if (dialoguePanelAnimator != null && !string.IsNullOrEmpty(openTrigger))
        {
            dialoguePanelAnimator.SetTrigger(openTrigger);
        }

        // Play sound
        PlaySound(dialogueOpenSound);

        // Trigger events
        currentNode.onEnter?.Invoke();
        OnDialogueStart?.Invoke(currentNode);

        SetupVisuals();
        ClearChoices();
        ShowLine();
    }

    void SetupVisuals()
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = currentNode.speakerName;
            speakerNameText.color = currentNode.speakerNameColor;
        }

        if (speakerPortrait != null)
        {
            if (currentNode.speakerPortrait != null)
            {
                speakerPortrait.sprite = currentNode.speakerPortrait;
            }

            speakerPortrait.enabled = true;
        }

        if (backgroundImage != null && currentNode.backgroundImage != null)
        {
            backgroundImage.sprite = currentNode.backgroundImage;
            backgroundImage.enabled = true;
        }

        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }
    }

    void ShowLine()
    {
        if (currentLineIndex >= currentNode.lines.Length)
        {
            ShowChoicesOrEnd();
            return;
        }

        string line = currentNode.lines[currentLineIndex];

        if (typewriter != null)
        {
            typewriter.StartTyping(dialogueText, line);

            if (currentNode.autoAdvance)
            {
                StartCoroutine(AutoAdvanceLine());
            }
        }
        else
        {
            dialogueText.text = line;
        }

        if (continueButton != null && !currentNode.autoAdvance)
        {
            StartCoroutine(ShowContinueButtonAfterTyping());
        }
    }

    IEnumerator ShowContinueButtonAfterTyping()
    {
        yield return new WaitUntil(() => !typewriter.IsTyping);
        continueButton.SetActive(true);
    }

    IEnumerator AutoAdvanceLine()
    {
        yield return new WaitUntil(() => !typewriter.IsTyping);
        yield return new WaitForSeconds(currentNode.autoAdvanceDelay);
        NextLine();
    }

    void NextLine()
    {
        if (typewriter.IsTyping) return;

        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }

        currentLineIndex++;

        if (currentLineIndex < currentNode.lines.Length)
        {
            ShowLine();
        }
        else
        {
            ShowChoicesOrEnd();
        }
    }

    void ShowChoicesOrEnd()
    {
        ClearChoices();

        if (currentNode.choices == null || currentNode.choices.Length == 0)
        {
            EndDialogue();
            return;
        }

        isShowingChoices = true;

        foreach (var choice in currentNode.choices)
        {
            if (!CanShowChoice(choice)) continue;

            Button btn = Instantiate(choiceButtonPrefab, choicesParent);

            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                btnText.text = choice.choiceText;
                btnText.color = choice.choiceColor;
            }

            bool canSelect = CanSelectChoice(choice);
            btn.interactable = canSelect;

            btn.onClick.AddListener(() => SelectChoice(choice));
        }

        choicesParent.gameObject.SetActive(true);
    }

    bool CanShowChoice(DialogueChoice choice)
    {
        // Flag requirement
        if (choice.requiresFlag && !StoryFlags.Has(choice.requiredFlag))
            return false;

        return true;
    }

    bool CanSelectChoice(DialogueChoice choice)
    {
        // Disabled choice
        if (choice.isDisabledChoice)
            return false;

        // Item requirement
        if (choice.requiresItem && InventoryManager.Instance != null)
        {
            if (InventoryManager.Instance.GetQuantity(choice.requiredItem) <= 0)
                return false;
        }

        // Stat requirement
        if (choice.requiresStat && PlayerStats.Instance != null)
        {
            if (PlayerStats.Instance.Get(choice.requiredStat) < choice.requiredStatValue)
                return false;
        }

        // Currency requirement
        if (choice.requiresCurrency && CurrencyManager.Instance != null)
        {
            if (!CurrencyManager.Instance.Has(choice.requiredCurrency, choice.requiredCurrencyAmount))
                return false;
        }

        return true;
    }

    void SelectChoice(DialogueChoice choice)
    {
        PlaySound(choiceSelectSound);

        // Consume item
        if (choice.consumeItem && choice.requiredItem != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(choice.requiredItem, 1);
        }

        // Set flag
        if (choice.setFlag && !string.IsNullOrEmpty(choice.flagToSet))
        {
            StoryFlags.Add(choice.flagToSet);
        }

        // Give rewards
        if (choice.giveReward)
        {
            GiveRewards(choice);
        }

        // Continue to next node
        if (choice.nextNode != null)
        {
            // Trigger exit event
            currentNode.onExit?.Invoke();

            // Start next node
            currentNode = choice.nextNode;
            currentLineIndex = 0;
            isShowingChoices = false;

            choicesParent.gameObject.SetActive(false);
            ClearChoices();

            currentNode.onEnter?.Invoke();
            SetupVisuals();
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void GiveRewards(DialogueChoice choice)
    {
        // Currency rewards
        if (choice.currencyRewards != null)
        {
            foreach (var reward in choice.currencyRewards)
            {
                reward.Grant();
            }
        }

        // Item rewards
        if (choice.itemRewards != null && InventoryManager.Instance != null)
        {
            foreach (var item in choice.itemRewards)
            {
                InventoryManager.Instance.AddItem(item, 1);
            }
        }

        // Experience reward
        if (choice.experienceReward > 0 && ProfileManager.Instance != null)
        {
            ProfileManager.Instance.AddExperience(choice.experienceReward);
        }
    }

    void ClearChoices()
    {
        foreach (Transform child in choicesParent)
        {
            Destroy(child.gameObject);
        }
    }

    void EndDialogue()
    {
        if (!isInDialogue) return;

        // Trigger exit event
        currentNode.onExit?.Invoke();

        // Play animation
        if (dialoguePanelAnimator != null && !string.IsNullOrEmpty(closeTrigger))
        {
            dialoguePanelAnimator.SetTrigger(closeTrigger);
            StartCoroutine(HidePanelAfterAnimation());
        }
        else
        {
            dialoguePanel.SetActive(false);
        }

        PlaySound(dialogueCloseSound);

        isInDialogue = false;
        isShowingChoices = false;
        currentNode = null;

        // Clear UI
        dialogueText.text = "";
        ClearChoices();

        // Callbacks
        OnDialogueEnd?.Invoke();
        onDialogueEnd?.Invoke();
    }

    IEnumerator HidePanelAfterAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        dialoguePanel.SetActive(false);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
        }
    }

    public bool IsInDialogue() => isInDialogue;
}
