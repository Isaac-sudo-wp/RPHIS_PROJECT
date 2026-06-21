using UnityEngine;
using System.Collections;

public class DamageFeedback : MonoBehaviour
{
    private Renderer enemyRenderer;
    private Color originalColor;
    public float flashDuration = 0.15f;

    void Start()
    {
        // Find the renderer on the character (sometimes it's on a child object!)
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;
    }

    public void TriggerFlash()
    {
        if (enemyRenderer != null)
            StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        enemyRenderer.material.color = Color.red; // Change to red
        yield return new WaitForSeconds(flashDuration);
        enemyRenderer.material.color = originalColor; // Change back
    }
}