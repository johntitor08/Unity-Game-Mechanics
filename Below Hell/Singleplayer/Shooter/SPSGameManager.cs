using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SPSGameManager : MonoBehaviour
{
    [SerializeField] private GameObject panelPlayerDied;
    [SerializeField] private GameObject panelOptions;
    public TMP_Text scoreText;
    public TMP_Text scoreTextInPanelPlayerDied;
    public bool isJustDied = false;

    private void Start()
    {
        panelPlayerDied.SetActive(false);
        panelOptions.SetActive(false);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelOptions.activeInHierarchy)
            {
                panelOptions.SetActive(false);

            }
            else
            {
                panelOptions.SetActive(true);

            }

        }

        if (panelOptions.activeInHierarchy)
        {
            Time.timeScale = 0;

        }
        else
        {
            Time.timeScale = 1;

        }

        if (isJustDied)
        {
            Invoke(nameof(PanelDied), 1f);
            isJustDied = false;

        }

    }

    public void PanelDied()
    {
        panelPlayerDied.SetActive(true);

    }

    public void ReturnToGame()
    {
        panelOptions.SetActive(false);

    }

    public void ReloadTheGame()
    {
        panelOptions.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public void LeaveRoom()
    {
        SceneManager.LoadScene("GameOptions");

    }

}