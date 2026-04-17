using UnityEngine;

public class MarketUI : MonoBehaviour
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
        if (!gameStarted)
            return;

        if (Input.GetKeyDown(KeyCode.S))
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
        marketPanel.SetActive(true);
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

        marketPanel.SetActive(false);
    }
}
