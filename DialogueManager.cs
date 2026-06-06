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

    [Header("Localization")]
    public DialogueLocalizationTable localizationTable;
    private System.Collections.Generic.Dictionary<DialogueNode, DialogueLocalizationTable.Entry> _locMap;

    [Header("Audio")]
    public AudioClip dialogueOpenSound;
    public AudioClip dialogueCloseSound;
    public AudioClip choiceSelectSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.AdoptSceneReferences(this);
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
        BuildLocMap();
    }

    void AdoptSceneReferences(DialogueManager fresh)
    {
        if (fresh == null)
            return;

        dialoguePanel = fresh.dialoguePanel;
        speakerNameText = fresh.speakerNameText;
        dialogueText = fresh.dialogueText;
        speakerPortrait = fresh.speakerPortrait;
        backgroundImage = fresh.backgroundImage;
        continueButton = fresh.continueButton;
        choicesPanel = fresh.choicesPanel;
        choicesContainer = fresh.choicesContainer;
        choiceButtonPrefab = fresh.choiceButtonPrefab;
    }

    bool IsPanelAnimatorAlive()
    {
        if (PanelAnimator == null)
            return false;

        return PanelAnimator is not UnityEngine.Object unityObj || unityObj != null;
    }

    void BuildLocMap()
    {
        _locMap = new System.Collections.Generic.Dictionary<DialogueNode, DialogueLocalizationTable.Entry>();

        if (localizationTable == null || localizationTable.entries == null)
            return;

        foreach (var e in localizationTable.entries)
        {
            if (e == null)
                continue;

            if (e.en != null)
                _locMap[e.en] = e;

            if (e.tr != null)
                _locMap[e.tr] = e;
        }
    }

    DialogueNode Localize(DialogueNode node)
    {
        if (node == null)
            return null;

        if (_locMap == null)
            BuildLocMap();

        if (_locMap.TryGetValue(node, out var e))
        {
            var want = LanguageManager.Current == GameLanguage.EN ? e.en : e.tr;
            var other = LanguageManager.Current == GameLanguage.EN ? e.tr : e.en;
            return want != null ? want : (other != null ? other : node);
        }

        return node;
    }

    void Update()
    {
        if (State != DialogueState.Typing && State != DialogueState.WaitingInput)
            return;

        bool advance = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);

        if (!advance)
            return;

        if (State == DialogueState.Typing && typewriter != null && typewriter.IsTyping)
            typewriter.Complete();
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
        currentNode = Localize(startNode);
        currentLineIndex = 0;
        onDialogueEnd = callback;
        State = DialogueState.Opening;
        dialoguePanel.SetActive(true);
        PlaySound(dialogueOpenSound);
        currentNode.onEnter?.Invoke();
        OnDialogueStart?.Invoke(currentNode);
        SetupVisuals();
        ClearChoices();

        if (IsPanelAnimatorAlive())
        {
            PanelAnimator.OpenDialoguePanel();
            float openDuration = PanelAnimator.DialogueOpenAnimationDuration();

            if (openDuration > 0f)
                StartCoroutine(WaitThenShowLine(openDuration));
            else
                ShowLine();
        }
        else
        {
            ShowLine();
        }
    }

    IEnumerator WaitThenShowLine(float delay)
    {
        yield return new WaitForSeconds(delay);
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
        if (State != DialogueState.Typing || (typewriter != null && typewriter.CurrentTarget != dialogueText))
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

        foreach (var choice in currentNode.choices)
        {
            var capturedChoice = choice;
            Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = capturedChoice.choiceText;
            btn.onClick.AddListener(() => SelectChoice(capturedChoice));
        }

        if (panelRect != null)
        {
            float panelHeight = currentNode.choices.Length * 100f;
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelHeight);

            if (containerRect != null)
                containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, panelHeight);
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

        if (choice.setFlag && !string.IsNullOrEmpty(choice.flagToSet))
            StoryFlags.Add(choice.flagToSet);

        currentNode.onExit?.Invoke();
        ApplyExitFlags(currentNode);

        if (choice.nextNode != null)
        {
            currentNode = Localize(choice.nextNode);
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
        {
            endedNode.onExit?.Invoke();
            ApplyExitFlags(endedNode);
        }

        currentNode = null;

        if (dialogueText != null)
            dialogueText.text = "";

        ClearChoices();
        PlaySound(dialogueCloseSound);

        if (IsPanelAnimatorAlive())
            PanelAnimator.CloseDialoguePanel();

        StartCoroutine(FinishDialogue(endedNode));
    }

    IEnumerator FinishDialogue(DialogueNode endedNode)
    {
        float closeDuration = IsPanelAnimatorAlive() ? PanelAnimator.DialogueCloseAnimationDuration() : 0.25f;

        if (closeDuration > 0)
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

    void ApplyExitFlags(DialogueNode node)
    {
        if (node == null || node.flagsToSetOnExit == null)
            return;

        foreach (var flag in node.flagsToSetOnExit)
            if (!string.IsNullOrEmpty(flag))
                StoryFlags.Add(flag);
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

    float DialogueOpenAnimationDuration();

    float DialogueCloseAnimationDuration();
}
