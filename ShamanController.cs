using UnityEngine;

#if !UNITY_WEBGL

using Firebase.Database;
using WebSocketSharp;

#endif

using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class ShamanController : MonoBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private ServicesManager servicesManager;
    private UserManager userManager;
    private MPSVGameManager gameManager;
    private SetFirst setFirst;
    private GameObject cannonball;
    private readonly float movementSpeed = 15;
    private float spawnObjectTime = 0;
    private readonly List<PlayerNameItem> playerNameItems = new();
    private int mapIndex;
    private int mapBackground;
    private int mapMountain;
    private int playerCount = 0;

    private void Start()
    {
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        servicesManager = GameObject.Find("NetworkManager").GetComponent<ServicesManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<MPSVGameManager>();
        setFirst = GameObject.Find("GameManager").GetComponent<SetFirst>();
        gameManager.registrationDateText.text = "Registration Date: " + userManager.user.registrationDate.ToString().Split(' ')[0];

#if UNITY_WEBGL

        StartCoroutine(setFirst.GetFirstFromDatabase(userManager.user, userManager.IdToken));

#else

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        StartCoroutine(setFirst.GetFirstFromDatabase(databaseReference, userManager.user));

#endif

        gameManager.sendMessageButton.onClick.AddListener(SendMessage);
        gameManager.setProfileImageButton.SetActive(true);
        SetLobbyMap();
        StartCoroutine(gameManager.GetFriendsFromDatabase());

        if (string.IsNullOrEmpty(userManager.user.tribe))
        {
            gameManager.tribeNameText.gameObject.SetActive(false);
            Vector2 genderText = gameManager.genderText.transform.localPosition;
            genderText.y -= 75;
            gameManager.genderText.transform.localPosition = genderText;
        }
    }

    private void Update()
    {
        if (!gameManager.isOnInputChatField)
        {
            float verticalHareket = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
            transform.Translate(0, verticalHareket, 0, Space.World);
            float horizontalHareket = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
            transform.Translate(horizontalHareket, 0, 0);

            if (Input.GetMouseButtonDown(0) && spawnObjectTime > 3)
            {
                Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                clickPosition.z = 0;
                if (cannonball) Destroy(cannonball);
                cannonball = Instantiate(gameManager.cannonballPrefab, clickPosition, Quaternion.identity);
                cannonball.GetComponent<NetworkObject>().Spawn();
                spawnObjectTime = 0;
            }

            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, 0, 150),
                Mathf.Clamp(transform.position.y, 0, 60),
                -10);

            gameManager.usernameText.text = userManager.user.username;
            gameManager.firstText.text = "First: " + userManager.user.first;
            gameManager.genderText.text = "Gender: " + userManager.user.gender;

            if (!string.IsNullOrEmpty(userManager.user.tribe))
                gameManager.tribeNameText.text = "Tribe: " + userManager.user.tribe;

            gameManager.panelPlayerList.SetActive(Input.GetKey(KeyCode.Tab));

            if (Input.GetKeyDown(KeyCode.P))
                gameManager.profilePanel.SetActive(!gameManager.profilePanel.activeInHierarchy);
            else if (Input.GetKeyDown(KeyCode.Escape))
                gameManager.panelOptions.SetActive(!gameManager.panelOptions.activeInHierarchy);
            else if (Input.GetKeyDown(KeyCode.C))
                gameManager.canvasChat.SetActive(!gameManager.canvasChat.activeInHierarchy);
            else if (Input.GetKeyDown(KeyCode.F))
                gameManager.friendListPanel.SetActive(!gameManager.friendListPanel.activeInHierarchy);

            if (servicesManager.joinedLobby?.Data != null &&
                servicesManager.joinedLobby.Data.ContainsKey("MapIndex") &&
                !string.IsNullOrEmpty(servicesManager.joinedLobby.Data["MapIndex"].Value) &&
                mapIndex.ToString() != servicesManager.joinedLobby.Data["MapIndex"].Value)
                GetLobbyMap();

            spawnObjectTime += Time.deltaTime;
            UpdatePlayerList();
        }
    }

    private void SendMessage()
    {
        string text = gameManager.messageInputField.text;
        if (string.IsNullOrEmpty(text.Trim()) || text.Length >= 25) return;

        if (text[0] == '/')
        {
            if (text.StartsWith("/profile "))
                StartCoroutine(gameManager.CheckPlayerUsernameFromDatabase(text[9..]));
            else if (text.StartsWith("/friend "))
                StartCoroutine(gameManager.AddFriendToDatabase(text[8..]));
        }
        else
        {
            if (gameManager.messageContentObject.transform.childCount >= 25)
                Destroy(gameManager.messageContentObject.transform.GetChild(gameManager.messageContentObject.transform.childCount - 1).GetComponent<NetworkObject>());

            GameObject message = Instantiate(gameManager.messagePrefab, gameManager.messageContentObject);
            message.GetComponent<Message>().SetMessageText(userManager.user.username + ": " + text);
            message.transform.localScale = gameManager.messageContentObject.transform.localScale;
        }

        gameManager.messageInputField.text = string.Empty;
    }

    private void UpdatePlayerList()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (string.IsNullOrEmpty(player.GetComponentInChildren<Canvas>().transform.Find("PlayerName").GetComponent<TMP_Text>().text))
                return;
        }

        if (playerCount == players.Length) return;
        playerCount = players.Length;
        foreach (PlayerNameItem item in playerNameItems) Destroy(item.gameObject);
        playerNameItems.Clear();

        foreach (GameObject player in players)
        {
            PlayerNameItem newItem = Instantiate(gameManager.playerNameItemPrefab, gameManager.playerNameItemContentObject);
            newItem.SetPlayerNickname(player.GetComponentInChildren<Canvas>().transform.Find("PlayerName").GetComponent<TMP_Text>().text);
            playerNameItems.Add(newItem);
        }
    }

    private async void SetLobbyMap()
    {
        try
        {
            mapIndex = Random.Range(0, gameManager.maps.Length);
            mapBackground = Random.Range(0, gameManager.backgrounds.Length);
            mapMountain = Random.Range(mapBackground * 2, mapBackground * 2 + 2);

            servicesManager.hostLobby = await LobbyService.Instance.UpdateLobbyAsync(servicesManager.hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "MapIndex", new DataObject(DataObject.VisibilityOptions.Public, mapIndex.ToString()) },
                    { "BackgroundIndex", new DataObject(DataObject.VisibilityOptions.Public, mapBackground.ToString()) },
                    { "MountainIndex", new DataObject(DataObject.VisibilityOptions.Public, mapMountain.ToString()) }
                }
            });

            servicesManager.joinedLobby = servicesManager.hostLobby;
            GetLobbyMap();
        }
        catch (LobbyServiceException e) { Debug.LogError(e.Message); }
    }

    private void GetLobbyMap()
    {
        mapIndex = int.Parse(servicesManager.joinedLobby.Data["MapIndex"].Value);
        mapBackground = int.Parse(servicesManager.joinedLobby.Data["BackgroundIndex"].Value);
        mapMountain = int.Parse(servicesManager.joinedLobby.Data["MountainIndex"].Value);

        for (int i = 0; i < gameManager.maps.Length; i++)
            gameManager.maps[i].SetActive(i == mapIndex);

        GameObject.FindWithTag("Background").GetComponent<RawImage>().texture = gameManager.backgrounds[mapBackground];
        GameObject.FindWithTag("Mountain").GetComponent<RawImage>().texture = gameManager.mountains[mapMountain];

        if (gameManager.maps[0].activeInHierarchy)
            transform.position = new Vector2(0, -0.9f);
    }
}
