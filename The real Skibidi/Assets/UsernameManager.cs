using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public class UsernameManager : MonoBehaviourPunCallbacks
{
    public static UsernameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetUsername(string username)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Username", username } });
    }

    public string GetUsername(PhotonView photonView)
    {
        if (photonView.Owner.CustomProperties.ContainsKey("Username"))
        {
            return (string)photonView.Owner.CustomProperties["Username"];
        }
        return "Unknown";
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        string username = newPlayer.NickName; 
        newPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Username", username } });

        Debug.Log($"{newPlayer.NickName} entered the room.");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Username"))
        {
            Debug.Log($"Player {targetPlayer.NickName} updated their username: {changedProps["Username"]}");
        }
    }
}
