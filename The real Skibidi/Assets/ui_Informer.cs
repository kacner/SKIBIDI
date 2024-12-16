using UnityEngine;
using TMPro; // Import for TextMeshPro
using System.Collections.Generic;
using Photon.Pun; // Import for Photon Networking

[System.Serializable]
public class TextTrigger
{
    [SerializeField] public TMP_Text textDisplay; // Reference to the Text component
    [SerializeField] public string customString = "Your custom text here!";
    [SerializeField] public string requiredTag = "Player"; // Tag to check for collision

    [HideInInspector]
    [SerializeField] public CanvasGroup textCanvasGroup; // CanvasGroup for fade animation

    [HideInInspector]
    public Coroutine activeCoroutine; // Track coroutine for this trigger
}

public class ui_Informer : MonoBehaviourPunCallbacks
{
    public List<TextTrigger> textTriggers = new List<TextTrigger>(); // List of triggers

    private void Start()
    {
        foreach (var textTrigger in textTriggers)
        {
            if (textTrigger.textDisplay != null)
            {
                textTrigger.textCanvasGroup = textTrigger.textDisplay.GetComponent<CanvasGroup>();
                if (textTrigger.textCanvasGroup == null)
                {
                    textTrigger.textCanvasGroup = textTrigger.textDisplay.gameObject.AddComponent<CanvasGroup>();
                }
                textTrigger.textDisplay.text = ""; // Ensure the text starts empty
                textTrigger.textCanvasGroup.alpha = 0; // Ensure text is invisible initially
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (var textTrigger in textTriggers)
        {
            // Check if the colliding object has the required tag
            if (other.CompareTag(textTrigger.requiredTag) && photonView.IsMine)
            {
                photonView.RPC("ShowTextRPC", RpcTarget.AllBuffered, textTriggers.IndexOf(textTrigger));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (var textTrigger in textTriggers)
        {
            // Check if the colliding object has the required tag
            if (other.CompareTag(textTrigger.requiredTag) && photonView.IsMine)
            {
                photonView.RPC("HideTextRPC", RpcTarget.AllBuffered, textTriggers.IndexOf(textTrigger));
            }
        }
    }

    [PunRPC]
    private void ShowTextRPC(int triggerIndex)
    {
        if (triggerIndex >= 0 && triggerIndex < textTriggers.Count)
        {
            var textTrigger = textTriggers[triggerIndex];
            if (textTrigger.textDisplay != null)
            {
                textTrigger.textDisplay.text = textTrigger.customString;
                if (textTrigger.activeCoroutine != null)
                    StopCoroutine(textTrigger.activeCoroutine);

                textTrigger.activeCoroutine = StartCoroutine(FadeText(textTrigger, 1)); // Fade in
            }
        }
    }

    [PunRPC]
    private void HideTextRPC(int triggerIndex)
    {
        if (triggerIndex >= 0 && triggerIndex < textTriggers.Count)
        {
            var textTrigger = textTriggers[triggerIndex];
            if (textTrigger.textDisplay != null)
            {
                if (textTrigger.activeCoroutine != null)
                    StopCoroutine(textTrigger.activeCoroutine);

                textTrigger.activeCoroutine = StartCoroutine(FadeText(textTrigger, 0)); // Fade out
            }
        }
    }

    private System.Collections.IEnumerator FadeText(TextTrigger textTrigger, float targetAlpha)
    {
        float duration = 0.5f; // Fade duration
        float startAlpha = textTrigger.textCanvasGroup.alpha;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textTrigger.textCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        textTrigger.textCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0)
        {
            textTrigger.textDisplay.text = ""; // Clear text when fully faded out
        }

        textTrigger.activeCoroutine = null; // Clear the reference
    }
}
