using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServicesManager : MonoBehaviour
{
    public static ServicesManager Instance { get; private set; }
    private UserManager userManager;
    [SerializeField] private TMP_InputField createRoomInputField;
    [SerializeField] private TMP_InputField joinRoomInputField;
    [SerializeField] private RoomItem roomItemPrefab;
    private List<RoomItem> roomItemsList = new List<RoomItem>();
    [SerializeField] private Transform roomListContentObject;
    public bool isHost;
    public Lobby hostLobby;
    public Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private float listLobbiesTimer;
    private string gameMode;
    [SerializeField] Canvas lobbyPanel;
    [SerializeField] Canvas transitionPanel;

    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }

    }

    private async void Start()
    {
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        await InitializeUnityAuthentication();
        SetupEvents();
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
        lobbyPanel.gameObject.SetActive(true);
        transitionPanel.gameObject.SetActive(false);

        if (SceneManager.GetActiveScene().name == "MPSLobby")
        {
            gameMode = "Shooter";

        }
        else if (SceneManager.GetActiveScene().name == "MPPLobby")
        {
            gameMode = "Platformer";

        }
        else if (SceneManager.GetActiveScene().name == "MPSVLobby")
        {
            gameMode = "Survivor";

        }

    }

    private async Task InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new();
            initializationOptions.SetProfile(userManager.user.username);
            await UnityServices.InitializeAsync(initializationOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        }

    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
        HandlePeriodicListLobbies();

    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;

            if (heartbeatTimer < 0)
            {
                heartbeatTimer = 15;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);

            }

        }

    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;

            if (lobbyUpdateTimer < 0)
            {
                lobbyUpdateTimer = 5;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;

            }

        }

    }

    private void HandlePeriodicListLobbies()
    {
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn && SceneManager.GetActiveScene().name.Contains("Lobby"))
        {
            listLobbiesTimer -= Time.deltaTime;

            if (listLobbiesTimer < 0)
            {
                listLobbiesTimer = 3;
                ListLobbies();

            }

        }

    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = createRoomInputField.text.Trim();
            int maxPlayers = 4;

            if (gameMode == "Survivor")
            {
                maxPlayers = 5;

            }

            if (lobbyName.Length < 3 || lobbyName.Length > 25)
            {
                return;

            }

            lobbyPanel.gameObject.SetActive(false);
            transitionPanel.gameObject.SetActive(true);

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1)

                    },
                    {
                        "MapIndex", new DataObject(DataObject.VisibilityOptions.Public, "", DataObject.IndexOptions.S2)

                    },
                    {
                        "BackgroundIndex", new DataObject(DataObject.VisibilityOptions.Public, "", DataObject.IndexOptions.S3)

                    },
                    {
                        "MountainIndex", new DataObject(DataObject.VisibilityOptions.Public, "", DataObject.IndexOptions.S4)

                    },

                }

            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)

                    }

                }

            });

#if !UNITY_WEBGL

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key,
                allocation.ConnectionData);

#else

            NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = true;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key,
                allocation.ConnectionData, isSecure: true);

#endif
            hostLobby = lobby;
            joinedLobby = lobby;
            StartHost();

        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            lobbyPanel.gameObject.SetActive(true);
            transitionPanel.gameObject.SetActive(false);

        }

    }

    private async void ListLobbies()
    {
        QueryLobbiesOptions queryLobbiesOptions = new()
        {
            Count = 25,

            Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                new QueryFilter(QueryFilter.FieldOptions.S1, gameMode, QueryFilter.OpOptions.EQ)

            },

            Order = new List<QueryOrder>
            {
                new QueryOrder(false, QueryOrder.FieldOptions.Created)

            }

        };

        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

        foreach (RoomItem roomItem in roomItemsList)
        {
            Destroy(roomItem.gameObject);

        }

        roomItemsList.Clear();

        foreach (Lobby lobby in queryResponse.Results)
        {
            RoomItem newRoomItem = Instantiate(roomItemPrefab, roomListContentObject);
            roomItemsList.Add(newRoomItem);
            newRoomItem.SetLobby(lobby);

        }

    }

    public async void QuickJoinLobby()
    {
        try
        {
            lobbyPanel.gameObject.SetActive(false);
            transitionPanel.gameObject.SetActive(true);

            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions()
            {
                Player = GetPlayer()

            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

#if !UNITY_WEBGL

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

#else

            NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = true;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData, isSecure: true);

#endif
            joinedLobby = lobby;
            StartClient();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby Service Exception: {e.Message}");
            lobbyPanel.gameObject.SetActive(true);
            transitionPanel.gameObject.SetActive(false);

        }

    }

    public async void JoinLobbyByCode()
    {
        try
        {
            if (joinRoomInputField.text.Length < 3 || joinRoomInputField.text.Length > 25)
            {
                return;

            }

            lobbyPanel.gameObject.SetActive(false);
            transitionPanel.gameObject.SetActive(true);

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
            {
                Player = GetPlayer()

            };

            string lobbyCode = joinRoomInputField.text;
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

#if !UNITY_WEBGL

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

#else

            NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = true;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData, isSecure: true);

#endif
            joinedLobby = lobby;
            StartClient();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
            lobbyPanel.gameObject.SetActive(true);
            transitionPanel.gameObject.SetActive(false);

        }

    }

    public async void JoinLobbyById(string lobbyId)
    {
        try
        {
            lobbyPanel.gameObject.SetActive(false);
            transitionPanel.gameObject.SetActive(true);

            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions()
            {
                Player = GetPlayer()

            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);
            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

#if !UNITY_WEBGL

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

#else

            NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = true;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData, isSecure: true);

#endif
            joinedLobby = lobby;
            StartClient();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
            lobbyPanel.gameObject.SetActive(true);
            transitionPanel.gameObject.SetActive(false);

        }

    }

    private async void DeleteLobby()
    {
        if (joinedLobby == null) return;
        string lobbyId = joinedLobby.Id;
        joinedLobby = null;

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);

        }
        catch (LobbyServiceException e)
        {
            if (e.Reason != LobbyExceptionReason.LobbyNotFound)
                Debug.LogError(e.Message);

        }

    }

    private async void LeaveLobby()
    {
        if (joinedLobby == null) return;

        try
        {
            bool wasHost = joinedLobby.Players.Count > 0 &&
                           AuthenticationService.Instance.PlayerId == joinedLobby.Players[0].Id;

            string lobbyId = joinedLobby.Id;
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId);

            if (wasHost)
            {
                MigrateLobbyHost();

            }
            else
            {
                joinedLobby = null;

            }

        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                joinedLobby = null;
            else
                Debug.LogError(e.Message);

        }

    }

    private async void MigrateLobbyHost()
    {
        if (joinedLobby == null || joinedLobby.Players.Count <= 1)
        {
            DeleteLobby();
            return;

        }

        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions()
            {
                HostId = joinedLobby.Players[1].Id

            });

        }
        catch (LobbyServiceException e)
        {
            if (e.Reason != LobbyExceptionReason.LobbyNotFound)
                Debug.LogError(e.Message);

        }

        joinedLobby = null;

    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            //Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);

        }

    }

    public Player GetPlayer()
    {
        string displayName = userManager.user.username.Length > 0
            ? char.ToUpper(userManager.user.username[0]) + userManager.user.username.Substring(1)
            : userManager.user.username;

        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, displayName)

                }

            }

        };

    }

    private void ClientConnected(ulong id)
    {

    }

    private void ClientDisconnected(ulong id)
    {
        // Only handle our own disconnection, not other players leaving
        if (joinedLobby == null) return;
        if (NetworkManager.Singleton != null && id != NetworkManager.Singleton.LocalClientId) return;

        LeaveLobby();

    }

    private void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {

            //Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            //Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed += (err) => {

            //Debug.LogError(err);

        };

        AuthenticationService.Instance.SignedOut += () => {

            //Debug.Log("Player signed out.");

        };

    }

    private void StartHost()
    {
        isHost = true;

        if (SceneManager.GetActiveScene().name == "MPSLobby")
        {
            SceneManager.LoadScene("MPSGame");

        }
        else if (SceneManager.GetActiveScene().name == "MPPLobby")
        {
            SceneManager.LoadScene("MPPGame");

        }
        else if (SceneManager.GetActiveScene().name == "MPSVLobby")
        {
            SceneManager.LoadScene("MPSVGame");

        }

    }

    private void StartClient()
    {
        isHost = false;

        if (SceneManager.GetActiveScene().name == "MPSLobby")
        {
            SceneManager.LoadScene("MPSGame");

        }
        else if (SceneManager.GetActiveScene().name == "MPPLobby")
        {
            SceneManager.LoadScene("MPPGame");

        }
        else if (SceneManager.GetActiveScene().name == "MPSVLobby")
        {
            SceneManager.LoadScene("MPSVGame");

        }

    }

    public void Back()
    {
        SceneManager.LoadScene("GameOptions");

    }

}
