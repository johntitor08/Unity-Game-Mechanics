using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class RoomItem : MonoBehaviour
{
    [SerializeField] private TMP_Text roomName;
    private ServicesManager servicesManager;
    private Lobby lobby;

    private void Awake()
    {
        var networkGo = GameObject.Find("NetworkManager");

        if (networkGo != null)
        {
            servicesManager = networkGo.GetComponent<ServicesManager>();

            if (servicesManager == null)
                Debug.LogWarning("ServicesManager bileþeni NetworkManager üzerinde bulunamadý.", this);
        }
        else
        {
            Debug.LogWarning("NetworkManager GameObject sahnede bulunamadý.", this);
        }
    }

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;

        if (roomName == null)
        {
            Debug.LogWarning("roomName (TMP_Text) Inspector'da atanmamýþ.", this);
            return;
        }

        roomName.text = lobby != null ? lobby.Name : "<null>";
    }

    public void OnClickItem()
    {
        if (servicesManager == null)
        {
            Debug.LogError("Join iþlemi baþarýsýz: ServicesManager null.", this);
            return;
        }

        if (lobby == null)
        {
            Debug.LogError("Join iþlemi baþarýsýz: Lobby null.", this);
            return;
        }

        servicesManager.JoinLobbyById(lobby.Id);
    }
}