using System.Collections.Generic;

[System.Serializable]
public class User
{
    public string username;
    public string userID;
    public string registrationDate;
    public bool isOnline;
    public bool isAdmin;
    public string profileImage;
    public int highscore;
    public int first;
    public int coin;
    public int level;
    public string[] friends;
    public List<InventoryItem> inventory;
    public string gender;
    public string tribe;
    public int selectedCharacterSkin;
    public int[] characterSkins;

    public User(string username, string userID, string registrationDate = null, bool isOnline = false)
    {
        this.username = username;
        this.userID = userID;
        this.registrationDate = registrationDate;
        this.isOnline = isOnline;
        isAdmin = false;
        profileImage = "";
        highscore = 0;
        first = 0;
        coin = 0;
        level = 1;
        friends = new string[0];
        inventory = new List<InventoryItem>();
        gender = "Non-binary";
        tribe = "";
        selectedCharacterSkin = 0;
        characterSkins = new int[0];
    }
}
