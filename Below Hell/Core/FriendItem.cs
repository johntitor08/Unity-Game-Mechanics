using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FriendItem : MonoBehaviour
{
    public TMP_Text playerNameText;

    public void OpenProfile()
    {
        if (SceneManager.GetActiveScene().name == "MPPGame")
        {
            StartCoroutine(GameObject.Find("GameManager").GetComponent<MPPGameManager>().CheckPlayerUsernameFromDatabase(playerNameText.text));

        }
        else if (SceneManager.GetActiveScene().name == "MPSVGame")
        {
            StartCoroutine(GameObject.Find("GameManager").GetComponent<MPSVGameManager>().CheckPlayerUsernameFromDatabase(playerNameText.text));

        }

    }

    public void RemoveFriend()
    {
        if (SceneManager.GetActiveScene().name == "MPPGame")
        {
            StartCoroutine(GameObject.Find("GameManager").GetComponent<MPPGameManager>().RemoveFriendFromDatabase(playerNameText.text));

        }
        else if (SceneManager.GetActiveScene().name == "MPSVGame")
        {
            StartCoroutine(GameObject.Find("GameManager").GetComponent<MPSVGameManager>().RemoveFriendFromDatabase(playerNameText.text));

        }

    }

}