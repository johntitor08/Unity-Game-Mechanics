using UnityEngine;

public class MarketUI : HotkeyPanelUI
{
    public static MarketUI Instance;
    public GameObject marketPanel;
    private bool gameStarted = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (!gameStarted || SaveSystem.IsLoading || PanelInputBlocked())
            return;

        if (Input.GetKeyDown(KeyCode.S) && marketPanel != null)
        {
            if (marketPanel.activeSelf)
                CloseAll();
            else
                OpenMarket();
        }
    }

    public void OnGameStarted()
    {
        gameStarted = true;
    }

    public void OpenMarket()
    {
        UIPanelAnimator.Show(marketPanel);
        OpenShop();
    }

    public void OpenShop()
    {
        if (MarketController.Instance != null && !MarketController.Instance.IsOpen())
        {
            if (ShopUI.Instance != null)
                ShopUI.Instance.ShowMarketClosed();

            return;
        }

        if (SellUI.Instance != null)
            SellUI.Instance.Close();

        if (ShopUI.Instance != null)
            ShopUI.Instance.Open();
    }

    public void OpenSell()
    {
        if (MarketController.Instance != null && !MarketController.Instance.IsOpen())
        {
            if (ShopUI.Instance != null)
                ShopUI.Instance.ShowMarketClosed();

            return;
        }

        if (ShopUI.Instance != null)
            ShopUI.Instance.Close();

        if (SellUI.Instance != null)
            SellUI.Instance.Open();
    }

    public void CloseAll()
    {
        if (ShopUI.Instance != null)
            ShopUI.Instance.Close();

        if (SellUI.Instance != null)
            SellUI.Instance.Close();

        UIPanelAnimator.Hide(marketPanel);
    }
}
