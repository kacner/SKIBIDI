using Movement;
using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PlayerWeapon : MonoBehaviourPunCallbacks
{
    [Header("Setup")]
    [SerializeField] public PlayerController playerController;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject hittingSparks;
    [SerializeField] private LayerMask playerHitMask;

    [Space]

    [Header("Weapond Settings")]
    [SerializeField] private float raycastDistance;
    [SerializeField] private float fireRate;
    [SerializeField] private float reloadTime;
    [SerializeField] private int maxAmmunition;
    [SerializeField] public float bloomAngleMaxAmout;

    private int ammunitionAmount;
    private bool canShoot = true;
    private bool isReloading = false;
    private bool isShooting;
    private bool reloadInput;

    [Space]

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleflash;
    [SerializeField] private Light light;
    [SerializeField] private GameObject trail;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private Sprite[] BulletHoles;
    private Recoil recoil;
    private CameraShake camerashake;
    private Animation animation;
    void Start()
    {
        ammunitionAmount = maxAmmunition;

        recoil = GetComponent<Recoil>();
        camerashake = GetComponent<CameraShake>();
        animation = GetComponent<Animation>();
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
        /*if (ammunitionAmount <= 0)
        {
            Debug.Log("Out of bullets! Reload needed.");
            return;
        }*/

        ammunitionAmount--;
        canShoot = false;

        muzzleflash.Play();
        StartCoroutine(Light());

        ShootRaycast();
        StartCoroutine(FireRateCooldown());
    }
    IEnumerator Light()
    {
        light.enabled = true;
        yield return new WaitForSeconds(0.1f);
        light.enabled = false;
    }


    private IEnumerator TrailAnimation(RaycastHit hit)
    {
        GameObject SpawnedTrail = Instantiate(trail, firePoint.position, Quaternion.identity);

        float timer = 0;
        float duration = 0.1f;
       
        while (timer < duration)
        {
            timer += Time.deltaTime;
            SpawnedTrail.transform.position = Vector3.Lerp(SpawnedTrail.transform.position, hit.point, timer / duration);
            yield return null;
        }
        StartCoroutine(HitSpark(hit.point, hit.normal));
        Destroy(SpawnedTrail);
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

        // Bloom Recoil - Adds inaccuracy based on movement
        float bloomAngle = 0f; // Default no bloom
        if (playerController.m_MoveInput.x != 0 || playerController.m_MoveInput.y != 0)
        {
            bloomAngle = bloomAngleMaxAmout * playerController.m_PlayerVelocity.magnitude / 2; // Increase bloom when moving
            Debug.LogWarning(bloomAngle);

            bloomAngle += Random.Range(-bloomAngle, bloomAngle); // Randomized bloom effect
            Quaternion bloomRotation = Quaternion.Euler(Random.Range(-bloomAngle, bloomAngle), 0, Random.Range(-bloomAngle, bloomAngle));
            shootDirection = bloomRotation * shootDirection; // Apply bloom
        }
        else
        {
            // add bloom to standing
        }


        if (Physics.Raycast(firePoint.position, shootDirection, out RaycastHit hit, raycastDistance, playerHitMask))
        {
            Debug.DrawLine(firePoint.position, hit.point, Color.red, 1f);
        }
        else
        {
            Debug.DrawLine(firePoint.position, firePoint.position + shootDirection * raycastDistance, Color.yellow, 1f);
        }

        StartCoroutine(TrailAnimation(hit));
        recoil.StartRecoil();
        camerashake.StartShake();
        animation.Play();
    }


    IEnumerator HitSpark(Vector3 pos, Vector3 normal)
    {
        Quaternion rotation = Quaternion.LookRotation(normal);
        GameObject spark = Instantiate(hittingSparks, pos, rotation);

        CreateBulletHole(pos, rotation);

        yield return new WaitForSeconds(0.1f);
        Destroy(spark);
    }

    private void CreateBulletHole(Vector3 pos, Quaternion rotation)
    {
        Vector3 adjustedPosition = pos + rotation * Vector3.forward * 0.01f;
        GameObject decal = Instantiate(bulletHolePrefab, adjustedPosition, rotation);

        SpriteRenderer spriteRenderer = decal.GetComponent<SpriteRenderer>();

        decal.transform.rotation = Quaternion.Euler(decal.transform.rotation.eulerAngles.x, decal.transform.rotation.eulerAngles.y, UnityEngine.Random.RandomRange(-360f, 360f));

        int rnd = UnityEngine.Random.RandomRange(0, BulletHoles.Length);
        spriteRenderer.sprite = BulletHoles[rnd];
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