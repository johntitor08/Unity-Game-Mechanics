using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    public static ProfileUI Instance;
    private bool isSubscribed = false;

    [Header("Profile Panel")]
    public GameObject profilePanel;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currencyText;
    public Slider expBar;
    public TextMeshProUGUI expText;

    [Header("Settings")]
    public bool showProfilePanel = true;
    public KeyCode toggleKey = KeyCode.P;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (profilePanel != null)
            profilePanel.SetActive(showProfilePanel);

        TrySubscribe();
    }

    void OnEnable()
    {
        if (ProfileManager.Instance == null)
        {
            ProfileManager.OnReady += TrySubscribe;
        }
    }

    void OnDisable()
    {
        if (isSubscribed && ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnProfileChanged -= UpdateProfileUI;
            isSubscribed = false;
        }

        ProfileManager.OnReady -= TrySubscribe;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && profilePanel != null)
            profilePanel.SetActive(!profilePanel.activeSelf);
    }

    void TrySubscribe()
    {
        if (ProfileManager.Instance != null && !isSubscribed)
        {
            ProfileManager.Instance.OnProfileChanged += UpdateProfileUI;
            isSubscribed = true;
            UpdateProfileUI();
        }
    }

    void UpdateProfileUI()
    {
        PlayerProfile profile;

        if (ProfileManager.Instance != null)
            profile = ProfileManager.Instance.profile;
        else return;
        
        playerNameText.text = profile.playerName;
        levelText.text = "Level " + profile.level;
        currencyText.text = profile.currency + " Gold";
        expBar.maxValue = profile.experienceToNextLevel;
        expBar.value = profile.experience;
        expText.text = $"{profile.experience} / {profile.experienceToNextLevel} XP";
    }
}
