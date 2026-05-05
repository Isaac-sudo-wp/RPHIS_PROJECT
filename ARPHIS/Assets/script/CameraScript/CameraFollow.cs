using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // Drag your Capsule here
    public Vector3 offset;          // The distance from the character
    public float smoothSpeed = 0.125f; // How smoothly it follows

    void LateUpdate()
    {
        // Calculate the desired position based on the offset
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly interpolate between the camera's current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Apply the position
        transform.position = smoothedPosition;

        // Optionally: Make the camera always look at the player
        // transform.LookAt(target); 
    }
}