using Photon.Pun;
using UnityEngine;

public class ClientPlayerMovment : MonoBehaviourPunCallbacks
{
    [SerializeField] private Rigidbody rb;

    // User input
    private float input_Horizontal;
    private float input_Vertical;
    private bool input_Jump;
    private bool input_sliding;

    [Header("Movment")]
    public float walking_Speed = 5f; // Deflut Movment speed
    public float crouching_Speed = 2.5f; // when they are crouching around
    public float sliding_Speed = 3f;  // when sliding
    public Vector3 movment_Force;

    // saving speed values
    private float sliding_Speed_Index;
    private float player_Speed_Index;

    [Header("Jumping settings")]
    public float jumping_Force; // jumping hight
    private bool can_Jump; // allowing to jump

    // Jumping Raycast
    public float ray_Distens_J;

    public LayerMask ground_Layer;
    public Vector3 ray_Offset_Down_J;
    private RaycastHit ground_raycast_Hit_J;


    [Header("Camera settings")]
    public float camera_Sens = 2f; // speed of looking

    public Transform player_Camera; // PlayerCam

    private float x_Rotation = 0f;
    private float mouseX;
    private float mouseY;

    [Space]
    [Header("Debug")]
    public bool debug;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (!photonView.IsMine)
        {
            player_Camera.gameObject.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        player_Speed_Index = walking_Speed;
        sliding_Speed_Index = sliding_Speed;
    }


    void Update()
    {
        if (photonView.IsMine)
        {
            UserInput();
        }
    }

    void UserInput()
    {
        input_Horizontal = Input.GetAxis("Horizontal"); // A , D
        input_Vertical = Input.GetAxis("Vertical"); // W, S
        input_Jump = Input.GetKey(KeyCode.Space); 
        input_sliding = Input.GetKey(KeyCode.LeftControl);
    }

    void PlayerMovment()
    {
        // Calculate movement direction
        Vector3 movementForce = transform.right * input_Horizontal + transform.forward * input_Vertical;
        // Apply velocity
        rb.linearVelocity = movementForce * walking_Speed;
    }

    void ReadDebug(float Text) // Lessen consol Spam
    {
        if(debug)
            Debug.Log(Text);
    }
}
