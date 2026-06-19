#if !UNITY_WEBGL

using Firebase.Database;

#else

using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

#endif

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MPSVGameManager : MonoBehaviour
{
#if !UNITY_WEBGL
    private DatabaseReference databaseReference;
#endif

    private ServicesManager servicesManager;
    private UserManager userManager;
    public GameObject profilePanel;
    public GameObject friendListPanel;
    public TMP_Text usernameText;
    public TMP_Text firstText;
    public TMP_Text genderText;
    public TMP_Text tribeNameText;
    public TMP_Text registrationDateText;
    public TMP_Text playerCompletedText;
    public GameObject setProfileImageButton;
    public GameObject panelPlayerList;
    public GameObject panelOptions;
    public GameObject panelPlayerCompleted;
    public GameObject canvasChat;
    public TMP_InputField messageInputField;
    public Transform messageContentObject;
    public bool isOnInputChatField = false;
    public GameObject[] maps;
    public Texture2D[] backgrounds;
    public Texture2D[] mountains;
    public PlayerNameItem playerNameItemPrefab;
    public Transform playerNameItemContentObject;
    public int mapIndex, mapBackground, mapMountain;
    public float elapsedTime;
    public bool isMapCompleted, isNewMapLoaded;
    public string winnerPlayerNickname;
    public Button sendMessageButton;
    public ProfileItem profileItemPrefab;
    public Transform profileItemContentObject;
    public FriendItem friendItemPrefab;
    public Transform friendItemContentObject;
    public ObjectDatabase slimesDatabase;
    public ObjectAnimationDatabase slimesJumpDatabase;
    public ObjectAnimationDatabase slimesFallDatabase;
    public GameObject[] emojis;
    public GameObject cannonballPrefab;
    public GameObject messagePrefab;

    private void Start()
    {
#if !UNITY_WEBGL

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

#endif
        servicesManager = GameObject.Find("NetworkManager").GetComponent<ServicesManager>();
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        profilePanel.SetActive(false);
        friendListPanel.SetActive(false);
        panelOptions.SetActive(false);
        canvasChat.SetActive(true);
        panelPlayerCompleted.SetActive(false);

        if (servicesManager.isHost)
        {
            NetworkManager.Singleton.StartHost();
            GameObject.FindWithTag("MainCVC").AddComponent<ShamanController>();
        }
        else NetworkManager.Singleton.StartClient();
    }

#if !UNITY_WEBGL

    public IEnumerator CheckPlayerUsernameFromDatabase(string playerNickname)
    {
        var task = databaseReference.Child("Users").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            string normalizedNickname = (char.ToUpper(playerNickname[0]) + playerNickname.Substring(1).ToLower()).Trim();
            foreach (var userSnapshot in task.Result.Children)
            {
                DataSnapshot usernameSnapshot = userSnapshot.Child("username");
                if (!usernameSnapshot.Exists || usernameSnapshot.Value.ToString() != normalizedNickname) continue;
                var profileImageValue = userSnapshot.Child("profileImage").Value;
                int firstValue = int.Parse(userSnapshot.Child("first").Value.ToString());
                bool isOnline = bool.Parse(userSnapshot.Child("isOnline").Value.ToString());
                string gender = userSnapshot.Child("gender").Value.ToString();
                var tribeValue = userSnapshot.Child("tribe").Value;
                string registrationDate = userSnapshot.Child("registrationDate").Value.ToString().Split(' ')[0];
                ProfileItem newProfileItem = Instantiate(profileItemPrefab, profileItemContentObject);
                newProfileItem.profileName.text = normalizedNickname;
                newProfileItem.profileFirst.text = "First: " + firstValue;
                newProfileItem.profileGender.text = "Gender: " + gender;
                newProfileItem.profileRegistrationDate.text = "Registration Date: " + registrationDate;
                newProfileItem.profileIsOnline.text = isOnline ? "Player is online" : "Player is offline";

                if (!string.IsNullOrEmpty(tribeValue?.ToString()))
                    newProfileItem.profileTribe.text = "Tribe: " + tribeValue;
                else
                {
                    newProfileItem.profileTribe.gameObject.SetActive(false);
                    Vector2 pos = newProfileItem.profileGender.transform.localPosition; pos.y -= 75;
                    newProfileItem.profileGender.transform.localPosition = pos;
                }
                if (!string.IsNullOrEmpty(profileImageValue?.ToString()))
                {
                    byte[] imageBytes = System.Convert.FromBase64String(profileImageValue.ToString());
                    Texture2D texture = new(2, 2); texture.LoadImage(imageBytes);
                    newProfileItem.profileImage.texture = texture;
                }
            }
        }
        else { var inner = task.Exception.InnerException ?? task.Exception; Debug.LogError("Failed to check player username: " + inner.Message); }
    }

    public IEnumerator GetFriendsFromDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("friends").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.Exception != null) { var inner = task.Exception.InnerException ?? task.Exception; Debug.LogError("Failed to get friends: " + inner.Message); yield break; }
        List<string> friends = new();

        if (task.Result.Exists)
        {
            foreach (var fs in task.Result.Children)
            {
                string friendUsername = fs.GetValue(true).ToString();
                friends.Add(friendUsername);
                FriendItem item = Instantiate(friendItemPrefab, friendItemContentObject);
                item.playerNameText.text = friendUsername;
            }

            userManager.user.friends = friends.ToArray();
        }
    }

    public IEnumerator AddFriendToDatabase(string playerNickname)
    {
        playerNickname = (char.ToUpper(playerNickname[0]) + playerNickname.Substring(1).ToLower()).Trim();
        if (playerNickname == userManager.user.username) yield break;
        var getUsersTask = databaseReference.Child("Users").GetValueAsync();
        yield return new WaitUntil(() => getUsersTask.IsCompleted);
        if (getUsersTask.Exception != null) yield break;
        string targetUserID = null;

        foreach (var us in getUsersTask.Result.Children)
        {
            var usSnap = us.Child("username");
            if (usSnap.Exists && usSnap.Value.ToString() == playerNickname) { targetUserID = us.Key; break; }
        }

        if (targetUserID == null) yield break;
        var friendsRef = databaseReference.Child("Users").Child(userManager.user.userID).Child("friends");
        var getFriendsTask = friendsRef.GetValueAsync();
        yield return new WaitUntil(() => getFriendsTask.IsCompleted);
        if (getFriendsTask.Exception != null) yield break;
        List<string> currentFriends = new();

        if (getFriendsTask.Result.Exists)
            foreach (var fs in getFriendsTask.Result.Children) currentFriends.Add(fs.GetValue(true).ToString());

        if (currentFriends.Contains(playerNickname)) yield break;
        currentFriends.Add(playerNickname);
        userManager.user.friends = currentFriends.ToArray();
        var updateTask = friendsRef.SetValueAsync(currentFriends.ToArray());
        yield return new WaitUntil(() => updateTask.IsCompleted);
        if (updateTask.Exception == null) { FriendItem item = Instantiate(friendItemPrefab, friendItemContentObject); item.playerNameText.text = playerNickname; }
        else { var inner = updateTask.Exception.InnerException ?? updateTask.Exception; Debug.LogError("Failed to add friend: " + inner.Message); }
    }

    public IEnumerator RemoveFriendFromDatabase(string playerNickname)
    {
        var friendsRef = databaseReference.Child("Users").Child(userManager.user.userID).Child("friends");
        var getFriendsTask = friendsRef.GetValueAsync();
        yield return new WaitUntil(() => getFriendsTask.IsCompleted);
        if (getFriendsTask.Exception != null) yield break;
        List<string> currentFriends = new();

        if (getFriendsTask.Result.Exists)
            foreach (var fs in getFriendsTask.Result.Children) currentFriends.Add(fs.GetValue(true).ToString());

        if (!currentFriends.Contains(playerNickname)) yield break;
        currentFriends.Remove(playerNickname);
        userManager.user.friends = currentFriends.ToArray();
        var updateTask = friendsRef.SetValueAsync(currentFriends.ToArray());
        yield return new WaitUntil(() => updateTask.IsCompleted);

        if (updateTask.Exception == null)
        {
            foreach (FriendItem fi in friendItemContentObject.GetComponentsInChildren<FriendItem>())
                if (fi.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text == playerNickname) { Destroy(fi.gameObject); break; }
        }
        else { var inner = updateTask.Exception.InnerException ?? updateTask.Exception; Debug.LogError("Failed to remove friend: " + inner.Message); }
    }

#else

    public IEnumerator CheckPlayerUsernameFromDatabase(string playerNickname)
    {
        string normalizedNickname = (char.ToUpper(playerNickname[0]) + playerNickname[1..].ToLower()).Trim();
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users.json?auth=" + userManager.IdToken;
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success) yield break;
        JObject usersJson = JObject.Parse(req.downloadHandler.text);

        foreach (var userEntry in usersJson)
        {
            JToken userData = userEntry.Value;
            if (userData["username"]?.ToString() != normalizedNickname) continue;
            string profileImageValue = userData["profileImage"]?.ToString();
            int firstValue = int.Parse(userData["first"]?.ToString() ?? "0");
            bool isOnline = bool.Parse(userData["isOnline"]?.ToString() ?? "false");
            string gender = userData["gender"]?.ToString() ?? "";
            string tribeValue = userData["tribe"]?.ToString();
            string registrationDate = userData["registrationDate"]?.ToString().Split(' ')[0] ?? "";
            ProfileItem newProfileItem = Instantiate(profileItemPrefab, profileItemContentObject);
            newProfileItem.profileName.text = normalizedNickname;
            newProfileItem.profileFirst.text = "First: " + firstValue;
            newProfileItem.profileGender.text = "Gender: " + gender;
            newProfileItem.profileRegistrationDate.text = "Registration Date: " + registrationDate;
            newProfileItem.profileIsOnline.text = isOnline ? "Player is online" : "Player is offline";

            if (!string.IsNullOrEmpty(tribeValue))
                newProfileItem.profileTribe.text = "Tribe: " + tribeValue;

            else
            {
                newProfileItem.profileTribe.gameObject.SetActive(false);
                Vector2 pos = newProfileItem.profileGender.transform.localPosition; pos.y -= 75;
                newProfileItem.profileGender.transform.localPosition = pos;
            }
            if (!string.IsNullOrEmpty(profileImageValue))
            {
                byte[] imageBytes = System.Convert.FromBase64String(profileImageValue);
                Texture2D texture = new Texture2D(2, 2); texture.LoadImage(imageBytes);
                newProfileItem.profileImage.texture = texture;
            }
        }
    }

    public IEnumerator GetFriendsFromDatabase()
    {
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + userManager.user.userID + "/friends.json?auth=" + userManager.IdToken;
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success) { Debug.LogError("Failed to get friends: " + req.error); yield break; }
        string jsonResponse = req.downloadHandler.text;
        if (jsonResponse == "null") yield break;
        foreach (var friend in JArray.Parse(jsonResponse))
        { FriendItem item = Instantiate(friendItemPrefab, friendItemContentObject); item.playerNameText.text = friend.ToString(); }
    }

    public IEnumerator AddFriendToDatabase(string playerNickname)
    {
        playerNickname = (char.ToUpper(playerNickname[0]) + playerNickname.Substring(1).ToLower()).Trim();
        if (playerNickname == userManager.user.username) yield break;
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + userManager.user.userID + "/friends.json?auth=" + userManager.IdToken;
        using UnityWebRequest getReq = UnityWebRequest.Get(url);
        yield return getReq.SendWebRequest();
        if (getReq.result != UnityWebRequest.Result.Success) yield break;
        List<string> currentFriends = new();
        string jsonResponse = getReq.downloadHandler.text;
        if (jsonResponse != "null") foreach (var f in JArray.Parse(jsonResponse)) currentFriends.Add(f.ToString());
        if (currentFriends.Contains(playerNickname)) yield break;
        currentFriends.Add(playerNickname);
        userManager.user.friends = currentFriends.ToArray();
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(currentFriends));
        using UnityWebRequest putReq = new UnityWebRequest(url, "PUT");
        putReq.uploadHandler = new UploadHandlerRaw(bodyRaw); putReq.downloadHandler = new DownloadHandlerBuffer();
        putReq.SetRequestHeader("Content-Type", "application/json");
        yield return putReq.SendWebRequest();
        if (putReq.result == UnityWebRequest.Result.Success) { FriendItem item = Instantiate(friendItemPrefab, friendItemContentObject); item.playerNameText.text = playerNickname; }
        else Debug.LogError("Failed to add friend: " + putReq.error);
    }

    public IEnumerator RemoveFriendFromDatabase(string playerNickname)
    {
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + userManager.user.userID + "/friends.json?auth=" + userManager.IdToken;
        using UnityWebRequest getReq = UnityWebRequest.Get(url);
        yield return getReq.SendWebRequest();
        if (getReq.result != UnityWebRequest.Result.Success) yield break;
        string jsonResponse = getReq.downloadHandler.text;
        if (jsonResponse == "null") yield break;
        List<string> currentFriends = new();
        foreach (var f in JArray.Parse(jsonResponse)) currentFriends.Add(f.ToString());
        if (!currentFriends.Contains(playerNickname)) yield break;
        currentFriends.Remove(playerNickname);
        userManager.user.friends = currentFriends.ToArray();
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(currentFriends));
        using UnityWebRequest putReq = new UnityWebRequest(url, "PUT");
        putReq.uploadHandler = new UploadHandlerRaw(bodyRaw); putReq.downloadHandler = new DownloadHandlerBuffer();
        putReq.SetRequestHeader("Content-Type", "application/json");
        yield return putReq.SendWebRequest();

        if (putReq.result == UnityWebRequest.Result.Success)
        {
            foreach (FriendItem fi in friendItemContentObject.GetComponentsInChildren<FriendItem>())
                if (fi.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text == playerNickname) { Destroy(fi.gameObject); break; }
        }
        else Debug.LogError("Failed to remove friend: " + putReq.error);
    }

#endif

    public void OnSelectInputChatField() { isOnInputChatField = true; }

    public void OnDeselectInputChatField() { isOnInputChatField = false; }

    public void ReturnToGame() { panelOptions.SetActive(false); }

    public void LeaveRoom()
    {
        panelOptions.SetActive(false);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("GameOptions");
    }
}
