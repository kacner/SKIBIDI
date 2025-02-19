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

    void RotateSway()
    {
        float tiltY = Input.GetAxis("Mouse X") * TiltStrenght;
        float tiltX = Input.GetAxis("Mouse Y") * TiltStrenght;
        tiltX = Mathf.Clamp(tiltX, -maxTilt, maxTilt);
        tiltY = Mathf.Clamp(tiltY, -maxTilt, maxTilt);
        Quaternion finalRot = Quaternion.Euler(new Vector3(tiltDirX ? -tiltX : 0, tiltDirY ? tiltY : 0, tiltDirZ ? tiltY : 0));
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRot * iniRotation, smoothTiltStr * Time.deltaTime);
    }

}
