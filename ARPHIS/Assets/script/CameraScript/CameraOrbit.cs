using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform player;      // Drag Capsule here
    public float sensitivity = 100f;
    private float mouseX, mouseY;

    void LateUpdate()
    {
        // 1. Follow the player's position
        transform.position = player.position;

        // 2. Rotate based on touch/mouse drag
        if (Input.GetMouseButton(0)) // For testing, or right side of screen touch
        {
            mouseX += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            // mouseY -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime; // Optional Up/Down
        }

        transform.rotation = Quaternion.Euler(0, mouseX, 0);
    }
}