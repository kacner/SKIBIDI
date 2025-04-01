using UnityEngine;
using Photon.Pun;
using Movement;

public class GunManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Gun & Pickup References")]
    [SerializeField] private GameObject[] guns;      
    [SerializeField] private GameObject[] gunPickups;        
    [SerializeField] private GunUiManager gunUiManager;    
    [SerializeField] private GunPickUp gunPickupScript;        
    [SerializeField] private Transform dropPoint;             
    [SerializeField] private PlayerController playerMovement;   

    [Header("Drop Settings")]
    [SerializeField] private float dropForce = 10f;
    [SerializeField] private float dropRotationFactor = 0.5f;

    // Private state variables for tracking the current weapons
    private PlayerWeapon[] weaponScripts;
    public string PrimaryGun = "";
    public string SecondaryGun = "";
    public string MeleeGun = "Karambit"; // default melee weapon name
    public string currentGun = "";
    private GunInventoryType selectedSlot = GunInventoryType.Melee;  // default slot

    private void Start()
    {
        // Cache weapon scripts for each gun gameobject
        weaponScripts = new PlayerWeapon[guns.Length];
        for (int i = 0; i < guns.Length; i++)
        {
            weaponScripts[i] = guns[i].GetComponentInChildren<PlayerWeapon>();
        }
        // Initialize by deactivating all and activating the melee weapon
        DeactivateAllGuns();
        ActivateGun(MeleeGun, GunInventoryType.Melee);
        gunUiManager.UpdateWeaponSlot(GunInventoryType.Melee, MeleeGun);
    }

    private void Update()
    {
        // Only process input if this PhotonView is owned by us
        if (!photonView.IsMine)
            return;

        HandleInput();
    }
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && selectedSlot != GunInventoryType.Primary)
        {
            selectedSlot = GunInventoryType.Primary;
            UpdateHeldItem();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && selectedSlot != GunInventoryType.Secondary)
        {
            selectedSlot = GunInventoryType.Secondary;
            UpdateHeldItem();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && selectedSlot != GunInventoryType.Melee)
        {
            selectedSlot = GunInventoryType.Melee;
            UpdateHeldItem();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (selectedSlot == GunInventoryType.Primary || selectedSlot == GunInventoryType.Secondary)
            {
                DropGun();
            }
        }
    }
    private void UpdateHeldItem()
    {
        DeactivateAllGuns();

        switch (selectedSlot)
        {
            case GunInventoryType.Primary:
                if (!string.IsNullOrEmpty(PrimaryGun))
                {
                    ActivateGun(PrimaryGun, GunInventoryType.Primary);
                }
                else if (!string.IsNullOrEmpty(SecondaryGun))
                {
                    // Fallback to secondary if primary is empty
                    selectedSlot = GunInventoryType.Secondary;
                    ActivateGun(SecondaryGun, GunInventoryType.Secondary);
                }
                else
                {
                    // Fallback to melee if both are empty
                    selectedSlot = GunInventoryType.Melee;
                    ActivateGun(MeleeGun, GunInventoryType.Melee);
                }
                break;

            case GunInventoryType.Secondary:
                if (!string.IsNullOrEmpty(SecondaryGun))
                {
                    ActivateGun(SecondaryGun, GunInventoryType.Secondary);
                }
                else if (!string.IsNullOrEmpty(PrimaryGun))
                {
                    // Fallback to primary if secondary is empty
                    selectedSlot = GunInventoryType.Primary;
                    ActivateGun(PrimaryGun, GunInventoryType.Primary);
                }
                else
                {
                    selectedSlot = GunInventoryType.Melee;
                    ActivateGun(MeleeGun, GunInventoryType.Melee);
                }
                break;

            case GunInventoryType.Melee:
                ActivateGun(MeleeGun, GunInventoryType.Melee);
                break;
        }
    }

    public void pickupGun(string gunName, GunInventoryType type)
    {
        gunUiManager.UpdateWeaponSlot(type, gunName);
        switch (type)
        {
            case GunInventoryType.Primary:
                if (string.IsNullOrEmpty(PrimaryGun))
                    PrimaryGun = gunName;
                // Immediately update if the current slot is primary
                if (selectedSlot == GunInventoryType.Primary)
                    UpdateHeldItem();
                break;

            case GunInventoryType.Secondary:
                SecondaryGun = gunName;
                if (selectedSlot == GunInventoryType.Secondary)
                    UpdateHeldItem();
                break;

            case GunInventoryType.Melee:
                MeleeGun = gunName;
                if (selectedSlot == GunInventoryType.Melee)
                    UpdateHeldItem();
                break;
        }
    }

    private void ActivateGun(string gunName, GunInventoryType type)
    {
        if (currentGun == gunName)
            return;

        gunUiManager.HighlightSelectedWeapon(type);

        for (int i = 0; i < guns.Length; i++)
        {
            if (guns[i].name.Contains(gunName))
            {
                guns[i].SetActive(true);

                currentGun = guns[i].name.Replace("GunHolder", "");
            }
            else
            {
                guns[i].SetActive(false);
            }
        }
    }
    private void DeactivateAllGuns()
    {
        currentGun = "";
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].SetActive(false);
            weaponScripts[i]?.ForceReset();
        }
    }
    private void DropGun()
    {
        string gunToDrop = "";

        if (selectedSlot == GunInventoryType.Primary && !string.IsNullOrEmpty(PrimaryGun))
        {
            gunToDrop = PrimaryGun;
            PrimaryGun = "";
            gunUiManager.UpdateWeaponSlot(GunInventoryType.Primary, "");
        }
        else if (selectedSlot == GunInventoryType.Secondary && !string.IsNullOrEmpty(SecondaryGun))
        {
            gunToDrop = SecondaryGun;
            SecondaryGun = "";
            gunUiManager.UpdateWeaponSlot(GunInventoryType.Secondary, "");
        }
        else
        {
            Debug.Log("No gun to drop in the current slot.");
            return;
        }


        for (int i = 0; i < gunPickups.Length; i++)
        {
            if (gunPickups[i].name.Contains(gunToDrop))
            {
                GameObject droppedGun = PhotonNetwork.Instantiate(gunPickups[i].name, dropPoint.position, Quaternion.identity);
                Rigidbody rb = droppedGun.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.AddForce(transform.forward * dropForce, ForceMode.Impulse);
                    rb.AddForce(transform.up * dropForce * 0.65f, ForceMode.Impulse);
                    rb.AddForce(playerMovement.m_PlayerVelocity * 5.5f, ForceMode.Impulse);

                    Vector3 randomTorque = new Vector3(
                        Random.Range(0.2f, 0.5f),
                        Random.Range(-0.01f, 0.01f),
                        Random.Range(-0.01f, 0.01f)
                    ) * dropForce * dropRotationFactor;

                    rb.AddTorque(randomTorque, ForceMode.Impulse);
                }

                break;
            }
        }

        UpdateHeldItem();
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)selectedSlot);
            stream.SendNext(PrimaryGun);
            stream.SendNext(SecondaryGun);
            stream.SendNext(currentGun);
        }
        else
        {
            this.selectedSlot = (GunInventoryType)(int)stream.ReceiveNext();
            this.PrimaryGun = (string)stream.ReceiveNext();
            this.SecondaryGun = (string)stream.ReceiveNext();
            this.currentGun = (string)stream.ReceiveNext();
            UpdateHeldItem();
        }
    }
}