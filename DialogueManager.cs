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

[RequireComponent(typeof(IDialoguePanelAnimator))]
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
    public event Action<DialogueNode, int> OnLineShown;
    public IDialoguePanelAnimator PanelAnimator { get; set; }

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

        PanelAnimator ??= GetComponent<IDialoguePanelAnimator>();
    }

    void Update()
    {
        if (State != DialogueState.Typing && State != DialogueState.WaitingInput)
            return;

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool advance = Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0) && !overUI);

        if (!advance)
            return;

        if (State == DialogueState.Typing && typewriter != null && typewriter.IsTyping)
            typewriter.Complete(dialogueText);
        else if (State == DialogueState.WaitingInput)
            NextLine();
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
        State = DialogueState.Opening;
        dialoguePanel.SetActive(true);
        PanelAnimator?.OpenDialoguePanel();
        PlaySound(dialogueOpenSound);
        currentNode.onEnter?.Invoke();
        OnDialogueStart?.Invoke(currentNode);
        SetupVisuals();
        ClearChoices();
        ShowLine();
    }

    public bool IsInDialogue() => State != DialogueState.Idle;

    void ShowLine()
    {
        if (currentNode == null)
            return;

        if (currentLineIndex >= currentNode.lines.Length)
        {
            ShowChoicesOrEnd();
            return;
        }

        OnLineShown?.Invoke(currentNode, currentLineIndex);
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
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceRoutine(currentNode.autoAdvanceDelay));
            else
                FinishTyping();
        }
    }

    void FinishTyping()
    {
        State = DialogueState.WaitingInput;

        if (continueButton != null)
            continueButton.SetActive(true);
    }

    void OnTypingFinished()
    {
        if (State != DialogueState.Typing)
            return;

        if (currentNode != null && currentNode.autoAdvance)
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceRoutine(currentNode.autoAdvanceDelay));
            return;
        }

        FinishTyping();
    }

    IEnumerator AutoAdvanceRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        autoAdvanceCoroutine = null;
        NextLine();
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

        if (choicesPanel == null || choicesContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("[DialogueManager] Choices UI references incomplete.");
            EndDialogue();
            return;
        }

        var panelRect = choicesPanel.GetComponent<RectTransform>();
        var containerRect = choicesContainer.GetComponent<RectTransform>();

        if (panelRect != null && containerRect != null)
        {
            int buttonHeight = (int)((RectTransform)choiceButtonPrefab.transform).rect.height;
            int panelHeight = currentNode.choices.Length * (buttonHeight + 25);
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelHeight);
            containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, panelHeight);
        }

        foreach (var choice in currentNode.choices)
        {
            Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;
            btn.onClick.AddListener(() => SelectChoice(choice));
        }

        choicesPanel.SetActive(true);
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

        if (endedNode != null)
            endedNode.onExit?.Invoke();

        currentNode = null;

        if (dialogueText != null)
            dialogueText.text = "";

        ClearChoices();
        PlaySound(dialogueCloseSound);
        float closeDuration = 0.25f;

        if (PanelAnimator != null)
        {
            PanelAnimator.CloseDialoguePanel();
            closeDuration = PanelAnimator.DialogueCloseAnimationDuration();
        }

        StartCoroutine(FinishDialogue(endedNode, closeDuration));
    }

    IEnumerator FinishDialogue(DialogueNode endedNode, float closeDuration)
    {
        yield return new WaitForSeconds(closeDuration);

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

public interface IDialoguePanelAnimator
{
    void OpenDialoguePanel();
    void CloseDialoguePanel();
    float DialogueCloseAnimationDuration();
}
