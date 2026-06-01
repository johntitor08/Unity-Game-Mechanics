using UnityEngine;

#if !UNITY_WEBGL

using Firebase.Database;

#else

using WebSocketSharp;
using UnityEngine.Networking;

#endif

using TMPro;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

[RequireComponent(typeof(SpriteRenderer))]
public class MPSVCharacterComponents : NetworkBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private ServicesManager servicesManager;
    private UserManager userManager;
    private MPSVGameManager gameManager;
    private SetFirst setFirst;
    private float horizontal;
    [SerializeField] private float speed;
    [SerializeField] private float jumpingPower;
    private bool canJump;
    private bool isFacingRight = true;
    [SerializeField] private float wallSlidingSpeed;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask fistWallLayer;
    [SerializeField] private LayerMask trampolineLayer;
    [SerializeField] private LayerMask finishLayer;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private LayerMask chestLayer;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Transform objectLaunchPoint;
    private List<string> playerNames = new();
    private Dictionary<string, int> selectedCharacterSkins = new();
    private List<PlayerNameItem> playerNameItems = new();
    private bool isMapCompleted;
    private string winnerPlayerNickname;
    private string messagedPlayerNickname;
    private string messagedPlayerMessageText;
    private GameObject emoji;
    private int mapIndex;
    private int mapBackground;
    private int mapMountain;
    private float elapsedTime;
    private bool isJumped = false;
    private bool isFell = true;
    private float emojiTime = 0;
    private int playerCount = 0;
    private static readonly WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        servicesManager = GameObject.Find("NetworkManager").GetComponent<ServicesManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<MPSVGameManager>();
        setFirst = GameObject.Find("GameManager").GetComponent<SetFirst>();
        gameManager.registrationDateText.text = "Registration Date: " + userManager.user.registrationDate.ToString().Split(' ')[0];
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (Player player in servicesManager.joinedLobby.Players)
        {
            if (!playerNames.Contains(player.Data["PlayerName"].Value))
                playerNames.Add(player.Data["PlayerName"].Value);
        }

        usernameText.text = char.ToUpper(userManager.user.username[0]) + userManager.user.username.Substring(1);

        int playerIndex = 0;

        foreach (GameObject player in players)
        {
            TMP_Text nameText = player.GetComponentInChildren<Canvas>().transform.Find("PlayerName").GetComponent<TMP_Text>();

            if (playerIndex < playerNames.Count)
            {
                string pName = playerNames[playerIndex];
                nameText.text = pName.Length > 0 ? char.ToUpper(pName[0]) + pName[1..] : pName;
            }

            playerIndex++;
        }

#if UNITY_WEBGL

        StartCoroutine(setFirst.GetFirstFromDatabase(userManager.user, userManager.IdToken));

#else

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        StartCoroutine(setFirst.GetFirstFromDatabase(databaseReference, userManager.user));

#endif

        gameManager.sendMessageButton.onClick.AddListener(SendMessage);

        foreach (GameObject player in players)
        {
            if (player != gameObject)
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player.GetComponent<Collider2D>());
        }

        if (gameObject.name.Contains("Slime"))
        {
            ObjectBase objectBase = gameManager.slimesDatabase.GetObject(userManager.user.selectedCharacterSkin);
            Texture2D characterTexture = objectBase.objectTexture;
            Sprite characterSprite = Sprite.Create(characterTexture, new Rect(0, 0, characterTexture.width, characterTexture.height), Vector2.one / 2, 300);
            GetComponent<SpriteRenderer>().sprite = characterSprite;
            SetCharacterSkin();
        }

        if (IsOwner)
        {
            usernameText.color = new Color32(250, 209, 17, 255);
            gameManager.setProfileImageButton.SetActive(true);
            SetDefaultMapPropertiesServerRpc();
            StartCoroutine(gameManager.GetFriendsFromDatabase());

            if (string.IsNullOrEmpty(userManager.user.tribe))
            {
                gameManager.tribeNameText.gameObject.SetActive(false);
                Vector2 genderText = gameManager.genderText.transform.localPosition;
                genderText.y -= 75;
                gameManager.genderText.transform.localPosition = genderText;
            }
        }
        else
        {
            usernameText.color = Color.white;
            gameManager.setProfileImageButton.SetActive(false);
        }
    }

    private void Update()
    {
        if (!gameManager.isOnInputChatField && IsOwner)
        {
            horizontal = Input.GetAxisRaw("Horizontal");

            if (IsGrounded())
            {
                canJump = true;

                if (!isFell && rb.linearVelocity.y == 0 && gameObject.name.Contains("Slime"))
                {
                    StartCoroutine(SlimeFallAnimation());
                    isFell = true;
                }
            }
            else if (IsWalled())
            {
                canJump = (horizontal < 0 && transform.localScale.x > 0) || (horizontal > 0 && transform.localScale.x < 0);
            }
            else if (IsTrampolined())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower * 2);

                if (!isFell && gameObject.name.Contains("Slime"))
                {
                    StartCoroutine(SlimeFallAnimation());
                    isFell = true;
                }

                canJump = true;
            }
            else if (IsFistWalled())
            {
                canJump = false;
            }
            else if (IsWatered())
            {
                canJump = true;
            }
            else if (IsFinished() && !isMapCompleted && elapsedTime > 10)
            {
                if (playerCount > 1)
                {
                    userManager.user.first++;

#if UNITY_WEBGL

                    StartCoroutine(setFirst.SaveFirstToDatabase(userManager.user, userManager.IdToken));

#else

                    StartCoroutine(setFirst.SaveFirstToDatabase(databaseReference, userManager.user));

#endif

                }

                float finishedTime = Mathf.Round(elapsedTime * 100) / 100;
                elapsedTime = 0;
                PlayerCompletedServerRpc(userManager.user.username, finishedTime);
                isMapCompleted = true;
            }

            if (Input.GetButtonDown("Jump"))
            {
                if (canJump)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);

                    if ((!isJumped || IsWatered()) && gameObject.name.Contains("Slime"))
                    {
                        StartCoroutine(SlimeJumpAnimation());
                        isJumped = true;
                    }

                    canJump = false;
                }
                else if (rb.linearVelocity.y > 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                }
            }

            gameManager.usernameText.text = char.ToUpper(userManager.user.username[0]) + userManager.user.username.Substring(1);
            gameManager.firstText.text = "First: " + userManager.user.first;
            gameManager.genderText.text = "Gender: " + userManager.user.gender;

            if (!string.IsNullOrEmpty(userManager.user.tribe))
                gameManager.tribeNameText.text = "Tribe: " + userManager.user.tribe;

            if (Input.GetKey(KeyCode.Tab))
                gameManager.panelPlayerList.SetActive(true);
            else
                gameManager.panelPlayerList.SetActive(false);

            if (Input.GetKeyDown(KeyCode.P))
            {
                gameManager.profilePanel.SetActive(!gameManager.profilePanel.activeInHierarchy);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameManager.panelOptions.SetActive(!gameManager.panelOptions.activeInHierarchy);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                gameManager.canvasChat.SetActive(!gameManager.canvasChat.activeInHierarchy);
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                if (gameManager.maps[0].activeInHierarchy)
                    transform.position = new Vector2(0, -0.9f);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                gameManager.friendListPanel.SetActive(!gameManager.friendListPanel.activeInHierarchy);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1)) DisplayEmojiServerRpc(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) DisplayEmojiServerRpc(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) DisplayEmojiServerRpc(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) DisplayEmojiServerRpc(3);
            else if (Input.GetKeyDown(KeyCode.Alpha5)) DisplayEmojiServerRpc(4);
            else if (Input.GetKeyDown(KeyCode.Alpha6)) DisplayEmojiServerRpc(5);
            else if (Input.GetKeyDown(KeyCode.Alpha7)) DisplayEmojiServerRpc(6);
            else if (Input.GetKeyDown(KeyCode.Alpha8)) DisplayEmojiServerRpc(7);
            else if (Input.GetKeyDown(KeyCode.Alpha9)) DisplayEmojiServerRpc(8);
            else if (Input.GetKeyDown(KeyCode.Alpha0)) DisplayEmojiServerRpc(9);

            if (objectLaunchPoint.childCount > 0 && emojiTime > 3)
                DestroyEmojiServerRpc();

            if (!IsServer &&
                servicesManager.joinedLobby?.Data != null &&
                servicesManager.joinedLobby.Data.ContainsKey("MapIndex") &&
                !string.IsNullOrEmpty(servicesManager.joinedLobby.Data["MapIndex"].Value) &&
                mapIndex.ToString() != servicesManager.joinedLobby.Data["MapIndex"].Value)
            {
                GetLobbyMap();
            }

            WallSlide();
            Flip();

            if (!IsServer)
                UpdatePlayerList();

            elapsedTime += Time.deltaTime;
            emojiTime += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded() => Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    private bool IsWalled() => Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    private bool IsTrampolined() => Physics2D.OverlapCircle(groundCheck.position, 0.2f, trampolineLayer);
    private bool IsFistWalled() => Physics2D.OverlapCircle(wallCheck.position, 0.2f, fistWallLayer);
    private bool IsFinished() => Physics2D.OverlapCircle(groundCheck.position, 0.2f, finishLayer);
    private bool IsWatered() => Physics2D.OverlapCircle(groundCheck.position, 0.2f, waterLayer);

    private void SetCharacterSkin()
    {
        if (!selectedCharacterSkins.ContainsKey(userManager.user.userID))
            StartCoroutine(GetSelectedCharacterSkinFromDatabase(userManager.user.userID));
    }

    private void WallSlide()
    {
        if (IsWalled() && horizontal != 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0 || !isFacingRight && horizontal > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
            Vector3 usernameTextLocalScale = usernameText.transform.localScale;
            usernameTextLocalScale.x *= -1;
            usernameText.transform.localScale = usernameTextLocalScale;
        }
    }

    private IEnumerator SlimeJumpAnimation()
    {
        if (!selectedCharacterSkins.ContainsKey(userManager.user.userID))
            yield break;

        ObjectAnimationBase slimeJumpAnimation = gameManager.slimesJumpDatabase.GetObject(selectedCharacterSkins[userManager.user.userID]);

        for (int i = 0; i < slimeJumpAnimation.objectAnimationTextures.Length; i++)
        {
            Texture2D characterTexture = slimeJumpAnimation.objectAnimationTextures[i];
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(characterTexture, new Rect(0, 0, characterTexture.width, characterTexture.height), Vector2.one / 2, 300);
            yield return _waitForSeconds0_1;
        }

        isFell = false;
    }

    private IEnumerator SlimeFallAnimation()
    {
        if (!selectedCharacterSkins.ContainsKey(userManager.user.userID))
            yield break;

        ObjectAnimationBase slimeFallAnimation = gameManager.slimesFallDatabase.GetObject(selectedCharacterSkins[userManager.user.userID]);

        for (int i = 0; i < slimeFallAnimation.objectAnimationTextures.Length; i++)
        {
            Texture2D characterTexture = slimeFallAnimation.objectAnimationTextures[i];
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(characterTexture, new Rect(0, 0, characterTexture.width, characterTexture.height), Vector2.one / 2, 300);
            yield return _waitForSeconds0_1;
        }

        isJumped = false;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DisplayEmojiServerRpc(int emojiId) => DisplayEmojiClientRpc(emojiId);

    [ClientRpc]
    private void DisplayEmojiClientRpc(int emojiId)
    {
        for (int i = 0; i < objectLaunchPoint.childCount; i++)
            Destroy(objectLaunchPoint.GetChild(i).gameObject);

        emoji = Instantiate(gameManager.emojis[emojiId], objectLaunchPoint);
        emoji.transform.SetParent(objectLaunchPoint.transform, true);
        emojiTime = 0;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DestroyEmojiServerRpc() => DestroyEmojiClientRpc();

    [ClientRpc]
    private void DestroyEmojiClientRpc()
    {
        for (int i = 0; i < objectLaunchPoint.childCount; i++)
            Destroy(objectLaunchPoint.GetChild(i).gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsOwner)
        {
            if (collision.gameObject.name == "Spawner" || collision.gameObject.CompareTag("Barrier"))
            {
                if (gameManager.maps[0].activeInHierarchy)
                    transform.position = new Vector2(0, -0.9f);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsOwner)
        {
            if (collision.gameObject.name == "CornerPoints" && rb.linearVelocity.y < 0)
                rb.AddForce(rb.linearVelocity);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetDefaultMapPropertiesServerRpc() => SetDefaultMapPropertiesClientRpc();

    [ClientRpc]
    private void SetDefaultMapPropertiesClientRpc()
    {
        if (IsServer && (IsOwner || isMapCompleted))
            SetLobbyMap();
        else
            GetLobbyMap();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayerCompletedServerRpc(string username, float finishedTime) => PlayerCompletedClientRpc(username, finishedTime);

    [ClientRpc]
    private void PlayerCompletedClientRpc(string username, float finishedTime)
    {
        winnerPlayerNickname = username;
        isMapCompleted = true;
        StartCoroutine(PlayerCompleted(finishedTime));
    }

    private IEnumerator PlayerCompleted(float finishedTime)
    {
        gameManager.panelPlayerCompleted.SetActive(true);
        gameManager.playerCompletedText.text = winnerPlayerNickname + " completed the map in " + finishedTime + " seconds.";
        yield return new WaitForSeconds(2);
        gameManager.panelPlayerCompleted.SetActive(false);

        if (IsServer)
            SetDefaultMapPropertiesServerRpc();
    }

    private void SendMessage()
    {
        SendMessageServerRpc(userManager.user.username, gameManager.messageInputField.text);

        if (!IsServer)
        {
            messagedPlayerNickname = userManager.user.username;
            messagedPlayerMessageText = gameManager.messageInputField.text;
            DisplayMessage(messagedPlayerNickname + ": " + messagedPlayerMessageText);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SendMessageServerRpc(string username, string message) => SendMessageClientRpc(username, message);

    [ClientRpc]
    private void SendMessageClientRpc(string username, string message)
    {
        if (IsServer)
        {
            messagedPlayerNickname = username;
            messagedPlayerMessageText = message;
        }
        else
        {
            messagedPlayerNickname = userManager.user.username;
            messagedPlayerMessageText = gameManager.messageInputField.text;
        }

        DisplayMessage(messagedPlayerNickname + ": " + messagedPlayerMessageText);
    }

    private void DisplayMessage(string messageText)
    {
        if (string.IsNullOrEmpty(messagedPlayerMessageText.Trim()) || messagedPlayerMessageText.Length >= 25)
            return;

        if (messagedPlayerMessageText[0] == '/')
        {
            if (messagedPlayerMessageText.StartsWith("/profile "))
                StartCoroutine(gameManager.CheckPlayerUsernameFromDatabase(messagedPlayerMessageText[9..]));
            else if (messagedPlayerMessageText.StartsWith("/friend "))
                StartCoroutine(gameManager.AddFriendToDatabase(messagedPlayerMessageText[8..]));
        }
        else
        {
            if (gameManager.messageContentObject.transform.childCount >= 25)
                Destroy(gameManager.messageContentObject.transform.GetChild(gameManager.messageContentObject.transform.childCount - 1).gameObject);

            GameObject message = Instantiate(gameManager.messagePrefab, gameManager.messageContentObject);
            message.GetComponent<Message>().SetMessageText(messageText);
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

        foreach (PlayerNameItem playerNameItem in playerNameItems)
            Destroy(playerNameItem.gameObject);

        playerNameItems.Clear();

        foreach (GameObject player in players)
        {
            PlayerNameItem newPlayerTextItem = Instantiate(gameManager.playerNameItemPrefab, gameManager.playerNameItemContentObject);
            newPlayerTextItem.SetPlayerNickname(player.GetComponentInChildren<Canvas>().transform.Find("PlayerName").GetComponent<TMP_Text>().text);
            playerNameItems.Add(newPlayerTextItem);
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
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void GetLobbyMap()
    {
        if (servicesManager.joinedLobby?.Data == null ||
            !servicesManager.joinedLobby.Data.ContainsKey("MapIndex") ||
            string.IsNullOrEmpty(servicesManager.joinedLobby.Data["MapIndex"].Value))
            return;

        mapIndex = int.Parse(servicesManager.joinedLobby.Data["MapIndex"].Value);
        mapBackground = int.Parse(servicesManager.joinedLobby.Data["BackgroundIndex"].Value);
        mapMountain = int.Parse(servicesManager.joinedLobby.Data["MountainIndex"].Value);

        for (int i = 0; i < gameManager.maps.Length; i++)
            gameManager.maps[i].SetActive(i == mapIndex);

        GameObject.FindWithTag("Background").GetComponent<RawImage>().texture = gameManager.backgrounds[mapBackground];
        GameObject.FindWithTag("Mountain").GetComponent<RawImage>().texture = gameManager.mountains[mapMountain];
        isMapCompleted = false;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (gameManager.maps[0].activeInHierarchy)
                player.transform.position = new Vector2(0, -0.9f);
        }
    }

#if !UNITY_WEBGL

    private IEnumerator GetSelectedCharacterSkinFromDatabase(string userID)
    {
        var task = databaseReference.Child("Users").Child(userID).Child("selectedCharacterSkin").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            DataSnapshot snapshot = task.Result;

            if (snapshot != null && snapshot.Exists)
            {
                int selectedCharacterSkin = int.Parse(snapshot.Value.ToString());
                selectedCharacterSkins[userID] = selectedCharacterSkin;
                ObjectBase objectBase = gameManager.slimesDatabase.GetObject(selectedCharacterSkin);
                Texture2D characterTexture = objectBase.objectTexture;
                Sprite characterSprite = Sprite.Create(characterTexture, new Rect(0, 0, characterTexture.width, characterTexture.height), Vector2.one / 2, 300);
                GetComponent<SpriteRenderer>().sprite = characterSprite;
            }
        }
        else
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to get selected character skin: " + inner.Message);
        }
    }

#else

    private IEnumerator GetSelectedCharacterSkinFromDatabase(string userID)
    {
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + userID + "/selectedCharacterSkin.json?auth=" + userManager.IdToken;

        using (UnityWebRequest getRequest = UnityWebRequest.Get(url))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                string responseText = getRequest.downloadHandler.text;

                if (responseText != "null")
                {
                    int selectedCharacterSkin = int.Parse(responseText);
                    selectedCharacterSkins[userID] = selectedCharacterSkin;
                    ObjectBase objectBase = gameManager.slimesDatabase.GetObject(selectedCharacterSkin);
                    Texture2D characterTexture = objectBase.objectTexture;
                    Sprite characterSprite = Sprite.Create(characterTexture, new Rect(0, 0, characterTexture.width, characterTexture.height), Vector2.one / 2, 300);
                    GetComponent<SpriteRenderer>().sprite = characterSprite;
                }
            }
            else
            {
                Debug.LogError("Failed to get selected character skin: " + getRequest.error);
            }
        }
    }

#endif

}
