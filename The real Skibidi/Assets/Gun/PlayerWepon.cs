using Movement;
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerWeapon : MonoBehaviourPunCallbacks
{
    [Header("Setup")]
    [SerializeField] private Transform firePoint;
    public Camera playerCamera;
    [SerializeField] private GameObject hittingSparks;
    [SerializeField] private LayerMask playerHitMask;
    [SerializeField] private TextMeshProUGUI ammoText;

    [Space]

    [Header("Weapond Settings")]
    [SerializeField] private float raycastDistance;
    [SerializeField] private float fireRate;
    [SerializeField] private float reloadTime;
    [SerializeField] private int maxAmmunition;
    [SerializeField] private int ammunitionAmount;

    [Space]

    [Header("Bloom")]
    [SerializeField] public float bloomAngleMaxAmout;
    [SerializeField] public float MaxVelocityForBloom;

    private bool canShoot = true;
    private bool isReloading = false;
    private bool isMouse0;
    private bool reloadInput;
    private PlayerController playerController;
    private WeaponRecoil weaponRecoil;

    [Space]

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleflash;
    [SerializeField] private Light light;
    [SerializeField] private GameObject trail;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private Sprite[] BulletHoles;
    private VisualRecoil recoil;
    private CameraShake camerashake;
    private Animation animation;

    void Start()
    {
        ammunitionAmount = maxAmmunition;
        recoil = GetComponent<VisualRecoil>();
        camerashake = GetComponent<CameraShake>();
        animation = GetComponent<Animation>();
        playerController = GetComponentInParent<PlayerController>();
        weaponRecoil = GetComponent<WeaponRecoil>();
    }
    public void ForceReset()
    {
        canShoot = true;
        isReloading = false;
        muzzleflash.Stop();
        light.enabled = false;
        
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        HandleInput();

        if (isMouse0 && canShoot && !isReloading)
        {
            StartCoroutine(AutoFire());
        }

        if (reloadInput && !isReloading && !isMouse0 && ammunitionAmount < maxAmmunition)
        {
            StartCoroutine(Reload());
        }
        if (ammunitionAmount == 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    void HandleInput()
    {
        isMouse0 = Input.GetKey(KeyCode.Mouse0);
        reloadInput = Input.GetKeyDown(KeyCode.R);
    }

    private IEnumerator AutoFire()
    {
        while (isMouse0 && canShoot && ammunitionAmount > 0 && !isReloading)
        {
            FireBullet();
            yield return new WaitForSeconds(fireRate);
        }
    }

    void FireBullet()
    {
        ammunitionAmount--;
        updateAmmoText();
        canShoot = false;

        muzzleflash.Play();
        StartCoroutine(Light());

        weaponRecoil.AddRecoil();

        ShootRaycast();
        StartCoroutine(FireRateCooldown());
    }
    IEnumerator Light()
    {
        light.enabled = true;
        yield return new WaitForSeconds(0.1f);
        light.enabled = false;
    }

    void updateAmmoText()
    {
        string AmmoCount = "";
        if (ammunitionAmount < 6)
        {
            for (int i = 0; i < ammunitionAmount; i++)
            {
                AmmoCount += 'l';
            }
        }
        else
        {
            AmmoCount = "lllll";
        }
        ammoText.text = $"{ammunitionAmount} / {maxAmmunition}  {AmmoCount}";
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
        Vector3 shootDirection = playerCamera.transform.forward;

        if (playerController.m_MoveInput.x != 0 || playerController.m_MoveInput.y != 0)
        {
            float bloomAngle = bloomAngleMaxAmout * Mathf.Min(playerController.m_PlayerVelocity.magnitude / 2, MaxVelocityForBloom);
            Quaternion bloomRotation = Quaternion.Euler(
                Random.Range(-bloomAngle, bloomAngle),
                Random.Range(-bloomAngle, bloomAngle),
                0
            );
            shootDirection = bloomRotation * shootDirection;
        }

        if (Physics.Raycast(firePoint.position, shootDirection, out RaycastHit hit, raycastDistance, playerHitMask))
        {
            Debug.DrawLine(firePoint.position, hit.point, Color.red, 1f);
            StartCoroutine(TrailAnimation(hit));
        }
        else
        {
            Vector3 endPoint = firePoint.position + shootDirection * raycastDistance;
            Debug.DrawLine(firePoint.position, endPoint, Color.yellow, 1f);

            RaycastHit fakeHit = new RaycastHit();
            fakeHit.point = endPoint;
            fakeHit.normal = -shootDirection;
            StartCoroutine(TrailAnimation(fakeHit));
        }

        recoil.StartRecoil();
        weaponRecoil.AddRecoil();
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

        decal.transform.rotation = Quaternion.Euler(decal.transform.rotation.eulerAngles.x, decal.transform.rotation.eulerAngles.y, Random.RandomRange(-360f, 360f));

        int rnd = Random.RandomRange(0, BulletHoles.Length);
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
        weaponRecoil.ResetRecoil();
        ammunitionAmount = maxAmmunition;
        isReloading = false;
        Debug.Log("Reload complete!");
        updateAmmoText();
    }
}