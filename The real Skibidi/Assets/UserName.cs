using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserName : MonoBehaviour
{
    public TextMeshProUGUI textComp;
    public bool hasChangedUsername = false;
    public Button startbutton;
    public void SetUsername()
    {
        print("Changed username");
        PhotonNetwork.NickName = textComp.text; // Set the username
        hasChangedUsername = true;
        startbutton.interactable = true;
    }
}
