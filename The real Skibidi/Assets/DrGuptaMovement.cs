using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;

public class DrGuptaMovement : MonoBehaviourPunCallbacks
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
        //UsernameTextObj.text = PhotonNetwork.NickName;
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

        Vector3 currentVelocity = rb.velocity;

        // Handle crouching behavior
        if (input_Crouch)
        {
            StartCrouching();
        }
        else
        {
            StopCrouching();
        }

        // Calculate movement input
        Vector2 input = new Vector2(input_Horizontal, input_Vertical);

        // Calculate friction to adjust velocity
        currentVelocity = CalculateFriction(currentVelocity);

        // Calculate movement and apply velocity based on input
        currentVelocity = CalculateMovement(input, currentVelocity);

        // Apply the calculated velocity to the Rigidbody
        rb.velocity = currentVelocity;

        // Jumping logic
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
            hasPreformedSlideBoost = true;
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            Vector3 jumpVelocity = CalculateJumpVelocity(rb.velocity.y);
            rb.velocity = new Vector3(rb.velocity.x, jumpVelocity.y, rb.velocity.z);
        }
    }

    private Vector3 CalculateFriction(Vector3 currentVelocity)
    {
        float speed = currentVelocity.magnitude;

        if (!isGrounded || input_Jump || speed == 0f)
            return currentVelocity;

        float drop = speed * 0.1f * Time.deltaTime; // Example friction value of 0.1
        return currentVelocity * (Mathf.Max(speed - drop, 0f) / speed);
    }

    private Vector3 CalculateMovement(Vector2 input, Vector3 currentVelocity)
    {

        float acceleration = isGrounded ? 5f : 2.5f; // Example values for ground and air acceleration
        float maxSpeed = isGrounded ? 7f : 5f; // Example values for ground and air speed

        Vector3 cameraYawRotation = new Vector3(0f, orientation.transform.rotation.eulerAngles.y, 0f);
        Vector3 desiredVelocity = Quaternion.Euler(cameraYawRotation) * new Vector3(input.x, 0f, input.y) * acceleration * Time.deltaTime;

        Vector3 horizontalCurrentVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        float speedFactor = Mathf.Max(0f, 1 - (horizontalCurrentVelocity.magnitude / maxSpeed));
        float alignmentFactor = Vector3.Dot(horizontalCurrentVelocity, desiredVelocity);
        Vector3 blendedVelocity = Vector3.Lerp(desiredVelocity, desiredVelocity * speedFactor, alignmentFactor);

        blendedVelocity.y = CalculateJumpVelocity(currentVelocity.y).y;

        return blendedVelocity;
    }

    public Vector3 CalculateJumpVelocity(float currentYVelocity)
    {
        Vector3 jumpVelocity = Vector3.zero;

        if (input_Jump && isGrounded)
            jumpVelocity = new Vector3(0f, jump_Force - currentYVelocity, 0f);

        return jumpVelocity;
    }
}
