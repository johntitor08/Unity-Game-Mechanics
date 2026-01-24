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
    private bool isClosing;

    private DialogueNode currentNode;
    private int currentLineIndex;
    private bool isShowingChoices;
    private bool isInDialogue;
    private System.Action onDialogueEnd;
    private WaitForSeconds autoAdvanceWait;
    public event System.Action<DialogueNode> OnDialogueStart;
    public event System.Action OnDialogueEnd;
    public event System.Action<DialogueChoice> OnChoiceSelected;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (speakerPortrait != null)
            speakerPortrait.enabled = true;

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

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            if (typewriter != null && skipTypewriterOnClick && typewriter.IsTyping)
            {
                typewriter.Complete(dialogueText, ParseLineSafe());
            }
            else if (typewriter == null || !typewriter.IsTyping)
            {
                NextLine();
            }
        }
    }

    public void StartDialogue_Button()
    {
        if (startNodeForButton == null)
        {
            Debug.LogWarning("DialogueManager: startNodeForButton null — assign a DialogueNode in the Inspector.");
            return;
        }

        StartDialogue(startNodeForButton);
    }

    public void StartDialogue(DialogueNode startNode, System.Action callback = null)
    {
        if (startNode == null)
        {
            Debug.LogWarning("DialogueManager: StartDialogue called with null node.");
            return;
        }

        StopAllCoroutines();
        if (isInDialogue) return;
        currentNode = startNode;
        currentLineIndex = 0;
        isShowingChoices = false;
        isInDialogue = true;
        onDialogueEnd = callback;
        OpenPanel();

        if (dialoguePanelAnimator != null)
            dialoguePanelAnimator.SetTrigger(openTrigger);

        PlaySound(dialogueOpenSound);
        if (currentNode != null && currentNode.onEnter != null)
            currentNode.onEnter.Invoke();
        if (OnDialogueStart != null)
            OnDialogueStart.Invoke(currentNode);
        SetupVisuals();
        ClearChoices();
        ShowLine();
    }

    string ParseLineSafe()
    {
        if (currentNode == null || currentNode.lines == null || currentLineIndex >= currentNode.lines.Length)
            return "";

        string line = currentNode.lines[currentLineIndex];

        if (ProfileManager.Instance != null)
            line = line.Replace("{playerName}", ProfileManager.Instance.profile.playerName);

        return line;
    }

    void SetupVisuals()
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = currentNode != null ? currentNode.speakerName : "";
            speakerNameText.color = currentNode != null ? currentNode.speakerNameColor : Color.white;
        }

        if (speakerPortrait != null)
        {
            speakerPortrait.sprite = currentNode != null ? currentNode.speakerPortrait : null;
            speakerPortrait.enabled = currentNode != null && currentNode.speakerPortrait != null;
        }

        if (backgroundImage != null)
        {
            backgroundImage.sprite = currentNode != null ? currentNode.backgroundImage : null;
            backgroundImage.enabled = currentNode != null && currentNode.backgroundImage != null;
        }

        if (continueButton != null)
            continueButton.SetActive(false);
    }

    void ShowLine()
    {
        if (currentNode == null || currentNode.lines == null || currentLineIndex >= currentNode.lines.Length)
        {
            ShowChoicesOrEnd();
            return;
        }

        string line = ParseLineSafe();

        if (typewriter != null)
        {
            typewriter.StartTyping(dialogueText, line);

            if (currentNode.autoAdvance)
            {
                autoAdvanceWait = new WaitForSeconds(currentNode.autoAdvanceDelay);
                StartCoroutine(AutoAdvanceLine());
            }
            else if (continueButton != null)
            {
                StartCoroutine(ShowContinueButtonAfterTyping());
            }
        }
        else
        {
            dialogueText.text = line;

            if (continueButton != null)
                continueButton.SetActive(true);
        }
    }

    IEnumerator ShowContinueButtonAfterTyping()
    {
        yield return new WaitUntil(() => typewriter == null || !typewriter.IsTyping);

        if (continueButton != null)
            continueButton.SetActive(true);
    }

    IEnumerator AutoAdvanceLine()
    {
        yield return new WaitUntil(() => typewriter == null || !typewriter.IsTyping);
        if (autoAdvanceWait != null) yield return autoAdvanceWait;
        NextLine();
    }

    void NextLine()
    {
        if (typewriter != null && typewriter.IsTyping) return;

        if (continueButton != null)
            continueButton.SetActive(false);

        currentLineIndex++;

        if (currentNode != null && currentNode.lines != null && currentLineIndex < currentNode.lines.Length)
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

        if (currentNode == null || currentNode.choices == null || currentNode.choices.Length == 0)
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

            btn.interactable = CanSelectChoice(choice);
            DialogueChoice localChoice = choice;
            btn.onClick.AddListener(() => SelectChoice(localChoice));
        }

        choicesParent.gameObject.SetActive(true);
    }

    bool CanShowChoice(DialogueChoice choice)
    {
        return choice != null && (!choice.requiresFlag || StoryFlags.Has(choice.requiredFlag));
    }

    bool CanSelectChoice(DialogueChoice choice)
    {
        if (choice == null) return false;
        if (choice.isDisabledChoice) return false;

        if (choice.requiresItem && InventoryManager.Instance != null && InventoryManager.Instance.GetQuantity(choice.requiredItem) <= 0)
            return false;

        if (choice.requiresStat && PlayerStats.Instance != null && PlayerStats.Instance.Get(choice.requiredStat) < choice.requiredStatValue)
            return false;

        if (choice.requiresCurrency && CurrencyManager.Instance != null && !CurrencyManager.Instance.Has(choice.requiredCurrency, choice.requiredCurrencyAmount))
            return false;

        return true;
    }

    void SelectChoice(DialogueChoice choice)
    {
        if (choice == null) return;
        PlaySound(choiceSelectSound);
        OnChoiceSelected?.Invoke(choice);

        // Item tüket
        if (choice.consumeItem && choice.requiredItem != null && InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveItem(choice.requiredItem, 1);

        // Flag ayarla
        if (choice.setFlag && !string.IsNullOrEmpty(choice.flagToSet))
            StoryFlags.Add(choice.flagToSet);

        // Ödüller
        if (choice.giveReward)
            GiveRewards(choice);

        // Sonraki node varsa devam et
        if (choice.nextNode != null)
        {
            // Mevcut node çıkış callback
            if (currentNode != null && currentNode.onExit != null)
                currentNode.onExit.Invoke();

            // Yeni node
            currentNode = choice.nextNode;
            currentLineIndex = 0;
            isShowingChoices = false;
            ClearChoices();

            // Yeni node giriş callback
            if (currentNode != null && currentNode.onEnter != null)
                currentNode.onEnter.Invoke();

            SetupVisuals();
            ShowLine();
        }
        else
        {
            // Yoksa diyaloğu bitir
            EndDialogue();
        }
    }

    void GiveRewards(DialogueChoice choice)
    {
        if (choice.currencyRewards != null)
            foreach (var reward in choice.currencyRewards) reward.Grant();

        if (choice.itemRewards != null && InventoryManager.Instance != null)
            foreach (var item in choice.itemRewards) InventoryManager.Instance.AddItem(item, 1);

        if (choice.experienceReward > 0 && ProfileManager.Instance != null)
            ProfileManager.Instance.AddExperience(choice.experienceReward);
    }

    void ClearChoices()
    {
        if (choicesParent == null) return;
        foreach (Transform child in choicesParent) Destroy(child.gameObject);
        choicesParent.gameObject.SetActive(false);
    }

    void EndDialogue()
    {
        if (!isInDialogue || isClosing) return;
        isClosing = true;

        if (currentNode != null && currentNode.onExit != null)
            currentNode.onExit.Invoke();

        if (dialoguePanelAnimator != null)
        {
            dialoguePanelAnimator.ResetTrigger(openTrigger);
            dialoguePanelAnimator.SetTrigger(closeTrigger);
            StartCoroutine(WaitForCloseAnimation());
        }
        else
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            isClosing = false;
        }

        PlaySound(dialogueCloseSound);
        isInDialogue = false;
        isShowingChoices = false;
        currentNode = null;
        dialogueText.text = "";
        ClearChoices();
        OnDialogueEnd?.Invoke();
        onDialogueEnd?.Invoke();
    }

    void OpenPanel()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialoguePanelAnimator != null)
        {
            dialoguePanelAnimator.ResetTrigger(closeTrigger);
            dialoguePanelAnimator.SetTrigger(openTrigger);
        }
    }

    IEnumerator WaitForCloseAnimation()
    {
        if (dialoguePanelAnimator == null)
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            isClosing = false;
            yield break;
        }

        float duration = 0f;
        AnimatorClipInfo[] clips = dialoguePanelAnimator.GetCurrentAnimatorClipInfo(0);

        if (clips.Length > 0)
            duration = clips[0].clip.length;

        yield return new WaitForSeconds(duration);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        isClosing = false;
        yield return null;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        isClosing = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
    }

    public bool IsInDialogue() => isInDialogue;
}
