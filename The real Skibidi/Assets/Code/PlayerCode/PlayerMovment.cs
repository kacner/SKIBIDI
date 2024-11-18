using UnityEngine;
using Photon.Pun;

public class PlayerMovment : MonoBehaviourPunCallbacks
{
    [SerializeField] private Rigidbody rb;
    public float moveSpeed = 5f;
    public float swayMovement;
    public float lookSpeed = 2f;
    public Transform playerCamera;

    private float x_Rotation = 0f;

    private float input_Horizontal;
    private float input_Veritcal;
    private bool input_Jump;

    public bool can_Jump;
    public float jumping_Force;

    public Vector3 ray_Offset_Down;
    public float ray_Distens;
    public LayerMask groundLayer; // Add this for layer masking

    void Awake()
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

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            UserInput();
            JumpingPlayer();
            MovePlayer();
            LookAround();
        }
    }

    void UserInput()
    {
        input_Horizontal = Input.GetAxis("Horizontal");
        input_Veritcal = Input.GetAxis("Vertical");
        input_Jump = Input.GetKey(KeyCode.Space);
    }

    private void MovePlayer()
    {
        Vector3 move = transform.right * input_Horizontal + transform.forward * input_Veritcal;
        transform.position += move * moveSpeed * Time.deltaTime;

        // rb.linearVelocity = new Vector3( Mathf.Lerp(rb.linearVelocity.x, input_Horizontal * moveSpeed, Time.deltaTime * swayMovement),rb.linearVelocity.y, Mathf.Lerp(rb.linearVelocity.z, input_Veritcal * moveSpeed, Time.deltaTime * swayMovement));


        if (can_Jump && input_Jump)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumping_Force, rb.linearVelocity.z);
            can_Jump = false;
            Debug.Log("DEBUG : player jumping");
        }
    }

    private void JumpingPlayer()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + ray_Offset_Down;
        Vector3 direction = Vector3.down;

        Debug.DrawRay(origin, direction * ray_Distens, Color.red);

        if (Physics.Raycast(origin, direction, out hit, ray_Distens, groundLayer))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                can_Jump = true;
                Debug.Log("DEBUG : can jump");
            }
            else
            {
                can_Jump = false;
                Debug.Log("DEBUG : cant jump");
            }
        }
        else
        {
            Debug.Log("DEBUG : cant jump");
        }
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        x_Rotation -= mouseY;
        x_Rotation = Mathf.Clamp(x_Rotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(x_Rotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
        playerCamera.transform.position = transform.position;
    }

}
