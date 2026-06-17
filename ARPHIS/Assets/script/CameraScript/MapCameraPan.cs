using UnityEngine;
using UnityEngine.EventSystems;

public class MapCameraPan : MonoBehaviour, IDragHandler
{
    [Header("Settings")]
    public float panSpeed = 1f; // Speed sensitivity multiplier
    [Range(1f, 20f)]
    public float smoothSpeed = 12f; // Gliding inertia smoothness
    
    [Header("Target Camera Setup")]
    public Transform mapCameraHolder; 
    
    [Header("Map Boundaries (HologramMapPlane)")]
    public Transform plainMeshFloor; // Drag your HologramMapPlane here!
    
    private float minX, maxX, minZ, maxZ;
    private bool boundariesSet = false;
    private Camera cachedMapCamera;
    private Vector3 targetPosition;
    
    void Start()
    {
        if (mapCameraHolder == null)
        {
            GameObject holder = GameObject.Find("MapCamera");
            if (holder == null) holder = GameObject.Find("MapCameraHolder");
            if (holder != null) mapCameraHolder = holder.transform;
        }

        if (mapCameraHolder != null)
        {
            cachedMapCamera = mapCameraHolder.GetComponent<Camera>();
            if (cachedMapCamera == null) cachedMapCamera = mapCameraHolder.GetComponentInChildren<Camera>();
            
            targetPosition = mapCameraHolder.position;
        }
        
        CalculateCameraBounds();
    }

    public void CalculateCameraBounds()
    {
        if (plainMeshFloor != null && cachedMapCamera != null)
        {
            Vector3 center = plainMeshFloor.position;
            // Standard Unity planes are 10 units wide per local scale unit
            Vector3 planeSize = new Vector3(plainMeshFloor.localScale.x * 10f, 0f, plainMeshFloor.localScale.z * 10f);
            
            float halfPlaneX = planeSize.x / 2f;
            float halfPlaneZ = planeSize.z / 2f;

            // DYNAMIC ORTHOGRAPHIC BOUND CLAMP:
            // This prevents the camera box from viewing outside the edges based on your camera's zoom/size
            float camVertExtent = cachedMapCamera.orthographicSize;
            float camHorizExtent = camVertExtent * cachedMapCamera.aspect;

            // Calculate the absolute safe limits so the camera view never crosses the plane boundaries
            minX = center.x - halfPlaneX + camHorizExtent;
            maxX = center.x + halfPlaneX - camHorizExtent;
            minZ = center.z - halfPlaneZ + camVertExtent;
            maxZ = center.z + halfPlaneZ - camVertExtent;

            // Failsafe: If the camera is zoomed out too far, lock it to the center of the plane
            if (minX > maxX) { minX = maxX = center.x; }
            if (minZ > maxZ) { minZ = maxZ = center.z; }

            boundariesSet = true;
            Debug.Log($"🗺️ Map bounds strictly locked. X Limits: {minX} to {maxX} | Z Limits: {minZ} to {maxZ}");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (mapCameraHolder == null || cachedMapCamera == null) return;
        
        Vector2 delta = eventData.delta;
        
        // Convert screen space pixels to precise world units based on orthographic bounds
        float unitsPerPixel = (cachedMapCamera.orthographicSize * 2f) / Screen.height;

        // FIXED DIRECTION: Map slides naturally with your hand gesture now
        targetPosition.x -= delta.x * panSpeed * unitsPerPixel;
        targetPosition.z -= delta.y * panSpeed * unitsPerPixel;
        
        // Enforce boundary constraints instantly on drag input
        if (boundariesSet)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
        }
    }

    void LateUpdate()
    {
        if (mapCameraHolder != null)
        {
            // Smoothly interpolate position coordinates
            Vector3 smoothPos = Vector3.Lerp(mapCameraHolder.position, targetPosition, Time.deltaTime * smoothSpeed);
            mapCameraHolder.position = new Vector3(smoothPos.x, mapCameraHolder.position.y, smoothPos.z);
        }
    }

    // FIXED: Changed cachedMapCamera.hasChanged to mapCameraHolder.hasChanged
    void Update()
    {
        if (mapCameraHolder != null && mapCameraHolder.hasChanged)
        {
            CalculateCameraBounds();
            mapCameraHolder.hasChanged = false;
        }
    }
}