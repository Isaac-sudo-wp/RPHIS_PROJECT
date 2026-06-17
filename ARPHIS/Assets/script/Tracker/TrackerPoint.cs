using UnityEngine;

public class MapTrackerBlink : MonoBehaviour
{
    public float blinkSpeed = 3f;
    private Material mat;
    private Color originalColor;

    void Start()
    {
        // Cache the material instance smoothly
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    void Update()
    {
        // Animate the alpha transparency smoothly over time using a sine wave
        float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
        mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
    }
}