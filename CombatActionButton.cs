using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatActionButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Button button;
    public TextMeshProUGUI actionNameText;
    public TextMeshProUGUI energyCostText;
    public Image iconImage;
    public GameObject notEnoughEnergyIndicator;
    public CombatAction action;

    public void Setup(CombatAction combatAction)
    {
        action = combatAction;

        if (actionNameText != null)
            actionNameText.text = action.actionName;

        if (energyCostText != null)
            energyCostText.text = $"{action.energyCost} Energy";

        if (iconImage != null && action.icon != null)
            iconImage.sprite = action.icon;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        UpdateInteractable(true);
    }

    void OnClick()
    {
        if (CombatManager.Instance == null)
            return;

        if (button != null)
            button.interactable = false;

        if (CombatUI.Instance != null)
            CombatUI.Instance.DisableAllActionButtons();

        CombatManager.Instance.ExecutePlayerAction(action);
    }

    public void UpdateInteractable(bool canUse)
    {
        bool hasEnoughEnergy = PlayerStats.Instance != null && PlayerStats.Instance.HasEnoughEnergy(action.energyCost);
        bool disabled = action.isDisabled;

        if (button != null)
            button.interactable = canUse && hasEnoughEnergy && !disabled;

        if (notEnoughEnergyIndicator != null)
            notEnoughEnergyIndicator.SetActive(!hasEnoughEnergy && !disabled);
    }
}
