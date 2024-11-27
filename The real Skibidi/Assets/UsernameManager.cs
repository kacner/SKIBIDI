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

    // Set the local player's username
    public void SetUsername(string username)
    {
        // Using Photon’s Hashtable
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Username", username } });
    }

    // Get the username from a PhotonView's owner custom properties
    public string GetUsername(PhotonView photonView)
    {
        if (photonView.Owner.CustomProperties.ContainsKey("Username"))
        {
            return (string)photonView.Owner.CustomProperties["Username"];
        }
        return "Unknown"; // Default if no username is set
    }

    // Synchronize player data when they enter the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Ensure the username is available for new players
        string username = newPlayer.NickName; // Use Photon nickname as fallback if custom property is not set
        newPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Username", username } });

        // Optionally: Update UI or other players about the new player's username
        Debug.Log($"{newPlayer.NickName} entered the room.");
    }

    // Optionally, update when player properties change
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Username"))
        {
            Debug.Log($"Player {targetPlayer.NickName} updated their username: {changedProps["Username"]}");
        }
    }
}
