using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOptionManager : MonoBehaviour
{
    [SerializeField] private GameObject modePanel;
    [SerializeField] private GameObject genrePanel;
    [SerializeField] private GameObject shooterButton;
    [SerializeField] private GameObject platformerButton;
    [SerializeField] private GameObject survivorButton;
    [SerializeField] private GameObject fighterButton;
    private string action;

    private void Start()
    {
        modePanel.SetActive(true);
        genrePanel.SetActive(false);
        Time.timeScale = 1;

        if (GameObject.Find("NetworkManager"))
        {
            Destroy(GameObject.Find("NetworkManager"));

        }

    }

    public void Singleplayer()
    {
        modePanel.SetActive(false);
        genrePanel.SetActive(true);
        shooterButton.SetActive(true);
        platformerButton.SetActive(false);
        survivorButton.SetActive(false);
        fighterButton.SetActive(true);
        action = "Singleplayer";

    }

    public void Multiplayer()
    {
        modePanel.SetActive(false);
        genrePanel.SetActive(true);
        shooterButton.SetActive(true);
        platformerButton.SetActive(true);
        survivorButton.SetActive(true);
        fighterButton.SetActive(false);
        action = "Multiplayer";

    }

    public void Shooter()
    {
        if (action == "Singleplayer")
        {
            SceneManager.LoadScene("SPSLobby");

        }
        else if (action == "Multiplayer")
        {
            SceneManager.LoadScene("MPSLobby");

        }

    }

    public void Platformer()
    {
        if (action == "Multiplayer")
        {
            SceneManager.LoadScene("MPPLobby");

        }

    }

    public void Survivor()
    {
        if (action == "Multiplayer")
        {
            SceneManager.LoadScene("MPSVLobby");

        }

    }

    public void Fighter()
    {
        if (action == "Singleplayer")
        {
            SceneManager.LoadScene("SPFLobby");

        }

    }

    public void Back()
    {
        modePanel.SetActive(true);
        genrePanel.SetActive(false);

    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");

    }

}