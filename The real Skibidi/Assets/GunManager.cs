using Movement;
using Photon.Pun;
using UnityEngine;
public class GunManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private GameObject[] guns;
    private PlayerWeapon[] playerWeapondScripts;
    public string PrimaryGun;
    public string SecondaryGun;
    public string Melee;
    [SerializeField] private int SelectedSlot = 3;
    [SerializeField] private GameObject[] gunPickups;
    [SerializeField] private Transform DropPoint;
    [SerializeField] private float DroppForce;
    [SerializeField] private float DroppRotationFactor = 0.5f;
    [SerializeField] private PlayerController playerMovement;
    [SerializeField] private GunPickUp gunPickupScript;
    [SerializeField] private GunUiManager gunUiManager;
    private void Start()
    {
       // if (!photonView.IsMine) return;
        Melee = "Karambit";
        playerWeapondScripts = new PlayerWeapon[guns.Length];
        for (int i = 0; i < guns.Length; i++)
        {
            playerWeapondScripts[i] = guns[i].GetComponentInChildren<PlayerWeapon>();
        }
        deActivateAllGuns(4);
        ActivateGun(Melee, GunInventoryType.Melee);
        gunUiManager.UpdateWeaponSlot(GunInventoryType.Melee, Melee);
    }
    void deActivateAllGuns(int deactivateSlot = 0)
    {

        if (deactivateSlot == 1)
            PrimaryGun = "";
        else if (deactivateSlot == 2)
            SecondaryGun = "";
        else if (deactivateSlot == 4)
        {
            PrimaryGun = "";
            SecondaryGun = "";
        }
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].SetActive(false);
            playerWeapondScripts[i]?.ForceReset();
        }
    }

    void ActivateGun(string GunName, GunInventoryType type)
    {
        gunUiManager.HighlightSelectedWeapon(type);
        if (type == GunInventoryType.Primary)
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
        else if (type == GunInventoryType.Secondary)
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
        else if (type == GunInventoryType.Melee)
        {
            for (int i = 0; i < guns.Length; i++)
            {
                if (guns[i].name.Contains(GunName))
                {
                    guns[i].SetActive(true);
                    Melee = guns[i].name.Replace("GunHolder", "");
                    break;
                }
                else
                {
                    guns[i].SetActive(false);
                }
            }
        }
    }

    public void pickupGun(string GunName, GunInventoryType type)
    {
        gunUiManager.UpdateWeaponSlot(type, GunName);
        if (type == GunInventoryType.Primary)
        {
            if (PrimaryGun == "")
            {
                deActivateAllGuns();
                ActivateGun(GunName, type);
                updateHeldItem(1);
            }

            PrimaryGun = GunName;
            if (SelectedSlot == 1)
            {
                ActivateGun(PrimaryGun, GunInventoryType.Primary);
            }
        }
        else if (type == GunInventoryType.Secondary)
        {
            SecondaryGun = GunName;
            if (SelectedSlot == 2)
            {
                ActivateGun(SecondaryGun, GunInventoryType.Secondary);
            }
        }
        else if (type == GunInventoryType.Melee)
        {
            Melee = GunName;
            if (SelectedSlot == 3)
            {
                ActivateGun(Melee, GunInventoryType.Melee);
            }
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.G) && (PrimaryGun != "" || SecondaryGun != ""))
            {
                StartCoroutine(gunPickupScript.GunMangerDrop());
                DropGun();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectedSlot = 1;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectedSlot = 2;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectedSlot = 3;
            }
        }

        Debug.LogError("Selected slot: " + SelectedSlot);

        if (SelectedSlot == 1 && PrimaryGun == "")
        {
            if (SecondaryGun != "")
                updateHeldItem(2);
            else
                updateHeldItem(3);
        }
        if (SelectedSlot == 2 && SecondaryGun == "")
        {
            if (PrimaryGun != "")
                updateHeldItem(1);
            else
                updateHeldItem(3);
        }

        //baserat på selectedslot välj vapen
        if (SelectedSlot == 1)
        {
            Debug.LogError("primary");
            updateHeldItem(1);
        }
        if (SelectedSlot == 2)
        {
            Debug.LogError("second");
            updateHeldItem(2);
        }
        if (SelectedSlot == 3)
        {
            Debug.LogError("kniw");
            updateHeldItem(3);
        }

    }

    void updateHeldItem(int newSlotSelected)
    {
                if (newSlotSelected < 1 || newSlotSelected > 3)
                {
                    Debug.LogWarning($"Invalid slot selection: {newSlotSelected}");
                    return;
                }
        if (newSlotSelected == 3)
        {
            deActivateAllGuns();
            ActivateGun(Melee, GunInventoryType.Melee);
            SelectedSlot = newSlotSelected;
        }
        else if (newSlotSelected == 1 && PrimaryGun != "")
        {
            deActivateAllGuns();
            ActivateGun(PrimaryGun, GunInventoryType.Primary);
            SelectedSlot = newSlotSelected;
        }
        else if (newSlotSelected == 2 && SecondaryGun != "")
        {
            deActivateAllGuns();
            ActivateGun(SecondaryGun, GunInventoryType.Secondary);
            SelectedSlot = newSlotSelected;
        }
    }

    private void DropGun()
    {
        if (SelectedSlot == 1 && PrimaryGun != "")
        {
            for (int i = 0; i < gunPickups.Length; i++)
            {
                if (gunPickups[i].name.Contains(PrimaryGun)) //succesfully dropps a gun
                {
                    gunUiManager.UpdateWeaponSlot(GunInventoryType.Primary, "");
                    GameObject dropppedGun = PhotonNetwork.Instantiate(gunPickups[i].name, DropPoint.position, Quaternion.identity);
                    print(dropppedGun);
                    deActivateAllGuns(1);
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
        else if (SelectedSlot == 2 && SecondaryGun != "")
        {
            for (int i = 0; i < gunPickups.Length; i++)
            {
                if (gunPickups[i].name.Contains(SecondaryGun)) //succesfully dropps a gun
                {
                    gunUiManager.UpdateWeaponSlot(GunInventoryType.Secondary, "");
                    GameObject dropppedGun = PhotonNetwork.Instantiate(gunPickups[i].name, DropPoint.position, Quaternion.identity);
                    print(dropppedGun);
                    deActivateAllGuns(2);
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
            print("You dont have any guns");
    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(SelectedSlot);
            stream.SendNext(PrimaryGun);
            stream.SendNext(SecondaryGun);
        }
        else if (stream.IsReading)
        {
            this.SelectedSlot = (int)stream.ReceiveNext();
            this.PrimaryGun = (string)stream.ReceiveNext();
            this.SecondaryGun = (string)stream.ReceiveNext();
        }
    }
}
