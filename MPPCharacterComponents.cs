using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine.Tilemaps;

#if !UNITY_WEBGL

using Firebase.Database;
using WebSocketSharp;

[RequireComponent(typeof(LineRenderer))]

#else

using UnityEngine.Networking;

#endif

#if UNITY_ANDROID

using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

#endif

public class MPPCharacterComponents : NetworkBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private static readonly WaitForSeconds _waitForSeconds0_1 = new(0.1f);
    private ServicesManager servicesManager;
    private UserManager userManager;
    private MPPGameManager gameManager;
    private SetFirst setFirst;
    private InventorySystem inventorySystem;
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
    private readonly List<string> playerNames = new();
    private readonly Dictionary<string, int> selectedCharacterSkins = new();
    private readonly List<PlayerNameItem> playerNameItems = new();
    private bool isMapCompleted;
    private string winnerPlayerNickname;
    private string messagedPlayerNickname;
    private string messagedPlayerMessageText;
    private LineRenderer lineRenderer;
    private GameObject emoji;
    private int mapIndex;
    private int mapBackground;
    private int mapMountain;
    private float elapsedTime;
    private readonly float launchSpeed = 10;
    private readonly float arrowWidth = 0.1f;
    private readonly float arrowMaxLength = 3;
    private bool isJumped = false;
    private bool isFell = true;
    private float emojiTime = 0;
    private int playerCount = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        servicesManager = GameObject.Find("NetworkManager").GetComponent<ServicesManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<MPPGameManager>();
        setFirst = GameObject.Find("GameManager").GetComponent<SetFirst>();
        inventorySystem = GameObject.Find("GameManager").GetComponent<InventorySystem>();
        gameManager.registrationDateText.text = "Registration Date: " + userManager.user.registrationDate.ToString().Split(' ')[0];
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = arrowWidth;
        lineRenderer.endWidth = arrowWidth;

        foreach (Player player in servicesManager.joinedLobby.Players)
        {
            if (!playerNames.Contains(player.Data["PlayerName"].Value))
            {
                playerNames.Add(player.Data["PlayerName"].Value);
            }
        }

        usernameText.text = char.ToUpper(userManager.user.username[0]) + userManager.user.username.Substring(1);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int playerIndex = 0;

        foreach (GameObject player in players)
        {
            TMP_Text nameText = player.GetComponentInChildren<Canvas>().transform.Find("PlayerName").GetComponent<TMP_Text>();

            if (playerIndex < playerNames.Count)
            {
                nameText.text = playerNames[playerIndex];
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

#if UNITY_ANDROID

        gameManager.canvasControl.SetActive(true);

        Entry onPointerDownLeft = new()
        {
            eventID = EventTriggerType.PointerDown
        };

        Entry onPointerDownRight = new()
        {
            eventID = EventTriggerType.PointerDown
        };

        Entry onPointerUp = new()
        {
            eventID = EventTriggerType.PointerUp
        };

        onPointerDownLeft.callback.AddListener((data) => { MoveLeft(); });
        onPointerDownRight.callback.AddListener((data) => { MoveRight(); });
        onPointerUp.callback.AddListener((data) => { StopMoving(); });
        gameManager.moveLeftButton.triggers.Add(onPointerDownLeft);
        gameManager.moveLeftButton.triggers.Add(onPointerUp);
        gameManager.moveRightButton.triggers.Add(onPointerDownRight);
        gameManager.moveRightButton.triggers.Add(onPointerUp);
        gameManager.moveUpButton.onClick.AddListener(MoveUp);

#endif

        foreach (GameObject player in players)
        {
            if (player != gameObject)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player.GetComponent<Collider2D>());
            }
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
#if !UNITY_ANDROID

            horizontal = Input.GetAxisRaw("Horizontal");

#endif

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
                if ((horizontal < 0 && transform.localScale.x > 0) || (horizontal > 0 && transform.localScale.x < 0))
                {
                    canJump = true;
                }
                else
                {
                    canJump = false;
                }
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
            else if (IsFinished() && !isMapCompleted && elapsedTime > 10 && !gameManager.panelPlayerCompleted.activeInHierarchy)
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

            if (IsChested())
            {
                if (userManager.user.coin < 12 && gameManager.maps[2].activeInHierarchy)
                {
                    return;
                }

                gameManager.chestInventoryPanel.SetActive(true);
                Collider2D chest = Physics2D.OverlapCircle(wallCheck.position, 0.1f, chestLayer);
                chest.GetComponent<Animator>().SetBool("isOpened", true);
            }
            else
            {
                gameManager.chestInventoryPanel.SetActive(false);
                GameObject[] chests = GameObject.FindGameObjectsWithTag("Chest");

                foreach (GameObject chest in chests)
                {
                    if (Vector2.Distance(transform.position, chest.transform.position) > 0.1f)
                    {
                        chest.GetComponent<Animator>().SetBool("isOpened", false);
                    }
                }
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
            gameManager.firstText.text = "First: " + userManager.user.first.ToString();
            gameManager.coinText.text = userManager.user.coin.ToString();
            gameManager.genderText.text = "Gender: " + userManager.user.gender.ToString();

            if (!string.IsNullOrEmpty(userManager.user.tribe))
            {
                gameManager.tribeNameText.text = "Tribe: " + userManager.user.tribe.ToString();
            }

            if (Input.GetKey(KeyCode.Tab))
            {
                gameManager.panelPlayerList.SetActive(true);
            }
            else
            {
                gameManager.panelPlayerList.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (gameManager.profilePanel.activeInHierarchy)
                    gameManager.profilePanel.SetActive(false);
                else
                    gameManager.profilePanel.SetActive(true);
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                if (gameManager.inventoryPanel.activeInHierarchy)
                    gameManager.inventoryPanel.SetActive(false);
                else
                    gameManager.inventoryPanel.SetActive(true);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                if (gameManager.shopPanel.activeInHierarchy)
                    gameManager.shopPanel.SetActive(false);
                else
                    gameManager.shopPanel.SetActive(true);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (gameManager.panelOptions.activeInHierarchy)
                    gameManager.panelOptions.SetActive(false);
                else
                    gameManager.panelOptions.SetActive(true);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                if (gameManager.canvasChat.activeInHierarchy)
                    gameManager.canvasChat.SetActive(false);
                else
                    gameManager.canvasChat.SetActive(true);
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                if (gameManager.maps[0].activeInHierarchy)
                    transform.position = new Vector2(-7, -3.5f);
                else if (gameManager.maps[1].activeInHierarchy)
                    transform.position = new Vector2(-8.5f, 72);
                else if (gameManager.maps[2].activeInHierarchy)
                    transform.position = new Vector2(-5, -1.4f);
                else if (gameManager.maps[3].activeInHierarchy)
                    transform.position = new Vector2(0, 0);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                if (gameManager.friendListPanel.activeInHierarchy)
                    gameManager.friendListPanel.SetActive(false);
                else
                    gameManager.friendListPanel.SetActive(true);
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
            {
                DestroyEmojiServerRpc();
            }

            if (gameManager.nameOfInventoryItemToUse == "Nuke" || gameManager.nameOfInventoryItemToUse == "Rocket")
            {
                lineRenderer.enabled = true;
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                DrawArrow(objectLaunchPoint.position, mousePosition);

                if (Input.GetMouseButtonDown(0))
                {
                    if (gameManager.nameOfInventoryItemToUse == "Rocket")
                        LaunchRocket(mousePosition);
                    else if (gameManager.nameOfInventoryItemToUse == "Nuke")
                        LaunchNuke(mousePosition);

                    gameManager.nameOfInventoryItemToUse = null;
                }
            }
            else
            {
                lineRenderer.enabled = false;
            }

            if (servicesManager.joinedLobby?.Data != null &&
                servicesManager.joinedLobby.Data.ContainsKey("MapIndex") &&
                !string.IsNullOrEmpty(servicesManager.joinedLobby.Data["MapIndex"].Value) &&
                mapIndex.ToString() != servicesManager.joinedLobby.Data["MapIndex"].Value)
            {
                GetLobbyMap();
            }

            WallSlide();
            Flip();
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
    private bool IsFinished() => Physics2D.OverlapCircle(groundCheck.position, 0.1f, finishLayer);
    private bool IsWatered() => Physics2D.OverlapCircle(groundCheck.position, 0.1f, waterLayer);
    private bool IsChested() => Physics2D.OverlapCircle(wallCheck.position, 0.1f, chestLayer);

    private void SetCharacterSkin()
    {
        if (!selectedCharacterSkins.ContainsKey(userManager.user.userID))
        {
            StartCoroutine(GetSelectedCharacterSkinFromDatabase(userManager.user.userID));
        }
    }

    private void WallSlide()
    {
        if (IsWalled() && horizontal != 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
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
        if (objectLaunchPoint.childCount > 0)
        {
            for (int i = 0; i < objectLaunchPoint.childCount; i++)
                Destroy(objectLaunchPoint.GetChild(i).gameObject);
        }

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
            if (collision.gameObject.name == "Spawner")
            {
                if (gameManager.maps[0].activeInHierarchy) transform.position = new Vector2(-7, -3.5f);
                else if (gameManager.maps[1].activeInHierarchy) transform.position = new Vector2(-8.5f, 72);
                else if (gameManager.maps[2].activeInHierarchy) transform.position = new Vector2(-5, -1.4f);
                else if (gameManager.maps[3].activeInHierarchy) transform.position = new Vector2(0, 0);
            }

            if (collision.gameObject.CompareTag("Barrier"))
            {
                if (gameManager.maps[1].activeInHierarchy) transform.position = new Vector2(-8.5f, 72);
                if (gameManager.maps[2].activeInHierarchy) transform.position = new Vector2(-5, -1.4f);
                if (gameManager.maps[3].activeInHierarchy) transform.position = new Vector2(0, 0);
            }

            if (collision.gameObject.CompareTag("Coin"))
            {
                userManager.user.coin++;
                Destroy(collision.gameObject);
            }

            if (collision.gameObject.CompareTag("Item"))
            {
                Texture2D itemImage = collision.gameObject.GetComponent<SpriteRenderer>().sprite.texture;
                string itemName = collision.gameObject.name;
                inventorySystem.AddItem(itemImage, itemName, 1);
                Destroy(collision.gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsOwner)
        {
            if (collision.gameObject.CompareTag("Barrier"))
            {
                if (gameManager.maps[1].activeInHierarchy) transform.position = new Vector2(-8.5f, 72);
                if (gameManager.maps[2].activeInHierarchy) transform.position = new Vector2(-5, -1.4f);
                if (gameManager.maps[3].activeInHierarchy) transform.position = new Vector2(0, 0);
            }

            if (collision.gameObject.name == "CornerPoints" && rb.linearVelocity.y < 0)
            {
                rb.AddForce(rb.linearVelocity);
            }
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
            SendMessageClientRpc(messagedPlayerNickname + ": " + messagedPlayerMessageText);

            if (IsOwner)
                gameManager.messageInputField.text = string.Empty;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SendMessageServerRpc(string username, string message)
    {
        if (IsOwner)
        {
            messagedPlayerNickname = username;
            messagedPlayerMessageText = message;
            SendMessageClientRpc(messagedPlayerNickname + ": " + messagedPlayerMessageText);
            gameManager.messageInputField.text = string.Empty;
        }
    }

    [ClientRpc]
    private void SendMessageClientRpc(string messageText)
    {
        if (!string.IsNullOrEmpty(messagedPlayerMessageText.Trim()) && messageText.Length <= 120)
        {
            if (messagedPlayerMessageText[0] == '/')
            {
                if (messagedPlayerMessageText.StartsWith("/profile "))
                {
                    string playerNickname = messagedPlayerMessageText[9..];
                    StartCoroutine(gameManager.CheckPlayerUsernameFromDatabase(playerNickname));
                }
                else if (messagedPlayerMessageText.StartsWith("/friend "))
                {
                    string playerNickname = messagedPlayerMessageText[8..];
                    StartCoroutine(gameManager.AddFriendToDatabase(playerNickname));
                }
            }
            else
            {
                if (gameManager.messageContentObject.transform.childCount >= 25)
                    Destroy(gameManager.messageContentObject.transform.GetChild(gameManager.messageContentObject.transform.childCount - 1).gameObject);

                GameObject message = Instantiate(gameManager.messagePrefab, gameManager.messageContentObject);
                message.GetComponent<Message>().SetMessageText(messageText);
                message.transform.localScale = gameManager.messageContentObject.transform.localScale;

                for (int i = 1; i < 8; i++)
                {
                    if (messageText.Length > i * 15)
                        message.GetComponent<RectTransform>().sizeDelta = new Vector2(message.GetComponent<RectTransform>().sizeDelta.x, message.GetComponent<RectTransform>().sizeDelta.y + 25);
                }
            }

            if (playerCount == 1)
                gameManager.messageInputField.text = string.Empty;
        }
    }

    private void UpdatePlayerList()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (string.IsNullOrEmpty(player.GetComponentInChildren<Canvas>().transform.Find("PlayerName").GetComponent<TMP_Text>().text))
                return;
        }

        if (playerCount == players.Length)
            return;

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
        mapIndex = int.Parse(servicesManager.joinedLobby.Data["MapIndex"].Value);
        mapBackground = int.Parse(servicesManager.joinedLobby.Data["BackgroundIndex"].Value);
        mapMountain = int.Parse(servicesManager.joinedLobby.Data["MountainIndex"].Value);

        for (int i = 0; i < gameManager.maps.Length; i++)
            gameManager.maps[i].SetActive(i == mapIndex);

        GameObject.FindWithTag("Background").GetComponent<RawImage>().texture = gameManager.backgrounds[mapBackground];
        GameObject.FindWithTag("Mountain").GetComponent<RawImage>().texture = gameManager.mountains[mapMountain];

        for (int i = 0; i < gameManager.chestInventoryItemContentObject.childCount; i++)
            Destroy(gameManager.chestInventoryItemContentObject.GetChild(i).gameObject);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (gameManager.maps[0].activeInHierarchy) player.transform.position = new Vector2(-7, -3.5f);
            else if (gameManager.maps[1].activeInHierarchy) player.transform.position = new Vector2(-8.5f, 72);
            else if (gameManager.maps[2].activeInHierarchy) player.transform.position = new Vector2(-5, -1.4f);
            else if (gameManager.maps[3].activeInHierarchy) player.transform.position = new Vector2(0, 0);
        }

        if (gameManager.maps[2].activeInHierarchy)
        {
            ChestInventoryItem newChestInventoryItem = Instantiate(gameManager.chestInventoryItemPrefab, gameManager.chestInventoryItemContentObject);
            newChestInventoryItem.SetItem(gameManager.rocketTexture, "Rocket", 1);
        }
        else if (gameManager.maps[3].activeInHierarchy)
        {
            ChestInventoryItem newChestInventoryItem = Instantiate(gameManager.chestInventoryItemPrefab, gameManager.chestInventoryItemContentObject);
            newChestInventoryItem.SetItem(gameManager.nukeTexture, "Nuke", 1);
        }
    }

    private void LaunchRocket(Vector3 mousePosition)
    {
        Vector3 direction = mousePosition - objectLaunchPoint.position;
        float launchAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, launchAngle);
        GameObject newRocket = Instantiate(gameManager.rocketPrefab, objectLaunchPoint.position, rotation);
        Rigidbody2D rb = newRocket.GetComponent<Rigidbody2D>();
        float launchX = launchSpeed * Mathf.Cos(launchAngle * Mathf.Deg2Rad);
        rb.linearVelocity = new Vector2(launchX, launchSpeed);
    }

    private void LaunchNuke(Vector3 mousePosition)
    {
        Vector3 direction = mousePosition - objectLaunchPoint.position;
        float launchAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, launchAngle);
        GameObject newNuke = Instantiate(gameManager.nukePrefab, objectLaunchPoint.position, rotation);
        Rigidbody2D rb = newNuke.GetComponent<Rigidbody2D>();
        float launchX = launchSpeed * Mathf.Cos(launchAngle * Mathf.Deg2Rad);
        rb.linearVelocity = new Vector2(launchX, launchSpeed);
    }

    private void DrawArrow(Vector3 startPoint, Vector3 endPoint)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
        float lineLength = Vector3.Distance(startPoint, endPoint);

        if (lineLength > arrowMaxLength)
        {
            float ratio = arrowMaxLength / lineLength;
            Vector3 newPosition = Vector3.Lerp(startPoint, endPoint, ratio);
            lineRenderer.SetPosition(1, newPosition);
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

#if UNITY_ANDROID

    private void MoveLeft() { horizontal = -1; }
    private void MoveRight() { horizontal = 1; }
    private void StopMoving() { horizontal = 0; }

    private void MoveUp()
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

        if (rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
    }

#endif

}
