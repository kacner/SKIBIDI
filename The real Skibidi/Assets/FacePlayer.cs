using Photon.Pun;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    private Transform localPlayerCamera;

    void Update()
    {
        if (localPlayerCamera == null)
        {
            FindLocalPlayerCamera();
        }

        if (localPlayerCamera != null)
        {
            Vector3 directionToFace = localPlayerCamera.position - transform.position;

            directionToFace.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(-directionToFace);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            print("calcuilatingdistance");
        }
    }

    void FindLocalPlayerCamera()
    {
        if (PhotonNetwork.IsConnected)
        {
            foreach (var photonView in FindObjectsOfType<PhotonView>())
            {
                if (photonView.IsMine)
                {
                    localPlayerCamera = photonView.gameObject.GetComponentInChildren<Camera>().transform;
                    Debug.Log("Local player's camera found.");
                    return;
                }
            }
        }
        else
        {
            localPlayerCamera = Camera.main?.transform;
            Debug.Log("Using main camera for single-player.");
        }
    }
}
