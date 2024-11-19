using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Threading;

public class PlayerMovment : MonoBehaviourPunCallbacks
{
    [SerializeField] private Rigidbody rb;

    // all inputs need to be put here for ablility to use outside this scrypt
    private float input_Horizontal;
    private float input_Veritcal;
    private bool input_Jump;
    private bool input_sliding;

    [Header("Movment")]
    public float walking_Speed = 5f;
    public float crouching_Speed = 2.5f;
    private bool can_Slid;
    public float sliding_Force = 3f; // Force applied during the dash
    public Vector3 movment_Force;
    private float sliding_Speed_Index;
    private float player_Speed_Index;

    [Header("Jumping settings")]
    public bool can_Jump;
    public float jumping_Force;

    public LayerMask groundLayer; // Add this for layer masking
    public Vector3 ray_Offset_Down;
    public float ray_Distens;

    [Header("Camera settings")]
    public float lookSpeed = 2f;
    public Transform playerCamera;
    private float x_Rotation = 0f;
    private float mouseX;
    private float mouseY;
    

    void Awake()
    {
        player_Speed_Index = walking_Speed;
        sliding_Speed_Index = sliding_Force;

        sliding_Force = sliding_Force + walking_Speed;

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
        input_sliding = Input.GetKey(KeyCode.LeftControl);
    }

    private void MovePlayer()
    {
        movment_Force = transform.right * input_Horizontal + transform.forward * input_Veritcal;

        if (input_sliding != true)
        {
            transform.position += movment_Force * walking_Speed * Time.deltaTime;
            Debug.Log($"players is walking");
        }

        if (input_sliding)
        {
            if (movment_Force.x == 0 && movment_Force.z == 0)
                can_Slid = false;
            else
                can_Slid = true;
            Crouching(can_Slid);
        }
        else // if there is no longer sliding input
        {
            walking_Speed = player_Speed_Index;
            sliding_Force = sliding_Speed_Index;
            Debug.Log($"move_Speed > {walking_Speed}");
            Debug.Log($"dashForce > {sliding_Force}");
        }
           
        if (can_Jump && input_Jump) // if it found ground
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumping_Force, rb.linearVelocity.z); // jumping force
            can_Jump = false; // one jump
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
                can_Jump = true; // if it found ground
            else
                can_Jump = false; // if there is no ground
        }
    }

    private void Crouching(bool slidingPerms)
    {
        if (sliding_Force < walking_Speed) // this is the main crouching system that works
        {
            transform.position += movment_Force * crouching_Speed * Time.deltaTime;
            slidingPerms = false;
        }

        if (slidingPerms) // this needs to be work on due to it wont go down on its own
        {
            transform.position += movment_Force * sliding_Force * Time.deltaTime;
            sliding_Force = sliding_Force + walking_Speed - Time.deltaTime;
            Debug.Log("Sliding force " + sliding_Force);
        }
    }

    private void LookAround()
    {
        mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        x_Rotation -= mouseY;
        x_Rotation = Mathf.Clamp(x_Rotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(x_Rotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Sync camera position with player position
        playerCamera.transform.position = transform.position;
    }


}
