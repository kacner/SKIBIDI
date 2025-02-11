using Photon.Pun.Demo.Cockpit;
using UnityEditor;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    [SerializeField] private Transform camera;
    private Vector3 IniPos;
    private Vector3 IniRotation;
    private Vector3 currentPos;
    private Vector3 targetpos;
    private Vector3 currentRotation;
    private Vector3 targetRotaion;
    [SerializeField] float punch;
    [SerializeField] private float snap;
    [SerializeField] private float returnAmount;
    [SerializeField] float recoilX;
    [SerializeField] float recoilY;
    [SerializeField] float recoilZ;

    private void Start()
    {
        IniPos = transform.localPosition;
    }

    private void Update()
    {
        targetRotaion = Vector3.Lerp(targetRotaion, Vector3.zero, returnAmount * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotaion, snap * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation) * new Quaternion(0, 180, 0, 0);
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
