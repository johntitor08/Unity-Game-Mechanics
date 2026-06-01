using TMPro;
using UnityEngine;

public class PlayerNameItem : MonoBehaviour
{
    public TMP_Text playerNickname;

    public void SetPlayerNickname(string playerNickname)
    {
        this.playerNickname.text = playerNickname;

    }

}