using System.Collections;
using UnityEngine;

public class WillBePickedUp : MonoBehaviour
{
    [SerializeField] private BoxCollider collider;
    private void Awake()
    {
        StartCoroutine(cooldown());
    }
    private void OnEnable()
    {
        StartCoroutine(cooldown());
    }
    IEnumerator cooldown()
    {
        collider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        collider.enabled = true;
    }
}
