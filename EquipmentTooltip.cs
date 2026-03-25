using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class EquipmentTooltip : MonoBehaviour
{
    public static EquipmentTooltip Instance;
    private CanvasGroup canvasGroup;
    private readonly List<TextMeshProUGUI> activeSetTexts = new();

    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI rarityText;
    public Transform setBonusParent;
    public TextMeshProUGUI setBonusTextPrefab;
    public Image backgroundImage;
    public RectTransform backgroundRect;

    void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    void Update()
    {
        if (canvasGroup.alpha > 0)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, Input.mousePosition, null, out Vector2 position);
            backgroundRect.anchoredPosition = position + new Vector2(10, -10);
        }
    }

    public void Show(EquipmentData equipment)
    {
        if (equipment == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        nameText.text = equipment.itemName;
        rarityText.text = equipment.rarity.ToString();
        statsText.text = equipment.GetStatsDescription();
        requirementsText.text = GetRequirementsText(equipment);
        Color rarityColor = equipment.GetRarityColor();
        nameText.color = rarityColor;
        rarityText.color = rarityColor;

        if (backgroundImage != null)
            backgroundImage.color = rarityColor * new Color(1f, 1f, 1f, 0.2f);

        UpdateSetBonuses(equipment);
    }

    void UpdateSetBonuses(EquipmentData equipment)
    {
        foreach (var t in activeSetTexts)
            if (t != null)
                Destroy(t.gameObject);

        activeSetTexts.Clear();

        if (EquipmentManager.Instance == null || setBonusParent == null || setBonusTextPrefab == null || equipment.setData == null)
            return;

        int pieces = EquipmentManager.Instance.GetEquippedSetPieces(equipment.setData.setID);

        foreach (var bonus in equipment.setData.bonuses)
        {
            string description = $"{bonus.requiredPieces}-Piece: +{bonus.value} {bonus.stat}";
            var text = Instantiate(setBonusTextPrefab, setBonusParent);
            text.text = description;
            bool isActive = pieces >= bonus.requiredPieces;
            text.color = isActive ? bonus.requiredPieces switch
            {
                2 => Color.green,
                3 => Color.cyan,
                4 => new Color(0.8f, 0f, 0.8f),
                _ => Color.white
            }
            : Color.gray;

            activeSetTexts.Add(text);
        }
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        foreach (var t in activeSetTexts)
        {
            if (t != null)
                Destroy(t.gameObject);
        }

        activeSetTexts.Clear();
    }

    string GetRequirementsText(EquipmentData equipment)
    {
        string text = "";

        if (equipment.requiredLevel > 1)
            text += $"Level {equipment.requiredLevel}+ required\n";

        if (equipment.requiredStatValue > 0)
            text += $"{equipment.requiredStat} {equipment.requiredStatValue}+ required";

        return text;
    }
}
