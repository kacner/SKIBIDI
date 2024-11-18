using UnityEngine;
using Photon.Pun;


public class PlayerMovment : MonoBehaviourPunCallbacks

{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public Transform playerCamera;

    private float xRotation = 0f;

    void Start()
    {
        // Disable the camera for other players
        if (!photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            MovePlayer();
            LookAround();
        }
    }

    private void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
        playerCamera.transform.position = transform.position;
    }
}

