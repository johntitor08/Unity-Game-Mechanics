using UnityEngine;

public class ChooseAuthenticationAction : MonoBehaviour
{
    [SerializeField] GameObject registerPanel;
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject optionPanel;

    private void Start()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(false);
        optionPanel.SetActive(true);

    }

    public void ActivateRegisterPanel()
    {
        registerPanel.SetActive(true);
        optionPanel.SetActive(false);

    }

    public void ActivateLoginPanel()
    {
        loginPanel.SetActive(true);
        optionPanel.SetActive(false);

    }

    public void BackToOptions()
    {
        if (registerPanel.activeInHierarchy)
        {
            registerPanel.SetActive(false);
            optionPanel.SetActive(true);

        }
        else if (loginPanel.activeInHierarchy)
        {
            loginPanel.SetActive(false);
            optionPanel.SetActive(true);

        }

    }

}
