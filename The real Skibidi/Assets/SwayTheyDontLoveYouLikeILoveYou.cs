using Photon.Pun;
using UnityEngine;

public class SwayTheyDontLoveYouLikeILoveYou : MonoBehaviourPunCallbacks
{
    [SerializeField] private float strenght;
    [SerializeField] private float maxSWAY;
    [SerializeField] private float Smoothing;

    [SerializeField] private float TiltStrenght;
    [SerializeField] private float maxTilt;
    [SerializeField] private float smoothTiltStr;
    [SerializeField] private bool tiltDirX, tiltDirY, tiltDirZ;
    Vector3 iniPos;
    Quaternion iniRotation;

    [Header("Bobbing Settings")]
    [SerializeField] private float bobbingSpeed = 5f;
    [SerializeField] private float bobbingAmount = 0.005f;
    private float bobTimer = 0;
    private void Start()
    {
        if (photonView.IsMine)
        {
            iniRotation = transform.localRotation;
            iniPos = transform.localPosition;
        }
    }
    private void Update()
    {
        if (photonView.IsMine)
        {
            Sway();
            RotateSway();
            ApplyBobbing();
        }
    }
    void Sway()
    {
        float moveX = Input.GetAxis("Mouse X") * strenght;
        float moveY = Input.GetAxis("Mouse Y") * strenght;
        moveX = Mathf.Clamp(moveX, -maxSWAY, maxSWAY);
        Vector3 finalPos = new Vector3(moveX, 0, moveY);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + iniPos, Smoothing * Time.deltaTime);
    }
    void GunWalk()
    {
        float YDisplacement = Mathf.Sin(1);
        transform.localPosition = new Vector3(transform.localPosition.x, YDisplacement, transform.localPosition.z);
    }
    void RotateSway()
    {
        float tiltY = Input.GetAxis("Mouse X") * TiltStrenght;
        float tiltX = Input.GetAxis("Mouse Y") * TiltStrenght;
        tiltX = Mathf.Clamp(tiltX, -maxTilt, maxTilt);
        tiltY = Mathf.Clamp(tiltY, -maxTilt, maxTilt);
        Quaternion finalRot = Quaternion.Euler(new Vector3(tiltDirX ? -tiltX : 0, tiltDirY ? tiltY : 0, tiltDirZ ? tiltY : 0));
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRot * iniRotation, smoothTiltStr * Time.deltaTime);
    }
    void ApplyBobbing()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
        {
            bobTimer += Time.deltaTime * bobbingSpeed;
            float bobOffset = Mathf.Sin(bobTimer) * bobbingAmount;
            Vector3 bobbingPos = new Vector3(0, bobOffset, 0);
            transform.localPosition += bobbingPos;
        }
        else
        {
            bobTimer = 0f;
        }
    }
}
