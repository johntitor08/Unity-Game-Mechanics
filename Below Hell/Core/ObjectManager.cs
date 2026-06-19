#if !UNITY_WEBGL

using Firebase.Database;

#else

using UnityEngine.Networking;
using Newtonsoft.Json;

#endif

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class ObjectManager : MonoBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private UserManager userManager;
    [SerializeField] private ObjectDatabase objectDatabase;
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private RawImage objectTexture;
    [SerializeField] private Button right;
    [SerializeField] private Button left;
    [SerializeField] private Button select;
    [SerializeField] private Button save;
    [SerializeField] private TMP_Text selected;
    private readonly List<int> characterSkins = new() { 0, 1, 2, 3, 4 };
    private int currentObject;
    private int selectedObject;

    private IEnumerator Start()
    {
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();

        yield return new WaitUntil(() => userManager != null && userManager.IsAuthenticated);

#if !UNITY_WEBGL

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

#endif

        objectNameText.gameObject.SetActive(false);
        objectTexture.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
        left.gameObject.SetActive(false);
        select.gameObject.SetActive(false);
        save.gameObject.SetActive(false);
        selected.gameObject.SetActive(false);
        yield return StartCoroutine(GetCharacterSkinsFromDatabase());
        yield return StartCoroutine(GetSelectedCharacterSkinFromDatabase());
    }

    public void NextButton()
    {
        int currentObjectIndex = Array.IndexOf(characterSkins.ToArray(), currentObject);
        currentObjectIndex++;
        currentObjectIndex = currentObjectIndex < characterSkins.Count ? currentObjectIndex : 0;
        currentObject = characterSkins[currentObjectIndex];
        UpdateObject(currentObject);
    }

    public void BackButton()
    {
        int currentObjectIndex = Array.IndexOf(characterSkins.ToArray(), currentObject);
        currentObjectIndex--;
        currentObjectIndex = currentObjectIndex >= 0 ? currentObjectIndex : characterSkins.Count - 1;
        currentObject = characterSkins[currentObjectIndex];
        UpdateObject(currentObject);
    }

    private void UpdateObject(int currentObject)
    {
        ObjectBase objectBase = objectDatabase.GetObject(currentObject);
        objectTexture.texture = objectBase.objectTexture;
        objectNameText.text = objectBase.objectName;

        if (currentObject == selectedObject)
        {
            selected.gameObject.SetActive(true);
            select.gameObject.SetActive(false);
        }
        else
        {
            selected.gameObject.SetActive(false);
            select.gameObject.SetActive(true);
        }
    }

    public void SelectCharacter()
    {
        selected.gameObject.SetActive(true);
        select.gameObject.SetActive(false);
        selectedObject = currentObject;
    }

#if !UNITY_WEBGL

    private IEnumerator SaveSelectedCharacterSkinToDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("selectedCharacterSkin").SetValueAsync(selectedObject);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            userManager.user.selectedCharacterSkin = selectedObject;
        }
        else
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to save selected character: " + inner.Message);
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
                selectedObject = int.Parse(snapshot.Value.ToString());
                objectNameText.gameObject.SetActive(true);
                objectTexture.gameObject.SetActive(true);
                right.gameObject.SetActive(true);
                left.gameObject.SetActive(true);
                save.gameObject.SetActive(true);
                currentObject = selectedObject;
                UpdateObject(selectedObject);
            }
        }
        else
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to get selected character: " + inner.Message);
        }
    }

    private IEnumerator GetCharacterSkinsFromDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("characterSkins").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to get character skins: " + inner.Message);
            yield break;
        }

        DataSnapshot snapshot = task.Result;

        if (snapshot.Exists)
        {
            foreach (var characterSkinSnapshot in snapshot.Children)
            {
                int characterSkin = int.Parse(characterSkinSnapshot.GetValue(true).ToString());
                characterSkins.Add(characterSkin);
            }
        }

        characterSkins.Sort();
    }

#else

    private IEnumerator SaveSelectedCharacterSkinToDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/selectedCharacterSkin.json?auth={userManager.IdToken}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(selectedObject.ToString());
        using UnityWebRequest req = new(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            userManager.user.selectedCharacterSkin = selectedObject;
        }
        else
        {
            Debug.LogError("Failed to save selected character: " + req.error);
        }
    }

    private IEnumerator GetSelectedCharacterSkinFromDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/selectedCharacterSkin.json?auth={userManager.IdToken}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string response = req.downloadHandler.text;

            if (response != "null")
            {
                selectedObject = int.Parse(response);
                objectNameText.gameObject.SetActive(true);
                objectTexture.gameObject.SetActive(true);
                right.gameObject.SetActive(true);
                left.gameObject.SetActive(true);
                save.gameObject.SetActive(true);
                currentObject = selectedObject;
                UpdateObject(selectedObject);
            }
        }
        else
        {
            Debug.LogError("Failed to get selected character: " + req.error);
        }
    }

    private IEnumerator GetCharacterSkinsFromDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/characterSkins.json?auth={userManager.IdToken}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string response = req.downloadHandler.text;

            if (response != "null")
            {
                List<int> skins = JsonConvert.DeserializeObject<List<int>>(response);

                foreach (int skin in skins)
                {
                    characterSkins.Add(skin);
                }

                characterSkins.Sort();
            }
        }
        else
        {
            Debug.LogError("Failed to get character skins: " + req.error);
        }
    }

#endif

    public void Save() => StartCoroutine(SaveSelectedCharacterSkinToDatabase());

    public void Back() => SceneManager.LoadScene("MainMenu");
}
