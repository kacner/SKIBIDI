using UnityEngine;
using Photon.Pun; // inporting Photon
using Photon.Realtime; // inportint Photon Live

public class LobbyManager : MonoBehaviourPunCallbacks // System call
{
    [SerializeField] int LobbySize;

    public void JoinRoom() // Looks for a random room form a list and joins it
    {
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Joining Random Room...");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) // if there was no room to make one for the person
    {
        Debug.Log("you longly as mf kill your slef now");
        CreateRoom();
    }

    void CreateRoom() // here it makes a room for the player with a random pin name with its own pin
    {
        Debug.Log("Creating a room. You are all alone");
        int randomNummber = Random.Range(0, 100000); // room pin
        RoomOptions options = new RoomOptions() // settings for the room
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = LobbySize
        };
        PhotonNetwork.CreateRoom("Room : " + randomNummber, options); // room name
        Debug.Log($"Created Room Pin : {randomNummber}"); // echos back the code
    }

    public override void OnCreateRoomFailed(short returnCode, string message) // if there was an error with the room or there was a room with the same name remake it
    {
        CreateRoom();
    }
}

