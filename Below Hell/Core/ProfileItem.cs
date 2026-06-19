using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileItem : MonoBehaviour
{
    public TMP_Text profileName;
    public RawImage profileImage;
    public TMP_Text profileFirst;
    public TMP_Text profileIsOnline;
    public TMP_Text profileGender;
    public TMP_Text profileTribe;
    public TMP_Text profileRegistrationDate;
    public Button profileClose;

    public void CloseProfile()
    {
        Destroy(gameObject);

    }

}