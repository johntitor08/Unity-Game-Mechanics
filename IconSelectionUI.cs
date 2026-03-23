using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class IconSelectionUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject selectionPanel;
    public KeyCode toggleKey = KeyCode.I;

    [Header("Grid")]
    public Transform iconGrid;
    public GameObject iconSlotPrefab;

    [Header("Database")]
    public ProfileIconDatabase iconDatabase;

    [Header("Selected Preview")]
    public Image previewImage;
    public TextMeshProUGUI previewNameText;
    public TextMeshProUGUI previewCostText;
    public Button selectButton;
    public TextMeshProUGUI selectButtonText;
    private string hoveredIconID;
    private readonly List<IconSlotUI> spawnedSlots = new();

    private void OnEnable()
    {
        ProfileManager.Instance.OnProfileChanged += OnProfileChanged;
        BuildGrid();
    }

    private void OnDisable()
    {
        if (ProfileManager.Instance != null)
            ProfileManager.Instance.OnProfileChanged -= OnProfileChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && selectionPanel != null)
        {
            selectionPanel.SetActive(!selectionPanel.activeSelf);

            if (selectionPanel.activeSelf)
                BuildGrid();
        }
    }

    private void BuildGrid()
    {
        foreach (var slot in spawnedSlots)
            if (slot != null) Destroy(slot.gameObject);

        spawnedSlots.Clear();
        if (iconDatabase == null || iconGrid == null || iconSlotPrefab == null) return;

        foreach (var entry in iconDatabase.icons)
        {
            var go = Instantiate(iconSlotPrefab, iconGrid);
            var slot = go.GetComponent<IconSlotUI>();
            if (slot == null) continue;
            bool unlocked = ProfileManager.Instance.IsIconUnlocked(entry.id);
            bool selected = ProfileManager.Instance.profile.profileIconID == entry.id;
            slot.Setup(entry, unlocked, selected, OnSlotClicked);
            spawnedSlots.Add(slot);
        }
    }

    private void OnSlotClicked(string iconID)
    {
        hoveredIconID = iconID;
        var entry = iconDatabase.icons.Find(e => e.id == iconID);
        bool unlocked = ProfileManager.Instance.IsIconUnlocked(iconID);
        bool selected = ProfileManager.Instance.profile.profileIconID == iconID;

        if (previewImage != null)
            previewImage.sprite = entry.sprite;

        if (previewNameText != null)
            previewNameText.text = entry.displayName;

        if (previewCostText != null)
            previewCostText.text = unlocked ? "Unlocked" : $"{entry.cost} Gold";

        if (selectButton != null)
        {
            selectButton.interactable = !selected;

            if (selectButtonText != null)
            {
                if (selected) selectButtonText.text = "Selected";
                else if (unlocked) selectButtonText.text = "Select";
                else selectButtonText.text = $"Buy ({entry.cost} Gold)";
            }
        }
    }

    public void OnSelectButtonClicked()
    {
        if (string.IsNullOrEmpty(hoveredIconID)) return;
        bool unlocked = ProfileManager.Instance.IsIconUnlocked(hoveredIconID);

        if (unlocked)
        {
            ProfileManager.Instance.SelectIcon(hoveredIconID);
        }
        else
        {
            var entry = iconDatabase.icons.Find(e => e.id == hoveredIconID);
            bool success = ProfileManager.Instance.PurchaseIcon(hoveredIconID, entry.cost);

            if (success)
                ProfileManager.Instance.SelectIcon(hoveredIconID);
            else
            {
                Debug.Log("Not enough currency.");
            }
        }
    }

    private void OnProfileChanged(PlayerProfile _) => BuildGrid();
}
