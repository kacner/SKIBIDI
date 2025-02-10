using Photon.Pun.UtilityScripts;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEditor;

public class KPist : MonoBehaviour
{
    [SerializeField] private ParticleSystem muzzleflash;
    [SerializeField] private ParticleSystem muzzleSpark;
    [SerializeField] private ParticleSystem muzzlesmoke;
    [SerializeField] private Light light;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Fire();
        }
    }



    public void Fire()
    {
        muzzleflash.Play();
        muzzleSpark.Play();
        muzzlesmoke.Play();
        StartCoroutine(Light());
    }
    IEnumerator Light()
    {
        light.enabled = true;
        yield return new WaitForSeconds(0.1f);
        light.enabled = false;
    }

}