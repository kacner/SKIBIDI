using UnityEngine;

public class GunPickUp : MonoBehaviour
{
    [SerializeField] private GunManager gunManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PickUpable")
        {
            gunManager.pickupGun(other.gameObject.name.Replace("Pickup", ""));
            Destroy(other.gameObject);
        }
    }
}
