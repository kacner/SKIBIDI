using UnityEngine;
using System.Collections;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float recoilX = 2f;
    [SerializeField] private float recoilY = 2f;
    [SerializeField] private float maxRecoilX = 20f;
    [SerializeField] private float maxRecoilY = 40f;
    [SerializeField] private float recoilRecoveryX = 10f;
    [SerializeField] private float recoilRecoveryY = 10f;

    [Header("Recoil Randomization")]
    [SerializeField] private float randomRecoilX = 0.5f;
    [SerializeField] private float randomRecoilY = 0.5f;

    private Vector2 currentRecoil;
    private Vector2 recoilVelocity;
    private Camera mainCamera;
    private bool isRecoiling;
    private Vector3 originalCameraRotation;
    private GameObject recoilContainer;

    void Start()
    {
        mainCamera = GetComponentInParent<Camera>();
        SetupRecoilContainer();
        ResetRecoil();
    }

    void SetupRecoilContainer()
    {
        // Create a container for recoil that sits between the camera and its parent
        recoilContainer = new GameObject("RecoilContainer");
        recoilContainer.transform.SetParent(mainCamera.transform.parent);
        recoilContainer.transform.localPosition = Vector3.zero;
        recoilContainer.transform.localRotation = Quaternion.identity;

        // Move the camera to be a child of the recoil container
        Vector3 originalPos = mainCamera.transform.localPosition;
        Quaternion originalRot = mainCamera.transform.localRotation;
        mainCamera.transform.SetParent(recoilContainer.transform);
        mainCamera.transform.localPosition = originalPos;
        mainCamera.transform.localRotation = originalRot;

        originalCameraRotation = recoilContainer.transform.localEulerAngles;
    }

    void Update()
    {
        if (!isRecoiling)
        {
            // Smoothly return recoil to zero
            currentRecoil.x = Mathf.SmoothDamp(currentRecoil.x, 0f, ref recoilVelocity.x, recoilRecoveryX * Time.deltaTime);
            currentRecoil.y = Mathf.SmoothDamp(currentRecoil.y, 0f, ref recoilVelocity.y, recoilRecoveryY * Time.deltaTime);

            // Apply the recovered rotation to the recoil container
            ApplyRecoilToContainer();
        }
    }

    public void AddRecoil()
    {
        // Calculate recoil amount with randomization
        float recoilAmountX = Random.Range(-randomRecoilX, randomRecoilX) * recoilX;
        float recoilAmountY = Random.Range(0f, randomRecoilY) * recoilY;

        // Add to current recoil, clamping to max values
        currentRecoil.x = Mathf.Clamp(currentRecoil.x + recoilAmountX, -maxRecoilX, maxRecoilX);
        currentRecoil.y = Mathf.Clamp(currentRecoil.y + recoilAmountY, 0f, maxRecoilY);

        // Apply the recoil
        ApplyRecoilToContainer();

        if (!isRecoiling)
        {
            StartCoroutine(RecoilCooldown());
        }
    }

    private void ApplyRecoilToContainer()
    {
        if (recoilContainer != null)
        {
            // Create new rotation with recoil
            Vector3 newRotation = new Vector3(
                ClampAngle(originalCameraRotation.x - currentRecoil.y, -90f, 90f),
                originalCameraRotation.y + currentRecoil.x,
                originalCameraRotation.z
            );

            // Apply the rotation to the container
            recoilContainer.transform.localRotation = Quaternion.Euler(newRotation);
        }
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    private IEnumerator RecoilCooldown()
    {
        isRecoiling = true;
        yield return new WaitForSeconds(0.1f);
        isRecoiling = false;
    }

    public void ResetRecoil()
    {
        currentRecoil = Vector2.zero;
        recoilVelocity = Vector2.zero;
        isRecoiling = false;
        if (recoilContainer != null)
        {
            recoilContainer.transform.localRotation = Quaternion.Euler(originalCameraRotation);
        }
    }
}