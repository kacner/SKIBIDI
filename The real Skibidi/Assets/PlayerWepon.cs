using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerWepon : MonoBehaviourPunCallbacks
{

    public GameObject round;
    [SerializeField] public Transform firePoint;

    public float fireRate;
    public float reloadTime;
    public int maxAmmunition;
    public bool canShoot;
    private int ammunitionAmout;

    public bool Shooting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ammunitionAmout = maxAmmunition;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            Shooting = Input.GetKeyDown(KeyCode.Mouse0);

            if (Shooting)
            {
                Debug.Log("player pressing mouse down");
                Firebullet();
            }
        }
    }

    void Firebullet()
    {
        if (ammunitionAmout <= 0)
        {
            canShoot = false;
            if(Input.GetKey(KeyCode.R))
                StartCoroutine(ReloadTime(reloadTime));
        }
        else
        {
            canShoot = true;
            StartCoroutine(Firerate(fireRate));
            canShoot = false;
            Debug.Log($"Bullet Amout : {ammunitionAmout}");
        }

        if(canShoot)
            Instantiate(round, firePoint.position, firePoint.rotation);

    }

    private IEnumerator Firerate(float rate)
    {
        canShoot = false;
        Debug.Log("Starting Firerate coroutine...");
        yield return new WaitForSeconds(rate); // Simulates fire rate delay
        Debug.Log("Firerate coroutine finished.");
    }

    private IEnumerator ReloadTime(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime); // Simulates fire rate delay
        ammunitionAmout = maxAmmunition;
    }
}

