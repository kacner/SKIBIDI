using UnityEngine;
using UnityEngine.UI;
using Photon.Pun; // Inporting photon libery


public class ServerManager : MonoBehaviourPunCallbacks // System call
{
    [SerializeField] Button StartButton;

    void Start()
    {
        StartButton.interactable = false;

        PhotonNetwork.ConnectUsingSettings(); // connecting to the sever
        Debug.Log("Connecting Photon ...");
    }

    public override void OnConnectedToMaster() // When connected
    {
        StartButton.interactable = true;
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log($"connected to sever in {PhotonNetwork.CloudRegion}");
    }

}