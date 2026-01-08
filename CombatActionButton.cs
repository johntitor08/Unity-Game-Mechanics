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

    private CombatAction action;

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
            button.onClick.AddListener(OnClick);

        UpdateInteractable(true);
    }

    void OnClick()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ExecutePlayerAction(action);
        }
    }

    public void UpdateInteractable(bool canUse)
    {
        bool hasEnoughEnergy = PlayerStats.Instance.HasEnoughEnergy(action.energyCost);
        bool interactable = canUse && hasEnoughEnergy;

        if (button != null)
            button.interactable = interactable;

        if (notEnoughEnergyIndicator != null)
            notEnoughEnergyIndicator.SetActive(!hasEnoughEnergy);
    }
}
