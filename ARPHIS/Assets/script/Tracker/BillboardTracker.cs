using UnityEngine;

public class BillboardTracker : MonoBehaviour
{
    private Camera mapCamera;
    
    void Start()
    {
        mapCamera = GameObject.Find("MapCamera")?.GetComponent<Camera>();
    }
    
    void LateUpdate()
    {
        if (mapCamera != null)
        {
            // Make the dot always face the camera
            transform.LookAt(transform.position + mapCamera.transform.forward, mapCamera.transform.up);
        }
    }
}