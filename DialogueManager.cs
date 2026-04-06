using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum DialogueState
{
    Idle,
    Opening,
    Typing,
    WaitingInput,
    Choices,
    Closing
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public DialogueState State { get; private set; } = DialogueState.Idle;
    private DialogueNode currentNode;
    private int currentLineIndex;
    private Action onDialogueEnd;
    private Coroutine autoAdvanceCoroutine;
    private AudioSource audioSource;
    public event Action<DialogueNode> OnDialogueStart;
    public event Action<DialogueChoice> OnChoiceSelected;
    public event Action<DialogueNode> OnDialogueEnd;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerPortrait;
    public Image backgroundImage;
    public GameObject continueButton;
    public GameObject choicesPanel;
    public Transform choicesContainer;
    public Button choiceButtonPrefab;

    [Header("Typewriter")]
    public Typewriter typewriter;

    [Header("Animation")]
    public Animator dialoguePanelAnimator;
    public string openTrigger = "Open";
    public string closeTrigger = "Close";
    public float closeFallbackDuration = 0.25f;

    [Header("Audio")]
    public AudioClip dialogueOpenSound;
    public AudioClip dialogueCloseSound;
    public AudioClip choiceSelectSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (typewriter != null)
            typewriter.OnTypingComplete += OnTypingFinished;
    }

    void Update()
    {
        if (State != DialogueState.Typing && State != DialogueState.WaitingInput)
            return;

        bool clickOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool spacePressed = Input.GetKeyDown(KeyCode.Space);
        bool clickPressed = Input.GetMouseButtonDown(0) && !clickOverUI;

        if (!spacePressed && !clickPressed)
            return;

        if (State == DialogueState.Typing && typewriter != null && typewriter.IsTyping)
        {
            typewriter.Complete(dialogueText);
        }
        else if (State == DialogueState.WaitingInput)
        {
            NextLine();
        }
    }

    void SetupVisuals()
    {
        if (currentNode == null)
        {
            Debug.LogError("[DialogueManager] SetupVisuals: currentNode is null.");
            return;
        }

        if (speakerNameText != null)
        {
            speakerNameText.text = currentNode.speakerName ?? "";
            speakerNameText.color = currentNode.speakerNameColor;
        }

        if (speakerPortrait != null)
        {
            speakerPortrait.sprite = currentNode.speakerPortrait;
            speakerPortrait.enabled = currentNode.speakerPortrait != null;
        }

        if (backgroundImage != null)
        {
            backgroundImage.sprite = currentNode.backgroundImage;
            backgroundImage.enabled = currentNode.backgroundImage != null;
        }
    }

    public void StartDialogue(DialogueNode startNode, Action callback = null)
    {
        if (startNode == null || State != DialogueState.Idle)
            return;

        StopAllCoroutines();
        autoAdvanceCoroutine = null;
        currentNode = startNode;
        currentLineIndex = 0;
        onDialogueEnd = callback;
        dialoguePanel.SetActive(true);

        if (dialoguePanelAnimator != null)
            dialoguePanelAnimator.SetTrigger(openTrigger);

        PlaySound(dialogueOpenSound);
        currentNode.onEnter?.Invoke();
        OnDialogueStart?.Invoke(currentNode);
        SetupVisuals();
        ClearChoices();
        ShowLine();
    }

    public bool IsInDialogue() => State != DialogueState.Idle;

    void OnTypingFinished()
    {
        if (State != DialogueState.Typing)
            return;

        if (currentNode != null && currentNode.autoAdvance)
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceRoutine(currentNode.autoAdvanceDelay));
            return;
        }

        State = DialogueState.WaitingInput;

        if (continueButton != null)
            continueButton.SetActive(true);
    }

    IEnumerator AutoAdvanceRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        autoAdvanceCoroutine = null;
        NextLine();
    }

    void ShowLine()
    {
        if (currentNode == null)
            return;

        if (currentLineIndex >= currentNode.lines.Length)
        {
            ShowChoicesOrEnd();
            return;
        }

        State = DialogueState.Typing;

        if (continueButton != null)
            continueButton.SetActive(false);

        string line = ParseLine();

        if (typewriter != null)
        {
            typewriter.StartTyping(dialogueText, line);
        }
        else
        {
            dialogueText.text = line;

            if (currentNode.autoAdvance)
            {
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceRoutine(currentNode.autoAdvanceDelay));
            }
            else
            {
                State = DialogueState.WaitingInput;

                if (continueButton != null)
                    continueButton.SetActive(true);
            }
        }
    }

    void NextLine()
    {
        if (State != DialogueState.WaitingInput)
            return;

        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        currentLineIndex++;

        if (currentNode != null && currentLineIndex >= currentNode.lines.Length)
        {
            ShowChoicesOrEnd();
            return;
        }

        ShowLine();
    }

    void ShowChoicesOrEnd()
    {
        ClearChoices();

        if (currentNode == null || currentNode.choices == null || currentNode.choices.Length == 0)
        {
            EndDialogue();
            return;
        }

        State = DialogueState.Choices;

        if (choicesPanel != null && choicesContainer != null)
        {
            RectTransform panelRect = choicesPanel.GetComponent<RectTransform>();
            RectTransform containerRect = choicesContainer.GetComponent<RectTransform>();

            if (panelRect != null && containerRect != null)
            {
                int buttonHeight = (int)(choiceButtonPrefab != null ? ((RectTransform)choiceButtonPrefab.transform).rect.height : 100);
                int panelHeight = currentNode.choices.Length * (buttonHeight + 25);
                panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelHeight);
                containerRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelHeight);
            }

            foreach (var choice in currentNode.choices)
            {
                Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;
                btn.onClick.AddListener(() => SelectChoice(choice));
            }

            choicesPanel.SetActive(true);
        }
    }

    void SelectChoice(DialogueChoice choice)
    {
        if (State != DialogueState.Choices)
            return;

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        ClearChoices();
        PlaySound(choiceSelectSound);
        OnChoiceSelected?.Invoke(choice);
        currentNode.onExit?.Invoke();

        if (choice.nextNode != null)
        {
            currentNode = choice.nextNode;
            currentLineIndex = 0;
            currentNode.onEnter?.Invoke();
            SetupVisuals();
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        if (State == DialogueState.Closing)
            return;

        State = DialogueState.Closing;

        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        DialogueNode endedNode = currentNode;

        if (currentNode != null)
            currentNode.onExit?.Invoke();

        currentNode = null;

        if (dialogueText != null)
            dialogueText.text = "";

        ClearChoices();
        PlaySound(dialogueCloseSound);

        if (dialoguePanelAnimator != null)
            dialoguePanelAnimator.SetTrigger(closeTrigger);

        StartCoroutine(FinishDialogue(endedNode));
    }

    IEnumerator FinishDialogue(DialogueNode endedNode)
    {
        float waitTime = closeFallbackDuration;

        if (dialoguePanelAnimator != null)
        {
            yield return null;
            AnimatorStateInfo info = dialoguePanelAnimator.GetCurrentAnimatorStateInfo(0);
            float clipLength = info.length;

            if (clipLength > 0f)
                waitTime = clipLength;
        }

        yield return new WaitForSeconds(waitTime);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        State = DialogueState.Idle;
        OnDialogueEnd?.Invoke(endedNode);
        onDialogueEnd?.Invoke();
    }

    void ClearChoices()
    {
        if (choicesContainer == null)
            return;

        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);
    }

    string ParseLine()
    {
        if (currentNode == null || currentNode.lines == null || currentLineIndex < 0 || currentLineIndex >= currentNode.lines.Length)
        {
            Debug.LogWarning("[DialogueManager] ParseLine called with invalid index.");
            return "";
        }

        string line = currentNode.lines[currentLineIndex];

        if (ProfileManager.Instance != null)
            line = line.Replace("{playerName}", ProfileManager.Instance.profile.playerName);

        return line;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(clip, 0.5f);
    }
}
