using UnityEngine;
using UnityEngine.EventSystems;

public class MapCameraPan : MonoBehaviour, IDragHandler
{
    [Header("Settings")]
    public float panSpeed = 10f;
    
    [Header("Map Boundaries")]
    public Terrain terrain;
    public float boundaryPadding = 50f;
    
    private Transform mapCameraHolder;
    private float minX, maxX, minZ, maxZ;
    private bool boundariesSet = false;
    
    void Start()
    {
        // Find MapCameraHolder
        GameObject holder = GameObject.Find("MapCameraHolder");
        if (holder != null)
            mapCameraHolder = holder.transform;
        else
            Debug.LogError("MapCameraHolder not found!");
        
        // Find Terrain if not assigned
        if (terrain == null)
            terrain = FindObjectOfType<Terrain>();
        
        // Calculate boundaries
        if (terrain != null)
        {
            Vector3 terrainSize = terrain.terrainData.size;
            Vector3 terrainPosition = terrain.transform.position;
            
            // Calculate boundaries with padding
            minX = terrainPosition.x + boundaryPadding;
            maxX = terrainPosition.x + terrainSize.x - boundaryPadding;
            minZ = terrainPosition.z + boundaryPadding;
            maxZ = terrainPosition.z + terrainSize.z - boundaryPadding;
            
            boundariesSet = true;
            
            Debug.Log($"=== TERRAIN BOUNDARIES ===");
            Debug.Log($"Min X: {minX}, Max X: {maxX}");
            Debug.Log($"Min Z: {minZ}, Max Z: {maxZ}");
            Debug.Log($"==========================");
        }
        else
        {
            Debug.LogWarning("No terrain found! Camera will move freely.");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (mapCameraHolder == null) return;
        
        Vector2 delta = eventData.delta;
        
        Vector3 newPosition = mapCameraHolder.position;
        newPosition.x += -delta.x * panSpeed;
        newPosition.z += -delta.y * panSpeed;
        
        // Apply boundaries to keep camera within terrain
        if (boundariesSet)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
        }
        
        mapCameraHolder.position = newPosition;
    }
}