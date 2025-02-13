using Movement;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    [SerializeField] private GameObject[] guns;
    private PlayerWeapon[] playerWeapondScripts;
    public string PrimaryGun;
    public string SecondaryGun;
    [SerializeField] private GameObject[] gunPickups;
    [SerializeField] private Transform DropPoint;
    [SerializeField] private float DroppForce;
    [SerializeField] private float DroppRotationFactor = 0.5f;
    [SerializeField] private PlayerController playerMovement;
    [SerializeField] private GunPickUp gunPickupScript;
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
        PrimaryGun = "";
        SecondaryGun = "";
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].SetActive(false);
            playerWeapondScripts[i].ForceReset();
        }
    }

    void ActivateGun(string GunName, bool isPrimary)
    {
        if (isPrimary)
        {

            for (int i = 0; i < guns.Length; i++)
            {
                if (guns[i].name.Contains(GunName))
                {
                    guns[i].SetActive(true);
                    PrimaryGun = guns[i].name.Replace("GunHolder", "");
                    break;
                }
                else
                {
                    guns[i].SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < guns.Length; i++)
            {
                if (guns[i].name.Contains(GunName))
                {
                    guns[i].SetActive(true);
                    SecondaryGun = guns[i].name.Replace("GunHolder", "");
                    break;
                }
                else
                {
                    guns[i].SetActive(false);
                }
            }
        }
    }

    public void pickupGun(string GunName, bool isPrimary)
    {
        ActivateGun(GunName, isPrimary);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(gunPickupScript.GunMangerDrop());
            DropGun();
        }
    }

    private void DropGun()
    {
        if (PrimaryGun != "")
        {
            for (int i = 0; i < gunPickups.Length; i++)
            {
                if (gunPickups[i].name.Contains(PrimaryGun)) //succesfully dropps a gun
                {
                    GameObject dropppedGun = Instantiate(gunPickups[i], DropPoint.position, Quaternion.identity);
                    deActivateAllGuns();
                    Rigidbody DroppedsRigidbody = dropppedGun.GetComponent<Rigidbody>();
                    DroppedsRigidbody.AddForce(transform.forward * DroppForce, ForceMode.Impulse);
                    DroppedsRigidbody.AddForce(transform.up * DroppForce * 0.65f, ForceMode.Impulse);
                    DroppedsRigidbody.AddForce(playerMovement.m_PlayerVelocity * 5.5f, ForceMode.Impulse);

                    Vector3 randomTorque = new Vector3(Random.Range(0.2f, 0.5f), Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f)) * DroppForce * DroppRotationFactor;

                    DroppedsRigidbody.AddTorque(randomTorque, ForceMode.Impulse);
                    break;
                }
                else
                    print("Couldent find yo stupid ahh gun hoe?");
            }
        }
        else
        {
            for (int i = 0; i < gunPickups.Length; i++)
            {
                if (gunPickups[i].name.Contains(SecondaryGun)) //succesfully dropps a gun
                {
                    GameObject dropppedGun = Instantiate(gunPickups[i], DropPoint.position, Quaternion.identity);
                    deActivateAllGuns();
                    Rigidbody DroppedsRigidbody = dropppedGun.GetComponent<Rigidbody>();
                    DroppedsRigidbody.AddForce(transform.forward * DroppForce, ForceMode.Impulse);
                    DroppedsRigidbody.AddForce(transform.up * DroppForce * 0.65f, ForceMode.Impulse);
                    DroppedsRigidbody.AddForce(playerMovement.m_PlayerVelocity * 5.5f, ForceMode.Impulse);

                    Vector3 randomTorque = new Vector3(Random.Range(0.2f, 0.5f), Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f)) * DroppForce * DroppRotationFactor;

                    DroppedsRigidbody.AddTorque(randomTorque, ForceMode.Impulse);
                    break;
                }
                else
                    print("Couldent find yo stupid ahh gun hoe?");
            }
        }
    }
}
