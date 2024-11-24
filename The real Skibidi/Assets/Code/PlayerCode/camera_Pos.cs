using Photon.Pun;
using UnityEngine;

public class camera_Pos : MonoBehaviourPunCallbacks
{
    public Transform cameraPosition;

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            transform.position = cameraPosition.position;
        }
    }
}
