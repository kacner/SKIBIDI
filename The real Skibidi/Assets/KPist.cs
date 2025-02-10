using Photon.Pun.UtilityScripts;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEditor;

public class KPist : MonoBehaviour
{


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Fire();
        }
    }



    public void Fire()
    {
    }
   

}