using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Slot Info")]
    public int slotIndex;

    [Header("UI References")]
    public TextMeshProUGUI slotLabel;
    public TextMeshProUGUI metaText;
    public Button saveButton;
    public Button loadButton;
    public Button deleteButton;
    private SaveUI parentUI;

    public void Initialize(int index, SaveUI parent)
    {
        slotIndex = index;
        parentUI = parent;

        if (slotLabel != null)
            slotLabel.text = $"Slot {index + 1}";

        if (saveButton != null)
            saveButton.onClick.AddListener(OnSave);

        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoad);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDelete);

        Refresh();
    }

    public void Refresh()
    {
        bool hasSave = SaveSystem.HasSaveFile(slotIndex);

        if (loadButton != null)
            loadButton.interactable = hasSave;

        if (deleteButton != null)
            deleteButton.interactable = hasSave;

        if (metaText != null)
        {
            if (hasSave)
            {
                var data = SaveSystem.PeekSlot(slotIndex);

                if (data != null)
                    metaText.text = BuildMeta(data);
                else
                    metaText.text = "Corrupted";
            }
            else
            {
                metaText.text = "Empty";
            }
        }
    }

    string BuildMeta(SaveData data)
    {
        string phase = data.currentTimePhase.ToString();
        string day = $"Day {data.currentDay}";
        string time = string.IsNullOrEmpty(data.savedAt) ? "" : $"  ·  {data.savedAt}";
        return $"{day}  ·  {phase}{time}";
    }

    public void SetInteractable(bool interactable)
    {
        if (saveButton != null)
            saveButton.interactable = interactable;

        if (loadButton != null)
            loadButton.interactable = interactable;

        if (deleteButton != null)
            deleteButton.interactable = interactable;
    }

    void OnSave()
    {
        if (SaveSystem.IsLoading)
            return;

        if (parentUI != null)
            parentUI.SetAllSlotsInteractable(false);

        SaveSystem.SetActiveSlot(slotIndex);
        SaveSystem.SaveGame(slotIndex);
        Refresh();

        if (parentUI != null)
            parentUI.SetAllSlotsInteractable(true);

        if (parentUI != null)
            parentUI.ShowToast("Game Saved!");
    }

    void OnLoad()
    {
        if (!SaveSystem.HasSaveFile(slotIndex))
            return;

        SaveSystem.SetActiveSlot(slotIndex);
        SaveSystem.LoadGame(slotIndex);
    }

    void OnDelete()
    {
        SaveSystem.DeleteSave(slotIndex);
        Refresh();
    }

    void OnDestroy()
    {
        if (saveButton != null)
            saveButton.onClick.RemoveListener(OnSave);

        if (loadButton != null)
            loadButton.onClick.RemoveListener(OnLoad);

        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(OnDelete);
    }
}
