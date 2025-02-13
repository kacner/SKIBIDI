using UnityEngine;

[System.Serializable]
public enum GunInventoryType
{
    Primary,
    Secondary,
    Melee
}

public class GunCustomGravity : MonoBehaviour
{
    [SerializeField] private float Gravity = 9.82f;
    private Rigidbody rb;
    public GunInventoryType gunInventoryType;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        rb.AddForce(-Vector3.up * Gravity, ForceMode.Acceleration);
    }
}
