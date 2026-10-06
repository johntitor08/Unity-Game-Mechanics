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

        if (slotLabel != null)
            slotLabel.text = $"{Loc.T("Slot", "Yuva")} {slotIndex + 1}";

        SetButton(loadButton, hasSave);
        SetButton(deleteButton, hasSave);

        if (metaText != null)
        {
            if (hasSave)
            {
                var data = SaveSystem.PeekSlot(slotIndex);
                metaText.text = data != null ? BuildMeta(data) : Loc.T("Corrupted", "Bozuk");
            }
            else
            {
                metaText.text = Loc.T("Empty", "Boş");
            }
        }
    }

    string BuildMeta(SaveData data)
    {
        string phase = TimeUI.GetPhaseName(data.currentTimePhase);
        string day = $"{Loc.T("Day", "Gün")} {data.currentDay}";
        string time = string.IsNullOrEmpty(data.savedAt) ? "" : $"\n{data.savedAt}";
        return $"{day} · {phase}{time}";
    }

    public void SetInteractable(bool interactable)
    {
        SetButton(saveButton, interactable);

        if (interactable)
        {
            Refresh();
        }
        else
        {
            SetButton(loadButton, false);
            SetButton(deleteButton, false);
        }
    }

    static void SetButton(Button button, bool interactable)
    {
        if (button == null)
            return;

        button.interactable = interactable;
        var label = button.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
            label.alpha = interactable ? 1f : 0.35f;
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
        {
            parentUI.SetAllSlotsInteractable(true);
            parentUI.ShowToast(Loc.T("Game Saved!", "Oyun Kaydedildi!"));
        }
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
