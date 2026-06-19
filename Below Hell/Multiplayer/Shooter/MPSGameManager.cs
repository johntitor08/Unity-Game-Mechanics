using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class MPSGameManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1);
    private ServicesManager servicesManager;
    public GameObject panelPlayerList;
    public GameObject panelPlayerDied;
    public GameObject panelOptions;
    public PlayerNameItem playerNameItemPrefab;
    public Transform playerNameItemContentObject;

    private void Start()
    {
        servicesManager = GameObject.Find("NetworkManager").GetComponent<ServicesManager>();
        panelPlayerList.SetActive(false);
        panelPlayerDied.SetActive(false);
        panelOptions.SetActive(false);

        if (servicesManager.isHost)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    public void ReturnToGame()
    {
        panelOptions.SetActive(false);
    }

    public void Respawn()
    {
        panelPlayerDied.SetActive(false);
        StartCoroutine(Restart());
    }

    private IEnumerator Restart()
    {
        NetworkManager.Singleton.Shutdown();
        yield return _waitForSeconds1;

        if (servicesManager.isHost)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    public void LeaveRoom()
    {
        panelOptions.SetActive(false);
        panelPlayerDied.SetActive(false);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("GameOptions");
    }
}
