#if !UNITY_WEBGL

using Firebase.Database;

#else

using UnityEngine.Networking;
using Newtonsoft.Json;

#endif

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class StoreManager : MonoBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private UserManager userManager;
    [SerializeField] private Button[] buyButtons;
    [SerializeField] private Button[] useButtons;
    [SerializeField] private TMP_Text[] priceTexts;
    [SerializeField] private TMP_Text[] usedTexts;
    [SerializeField] private Transform slimeContentObject;
    [SerializeField] private TMP_Text coinText;

    private IEnumerator Start()
    {
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        yield return new WaitUntil(() => userManager != null && userManager.IsAuthenticated);

#if !UNITY_WEBGL

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

#endif

        yield return StartCoroutine(GetCoinFromDatabase());
        yield return StartCoroutine(GetCharacterSkinsFromDatabase());
        yield return StartCoroutine(GetSelectedCharacterSkinFromDatabase());
    }

    private void Update()
    {
        coinText.text = userManager.user.coin.ToString();
    }

    public void Buy(int itemIndex)
    {
        int price = int.Parse(priceTexts[itemIndex].text);

        if (userManager.user.coin >= price)
        {
            buyButtons[itemIndex].gameObject.SetActive(false);
            priceTexts[itemIndex].gameObject.SetActive(false);
            userManager.user.coin -= price;
            StartCoroutine(AddCharacterSkinToDatabase(itemIndex + 5));
        }
    }

    public void Use(int itemIndex)
    {
        for (int i = 0; i < userManager.user.characterSkins.Length; i++)
        {
            useButtons[userManager.user.characterSkins[i] - 5].gameObject.SetActive(userManager.user.characterSkins[i] != itemIndex + 5);
            usedTexts[userManager.user.characterSkins[i] - 5].gameObject.SetActive(userManager.user.characterSkins[i] == itemIndex + 5);
        }

        StartCoroutine(SaveSelectedCharacterSkinToDatabase(itemIndex + 5));
    }

#if !UNITY_WEBGL

    private IEnumerator SaveSelectedCharacterSkinToDatabase(int selectedCharacterSkinIndex)
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("selectedCharacterSkin").SetValueAsync(selectedCharacterSkinIndex);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            userManager.user.selectedCharacterSkin = selectedCharacterSkinIndex;
        }
    }

    private IEnumerator GetSelectedCharacterSkinFromDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("selectedCharacterSkin").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            DataSnapshot snapshot = task.Result;

            if (snapshot != null && snapshot.Exists)
            {
                userManager.user.selectedCharacterSkin = int.Parse(snapshot.Value.ToString());
            }
        }
    }

    private IEnumerator GetCoinFromDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("coin").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            DataSnapshot snapshot = task.Result;

            if (snapshot != null && snapshot.Exists)
            {
                userManager.user.coin = int.Parse(snapshot.Value.ToString());
            }
        }
    }

    private IEnumerator GetCharacterSkinsFromDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("characterSkins").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.Exception != null) yield break;
        DataSnapshot snapshot = task.Result;
        List<int> characterSkins = new();

        if (snapshot.Exists)
        {
            foreach (var skinSnapshot in snapshot.Children)
            {
                characterSkins.Add(int.Parse(skinSnapshot.GetValue(true).ToString()));
            }

            characterSkins.Sort();
            userManager.user.characterSkins = characterSkins.ToArray();

            for (int i = 0; i < slimeContentObject.childCount; i++)
            {
                if (!userManager.user.characterSkins.Contains(i + 5))
                {
                    buyButtons[i].gameObject.SetActive(true);
                    priceTexts[i].gameObject.SetActive(true);
                }
            }

            for (int i = 0; i < userManager.user.characterSkins.Length; i++)
            {
                useButtons[userManager.user.characterSkins[i] - 5].gameObject.SetActive(userManager.user.characterSkins[i] != userManager.user.selectedCharacterSkin);
                usedTexts[userManager.user.characterSkins[i] - 5].gameObject.SetActive(userManager.user.characterSkins[i] == userManager.user.selectedCharacterSkin);
            }
        }
        else
        {
            for (int i = 0; i < slimeContentObject.childCount; i++)
            {
                buyButtons[i].gameObject.SetActive(true);
                priceTexts[i].gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator AddCharacterSkinToDatabase(int characterSkinToAdd)
    {
        var getTask = databaseReference.Child("Users").Child(userManager.user.userID).Child("characterSkins").GetValueAsync();
        yield return new WaitUntil(() => getTask.IsCompleted);
        if (getTask.Exception != null) yield break;
        List<int> characterSkins = new();

        if (getTask.Result.Exists)
        {
            foreach (var skinSnapshot in getTask.Result.Children)
            {
                characterSkins.Add(int.Parse(skinSnapshot.GetValue(true).ToString()));
            }
        }

        characterSkins.Add(characterSkinToAdd);
        characterSkins.Sort();
        userManager.user.characterSkins = characterSkins.ToArray();
        var setSkinsTask = databaseReference.Child("Users").Child(userManager.user.userID).Child("characterSkins").SetValueAsync(userManager.user.characterSkins);
        yield return new WaitUntil(() => setSkinsTask.IsCompleted);
        if (setSkinsTask.Exception != null) yield break;
        var setCoinTask = databaseReference.Child("Users").Child(userManager.user.userID).Child("coin").SetValueAsync(userManager.user.coin);
        yield return new WaitUntil(() => setCoinTask.IsCompleted);
        if (setCoinTask.Exception != null) yield break;
        useButtons[characterSkinToAdd - 5].gameObject.SetActive(true);
    }

#else

    private IEnumerator SaveSelectedCharacterSkinToDatabase(int selectedCharacterSkinIndex)
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/selectedCharacterSkin.json?auth={userManager.IdToken}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(selectedCharacterSkinIndex.ToString());
        using UnityWebRequest req = new(url, "PUT");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            userManager.user.selectedCharacterSkin = selectedCharacterSkinIndex;
        }
    }

    private IEnumerator GetSelectedCharacterSkinFromDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/selectedCharacterSkin.json?auth={userManager.IdToken}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string text = req.downloadHandler.text;

            if (text != "null")
            {
                userManager.user.selectedCharacterSkin = int.Parse(text);
            }
        }
    }

    private IEnumerator GetCoinFromDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/coin.json?auth={userManager.IdToken}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string text = req.downloadHandler.text;

            if (text != "null")
            {
                userManager.user.coin = int.Parse(text);
            }
        }
    }

    private IEnumerator GetCharacterSkinsFromDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/characterSkins.json?auth={userManager.IdToken}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success) yield break;
        string json = req.downloadHandler.text;

        if (json == "null")
        {
            for (int i = 0; i < slimeContentObject.childCount; i++)
            {
                buyButtons[i].gameObject.SetActive(true);
                priceTexts[i].gameObject.SetActive(true);
            }

            yield break;
        }

        List<int> characterSkins = JsonConvert.DeserializeObject<List<int>>(json);
        characterSkins.Sort();
        userManager.user.characterSkins = characterSkins.ToArray();

        for (int i = 0; i < slimeContentObject.childCount; i++)
        {
            if (!userManager.user.characterSkins.Contains(i + 5))
            {
                buyButtons[i].gameObject.SetActive(true);
                priceTexts[i].gameObject.SetActive(true);
            }
        }

        for (int i = 0; i < userManager.user.characterSkins.Length; i++)
        {
            useButtons[userManager.user.characterSkins[i] - 5].gameObject.SetActive(userManager.user.characterSkins[i] != userManager.user.selectedCharacterSkin);
            usedTexts[userManager.user.characterSkins[i] - 5].gameObject.SetActive(userManager.user.characterSkins[i] == userManager.user.selectedCharacterSkin);
        }
    }

    private IEnumerator AddCharacterSkinToDatabase(int characterSkinToAdd)
    {
        string urlGet = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/characterSkins.json?auth={userManager.IdToken}";
        using UnityWebRequest getReq = UnityWebRequest.Get(urlGet);
        yield return getReq.SendWebRequest();
        if (getReq.result != UnityWebRequest.Result.Success) yield break;
        List<int> characterSkins = new();
        string existingJson = getReq.downloadHandler.text;

        if (existingJson != "null")
        {
            characterSkins = JsonConvert.DeserializeObject<List<int>>(existingJson);
        }

        characterSkins.Add(characterSkinToAdd);
        characterSkins.Sort();
        userManager.user.characterSkins = characterSkins.ToArray();
        string urlSet = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/characterSkins.json?auth={userManager.IdToken}";
        byte[] skinsBody = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userManager.user.characterSkins));
        using UnityWebRequest setReq = new(urlSet, "PUT");
        setReq.uploadHandler   = new UploadHandlerRaw(skinsBody);
        setReq.downloadHandler = new DownloadHandlerBuffer();
        setReq.SetRequestHeader("Content-Type", "application/json");
        yield return setReq.SendWebRequest();
        if (setReq.result != UnityWebRequest.Result.Success) yield break;
        string urlCoin = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/coin.json?auth={userManager.IdToken}";
        byte[] coinBody = System.Text.Encoding.UTF8.GetBytes(userManager.user.coin.ToString());
        using UnityWebRequest coinReq = new(urlCoin, "PUT");
        coinReq.uploadHandler   = new UploadHandlerRaw(coinBody);
        coinReq.downloadHandler = new DownloadHandlerBuffer();
        coinReq.SetRequestHeader("Content-Type", "application/json");
        yield return coinReq.SendWebRequest();

        if (coinReq.result == UnityWebRequest.Result.Success)
        {
            useButtons[characterSkinToAdd - 5].gameObject.SetActive(true);
        }
    }

#endif

    public void Back() => SceneManager.LoadScene("MainMenu");
}
