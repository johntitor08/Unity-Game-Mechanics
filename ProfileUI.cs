using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [Header("Profile Panel")]
    public GameObject profilePanel;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currencyText;
    public Slider experienceSlider;
    public TextMeshProUGUI experienceText;

    [Header("Stats Display")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI charismaText;

    void Start()
    {
        ProfileManager.Instance.OnProfileChanged += UpdateUI;
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            profilePanel.SetActive(!profilePanel.activeSelf);
    }

    void UpdateUI()
    {
        var profile = ProfileManager.Instance.profile;
        playerNameText.text = profile.playerName;
        levelText.text = "Level " + profile.level;
        currencyText.text = profile.currency + " Gold";
        experienceSlider.maxValue = profile.experienceToNextLevel;
        experienceSlider.value = profile.experience;
        experienceText.text = profile.experience + " / " + profile.experienceToNextLevel + " XP";
        healthText.text = "Health: " + PlayerStats.Instance.Get(StatType.Health);
        energyText.text = "Energy: " + PlayerStats.Instance.Get(StatType.Energy);
        strengthText.text = "Strength: " + PlayerStats.Instance.Get(StatType.Strength);
        intelligenceText.text = "Intelligence: " + PlayerStats.Instance.Get(StatType.Intelligence);
        charismaText.text = "Charisma: " + PlayerStats.Instance.Get(StatType.Charisma);
    }
}
