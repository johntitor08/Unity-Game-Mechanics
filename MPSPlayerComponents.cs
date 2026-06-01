using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MPSPlayerComponents : NetworkBehaviour
{
    private UserManager userManager;
    private ServicesManager servicesManager;
    [SerializeField] private MPSShootingSystem shootingSystem;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer spriteRendererIcon;
    private MPSGameManager gameManager;
    [SerializeField] private TMP_Text usernameText;
    private readonly Dictionary<string, string> playerNames = new();
    private readonly List<PlayerNameItem> playerNameItems = new();
    public int maxHealth = 30;
    private int currentHealth;
    private float timeInvisibility = 0;
    private float elapsedTime;
    private readonly float transitionTime = 1;
    private float timeBulletFrequency = 0;
    private bool invisibleState = false;
    private bool isBulletFrequent = false;
    private float timeBiggerBullet = 0;
    private int playerCount = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        servicesManager = GameObject.Find("NetworkManager").GetComponent<ServicesManager>();
        spriteRendererIcon = transform.Find("MinimapIcon").GetComponent<SpriteRenderer>();
        gameManager = GameObject.Find("GameManager").GetComponent<MPSGameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        elapsedTime = transitionTime * 2;

        foreach (Player p in servicesManager.joinedLobby.Players)
        {
            playerNames[p.Id] = p.Data["PlayerName"].Value;
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObj in players)
        {
            if (!playerObj.TryGetComponent<NetworkObject>(out var netObj)) continue;
            ulong clientId = netObj.OwnerClientId;
            int lobbyIndex = (int)clientId;

            if (lobbyIndex < servicesManager.joinedLobby.Players.Count)
            {
                string lobbyPlayerId = servicesManager.joinedLobby.Players[lobbyIndex].Id;

                if (playerNames.TryGetValue(lobbyPlayerId, out string playerName))
                {
                    playerObj.GetComponentInChildren<Canvas>().transform.Find("PlayerName").GetComponent<TMP_Text>().text = playerName;
                }
            }
        }

        if (IsOwner)
        {
            usernameText.color = new Color32(255, 221, 0, 255);
            transform.position = new Vector3(Random.Range(0, 100), Random.Range(-25, 50));
        }
        else
        {
            usernameText.color = Color.white;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (timeInvisibility > 0 && !invisibleState)
            {
                invisibleState = true;
                BeInvisibleServerRpc();
            }
            else if (timeInvisibility <= 0 && invisibleState)
            {
                invisibleState = false;
                BeVisibleServerRpc();
            }

            if (timeBulletFrequency > 0)
            {
                shootingSystem.fireFrequency = 0.2f;
                isBulletFrequent = true;

            }
            else
            {
                shootingSystem.fireFrequency = 0.5f;
                isBulletFrequent = false;

            }

            if (timeBiggerBullet > 0)
            {
                shootingSystem.isBiggerBullet = true;

            }
            else
            {
                shootingSystem.isBiggerBullet = false;

            }

            if (Input.GetKey(KeyCode.Tab))
            {
                gameManager.panelPlayerList.SetActive(true);

            }
            else
            {
                gameManager.panelPlayerList.SetActive(false);

            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (gameManager.panelOptions.activeInHierarchy)
                {
                    gameManager.panelOptions.SetActive(false);

                }
                else
                {
                    gameManager.panelOptions.SetActive(true);

                }

            }

            usernameText.text = userManager.user.username;
            UpdatePlayerList();
            timeInvisibility -= Time.deltaTime;
            timeBulletFrequency -= Time.deltaTime;
            timeBiggerBullet -= Time.deltaTime;

        }

    }

    [Rpc(SendTo.Server)]
    public void TakeDamageServerRpc(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            NetworkObject.Despawn();
        }

        UpdateHealthClientRpc(currentHealth);
    }

    [ClientRpc]
    private void UpdateHealthClientRpc(int health)
    {
        currentHealth = health;
    }

    [ClientRpc]
    private void TakeDamageClientRpc(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            if (IsOwner)
            {
                gameManager.panelPlayerDied.SetActive(true);

            }

            if (IsServer)
            {
                Destroy(gameObject);

            }

        }

        spriteRenderer.color -= new Color32(0, 5, 5, 0);

    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void BeInvisibleServerRpc()
    {
        BeInvisibleClientRpc();

    }

    [ClientRpc]
    private void BeInvisibleClientRpc()
    {
        if (elapsedTime <= transitionTime)
        {
            elapsedTime += Time.deltaTime;

        }

        float time = elapsedTime / transitionTime;
        float alpha = Mathf.Lerp(1, 0, time);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        spriteRendererIcon.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void BeVisibleServerRpc()
    {
        BeVisibleClientRpc();

    }

    [ClientRpc]
    private void BeVisibleClientRpc()
    {
        if (elapsedTime - transitionTime <= transitionTime)
        {
            elapsedTime += Time.deltaTime;

        }

        float time = (elapsedTime - transitionTime) / transitionTime;
        float alpha = Mathf.Lerp(0, 1, time);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        spriteRendererIcon.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsOwner)
        {
            if (collision.gameObject.CompareTag("Invisibility"))
            {
                timeInvisibility = 10;

                if (invisibleState)
                {
                    timeInvisibility += 10;

                }
                else
                {
                    elapsedTime = 0;

                }

                Destroy(collision.gameObject);

            }

            if (collision.gameObject.CompareTag("BulletFrequency"))
            {
                timeBulletFrequency = 10;

                if (isBulletFrequent)
                {
                    timeBulletFrequency += 10;

                }

                Destroy(collision.gameObject);

            }

            if (collision.gameObject.CompareTag("Health"))
            {
                currentHealth += 1;
                spriteRenderer.color += new Color32(0, 5, 5, 0);
                spriteRendererIcon.color += new Color32(0, 5, 5, 0);
                Destroy(collision.gameObject);

            }

            if (collision.gameObject.CompareTag("BiggerBullet"))
            {
                timeBiggerBullet = 10;

                if (shootingSystem.isBiggerBullet)
                {
                    timeBiggerBullet += 10;

                }

                Destroy(collision.gameObject);

            }

        }

    }

    private void UpdatePlayerList()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (string.IsNullOrEmpty(player.GetComponentInChildren<Canvas>().transform.Find("PlayerName").GetComponent<TMP_Text>().text))
            {
                return;

            }

        }

        if (playerCount == players.Length)
        {
            return;

        }

        playerCount = players.Length;

        foreach (PlayerNameItem playerNameItem in playerNameItems)
        {
            Destroy(playerNameItem.gameObject);

        }

        playerNameItems.Clear();

        foreach (GameObject player in players)
        {
            PlayerNameItem newPlayerTextItem = Instantiate(gameManager.playerNameItemPrefab, gameManager.playerNameItemContentObject);
            newPlayerTextItem.SetPlayerNickname(player.GetComponentInChildren<Canvas>().transform.Find("PlayerName").GetComponent<TMP_Text>().text);
            playerNameItems.Add(newPlayerTextItem);

        }

    }

}