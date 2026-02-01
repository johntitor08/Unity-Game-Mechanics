using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameStartNameInput : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1f);
    private static readonly WaitForSeconds _waitForSeconds1_5 = new(1.5f);
    private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);
    public static GameStartNameInput Instance;
    private bool typewriterFinished = false;
    private bool hasStarted = false;

    [Header("Panels")]
    public GameObject nameInputPanel;

    [Header("Input Fields")]
    public TMP_InputField nameInputField;
    public TextMeshProUGUI characterCountText;
    public TextMeshProUGUI errorMessageText;

    [Header("Buttons")]
    public Button confirmButton;
    public Button randomNameButton;

    [Header("Visual Elements")]
    public Image playerPreviewImage;
    public TextMeshProUGUI welcomeText;
    public TextMeshProUGUI instructionText;

    [Header("Name Settings")]
    public int minNameLength = 3;
    public int maxNameLength = 15;
    public bool allowSpaces = true;
    public bool allowNumbers = false;
    public bool allowSpecialCharacters = false;

    [Header("Start Dialogue")]
    public DialogueNode startDialogueNode;

    [Header("Random Names")]
    public string[] randomNames = new string[]
    {
        "Raven",
        "Blaze",
        "Shadow",
        "Storm",
        "Ash",
        "Viper",
        "Fang",
        "Drake",
        "Phoenix",
        "Ghost",
        "Onyx",
        "Nova",
        "Reaper",
        "Wolf",
        "Hawk",
        "Valor",
        "Iron",
        "Steel",
        "Crusader",
        "Knightfall",
        "Bulwark",
        "Sentinel",
        "Warden",
        "Paladin",
        "Vanguard",
        "Shade",
        "Night",
        "Blade",
        "Silent",
        "Rift",
        "Specter",
        "Venom",
        "Whisper",
        "Obsidian",
        "Hex",
        "Void",
        "Arcane",
        "Eclipse",
        "Runeblade",
        "Frost",
        "Inferno",
        "Aether",
        "Oracle",
        "Zephyr",
        "Tempest",
        "Titan",
        "Colossus",
        "Juggernaut",
        "Phantom",
        "Mirage",
        "Cipher",
        "NovaStrike",
        "ShadowFang",
        "CrimsonWolf",
        "SilverHawk",
        "GoldenDragon",
        "DarkPhoenix",
        "StormRider",
        "LunarKnight",
        "SolarFlare",
        "ThunderClaw",
        "BlazingArrow",
        "FrostBite",
        "VenomousViper",
        "SilentShadow",
        "ObsidianBlade",
        "ArcaneMage",
        "VoidWalker",
        "NightStalker",
        "RiftRunner",
        "SpectralWraith",
        "CelestialGuardian",
        "GalacticRanger",
        "CosmicSorcerer",
        "StellarWarrior",
        "QuantumAssassin",
        "NeonNinja",
        "CyberSamurai",
        "TechnoMage",
        "DigitalDruid",
        "PixelPaladin",
        "VirtualValkyrie",
        "MatrixMonk",
        "DataDemon",
        "CodeCrusader",
        "ScriptSage",
        "BinaryBerserker",
        "AlgorithmArcher",
        "LogicLancer",
        "SyntaxSwordsman"
    };

    [Header("Audio")]
    public AudioClip confirmSound;
    public AudioClip errorSound;
    public AudioClip typeSound;

    [Header("Animation")]
    public Animator panelAnimator;
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.05f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        SetupUI();
        CheckExistingProfile();
    }

    void SetupUI()
    {
        typewriterFinished = !useTypewriterEffect;

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(TryConfirm);
            confirmButton.interactable = false;
        }

        if (randomNameButton != null)
            randomNameButton.onClick.AddListener(OnRandomNameClicked);

        if (nameInputField != null)
        {
            nameInputField.characterLimit = maxNameLength;
            nameInputField.onValueChanged.AddListener(OnNameChanged);
            nameInputField.onSubmit.AddListener(OnSubmit);
        }

        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);

        if (useTypewriterEffect && welcomeText != null)
        {
            string originalText = welcomeText.text;
            welcomeText.text = "";
            StartCoroutine(TypewriterEffect(welcomeText, originalText, typewriterSpeed));
        }
    }

    void CheckExistingProfile()
    {
        if (SaveSystem.HasSaveFile())
        {
            if (nameInputPanel != null)
                nameInputPanel.SetActive(false);

            return;
        }

        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);

            if (nameInputField != null)
                StartCoroutine(FocusInputField());
        }
    }

    IEnumerator FocusInputField()
    {
        yield return _waitForSeconds0_5;

        if (nameInputField != null)
        {
            nameInputField.ActivateInputField();
        }
    }

    void OnNameChanged(string newName)
    {
        // Update character count
        if (characterCountText != null)
        {
            characterCountText.text = $"{newName.Length} / {maxNameLength}";
        }

        // Validate name
        bool isValid = ValidateName(newName, out string errorMessage);

        // Update error message
        if (errorMessageText != null)
        {
            if (!isValid && newName.Length > 0)
            {
                errorMessageText.text = errorMessage;
                errorMessageText.gameObject.SetActive(true);
            }
            else
            {
                errorMessageText.gameObject.SetActive(false);
            }
        }

        // Update confirm button
        if (confirmButton != null)
        {
            confirmButton.interactable = isValid && typewriterFinished;
        }

        // Play typing sound
        if (typeSound != null && newName.Length > 0)
        {
            PlaySound(typeSound);
        }
    }

    bool ValidateName(string name, out string errorMessage)
    {
        errorMessage = "";

        // Check if empty
        if (string.IsNullOrWhiteSpace(name))
        {
            errorMessage = "Ýsim boþ olamaz!";
            return false;
        }

        // Check minimum length
        if (name.Length < minNameLength)
        {
            errorMessage = $"Ýsim en az {minNameLength} karakter olmalý!";
            return false;
        }

        // Check maximum length
        if (name.Length > maxNameLength)
        {
            errorMessage = $"Ýsim en fazla {maxNameLength} karakter olabilir!";
            return false;
        }

        // Check for spaces
        if (!allowSpaces && name.Contains(" "))
        {
            errorMessage = "Ýsimde boþluk kullanýlamaz!";
            return false;
        }

        // Check for numbers
        if (!allowNumbers && ContainsNumbers(name))
        {
            errorMessage = "Ýsimde sayý kullanýlamaz!";
            return false;
        }

        // Check for special characters
        if (!allowSpecialCharacters && ContainsSpecialCharacters(name))
        {
            errorMessage = "Ýsimde özel karakter kullanýlamaz!";
            return false;
        }

        // Check for profanity (optional)
        if (IsProfane(name))
        {
            errorMessage = "Lütfen uygun bir isim seçin!";
            return false;
        }

        return true;
    }

    bool ContainsNumbers(string text)
    {
        foreach (char c in text)
        {
            if (char.IsDigit(c))
                return true;
        }
        return false;
    }

    bool ContainsSpecialCharacters(string text)
    {
        foreach (char c in text)
        {
            if (!char.IsLetterOrDigit(c) && c != ' ' && c != '_')
                return true;
        }
        return false;
    }

    bool IsProfane(string text)
    {
        // Add profanity filter here if needed
        string[] badWords = { }; // Add words to filter

        string lowerText = text.ToLower();
        foreach (string badWord in badWords)
        {
            if (lowerText.Contains(badWord.ToLower()))
                return true;
        }

        return false;
    }

    void OnSubmit(string value)
    {
        if (!typewriterFinished)
            return;

        TryConfirm();
    }

    void TryConfirm()
    {
        if (!typewriterFinished)
            return;

        if (nameInputField == null)
            return;

        string playerName = nameInputField.text.Trim();

        if (!ValidateName(playerName, out string error))
        {
            ShowError(error);
            return;
        }

        OnConfirmClicked();
    }

    void OnConfirmClicked()
    {
        string playerName = nameInputField.text.Trim();

        if (!ValidateName(playerName, out string errorMessage))
        {
            ShowError(errorMessage);
            PlaySound(errorSound);
            return;
        }

        confirmButton.gameObject.SetActive(false);
        randomNameButton.gameObject.SetActive(false);
        StartGame(playerName);
        PlaySound(confirmSound);
    }

    void OnRandomNameClicked()
    {
        if (randomNames.Length == 0) return;

        string randomName = randomNames[Random.Range(0, randomNames.Length)];

        if (nameInputField != null)
        {
            nameInputField.text = randomName;
        }
    }

    void StartGame(string playerName)
    {
        if (hasStarted) return;
        hasStarted = true;

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.SetPlayerName(playerName);
        }

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.RefreshAll();
        }

        if (welcomeText != null)
            StartCoroutine(ShowWelcomeMessage(playerName));
        else
            CompleteStart();

        PlaySound(confirmSound);
    }

    IEnumerator ShowWelcomeMessage(string playerName)
    {
        string welcomeMessage = $"Hoþ geldin, {playerName}!";

        if (welcomeText != null)
        {
            welcomeText.text = "";
            yield return StartCoroutine(TypewriterEffect(welcomeText, welcomeMessage, typewriterSpeed));
            yield return _waitForSeconds1_5;
        }

        CompleteStart();
    }

    void CompleteStart()
    {
        // Animate panel out
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger("FadeOut");
            StartCoroutine(WaitForAnimation());
        }
        else
        {
            FinalizeStart();
        }
    }

    IEnumerator WaitForAnimation()
    {
        yield return _waitForSeconds1;
        FinalizeStart();
    }

    void FinalizeStart()
    {
        // Hide name input panel
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);

        // Initialize game systems
        InitializeGame();
    }

    void InitializeGame()
    {
        // Initialize player stats
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.FullRestore();
        }

        // Set starting currency
        if (ProfileManager.Instance != null)
        {
            // Already has starting currency from profile
        }

        if (DialogueManager.Instance != null && startDialogueNode != null)
        {
            DialogueManager.Instance.StartDialogue(startDialogueNode);
        }
    }

    void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
            StartCoroutine(ShakeEffect(errorMessageText.gameObject));
        }
    }

    IEnumerator ShakeEffect(GameObject target)
    {
        Vector3 originalPos = target.transform.localPosition;
        float duration = 0.5f;
        float magnitude = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            target.transform.localPosition = originalPos + new Vector3(x, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.transform.localPosition = originalPos;
    }

    IEnumerator TypewriterEffect(TextMeshProUGUI text, string fullText, float speed)
    {
        foreach (char c in fullText)
        {
            text.text += c;
            yield return new WaitForSeconds(speed);
        }

        typewriterFinished = true;

        if (confirmButton != null && nameInputField != null)
        {
            if (ValidateName(nameInputField.text, out _))
            {
                confirmButton.interactable = true;
            }
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.7f);
        }
    }

    // Public method to reset and show name input again
    public void ShowNameInput()
    {
        hasStarted = false;

        if (nameInputPanel != null)
            nameInputPanel.SetActive(true);

        if (nameInputField != null)
        {
            nameInputField.text = "";
            nameInputField.ActivateInputField();
        }
    }
}
