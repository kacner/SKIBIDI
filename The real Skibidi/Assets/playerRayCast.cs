using Movement;
using Photon.Pun;
using UnityEngine;

public class playerRayCast : MonoBehaviour
{
    [SerializeField] private Camera playerCamera; 
    [SerializeField] private float rayDistance = 100f; 
    [SerializeField] private LayerMask rayLayerMask; 
    [SerializeField] private GameObject ActionText;

    private void Update()
    {
        ShootRayFromCrosshair();
    }
    void ShootRayFromCrosshair()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, rayLayerMask))
        {
            Debug.Log("Hit object: " + hit.collider.name);
            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
            ActionText.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E) && ActionText.activeSelf)
            {
                PhotonView targetPhotonView = hit.collider.gameObject.GetComponent<PhotonView>();
                if (targetPhotonView != null && !targetPhotonView.IsMine)
                {
                    StartCoroutine(hit.collider.gameObject.GetComponent<PlayerController>().die());
                    Invoke("Minimize", 0.2f);
                }
            }
        }
        else
        {
            ActionText.SetActive(false);
        }
    }
    void Minimize(PhotonView targetPhotonView)
    {
        targetPhotonView.RPC("RPC_MinimizeWindow", RpcTarget.OthersBuffered);
    }

}
