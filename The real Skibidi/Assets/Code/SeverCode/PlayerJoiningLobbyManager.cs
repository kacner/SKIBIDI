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
        PhotonNetwork.Instantiate("Player", transform.position, Quaternion.identity); // when player joins add chericter to the world
    }
}

