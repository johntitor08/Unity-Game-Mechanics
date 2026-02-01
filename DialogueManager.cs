using System;
using System.Collections;
using TMPro;
using UnityEngine;
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
    private static readonly WaitForSeconds WAIT_CLOSE = new(0.25f);
    public static DialogueManager Instance;
    public DialogueState State { get; private set; } = DialogueState.Idle;
    private DialogueNode currentNode;
    private int currentLineIndex;
    private Action onDialogueEnd;
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
    }

    void Update()
    {
        if (State != DialogueState.Typing && State != DialogueState.WaitingInput)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (State == DialogueState.Typing && typewriter != null && typewriter.IsTyping)
            {
                typewriter.Complete(dialogueText);
                State = DialogueState.WaitingInput;

                if (continueButton != null)
                    continueButton.SetActive(true);
            }
            else if (State == DialogueState.WaitingInput)
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueNode startNode, Action callback = null)
    {
        if (startNode == null || State != DialogueState.Idle)
            return;

        if (typewriter != null)
        {
            typewriter.OnTypingComplete += OnTypingFinished;
        }

        StopAllCoroutines();
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

    void OnTypingFinished()
    {
        if (State != DialogueState.Typing)
            return;

        State = DialogueState.WaitingInput;

        if (continueButton != null)
            continueButton.SetActive(true);
    }

    void ShowLine()
    {
        if (currentNode == null) return;

        if (currentLineIndex >= currentNode.lines.Length)
        {
            ShowChoicesOrEnd();
            return;
        }

        State = DialogueState.Typing;

        if (continueButton != null)
            continueButton.SetActive(false);

        string line = ParseLineSafe();

        if (typewriter != null)
        {
            typewriter.StartTyping(dialogueText, line);
        }
        else
        {
            dialogueText.text = line;
            State = DialogueState.WaitingInput;

            if (continueButton != null)
                continueButton.SetActive(true);
        }
    }

    void NextLine()
    {
        if (State != DialogueState.WaitingInput)
            return;

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

        if (currentNode.choices == null || currentNode.choices.Length == 0)
        {
            EndDialogue();
            return;
        }

        State = DialogueState.Choices;
        float panelHeight = currentNode.choices.Length * 100f;
        RectTransform panelRect = choicesPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelHeight);
        RectTransform containerRect = choicesContainer.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelHeight);

        foreach (var choice in currentNode.choices)
        {
            Button btn = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;
            btn.onClick.AddListener(() => SelectChoice(choice));
        }

        choicesPanel.SetActive(true);
    }

    void SelectChoice(DialogueChoice choice)
    {
        if (State != DialogueState.Choices)
            return;

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

        if (typewriter != null)
        {
            typewriter.OnTypingComplete -= OnTypingFinished;
        }

        State = DialogueState.Closing;
        DialogueNode endedNode = currentNode;

        if (currentNode != null)
            currentNode.onExit?.Invoke();

        currentNode = null;
        dialogueText.text = "";
        ClearChoices();
        PlaySound(dialogueCloseSound);

        if (dialoguePanelAnimator != null)
            dialoguePanelAnimator.SetTrigger(closeTrigger);

        StartCoroutine(FinishDialogue(endedNode));
    }

    IEnumerator FinishDialogue(DialogueNode endedNode)
    {
        yield return WAIT_CLOSE;
        dialoguePanel.SetActive(false);
        State = DialogueState.Idle;
        OnDialogueEnd?.Invoke(endedNode);
        onDialogueEnd?.Invoke();
    }

    void SetupVisuals()
    {
        if (currentNode == null)
        {
            Debug.LogError("SetupVisuals failed: currentNode is NULL");
            return;
        }

        if (speakerNameText != null)
        {
            speakerNameText.text = currentNode.speakerName ?? "";
            speakerNameText.color = Color.wheat;
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

    void ClearChoices()
    {
        foreach (Transform c in choicesContainer)
            Destroy(c.gameObject);
    }

    string ParseLineSafe()
    {
        string line = currentNode.lines[currentLineIndex];

        if (ProfileManager.Instance != null)
            line = line.Replace("{playerName}",
                ProfileManager.Instance.profile.playerName);

        return line;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(clip,
                Camera.main.transform.position, 0.5f);
    }

    public bool IsInDialogue()
    {
        return State != DialogueState.Idle;
    }
}
