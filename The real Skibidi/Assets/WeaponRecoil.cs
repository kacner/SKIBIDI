/*using UnityEngine;
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

    [Header("Lerp Settings")]
    [SerializeField] private float recoilLerpSpeed = 10f;
    [SerializeField] private float recoveryLerpSpeed = 5f;

    private Vector2 currentRecoil;
    private Vector2 recoilVelocity;
    private Camera mainCamera;
    private bool isRecoiling;
    private Vector3 originalCameraRotation;
    private GameObject recoilContainer;
    private Quaternion targetRotation;
    void Start()
    {
        mainCamera = GetComponentInParent<Camera>();
        SetupRecoilContainer();
        ResetRecoil();
    }

    void SetupRecoilContainer()
    {
        recoilContainer = new GameObject("RecoilContainer");
        recoilContainer.transform.SetParent(mainCamera.transform.parent);
        recoilContainer.transform.localPosition = Vector3.zero;
        recoilContainer.transform.localRotation = Quaternion.identity;

        Vector3 originalPos = mainCamera.transform.localPosition;
        Quaternion originalRot = mainCamera.transform.localRotation;
        mainCamera.transform.SetParent(recoilContainer.transform);
        mainCamera.transform.localPosition = originalPos;
        mainCamera.transform.localRotation = originalRot;

        originalCameraRotation = recoilContainer.transform.localEulerAngles;
        targetRotation = Quaternion.Euler(originalCameraRotation);
    }

    void Update()
    {
        if (!isRecoiling)
        {
            currentRecoil.x = Mathf.SmoothDamp(currentRecoil.x, 0f, ref recoilVelocity.x, recoilRecoveryX * Time.deltaTime);
            currentRecoil.y = Mathf.SmoothDamp(currentRecoil.y, 0f, ref recoilVelocity.y, recoilRecoveryY * Time.deltaTime);

            Vector3 newRotation = new Vector3(
                ClampAngle(originalCameraRotation.x - currentRecoil.y, -90f, 90f),
                originalCameraRotation.y + currentRecoil.x,
                originalCameraRotation.z
            );
            targetRotation = Quaternion.Euler(newRotation);

            if (recoilContainer != null)
            {
                recoilContainer.transform.localRotation = Quaternion.Lerp(
                    recoilContainer.transform.localRotation,
                    targetRotation,
                    Time.deltaTime * recoveryLerpSpeed
                );
            }
        }
    }

    public void AddRecoil()
    {
        float recoilAmountX = Random.Range(-randomRecoilX, randomRecoilX) * recoilX;
        float recoilAmountY = Random.Range(0f, randomRecoilY) * recoilY;

        currentRecoil.x = Mathf.Clamp(currentRecoil.x + recoilAmountX, -maxRecoilX, maxRecoilX);

        float newMaxRecoil = maxRecoilY + maxRecoilY * Random.Range(-0.15f, 0.15f);
        currentRecoil.y = Mathf.Clamp(currentRecoil.y + recoilAmountY, 0f, newMaxRecoil);

        Vector3 newRotation = new Vector3(ClampAngle(originalCameraRotation.x - currentRecoil.y, -90f, 90f), originalCameraRotation.y + currentRecoil.x, originalCameraRotation.z);
        targetRotation = Quaternion.Euler(newRotation);

        if (!isRecoiling)
        {
            StartCoroutine(RecoilCooldown());
        }
    }

    private void ApplyRecoilToContainer()
    {
        if (recoilContainer != null)
        {
            Vector3 newRotation = new Vector3(
                ClampAngle(originalCameraRotation.x - currentRecoil.y, -90f, 90f),
                originalCameraRotation.y + currentRecoil.x,
                originalCameraRotation.z
            );

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

        float elapsedTime = 0f;
        Quaternion startRotation = recoilContainer.transform.localRotation;

        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / 0.1f;

            recoilContainer.transform.localRotation = Quaternion.Lerp(
                startRotation,
                targetRotation,
                t * recoilLerpSpeed
            );

            yield return null;
        }

        isRecoiling = false;
    }

    public void ResetRecoil()
    {
        currentRecoil = Vector2.zero;
        recoilVelocity = Vector2.zero;
        isRecoiling = false;
        targetRotation = Quaternion.Euler(originalCameraRotation);
        if (recoilContainer != null)
        {
            recoilContainer.transform.localRotation = targetRotation;
        }
    }
}*/using UnityEngine;
using System.Collections;
using Photon.Pun;

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

    [Header("Lerp Settings")]
    [SerializeField] private float recoilLerpSpeed = 10f;
    [SerializeField] private float recoveryLerpSpeed = 5f;

    [Header("Camera Shake")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private float shakeIntensityMultiplier = 0.1f;
    [SerializeField] private float shakeDurationMultiplier = 0.1f;

    private Vector2 currentRecoil;
    private Vector2 recoilVelocity;
    private Camera mainCamera;
    private bool isRecoiling;
    private Vector3 originalCameraRotation;
    private GameObject recoilContainer;
    private Quaternion targetRotation;

    void Start()
    {
        mainCamera = GetComponentInParent<Camera>();

        if (cameraShake == null)
        {
            cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake == null)
            {
                cameraShake = mainCamera.gameObject.AddComponent<CameraShake>();
                cameraShake.cam = mainCamera;
            }
        }

        SetupRecoilContainer();
        ResetRecoil();
    }

    void SetupRecoilContainer()
    {
        if (Application.isEditor)
            recoilContainer = new GameObject("RecoilContainer");
        else
            recoilContainer = PhotonNetwork.Instantiate("RecoilContainer", Vector3.zero, Quaternion.identity, 0);

        recoilContainer.transform.SetParent(mainCamera.transform.parent);
        recoilContainer.transform.localPosition = Vector3.zero;
        recoilContainer.transform.localRotation = Quaternion.identity;

        Vector3 originalPos = mainCamera.transform.localPosition;
        Quaternion originalRot = mainCamera.transform.localRotation;
        mainCamera.transform.SetParent(recoilContainer.transform);
        mainCamera.transform.localPosition = originalPos;
        mainCamera.transform.localRotation = originalRot;

        originalCameraRotation = recoilContainer.transform.localEulerAngles;
        targetRotation = Quaternion.Euler(originalCameraRotation);
    }

    void Update()
    {
        if (!isRecoiling)
        {
            currentRecoil.x = Mathf.SmoothDamp(currentRecoil.x, 0f, ref recoilVelocity.x, recoilRecoveryX * Time.deltaTime);
            currentRecoil.y = Mathf.SmoothDamp(currentRecoil.y, 0f, ref recoilVelocity.y, recoilRecoveryY * Time.deltaTime);

            Vector3 newRotation = new Vector3(ClampAngle(originalCameraRotation.x - currentRecoil.y, -90f, 90f), originalCameraRotation.y + currentRecoil.x, originalCameraRotation.z);
            targetRotation = Quaternion.Euler(newRotation);

            if (recoilContainer != null)
                recoilContainer.transform.localRotation = Quaternion.Lerp(recoilContainer.transform.localRotation, targetRotation, Time.deltaTime * recoveryLerpSpeed);
        }
    }

    public void AddRecoil()
    {

        float recoilAmountX = Random.Range(-randomRecoilX, randomRecoilX) * recoilX;
        float recoilAmountY = Random.Range(0f, randomRecoilY) * recoilY;

        currentRecoil.x = Mathf.Clamp(currentRecoil.x + recoilAmountX, -maxRecoilX, maxRecoilX);
        float newMaxRecoil = maxRecoilY + maxRecoilY * Random.Range(-0.15f, 0.15f);
        currentRecoil.y = Mathf.Clamp(currentRecoil.y + recoilAmountY, 0f, newMaxRecoil);

        Vector3 newRotation = new Vector3(ClampAngle(originalCameraRotation.x - currentRecoil.y, -90f, 90f), originalCameraRotation.y + currentRecoil.x, originalCameraRotation.z);
        targetRotation = Quaternion.Euler(newRotation);

        float shakeDuration = Mathf.Max(Mathf.Abs(recoilAmountX), recoilAmountY) * shakeDurationMultiplier;
        float shakeIntensity = Mathf.Max(Mathf.Abs(recoilAmountX), recoilAmountY) * shakeIntensityMultiplier;


        Vector3 shakeDirection = new Vector3(-recoilAmountY, recoilAmountX, 0f).normalized;


        if (cameraShake != null)
        {
            cameraShake.StartDirectionalShake(shakeDuration, shakeIntensity, shakeDirection);
        }

        if (!isRecoiling)
        {
            StartCoroutine(RecoilCooldown());
        }
    }

    private void ApplyRecoilToContainer()
    {
        if (recoilContainer != null)
        {
            Vector3 newRotation = new Vector3(ClampAngle(originalCameraRotation.x - currentRecoil.y, -90f, 90f), originalCameraRotation.y + currentRecoil.x, originalCameraRotation.z);

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

        float elapsedTime = 0f;
        Quaternion startRotation = recoilContainer.transform.localRotation;

        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / 0.1f;

            recoilContainer.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t * recoilLerpSpeed);

            yield return null;
        }

        isRecoiling = false;
    }

    public void ResetRecoil()
    {
        currentRecoil = Vector2.zero;
        recoilVelocity = Vector2.zero;
        isRecoiling = false;
        targetRotation = Quaternion.Euler(originalCameraRotation);

        if (recoilContainer != null)
            recoilContainer.transform.localRotation = targetRotation;
    }
}