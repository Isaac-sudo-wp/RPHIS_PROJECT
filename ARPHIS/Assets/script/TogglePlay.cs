using UnityEngine;

public class TogglePlay : MonoBehaviour
{
    [Header("Drag GameObjects from Hierarchy")]
    public GameObject tabletPanel;           
    public GameObject playerCapsule;         
    public GameObject cameraPivot;
    public GameObject imgMapView; // The map image that toggles
    
    [Header("Map Camera (for trackers)")]
    public Camera mapCamera; // Drag MapCamera here
    public GameObject mapTrackerManager; // Drag MapTrackerManager here

    private PlayerMovement playerMove;
    private CameraFollow cameraFollow;
    private CameraOrbit cameraOrbit;
    private bool isMapOpen = false;

    void Start()
    {
        // Get script components
        if (playerCapsule != null)
            playerMove = playerCapsule.GetComponent<PlayerMovement>();
        
        if (cameraPivot != null)
        {
            cameraFollow = cameraPivot.GetComponent<CameraFollow>();
            cameraOrbit = cameraPivot.GetComponent<CameraOrbit>();
        }
        
        // Setup initial state - Tablet homescreen VISIBLE, map image HIDDEN
        if (tabletPanel != null) 
            tabletPanel.SetActive(true);  // Tablet homescreen is visible
        if (imgMapView != null) 
            imgMapView.SetActive(false);   // Map image is hidden initially
        if (mapCamera != null)
            mapCamera.enabled = false;      // MapCamera disabled initially
        
        // Player and camera scripts are ACTIVE initially (game is playing)
        if (playerMove != null)
            playerMove.enabled = true;
        if (cameraFollow != null)
            cameraFollow.enabled = true;
        if (cameraOrbit != null)
            cameraOrbit.enabled = true;
        
        isMapOpen = false;
    }

    // Call this from btnMap OnClick - Opens the MAP VIEW
    public void ToggleMapImage()
    {
        if (imgMapView != null)
        {
            isMapOpen = !imgMapView.activeSelf;
            imgMapView.SetActive(isMapOpen);
            
            // Enable/Disable MapCamera with the map
            if (mapCamera != null)
                mapCamera.enabled = isMapOpen;
            
            // Enable/Disable MapTrackerManager with the map
            if (mapTrackerManager != null)
                mapTrackerManager.SetActive(isMapOpen);
            
            // Disable player and camera scripts when map is OPEN
            if (playerMove != null)
                playerMove.enabled = !isMapOpen;
            if (cameraFollow != null)
                cameraFollow.enabled = !isMapOpen;
            if (cameraOrbit != null)
                cameraOrbit.enabled = !isMapOpen;
            
            Debug.Log($"Map: {(isMapOpen ? "OPEN - Game PAUSED" : "CLOSED - Game RESUMED")}");
        }
    }
    
    // Call this from btnBack - CLOSES the map and returns to tablet homescreen
    public void CloseMap()
    {
        if (imgMapView != null && imgMapView.activeSelf)
        {
            imgMapView.SetActive(false);
            isMapOpen = false;
            
            // Disable MapCamera when map closes
            if (mapCamera != null)
                mapCamera.enabled = false;
            
            // Disable MapTrackerManager when map closes
            if (mapTrackerManager != null)
                mapTrackerManager.SetActive(false);
            
            // Re-enable player and camera scripts
            if (playerMove != null)
                playerMove.enabled = true;
            if (cameraFollow != null)
                cameraFollow.enabled = true;
            if (cameraOrbit != null)
                cameraOrbit.enabled = true;
            
            Debug.Log("Map CLOSED - Returned to tablet homescreen");
        }
    }
}