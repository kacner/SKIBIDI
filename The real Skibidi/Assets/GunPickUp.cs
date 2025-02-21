using Photon.Pun;
using System.Collections;
using UnityEngine;

public class GunPickUp : MonoBehaviourPunCallbacks
{
    [SerializeField] private GunManager gunManager;
    [SerializeField] private Collider collider;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PickUpable" && photonView.IsMine)
        {
            GunCustomGravity gunGravity = other.GetComponent<GunCustomGravity>();
            string ItemName = other.gameObject.name.Replace("Pickup", "");


            if (gunManager.PrimaryGun == "" && gunGravity.gunInventoryType == GunInventoryType.Primary)
            {
                gunManager.pickupGun(ItemName.Replace("(Clone)", ""), gunGravity.gunInventoryType);
                if (Application.isEditor)
                    Destroy(other.gameObject);
                else
                    PhotonNetwork.Destroy(other.gameObject);
            }
            if (gunManager.SecondaryGun == "" && gunGravity.gunInventoryType == GunInventoryType.Secondary)
            {
                gunManager.pickupGun(ItemName.Replace("(Clone)", ""), gunGravity.gunInventoryType);
                if (Application.isEditor)
                    Destroy(other.gameObject);
                else
                    PhotonNetwork.Destroy(other.gameObject);
            }
        }
    }

    public IEnumerator GunMangerDrop()
    {
        collider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        collider.enabled = true;
    }
}