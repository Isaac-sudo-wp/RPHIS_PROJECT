using UnityEngine;
using UnityEngine.EventSystems;

public class MapCameraPan : MonoBehaviour, IDragHandler
{
    [Header("Settings")]
    public float panSpeed = 0.05f; // Decreased from 10f to make dragging responsive and smooth on mobile canvases
    
    [Header("Target Camera Setup")]
    [Tooltip("Drag your secondary Map Camera or its parent holder here to execute panning movements directly.")]
    public Transform mapCameraHolder; 
    
    [Header("Map Boundaries")]
    [Tooltip("If using a standard Terrain component, assign it here.")]
    public Terrain terrain;
    
    [Tooltip("If using a flat plane or mesh floor instead of a terrain, drag it here to generate matching boundaries.")]
    public Transform plainMeshFloor;
    
    public float boundaryPadding = 50f;
    
    private float minX, maxX, minZ, maxZ;
    private bool boundariesSet = false;
    
    void Start()
    {
        // FIX: Backup fallback check. If you forgot to assign it in the inspector, 
        // it checks the old-fashioned way so it doesn't crash on boot!
        if (mapCameraHolder == null)
        {
            GameObject holder = GameObject.Find("MapCameraHolder");
            if (holder != null)
            {
                mapCameraHolder = holder.transform;
            }
            else
            {
                Debug.LogError("🚨 MapCameraHolder is missing from your scene hierarchy, or hasn't been dragged into the script slot!");
            }
        }
        
        // --- CHOOSE BOUNDARY STRATEGY DYNAMICALLY ---
        
        // Option A: Handle standard high-poly terrain blocks
        if (terrain != null)
        {
            Vector3 terrainSize = terrain.terrainData.size;
            Vector3 terrainPosition = terrain.transform.position;
            
            minX = terrainPosition.x + boundaryPadding;
            maxX = terrainPosition.x + terrainSize.x - boundaryPadding;
            minZ = terrainPosition.z + boundaryPadding;
            maxZ = terrainPosition.z + terrainSize.z - boundaryPadding;
            
            boundariesSet = true;
            Debug.Log($"🗺️ Bound Matrix Set via Terrain Asset bounds.");
        }
        // Option B: Handle lightweight modular city planes or floor shapes safely
        else if (plainMeshFloor != null)
        {
            // Read local mesh scaling factors directly to extract boundary widths
            Vector3 center = plainMeshFloor.position;
            Vector3 size = plainMeshFloor.localScale * 10f; // Multiplied by standard 10x factor for primitive planes
            
            minX = center.x - (size.x / 2f) + boundaryPadding;
            maxX = center.x + (size.x / 2f) - boundaryPadding;
            minZ = center.z - (size.z / 2f) + boundaryPadding;
            maxZ = center.z + (size.z / 2f) - boundaryPadding;
            
            boundariesSet = true;
            Debug.Log($"🗺️ Bound Matrix Set via flat City Plane layout.");
        }
        else
        {
            Debug.LogWarning("⚠️ No Terrain or Mesh Floor assigned. Your map tracking camera will glide without bounds.");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (mapCameraHolder == null) return;
        
        Vector2 delta = eventData.delta;
        Vector3 newPosition = mapCameraHolder.position;
        
        // Translate screen drag tracking coordinates securely into horizontal vectors
        newPosition.x += -delta.x * panSpeed;
        newPosition.z += -delta.y * panSpeed;
        
        // Enforce safety clamp checks
        if (boundariesSet)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
        }
        
        mapCameraHolder.position = newPosition;
    }
}