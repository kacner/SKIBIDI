using Photon.Pun.Demo.Cockpit;
using UnityEditor;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    [SerializeField] private Transform camera;
    private Vector3 IniPos;
    private Vector3 currentPos;
    private Vector3 targetpos;
    private Vector3 currentRotation;
    private Vector3 targetRotaion;
    [SerializeField] private float punch;
    [SerializeField] private float snap;
    [SerializeField] private float returnAmount;
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;
    [SerializeField] private float GunRotationOffset = 180;

    private void Start()
    {
        IniPos = transform.localPosition;
    }

    private void Update()
    {
        targetRotaion = Vector3.Lerp(targetRotaion, Vector3.zero, returnAmount * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotaion, snap * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation) * new Quaternion(0, GunRotationOffset, 0, 0);
        //camera.localRotation = Quaternion.Euler(currentRotation);
        back();
    }

    public void StartRecoil()
    {
        targetpos -= new Vector3(0, 0, punch);
        targetRotaion += new Vector3(recoilX, Random.RandomRange(-recoilY, recoilY), Random.RandomRange(-recoilZ, recoilZ));
    }

    void back()
    {
        targetpos = Vector3.Lerp(targetpos, IniPos, returnAmount * Time.deltaTime);
        currentPos = Vector3.Lerp(currentPos, targetpos, snap * Time.deltaTime);
        transform.localPosition = currentPos;
    }
}
