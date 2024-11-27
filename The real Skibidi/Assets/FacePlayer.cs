using Photon.Pun;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    private Transform localCamera;

    void Start()
    {
        FindLocalCamera();
    }

    void Update()
    {
        if (localCamera != null)
        {
            FaceCamera();
        }
        else
        {
            FindLocalCamera();
        }
    }

    private void FindLocalCamera()
    {
        localCamera = Camera.main?.transform;

        if (localCamera != null)
        {
            Debug.Log("Local camera found for client.");
        }
        else
        {
            Debug.LogWarning("Local camera not found. Retrying...");
        }
    }

    private void FaceCamera()
    {
        Vector3 directionToFace = localCamera.position - transform.position;

        directionToFace.y = 0;

        transform.rotation = Quaternion.LookRotation(-directionToFace);
    }
}