using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    private Rigidbody rb;

    public float input_Horizontal;
    public float input_Vertical;
    private bool input_Jump;
    private bool input_Crouch;
    private bool input_Sprint;

    [Header("Movement")]
    public float walking_Speed = 5f;
    public float running_Speed = 7f;
    public float crouching_Speed = 2.5f;
    public float sliding_Speed;
    public float OG_sliding_Speed = 5f;
    [SerializeField] private float Slide_Cooldown = 1f;
    [SerializeField] private float Slide_Cooldown_Timer = 1f;
    [SerializeField] private Vector3 moveDirection;
    public float fall_GravityMultiplier = 2.5f;
    public Transform orientation;
    private bool hasPreformedSlideBoost = false;

    [Header("Jumping")]
    public float jump_Force = 5f;
    public LayerMask groundLayer;
    public Vector3 ray_Offset_Down;
    public float ray_Distance = 1.2f;
    [SerializeField] private bool isGrounded;
    public Vector3 ray_Offset_Up;
    public float ray_Distance_up = 1.2f;
    [SerializeField] private bool isRoofied;

    private bool isCrouching;

    [Header("Username")]
    public TextMeshPro UsernameTextObj;

    void Awake()
    {
        sliding_Speed = OG_sliding_Speed;
        rb = GetComponent<Rigidbody>();

        UsernameTextObj.text = PhotonNetwork.NickName;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleInput();
            CheckGrounded();
            MovePlayer();
            CheckRoofied();
        }
    }

    void HandleInput()
    {
        input_Horizontal = Input.GetAxisRaw("Horizontal");
        input_Vertical = Input.GetAxisRaw("Vertical");
        input_Jump = Input.GetKeyDown(KeyCode.Space);
        input_Crouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        input_Sprint = Input.GetKey(KeyCode.LeftShift);
    }

    void CheckGrounded()
    {
        Vector3 origin = transform.position + ray_Offset_Down * transform.localScale.y;
        Vector3 direction = Vector3.down;

        isGrounded = Physics.Raycast(origin, direction, ray_Distance, groundLayer);
        Debug.DrawRay(origin, direction * ray_Distance, isGrounded ? Color.green : Color.red);
    }
    void CheckRoofied()
    {
        isRoofied = Physics.Raycast(transform.position + ray_Offset_Up * transform.localScale.y, Vector3.up, ray_Distance_up, groundLayer);
        Debug.DrawRay(transform.position + ray_Offset_Up * transform.localScale.y, Vector3.up * ray_Distance, isGrounded ? Color.green : Color.red);
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (!isGrounded)
            {
                if (rb.velocity.y < 0)
                {
                    // Apply stronger gravity when falling
                    rb.AddForce(Vector3.down * fall_GravityMultiplier, ForceMode.Acceleration);
                }
                else
                {
                    // Default gravity
                    rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
                }
            }
        }
    }

    void MovePlayer()
    {
        Slide_Cooldown_Timer -= Time.deltaTime;
        Slide_Cooldown_Timer = Mathf.Clamp(Slide_Cooldown_Timer, 0, Slide_Cooldown);


        if (isGrounded)
        {
            moveDirection = orientation.forward * input_Vertical + orientation.right * input_Horizontal;
        }

        float speed = isCrouching ? crouching_Speed : (input_Sprint ? running_Speed : walking_Speed);

        if (input_Crouch)
        {
            StartCrouching();
        }
        else
        {
            StopCrouching();
        }

        rb.AddForce(moveDirection.normalized * speed * Time.deltaTime, ForceMode.VelocityChange);

        if (input_Jump)
            Jump();
    }

    void StartCrouching()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
            if (Slide_Cooldown_Timer <= 0 && isGrounded)
            {
                PerformSlideBoost();
                sliding_Speed = OG_sliding_Speed;
                Slide_Cooldown_Timer = Slide_Cooldown;
            }
        }
    }

    void StopCrouching()
    {
        if (isCrouching && !isRoofied)
        {
            isCrouching = false;
            transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
            hasPreformedSlideBoost = false;
        }
    }
    void PerformSlideBoost()
    {
        if (moveDirection != Vector3.zero && !hasPreformedSlideBoost)
        {
            rb.AddForce(moveDirection * sliding_Speed, ForceMode.Force);
            print("Boosted");
            hasPreformedSlideBoost = true;
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jump_Force, rb.linearVelocity.z);
            //StopCrouching();
        }
    }
}