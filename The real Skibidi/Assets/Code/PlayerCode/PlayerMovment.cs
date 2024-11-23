using UnityEngine;
using Photon.Pun;

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
    public float Running_Speed = 5f;
    public float crouching_Speed = 2.5f;
    public float sliding_Speed = 3f; // Force applied during the dash
    private float OG_sliding_Speed;
    private float OG_player_Speed;
    private bool can_Slid;
    private bool can_Slid_Called;
    public Vector3 movement_Force;
    private bool isSprinting;
    public Vector3 MoveDir;

    public Transform orientation;

    [Header("Jumping settings")]
    public bool can_Jump;
    public float jumping_Force;

    public LayerMask groundLayer; // Add this for layer masking
    public Vector3 ray_Offset_Down;
    public float ray_Distens;
    private RaycastHit ground_raycast_Hit;


    void Awake()
    {
        OG_player_Speed = walking_Speed;
        OG_sliding_Speed = sliding_Speed;

        sliding_Speed = sliding_Speed + walking_Speed;

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            UserInput();
            MovePlayer();
            JumpingPlayer();
        }
    }

    void UserInput()
    {
        input_Horizontal = Input.GetAxis("Horizontal");
        input_Veritcal = Input.GetAxis("Vertical");
        input_Jump = Input.GetKey(KeyCode.Space);
        input_sliding = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        isSprinting = Input.GetKey(KeyCode.LeftShift);
    }

    /*void MovePlayer()
    {
        movment_Force = transform.right * input_Horizontal + transform.forward * input_Veritcal;
        transform.position += movment_Force * walking_Speed * Time.deltaTime;
        if (input_sliding != true)
        {
            transform.position += movment_Force * walking_Speed * Time.deltaTime;
            Debug.Log($"players is walking");
        }

        if (input_sliding)
        {
            if (movment_Force.x == 0 && movment_Force.z == 0 && can_Slid_Called != true)
            {
                can_Slid_Called = true;
                can_Slid = false;
            }
            else if (movment_Force.x != 0 && movment_Force.z != 0 && can_Slid_Called != true)
                can_Slid = true;

            Crouching(can_Slid);
            transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
        }
        else // if there is no longer sliding input
        {
            walking_Speed = player_Speed_Index;
            sliding_Force = sliding_Speed_Index;
            transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
            can_Slid_Called = false;
        }

        if (can_Jump && input_Jump) // if it found ground
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumping_Force, rb.linearVelocity.z); // jumping force
            can_Jump = false; // one jump
        }
    }*/

    void MovePlayer()
    {
        MoveDir = orientation.forward * input_Veritcal + orientation.right * input_Horizontal;

        if (!input_sliding)
        {
            if (isSprinting)
            {
                print("Running");
                rb.AddForce(MoveDir * Running_Speed * Time.deltaTime, ForceMode.Force);
            }
            else
            {
                print("Walking");
                rb.AddForce(MoveDir * walking_Speed * Time.deltaTime, ForceMode.Force);
            }
        }
        else
        {

            if (MoveDir == Vector3.zero && !can_Slid_Called) //ignore canslid called
            {
                can_Slid_Called = true; //ignore this
                can_Slid = false;
            }
            else if (MoveDir != Vector3.zero && !can_Slid_Called) //ignore canslide called
                can_Slid = true;

            Crouching(can_Slid);
            transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
        }
    }

    void JumpingPlayer()
    {
        Vector3 origin = transform.position + ray_Offset_Down * transform.localScale.y + Vector3.up * 0.1f;
        Vector3 direction = Vector3.down;

        // Perform the raycast
        bool raycolider = Physics.Raycast(origin, direction, out ground_raycast_Hit, ray_Distens);

        if (raycolider)
        {
            Debug.DrawRay(origin, direction * ray_Distens, Color.green);
            Debug.Log($"DEBUG JPC : Raycast hit object - Name: {ground_raycast_Hit.collider.name}, Tag: {ground_raycast_Hit.collider.tag}");

            if (ground_raycast_Hit.collider.CompareTag("Ground"))
            {
                can_Jump = true;
                Debug.Log("DEBUG JPC : Raycast hit the ground.");
            }
            else
            {
                can_Jump = false;
                Debug.Log("DEBUG JPC : Raycast did not hit the ground.");
            }
        }
        else
        {
            can_Jump = false;
            Debug.DrawRay(origin, direction * ray_Distens, Color.red);
            Debug.LogWarning("DEBUG JPC : Raycast did not hit anything.");
        }
    }


    void Crouching(bool isSlidingAllowed)
    {
        if (!isSlidingAllowed)
            rb.AddForce(MoveDir * crouching_Speed * Time.deltaTime, ForceMode.Force); //crouching without sliding

        if (sliding_Speed < walking_Speed) // this is the main crouching system that works
        {
            rb.AddForce(MoveDir * crouching_Speed * Time.deltaTime, ForceMode.Force); //crouching without sliding
            isSlidingAllowed = false;
        }

        if (isSlidingAllowed) // this needs to be work on due to it wont go down on its own
        {
            rb.AddForce(MoveDir * sliding_Speed * Time.deltaTime, ForceMode.Force); //sliding
            sliding_Speed -= Time.deltaTime * 5; // replace latter with animashon curve
        }
    }
}