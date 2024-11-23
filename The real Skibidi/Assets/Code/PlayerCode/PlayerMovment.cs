using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    private Rigidbody rb;

    private float input_Horizontal;
    private float input_Vertical;
    private bool input_Jump;
    private bool input_Crouch;
    private bool input_Sprint;

    [Header("Movement")]
    public float walking_Speed = 5f;
    public float running_Speed = 7f;
    public float crouching_Speed = 2.5f;
    public float sliding_Multiplier = 5f;
    private bool hasPerformedSlide;
    [SerializeField] private float Slide_Cooldown = 1f;
    [SerializeField] private float Slide_Cooldown_Timer = 1f;

    private float original_Speed;
    private Vector3 moveDirection;

    public Transform orientation;

    [Header("Jumping")]
    public float jump_Force = 5f;
    public LayerMask groundLayer;
    public Vector3 ray_Offset_Down;
    public float ray_Distance = 1.2f;
    private bool isGrounded;

    private bool isCrouching;

    void Awake()
    {
        original_Speed = walking_Speed;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleInput();
            CheckGrounded();
            MovePlayer();
        }
    }

    void HandleInput()
    {
        input_Horizontal = Input.GetAxis("Horizontal");
        input_Vertical = Input.GetAxis("Vertical");
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

    void MovePlayer()
    {
        Slide_Cooldown_Timer -= Time.deltaTime;
        Slide_Cooldown_Timer = Mathf.Clamp(Slide_Cooldown_Timer, 0, Slide_Cooldown);

        moveDirection = orientation.forward * input_Vertical + orientation.right * input_Horizontal;
        if (input_Crouch && Slide_Cooldown_Timer <= 0)
        {
            StartCrouching();
        }
        else
        {
            StopCrouching();
        }

        float speed = isCrouching ? crouching_Speed : (input_Sprint ? running_Speed : walking_Speed);
        rb.AddForce(moveDirection.normalized * speed * Time.deltaTime, ForceMode.VelocityChange);

        if (input_Jump)
            Jump();
    }

    void StartCrouching()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            hasPerformedSlide = false;
            transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
            PerformSlideBoost();
        }
    }

    void StopCrouching()
    {
        if (isCrouching)
        {
            isCrouching = false;
            transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
            Slide_Cooldown_Timer = Slide_Cooldown;
        }
    }
    void PerformSlideBoost()
    {
        if (!hasPerformedSlide && moveDirection != Vector3.zero) // Only boost if there's movement
        {
            Vector3 boostedVelocity = rb.velocity * sliding_Multiplier;
            rb.velocity = new Vector3(boostedVelocity.x, rb.velocity.y, boostedVelocity.z); // Preserve vertical velocity
            hasPerformedSlide = true;
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jump_Force, rb.velocity.z);
        }
    }
}