using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Camera cam;
    public float shakeDuration = 0.15f;
    public float shakeIntensity = 0.3f;
    public float decreaseFactor = 1.5f;

    // Weapon shake parameters
    public Transform weaponTransform; // Reference to the weapon
    public float weaponShakeMultiplier = 1.5f; // Make weapon shake more/less than camera
    public Vector3 weaponShakeRotation = new Vector3(2f, 1f, 1f); // Rotation angles for weapon

    // Internal variables
    Vector3 originalPosition;
    Vector3 originalWeaponPosition;
    Quaternion originalWeaponRotation;
    float currentShakeDuration = 0f;
    bool isShaking = false;

    void Awake()
    {
        originalPosition = cam.transform.localPosition;
        if (weaponTransform != null)
        {
            originalWeaponPosition = weaponTransform.localPosition;
            originalWeaponRotation = weaponTransform.localRotation;
        }
    }

    void Update()
    {
        if (isShaking)
        {
            if (currentShakeDuration > 0)
            {
                // Camera position shake
                cam.transform.localPosition = originalPosition + Random.insideUnitSphere * shakeIntensity;

                // Weapon shake
                if (weaponTransform != null)
                {
                    // Position shake
                    Vector3 weaponShake = Random.insideUnitSphere * (shakeIntensity * weaponShakeMultiplier);
                    weaponTransform.localPosition = originalWeaponPosition + weaponShake;

                    // Rotation shake
                    float shakeProgress = currentShakeDuration / shakeDuration;
                    Vector3 rotationShake = new Vector3(
                        Random.Range(-1f, 1f) * weaponShakeRotation.x,
                        Random.Range(-1f, 1f) * weaponShakeRotation.y,
                        Random.Range(-1f, 1f) * weaponShakeRotation.z
                    ) * shakeProgress;

                    weaponTransform.localRotation = originalWeaponRotation * Quaternion.Euler(rotationShake);
                }

                currentShakeDuration -= Time.deltaTime * decreaseFactor;
            }
            else
            {
                // Reset shake
                isShaking = false;
                currentShakeDuration = 0f;
                cam.transform.localPosition = originalPosition;

                if (weaponTransform != null)
                {
                    weaponTransform.localPosition = originalWeaponPosition;
                    weaponTransform.localRotation = originalWeaponRotation;
                }
            }
        }
    }

    public void StartShake()
    {
        currentShakeDuration = shakeDuration;
        isShaking = true;
    }

    public void StartShake(float duration, float intensity)
    {
        shakeDuration = duration;
        shakeIntensity = intensity;
        currentShakeDuration = duration;
        isShaking = true;
    }
}
