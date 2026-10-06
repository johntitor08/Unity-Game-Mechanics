using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsPanel : MonoBehaviour
{
    public GameObject panel;
    public GameObject buttonPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public Button openButton;
    public Button closeButton;

    void Start()
    {
        if (openButton != null)
            openButton.onClick.AddListener(Open);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if (panel != null && panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open()
    {
        Refresh();

        if (buttonPanel != null)
            buttonPanel.SetActive(false);

        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);

        if (buttonPanel != null)
            buttonPanel.SetActive(true);
    }

    void Refresh()
    {
        if (titleText != null)
            titleText.text = Loc.T("Credits", "Emeği Geçenler");

        if (bodyText == null)
            return;

        bodyText.text = Loc.T("<b>A game by noraStudios</b>\n\n" + "<b>Music</b>\n" + "\"Angevin\", \"Five Armies\", \"Lord of the Land\", \"Night Vigil\", \"Suonatore di Liuto\"\n" + "Kevin MacLeod (incompetech.com)\n" + "Licensed under Creative Commons: By Attribution 4.0\n" + "creativecommons.org/licenses/by/4.0\n\n" + "<b>Icons</b>\n" + "Lorc, Delapouite, Guard13007, Skoll, Lucas, Faithtoken and Caro Asercion — game-icons.net\n" + "Licensed under CC BY 3.0 (creativecommons.org/licenses/by/3.0). Some icons recoloured or modified.\n\n" + "<b>Fonts</b>\n" + "Arvo (Anton Koovit), Roboto Slab (Christian Robertson), Liberation Sans (Red Hat)\n" + "SIL Open Font License 1.1 / Apache License 2.0\n\n" + "Made with Unity. Steam integration via Steamworks.NET (MIT).\n\n" + "Thank you for playing.", "<b>noraStudios oyunu</b>\n\n" + "<b>Müzik</b>\n" + "\"Angevin\", \"Five Armies\", \"Lord of the Land\", \"Night Vigil\", \"Suonatore di Liuto\"\n" + "Kevin MacLeod (incompetech.com)\n" + "Creative Commons Atıf 4.0 lisansı ile\n" + "creativecommons.org/licenses/by/4.0\n\n" + "<b>İkonlar</b>\n" + "Lorc, Delapouite, Guard13007, Skoll, Lucas, Faithtoken ve Caro Asercion — game-icons.net\n" + "CC BY 3.0 lisansı ile (creativecommons.org/licenses/by/3.0). Bazı ikonlar yeniden renklendirildi ya da değiştirildi.\n\n" + "<b>Fontlar</b>\n" + "Arvo (Anton Koovit), Roboto Slab (Christian Robertson), Liberation Sans (Red Hat)\n" + "SIL Open Font License 1.1 / Apache License 2.0\n\n" + "Unity ile yapıldı. Steam entegrasyonu: Steamworks.NET (MIT).\n\n" + "Oynadığın için teşekkürler.");
    }
}
