﻿using Photon.Pun;
using TMPro;
using UnityEngine;
using ExitGames.Client.Photon;

namespace Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviourPunCallbacks
    {
        [System.Serializable]
        public class MovementSettings
        {
            public float MaxSpeed;
            public float Acceleration;
            public float Deceleration;

            public MovementSettings(float maxSpeed, float accel, float decel)
            {
                MaxSpeed = maxSpeed;
                Acceleration = accel;
                Deceleration = decel;
            }
        }

        [Header("Aiming")]
        [SerializeField] private Camera m_Camera;
        [SerializeField] private MouseLook m_MouseLook = new MouseLook();
        public float normalFOV = 60f;    // Default FOV
        public float zoomedFOV = 40f;    // FOV when right-click is held
        public float transitionSpeed = 5f; // Speed of FOV transition

        private float targetFOV;

        [Header("Movement")]
        [SerializeField] private float m_Friction = 6;
        [SerializeField] private float m_Gravity = 20;
        [SerializeField] private float m_JumpForce = 8;
        [Tooltip("Automatically jump when holding jump button")]
        [SerializeField] private bool m_AutoBunnyHop = false;
        [Tooltip("How precise air control is")]
        [SerializeField] private float m_AirControl = 0.3f;
        [SerializeField] private MovementSettings m_GroundSettings = new MovementSettings(7, 14, 10);
        [SerializeField] private MovementSettings m_AirSettings = new MovementSettings(7, 2, 2);
        [SerializeField] private MovementSettings m_StrafeSettings = new MovementSettings(1, 50, 50);
        [Header("Crouch / Dash")]
        [SerializeField] private float sliding_Speed = 5;
        [SerializeField] private float Slide_Cooldown = 1f;
        [SerializeField] private float Slide_Cooldown_Timer = 1f;
        [SerializeField] private Vector3 moveDirection;
        private float OG_sliding_Speed;
        private bool input_Crouch;
        private bool isCrouching;
        [Header("RoofCheck")]
        [SerializeField] private Vector3 ray_Offset_Up;
        [SerializeField] private float ray_Distance_up = 1.2f;
        [SerializeField] private bool isRoofied;
        [Header("Username")]
        public TextMeshPro UsernameTextObj;
        public float Speed { get { return m_Character.velocity.magnitude; } }

        private CharacterController m_Character;
        private Vector3 m_MoveDirectionNorm = Vector3.zero;
        private Vector3 m_PlayerVelocity = Vector3.zero;

        private bool m_JumpQueued = false;

        private float m_PlayerFriction = 0;

        private Vector3 m_MoveInput;
        private Transform m_Tran;
        private Transform m_CamTran;

        [Space(10)]

        [Header("SkinSettings")]

        public MeshFilter meshfilter;
        public Mesh Headless;
        public Mesh NotHeadless;


        private void Awake()
        {
            if (photonView.IsMine)
            {
                OG_sliding_Speed = sliding_Speed;
                m_Tran = transform;
                m_Character = GetComponent<CharacterController>();

                if (!m_Camera)
                    m_Camera = Camera.main;

                m_CamTran = m_Camera.transform;
                m_MouseLook.Init(m_Tran, m_CamTran, photonView);

                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "Username", PhotonNetwork.NickName } });

                UsernameTextObj.text = PhotonNetwork.NickName;

                meshfilter.mesh = Headless;
            }
            else
            {
                m_Camera.gameObject.SetActive(false);
                string otherPlayerUsername = UsernameManager.Instance.GetUsername(photonView);

                UsernameTextObj.text = otherPlayerUsername;
                meshfilter.mesh = NotHeadless;
            }
        }

        private void Update()
        {
            print(PhotonNetwork.NickName);
            if (photonView.IsMine)
            {

                CheckRoofied();

                m_MoveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                m_MouseLook.UpdateCursorLock();
                QueueJump();

                if (m_Character.isGrounded)
                {
                    GroundMove();
                }
                else
                {
                    AirMove();
                }

                if (Input.GetMouseButton(1))
                {
                    targetFOV = zoomedFOV;
                }
                else
                {
                    targetFOV = normalFOV;
                }

                m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, targetFOV, transitionSpeed * Time.deltaTime);

                m_MouseLook.LookRotation(m_Tran, m_CamTran);

                m_Character.Move(m_PlayerVelocity * Time.deltaTime);

                input_Crouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

                if (input_Crouch)
                {
                    StartCrouching();
                }
                else
                {
                    StopCrouching();
                }
            }
        }
        private void QueueJump()
        {
            if (m_AutoBunnyHop)
            {
                m_JumpQueued = Input.GetButton("Jump");
                return;
            }

            if (Input.GetButtonDown("Jump") && !m_JumpQueued)
            {
                m_JumpQueued = true;
            }

            if (Input.GetButtonUp("Jump"))
            {
                m_JumpQueued = false;
            }
        }

        private void AirMove()
        {
            float accel;

            var wishdir = new Vector3(m_MoveInput.x, 0, m_MoveInput.z);
            wishdir = m_Tran.TransformDirection(wishdir);

            float wishspeed = wishdir.magnitude;
            wishspeed *= m_AirSettings.MaxSpeed;

            wishdir.Normalize();
            m_MoveDirectionNorm = wishdir;

            float wishspeed2 = wishspeed;
            if (Vector3.Dot(m_PlayerVelocity, wishdir) < 0)
            {
                accel = m_AirSettings.Deceleration;
            }
            else
            {
                accel = m_AirSettings.Acceleration;
            }

            // If the player is ONLY strafing left or right
            if (m_MoveInput.z == 0 && m_MoveInput.x != 0)
            {
                if (wishspeed > m_StrafeSettings.MaxSpeed)
                {
                    wishspeed = m_StrafeSettings.MaxSpeed;
                }

                accel = m_StrafeSettings.Acceleration;
            }

            Accelerate(wishdir, wishspeed, accel);
            if (m_AirControl > 0)
            {
                AirControl(wishdir, wishspeed2);
            }

            // Apply gravity
            m_PlayerVelocity.y -= m_Gravity * Time.deltaTime;
        }
        private void AirControl(Vector3 targetDir, float targetSpeed)
        {
            if (Mathf.Abs(m_MoveInput.z) < 0.001 || Mathf.Abs(targetSpeed) < 0.001)
            {
                return;
            }

            float zSpeed = m_PlayerVelocity.y;
            m_PlayerVelocity.y = 0;
            float speed = m_PlayerVelocity.magnitude;
            m_PlayerVelocity.Normalize();

            float dot = Vector3.Dot(m_PlayerVelocity, targetDir);
            float k = 32;
            k *= m_AirControl * dot * dot * Time.deltaTime;

            if (dot > 0)
            {
                m_PlayerVelocity.x *= speed + targetDir.x * k;
                m_PlayerVelocity.y *= speed + targetDir.y * k;
                m_PlayerVelocity.z *= speed + targetDir.z * k;

                m_PlayerVelocity.Normalize();
                m_MoveDirectionNorm = m_PlayerVelocity;
            }

            m_PlayerVelocity.x *= speed;
            m_PlayerVelocity.y = zSpeed;
            m_PlayerVelocity.z *= speed;
        }

        private void GroundMove()
        {
            // if jumpqueued then allply no friktion
            if (!m_JumpQueued)
            {
                ApplyFriction(1.0f);
            }
            else
            {
                ApplyFriction(0);
            }

            var wishdir = new Vector3(m_MoveInput.x, 0, m_MoveInput.z);
            wishdir = m_Tran.TransformDirection(wishdir);
            wishdir.Normalize();
            m_MoveDirectionNorm = wishdir;

            var wishspeed = wishdir.magnitude;
            wishspeed *= m_GroundSettings.MaxSpeed;

            Accelerate(wishdir, wishspeed, m_GroundSettings.Acceleration);

            // reset the gravity 
            m_PlayerVelocity.y = -m_Gravity * Time.deltaTime;

            if (m_JumpQueued)
            {
                m_PlayerVelocity.y = m_JumpForce;
                m_JumpQueued = false;
            }
        }

        private void ApplyFriction(float t)
        {
            Vector3 vec = m_PlayerVelocity; 
            vec.y = 0;
            float speed = vec.magnitude;
            float drop = 0;

            //friction if gruonded.
            if (m_Character.isGrounded)
            {
                float control = speed < m_GroundSettings.Deceleration ? m_GroundSettings.Deceleration : speed;
                drop = control * m_Friction * Time.deltaTime * t;
            }

            float newSpeed = speed - drop;
            m_PlayerFriction = newSpeed;
            if (newSpeed < 0)
            {
                newSpeed = 0;
            }

            if (speed > 0)
            {
                newSpeed /= speed;
            }

            m_PlayerVelocity.x *= newSpeed;
            // playerVelocity.y *= newSpeed;
            m_PlayerVelocity.z *= newSpeed;
        }
        private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
        {
            float currentspeed = Vector3.Dot(m_PlayerVelocity, targetDir);
            float addspeed = targetSpeed - currentspeed;
            if (addspeed <= 0)
            {
                return;
            }

            float accelspeed = accel * Time.deltaTime * targetSpeed;
            if (accelspeed > addspeed)
            {
                accelspeed = addspeed;
            }

            m_PlayerVelocity.x += accelspeed * targetDir.x;
            m_PlayerVelocity.z += accelspeed * targetDir.z;
        }

        void StartCrouching()
        {
            if (!isCrouching)
            {
                isCrouching = true;
                transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
                if (Slide_Cooldown_Timer <= 0 && m_Character.isGrounded)
                {
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
            }
        }
        void CheckRoofied()
        {
            Vector3 origin = transform.position + ray_Offset_Up * transform.localScale.y;
            Vector3 direction = Vector3.up;

            isRoofied = Physics.Raycast(origin, direction, ray_Distance_up);

            Color rayColor = isRoofied ? Color.red : Color.green;
            Debug.DrawRay(origin, direction * ray_Distance_up, rayColor);
        }
    }
}