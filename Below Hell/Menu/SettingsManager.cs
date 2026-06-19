using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    private UserManager userManager;

    private void Start()
    {
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();

    }

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");

    }

}