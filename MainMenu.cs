#if !UNITY_WEBGL

using Firebase.Database;

#else

using UnityEngine.Networking;

#endif

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private UserManager userManager;
    [SerializeField] private TMP_InputField tribeNameText;
    [SerializeField] private TMP_Text genderText;
    private bool isTribeCreated = false;
    [SerializeField] private GameObject genderPanel;

    private IEnumerator Start()
    {
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();

        yield return new WaitUntil(() => userManager != null && userManager.IsAuthenticated);

#if !UNITY_WEBGL

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

#endif

        genderPanel.SetActive(false);
        yield return StartCoroutine(GetGenderFromDatabase());
    }

    public void Male() => genderText.text = "Male";

    public void Female() => genderText.text = "Female";

    public void NonBinary() => genderText.text = "Non-binary";

    public void SaveGender() => StartCoroutine(SaveGenderToDatabase(genderText.text));

    public void CreateTribe()
    {
        if (userManager.user.coin >= 500 && !isTribeCreated)
        {
            StartCoroutine(SaveTribeToDatabase(tribeNameText.text));
            isTribeCreated = true;
        }
    }

    public void StartGame() => SceneManager.LoadScene("GameOptions");

    public void CharacterMenu() => SceneManager.LoadScene("CharacterSelection");

    public void Store() => SceneManager.LoadScene("Store");

    public void Settings() => SceneManager.LoadScene("Settings");

#if !UNITY_WEBGL

    private IEnumerator SaveGenderToDatabase(string gender)
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("gender").SetValueAsync(gender);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            userManager.user.gender = gender;
        }
    }

    private IEnumerator GetGenderFromDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("gender").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            DataSnapshot snapshot = task.Result;

            if (snapshot != null && snapshot.Exists)
            {
                genderText.text = snapshot.Value.ToString();
                userManager.user.gender = snapshot.Value.ToString();
                genderPanel.SetActive(true);
            }
        }
    }

    private IEnumerator SaveTribeToDatabase(string tribe)
    {
        tribe = tribe.Trim();
        if (tribe.Length < 5 || tribe.Length > 20) yield break;
        var getTribeTask = databaseReference.Child("Users").Child(userManager.user.userID).Child("tribe").GetValueAsync();
        yield return new WaitUntil(() => getTribeTask.IsCompleted);

        if (getTribeTask.Exception == null)
        {
            DataSnapshot snapshot = getTribeTask.Result;

            if (snapshot != null && snapshot.Exists && !string.IsNullOrEmpty(snapshot.Value.ToString()))
            {
                yield break;
            }
        }

        tribe = char.ToUpper(tribe[0]) + tribe.Substring(1).ToLower();
        var setTribeTask = databaseReference.Child("Users").Child(userManager.user.userID).Child("tribe").SetValueAsync(tribe);
        yield return new WaitUntil(() => setTribeTask.IsCompleted);

        if (setTribeTask.Exception == null)
        {
            userManager.user.coin -= 500;
            userManager.user.tribe = tribe;
            StartCoroutine(SaveCoinToDatabase());
        }
    }

    private IEnumerator SaveCoinToDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("coin").SetValueAsync(userManager.user.coin);
        yield return new WaitUntil(() => task.IsCompleted);
    }

#else

    private IEnumerator SaveGenderToDatabase(string gender)
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/gender.json?auth={userManager.IdToken}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes('"' + gender + '"');
        using UnityWebRequest req = new(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            userManager.user.gender = gender;
        }
    }

    private IEnumerator GetGenderFromDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/gender.json?auth={userManager.IdToken}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string gender = req.downloadHandler.text;

            if (gender != "null")
            {
                genderText.text = gender;
                userManager.user.gender = gender;
                genderPanel.SetActive(true);
            }
        }
    }

    private IEnumerator SaveTribeToDatabase(string tribe)
    {
        tribe = tribe.Trim();
        if (tribe.Length < 5 || tribe.Length > 20) yield break;
        string urlGet = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/tribe.json?auth={userManager.IdToken}";
        using UnityWebRequest getReq = UnityWebRequest.Get(urlGet);
        yield return getReq.SendWebRequest();
        if (getReq.result != UnityWebRequest.Result.Success) yield break;
        if (getReq.downloadHandler.text != "null") yield break;
        tribe = char.ToUpper(tribe[0]) + tribe.Substring(1).ToLower();
        string urlSet = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/tribe.json?auth={userManager.IdToken}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes('"' + tribe + '"');
        using UnityWebRequest setReq = new(urlSet, "PUT");
        setReq.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        setReq.downloadHandler = new DownloadHandlerBuffer();
        setReq.SetRequestHeader("Content-Type", "application/json");
        yield return setReq.SendWebRequest();

        if (setReq.result == UnityWebRequest.Result.Success)
        {
            userManager.user.coin -= 500;
            userManager.user.tribe = tribe;
            yield return StartCoroutine(SaveCoinToDatabase());
        }
    }

    private IEnumerator SaveCoinToDatabase()
    {
        string url = $"https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/{userManager.user.userID}/coin.json?auth={userManager.IdToken}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(userManager.user.coin.ToString());
        using UnityWebRequest req = new(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
    }

#endif
}
