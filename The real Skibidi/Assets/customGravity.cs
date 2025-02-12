using UnityEngine;

public class customGravity : MonoBehaviour
{
    [SerializeField] private float Gravity = 9.82f;
    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        rb.AddForce(-Vector3.up * Gravity, ForceMode.Acceleration);
    }
}
