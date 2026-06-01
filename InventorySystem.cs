#if !UNITY_WEBGL

using Firebase.Database;

#else

using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private UserManager userManager;
    private readonly int inventorySize = 20;
    [SerializeField] private InventoryItem inventoryItemPrefab;
    [SerializeField] private Transform contentObject;
    [SerializeField] private ObjectDatabase objectDatabase;

    private IEnumerator Start()
    {
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();

        yield return new WaitUntil(() => userManager != null && userManager.IsAuthenticated);

#if !UNITY_WEBGL

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

#endif

        StartCoroutine(GetInventoryItemsFromDatabase());
    }

    public void AddItem(Texture2D itemImage, string itemName, int itemQuantity)
    {
        if (userManager.user.inventory.Count < inventorySize)
        {
            int itemIndex = FindItemIndexByName(itemName);

            if (itemIndex != -1)
            {
                int quantity = int.Parse(userManager.user.inventory[itemIndex].itemQuantity.text);
                quantity++;
                userManager.user.inventory[itemIndex].itemQuantity.text = quantity.ToString();
            }
            else
            {
                InventoryItem newItem = Instantiate(inventoryItemPrefab, contentObject);
                newItem.SetItem(itemImage, itemName, itemQuantity);
                userManager.user.inventory.Add(newItem);
                int index = Array.IndexOf(userManager.user.inventory.ToArray(), newItem);
                float xOffset = 75 + (index % 4) * 150;
                float yOffset = -75 - (index / 4) * 150;
                newItem.transform.localPosition = new Vector2(xOffset, yOffset);
            }

            StartCoroutine(SaveInventoryItemsToDatabase());
        }
    }

    public void RemoveItem(InventoryItem item)
    {
        if (userManager.user.inventory.Contains(item))
        {
            if (int.Parse(item.itemQuantity.text) > 1)
            {
                int quantity = int.Parse(item.itemQuantity.text);
                quantity--;
                item.itemQuantity.text = quantity.ToString();
            }
            else
            {
                userManager.user.inventory.Remove(item);
                Destroy(item.gameObject);
                SortInventoryItems();
            }

            StartCoroutine(SaveInventoryItemsToDatabase());
        }
    }

    private int FindItemIndexByName(string itemName)
    {
        for (int i = 0; i < userManager.user.inventory.Count; i++)
        {
            if (userManager.user.inventory[i].itemName.text == itemName)
                return i;
        }

        return -1;
    }

    private void SortInventoryItems()
    {
        foreach (InventoryItem item in userManager.user.inventory)
        {
            int index = Array.IndexOf(userManager.user.inventory.ToArray(), item);
            float xOffset = 75 + (index % 4) * 150;
            float yOffset = -75 - (index / 4) * 150;
            item.transform.localPosition = new Vector2(xOffset, yOffset);
        }
    }

#if !UNITY_WEBGL

    private IEnumerator GetInventoryItemsFromDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("inventory").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.Exception != null) yield break;
        DataSnapshot snapshot = task.Result;
        if (!snapshot.Exists) yield break;
        List<InventoryItem> inventoryItems = new();

        foreach (var itemSnapshot in snapshot.Children)
        {
            string itemName = itemSnapshot.Key;
            int itemQuantity = int.Parse(itemSnapshot.Value.ToString());
            ObjectBase objectBase = objectDatabase.GetObjectByName(itemName);
            InventoryItem newItem = Instantiate(inventoryItemPrefab, contentObject);
            newItem.SetItem(objectBase.objectTexture, itemName, itemQuantity);
            inventoryItems.Add(newItem);
        }

        userManager.user.inventory = inventoryItems;
        userManager.user.inventory.Capacity = inventorySize;
        SortInventoryItems();
    }

    private IEnumerator SaveInventoryItemsToDatabase()
    {
        var inventoryRef = databaseReference.Child("Users").Child(userManager.user.userID).Child("inventory");

        if (userManager.user.inventory.Count == 0)
        {
            var removeTask = inventoryRef.RemoveValueAsync();
            yield return new WaitUntil(() => removeTask.IsCompleted);
            yield break;
        }

        Dictionary<string, int> inventoryData = new();

        foreach (InventoryItem item in userManager.user.inventory)
        {
            inventoryData[item.itemName.text] = int.Parse(item.itemQuantity.text);
        }

        var setTask = inventoryRef.SetValueAsync(inventoryData);
        yield return new WaitUntil(() => setTask.IsCompleted);
    }

#else

    private IEnumerator GetInventoryItemsFromDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/inventory.json?auth={userManager.IdToken}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success) yield break;
        string json = req.downloadHandler.text;
        if (json == "null") yield break;
        Dictionary<string, int> data = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        List<InventoryItem> inventoryItems = new();

        foreach (var item in data)
        {
            ObjectBase objectBase = objectDatabase.GetObjectByName(item.Key);
            InventoryItem newItem = Instantiate(inventoryItemPrefab, contentObject);
            newItem.SetItem(objectBase.objectTexture, item.Key, item.Value);
            inventoryItems.Add(newItem);
        }

        userManager.user.inventory = inventoryItems;
        userManager.user.inventory.Capacity = inventorySize;
        SortInventoryItems();
    }

    private IEnumerator SaveInventoryItemsToDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/inventory.json?auth={userManager.IdToken}";

        if (userManager.user.inventory.Count == 0)
        {
            using UnityWebRequest deleteReq = UnityWebRequest.Delete(url);
            yield return deleteReq.SendWebRequest();
            yield break;
        }

        Dictionary<string, int> inventoryData = new();

        foreach (InventoryItem item in userManager.user.inventory)
        {
            inventoryData[item.itemName.text] = int.Parse(item.itemQuantity.text);
        }

        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(inventoryData));
        using UnityWebRequest req = new(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
    }

#endif
}
