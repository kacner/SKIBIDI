using UnityEngine;

public class GunManager : MonoBehaviour
{
    [SerializeField] private GameObject[] guns;
    private PlayerWeapon[] playerWeapondScripts;
    [SerializeField] private string activeGun;
    [SerializeField] private GameObject[] gunPickups;
    [SerializeField] private Transform DropPoint;
    [SerializeField] private float DroppForce;
    private void Start()
    {
        playerWeapondScripts = new PlayerWeapon[guns.Length];
        for (int i = 0; i < guns.Length; i++)
        {
            playerWeapondScripts[i] = guns[i].GetComponentInChildren<PlayerWeapon>();
        }
        deActivateAllGuns();
    }

    void deActivateAllGuns()
    {
        activeGun = "";
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].SetActive(false);
            playerWeapondScripts[i].ForceReset();
        }
    }

    void ActivateGun(string GunName)
    {
        for (int i = 0; i < guns.Length; i++)
        {
            if (guns[i].name.Contains(GunName))
            {
                guns[i].SetActive(true);
                activeGun = guns[i].name.Replace("GunHolder", "");
                break;
            }
            else
            {
                guns[i].SetActive(false);
            }
        }
    }


    public void pickupGun(string GunName)
    {
        ActivateGun(GunName);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.G))
        {
            DropGun();
        }
    }

    private void DropGun()
    {
        if (activeGun != "")
        {
            for (int i = 0; i < gunPickups.Length; i++)
            {
                if (gunPickups[i].name.Contains(activeGun)) //succesfully dropps a gun
                {
                    GameObject dropppedGun = Instantiate(gunPickups[i], DropPoint.position, Quaternion.identity);
                    deActivateAllGuns();
                    Rigidbody DroppedsRigidbody = dropppedGun.GetComponent<Rigidbody>();
                    DroppedsRigidbody.AddForce(transform.forward * DroppForce, ForceMode.Impulse);
                    DroppedsRigidbody.AddForce(transform.up * DroppForce * 0.65f, ForceMode.Impulse);
                    break;
                }
                else
                    print("Couldent find yo stupid ahh gun hoe?");
            }
        }
    }
}
