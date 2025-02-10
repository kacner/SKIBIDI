using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PlayerWeapon : MonoBehaviourPunCallbacks
{
    public GameObject round;

    // Shooting Setup
    public Transform firePoint;   // Where bullets originate
    public Camera playerCamera;   // The player's camera for aiming
    public GameObject hittingSparks;
    public LayerMask playerHitMask;


    // Weapon Stats
    public float raycastDistance = 100f;
    public float fireRate = 0.2f;
    public float reloadTime = 2f;
    public int maxAmmunition = 10;

    private int ammunitionAmount;
    private bool canShoot = true;
    private bool isReloading = false;

    private bool isShooting;
    private bool reloadInput;

    void Start()
    {
        ammunitionAmount = maxAmmunition;

        if (round == null)
            Debug.LogError("No bullet assigned!");

        if (firePoint == null)
            Debug.LogError("FirePoint is not assigned!");

        if (playerCamera == null)
            Debug.LogError("Player Camera is not assigned!");
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        HandleInput();

        if (isShooting && canShoot && !isReloading)
        {
            StartCoroutine(AutoFire());
        }

        if (reloadInput && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    void HandleInput()
    {
        isShooting = Input.GetKey(KeyCode.Mouse0); // Hold for full-auto
        reloadInput = Input.GetKeyDown(KeyCode.R); // Press R to reload
    }

    private IEnumerator AutoFire()
    {
        while (isShooting && canShoot && ammunitionAmount > 0 && !isReloading)
        {
            FireBullet();
            yield return new WaitForSeconds(fireRate);
        }
    }

    void FireBullet()
    {
        if (ammunitionAmount <= 0)
        {
            Debug.Log("Out of bullets! Reload needed.");
            return;
        }

        ammunitionAmount--;
        canShoot = false;

        Debug.Log($"Bullet Amount: {ammunitionAmount}");

        ShootRaycast();
        StartCoroutine(FireRateCooldown());
    }

    void ShootRaycast()
    {
        Ray cameraRay = playerCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, raycastDistance, playerHitMask))
            targetPoint = hitInfo.point;
        else
            targetPoint = cameraRay.origin + cameraRay.direction * raycastDistance;

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;

        if (Physics.Raycast(firePoint.position, shootDirection, out RaycastHit hit, raycastDistance, playerHitMask))
        {
            Debug.DrawLine(firePoint.position, hit.point, Color.red, 1f);
            PlayerHitDetect(hit.collider.gameObject);
        }
        else
        {
            Debug.DrawLine(firePoint.position, firePoint.position + shootDirection * raycastDistance, Color.yellow, 1f);
        }
    }

    void PlayerHitDetect(GameObject hitObject)
    {
        Debug.Log("Hit object: " + hitObject.name);
        photonView.RPC("RPC_InteractWithObject", RpcTarget.All, hitObject.name);
    }

    private IEnumerator FireRateCooldown()
    {
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);
        ammunitionAmount = maxAmmunition;
        isReloading = false;
        Debug.Log("Reload complete!");
    }
}