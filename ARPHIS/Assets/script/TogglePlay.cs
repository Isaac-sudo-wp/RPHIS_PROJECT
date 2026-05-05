using UnityEngine;

public class TogglePlay : MonoBehaviour
{
    [Header("Drag GameObjects from Hierarchy")]
    public GameObject tabletPanel;           
    public GameObject playerCapsule;         
    public GameObject cameraPivot;
    public GameObject imgMapView; // The map image that toggles

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
        
        // Setup initial state
        if (tabletPanel != null) 
            tabletPanel.SetActive(true);
        if (imgMapView != null) 
            imgMapView.SetActive(false);
    }

    // Call this from btnMap OnClick
    public void ToggleMapImage()
    {
        if (imgMapView != null)
        {
            isMapOpen = !imgMapView.activeSelf;
            imgMapView.SetActive(isMapOpen);
            
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
    
    // Optional: Call this from btnBack to close the map
    public void CloseMap()
    {
        if (imgMapView != null && imgMapView.activeSelf)
        {
            imgMapView.SetActive(false);
            isMapOpen = false;
            
            // Re-enable player and camera scripts
            if (playerMove != null)
                playerMove.enabled = true;
            if (cameraFollow != null)
                cameraFollow.enabled = true;
            if (cameraOrbit != null)
                cameraOrbit.enabled = true;
            
            Debug.Log("Map CLOSED - Game RESUMED");
        }
    }
}