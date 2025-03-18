using Movement;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private bool isAutomatic = true;

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
    private List<GameObject> trails = new List<GameObject>();

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
        Debug.Log("Start");
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

        foreach (GameObject item in trails)
        {

            if (!PhotonNetwork.IsConnected)
                Destroy(item);
            else
                PhotonNetwork.Destroy(item);
        }
        trails.Clear();
    }

    void Update()
    {
        print("test");
        Debug.Log("test");
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
        print(isMouse0);
    }

    void HandleInput()
    {
        if (photonView.IsMine)
        {
            if (isAutomatic)
                isMouse0 = Input.GetKey(KeyCode.Mouse0);
            else
                isMouse0 = Input.GetKeyDown(KeyCode.Mouse0);

            reloadInput = Input.GetKeyDown(KeyCode.R);
        }
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
        if (photonView.IsMine)
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
    }


    private IEnumerator TrailAnimation(RaycastHit hit)
    {
        GameObject SpawnedTrail;
        if (!PhotonNetwork.IsConnected)
            SpawnedTrail = Instantiate(trail, firePoint.position, Quaternion.identity);
        else
            SpawnedTrail = PhotonNetwork.Instantiate(trail.name, firePoint.position, Quaternion.identity);

        trails.Add(SpawnedTrail);

        float timer = 0;
        float duration = 0.1f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            SpawnedTrail.transform.position = Vector3.Lerp(SpawnedTrail.transform.position, hit.point, timer / duration);
            yield return null;
        }
        StartCoroutine(HitSpark(hit.point, hit.normal, hit.collider));

        if (!PhotonNetwork.IsConnected)
            Destroy(SpawnedTrail);
        else
            PhotonNetwork.Destroy(SpawnedTrail);
    }
    void ShootRaycast()
    {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

            Ray ray = playerCamera.ScreenPointToRay(screenCenter);
            Vector3 shootDirection = ray.direction;

            if (playerController.m_MoveInput.x != 0 || playerController.m_MoveInput.y != 0)
            {
                float bloomAngle = bloomAngleMaxAmout * Mathf.Min(Mathf.Abs(playerController.m_PlayerVelocity.magnitude) / 2, MaxVelocityForBloom);
                Quaternion bloomRotation = Quaternion.Euler(
                    Random.Range(-bloomAngle, bloomAngle),
                    Random.Range(-bloomAngle, bloomAngle),
                    0
                );
                shootDirection = bloomRotation * shootDirection;
            }

            if (Physics.Raycast(playerCamera.transform.position, shootDirection, out RaycastHit hit, raycastDistance, playerHitMask))
            {
                Debug.DrawLine(playerCamera.transform.position, hit.point, Color.red, 1f);

                StartCoroutine(TrailAnimation(hit));
            }
            else
            {
                Vector3 endPoint = playerCamera.transform.position + shootDirection * raycastDistance;
                Debug.DrawLine(playerCamera.transform.position, endPoint, Color.yellow, 1f);

                RaycastHit fakeHit = new RaycastHit();
                fakeHit.point = endPoint;
                fakeHit.normal = -shootDirection;
                StartCoroutine(TrailAnimation(fakeHit));
            }

            recoil.StartRecoil();
            weaponRecoil.AddRecoil();
            camerashake.StartShake();
            if (animation != null)
                animation.Play();
    }

    IEnumerator HitSpark(Vector3 pos, Vector3 normal, Collider hit)
    {
        Quaternion rotation = Quaternion.LookRotation(normal);

        GameObject spark;

        if (!PhotonNetwork.IsConnected)
            spark = Instantiate(hittingSparks, pos, rotation);
        else
            spark = PhotonNetwork.Instantiate(hittingSparks.name, pos, rotation);

        CreateBulletHole(pos, rotation, hit);

        yield return new WaitForSeconds(5f);
        
        if (!PhotonNetwork.IsConnected)
            Destroy(spark);
        else
            PhotonNetwork.Destroy(spark);
    }

    private void CreateBulletHole(Vector3 pos, Quaternion rotation, Collider hit)
    {
        Vector3 adjustedPosition = pos + rotation * Vector3.forward * 0.01f;

        GameObject decal;

        if (!PhotonNetwork.IsConnected)
            decal = Instantiate(bulletHolePrefab, adjustedPosition, rotation);
        else
            decal = PhotonNetwork.Instantiate(bulletHolePrefab.name, adjustedPosition, rotation);
            
        decal.transform.parent = hit.transform;

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