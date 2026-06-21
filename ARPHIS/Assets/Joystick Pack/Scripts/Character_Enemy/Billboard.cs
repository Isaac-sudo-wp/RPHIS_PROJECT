using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        // Automatically find the player's camera
        mainCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Force the health bar to always look exactly at the camera
        transform.LookAt(transform.position + mainCamera.forward);
    }
}