using UnityEngine;

public class GunPickUp : MonoBehaviour
{
    [SerializeField] private GunManager gunManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PickUpable")
        {
            string ItemName = other.gameObject.name.Replace("Pickup", "");
            gunManager.pickupGun(ItemName.Replace("(Clone)", ""));
            Destroy(other.gameObject);
        }
    }
}
