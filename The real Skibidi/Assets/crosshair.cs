using UnityEngine;
using UnityEngine.UI;

public class crosshair : MonoBehaviour
{
    public Color crosshairColor = Color.green;
    public float lineThickness = 3f;
    public float lineLength = 10f;
    public float gapSize = 5f;

    private RectTransform top, bottom, left, right;
    private Camera playerCamera;

    private bool isCreated = false;

    void Start()
    {
        playerCamera = Camera.main;
        CreateCrosshair();
    }

    void CreateCrosshair()
    {
        top = CreateLine("Top");
        bottom = CreateLine("Bottom");
        left = CreateLine("Left");
        right = CreateLine("Right");

        UpdateCrosshair();
    }

    private void OnValidate()
    {
        UpdateCrosshair();
    }

    RectTransform CreateLine(string name)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(transform);
        line.transform.localScale = Vector3.one;

        Image image = line.AddComponent<Image>();
        image.color = crosshairColor;

        RectTransform rect = line.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(lineThickness, lineLength);
        return rect;
    }

    void UpdateCrosshair()
    {
        // Positioning relative to the center of the screen
        top.anchoredPosition = new Vector2(0, gapSize + lineLength / 2f);
        bottom.anchoredPosition = new Vector2(0, -gapSize - lineLength / 2f);
        left.anchoredPosition = new Vector2(-gapSize - lineLength / 2f, 0);
        right.anchoredPosition = new Vector2(gapSize + lineLength / 2f, 0);

        // Rotate horizontal lines
        left.localEulerAngles = new Vector3(0, 0, 90);
        right.localEulerAngles = new Vector3(0, 0, 90);
    }

    /*void Update()
    {
        if (playerCamera == null) return;

        // Keep crosshair centered on the screen (viewport center)
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        transform.position = screenCenter;
    }*/
}