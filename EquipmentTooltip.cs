using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentTooltip : MonoBehaviour
{
    public static EquipmentTooltip Instance;

    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI rarityText;
    public Transform setBonusParent;
    public TextMeshProUGUI setBonusTextPrefab;
    public Image backgroundImage;
    public RectTransform backgroundRect;

    private CanvasGroup canvasGroup;
    private readonly List<TextMeshProUGUI> activeSetTexts = new();

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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform,
                Input.mousePosition,
                null,
                out Vector2 position
            );
            backgroundRect.anchoredPosition = position + new Vector2(10, -10);
        }
    }

    public void Show(EquipmentData equipment)
    {
        if (equipment == null) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        nameText.text = equipment.itemName;
        rarityText.text = equipment.rarity.ToString();
        statsText.text = equipment.GetStatsDescription();
        requirementsText.text = GetRequirementsText(equipment);
        Color rarityColor = equipment.GetRarityColor();
        nameText.color = rarityColor;
        rarityText.color = rarityColor;

        if (backgroundImage != null)
            backgroundImage.color = rarityColor * new Color(1f, 1f, 1f, 0.2f);

        // Set bonuslarýný göster
        UpdateSetBonuses(equipment);
    }

    void UpdateSetBonuses(EquipmentData equipment)
    {
        // Temizle
        foreach (var t in activeSetTexts)
        {
            if (t != null) Destroy(t.gameObject);
        }

        activeSetTexts.Clear();
        if (setBonusParent == null || setBonusTextPrefab == null) return;
        var setBonuses = EquipmentManager.Instance.GetActiveSetBonusDescriptions();

        foreach (var bonus in setBonuses)
        {
            var text = Instantiate(setBonusTextPrefab, setBonusParent);
            text.text = bonus;
            if (bonus.StartsWith("2-Piece")) text.color = Color.green;
            else if (bonus.StartsWith("3-Piece")) text.color = Color.cyan;
            else if (bonus.StartsWith("4-Piece")) text.color = new Color(0.8f, 0f, 0.8f); // purple
            activeSetTexts.Add(text);
        }
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        // Set bonuslarý temizle
        foreach (var t in activeSetTexts)
        {
            if (t != null) Destroy(t.gameObject);
        }

        activeSetTexts.Clear();
    }

    string GetRequirementsText(EquipmentData equipment)
    {
        string text = "";

        if (equipment.requiredLevel > 1)
            text += $"Level {equipment.requiredLevel}+ required\n";

        if (equipment.requiredStatValue > 0)
            text += $"{equipment.requiredStat} {equipment.requiredStatValue}+ required\n";

        return text;
    }
}
