using UnityEngine;
using Photon.Pun;


public class JoiningLobbyManiger : MonoBehaviourPunCallbacks
{
    [SerializeField] int sceneIndex;

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnJoinedRoom() // command to join a room
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Starting game");
            PhotonNetwork.LoadLevel(sceneIndex);
        }
    }

}
