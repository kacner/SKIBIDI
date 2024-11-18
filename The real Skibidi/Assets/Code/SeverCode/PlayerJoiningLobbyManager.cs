using UnityEngine;
using Photon.Pun;


public class PlayerJoiningLobbyManager : MonoBehaviour
{
    void Start()
    {
        Invoke("CreatePlayer", 0.5f);
    }

    void CreatePlayer()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(0, 3, 0), Quaternion.identity); // when player joins add chericter to the world
    }
}

