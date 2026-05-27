using UnityEngine;
using UnityEngine.EventSystems;

public class CameraOrbit : MonoBehaviour
{
    public Transform player;
    public float sensitivity = 0.5f;
    public float verticalLimit = 80f;
    
    [Tooltip("The lowest angle the camera can look down. Setting this close to 0 prevents looking under the map plane void.")]
    public float minimumVerticalAngle = -5f; // ADJUST THIS to completely block looking under the sidewalk!
    
    [Header("Pause Settings")]
    public GameObject pausePanel;
    public GameObject settingPanel;
    
    private float mouseX = 0f;
    private float mouseY = 0f;
    private bool isPaused = false;

    void Start()
    {
        if (player != null)
        {
            mouseX = transform.eulerAngles.y;
            mouseY = transform.eulerAngles.x;
            if (mouseY > 180f) mouseY -= 360f;
        }
        
        float savedMultiplier = PlayerPrefs.GetFloat("CameraSensitivity", 1f);
        sensitivity = 0.5f * savedMultiplier;
    }

    void Update()
    {
        isPaused = false;
        
        if (pausePanel != null && pausePanel.activeSelf)
            isPaused = true;
        
        if (settingPanel != null && settingPanel.activeSelf)
            isPaused = true;
        
        if (Time.timeScale == 0)
            isPaused = true;
    }

    void LateUpdate()
    {
        if (player == null) return;
        
        transform.position = player.position;
        if (isPaused) return;

        // Handle camera rotation with touch (Android)
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                
                // Check if touch is NOT over UI
                if (!IsPointerOverUI(touch.position))
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        mouseX += touch.deltaPosition.x * sensitivity;
                        mouseY -= touch.deltaPosition.y * sensitivity;
                        
                        // FIX: Clamps the vertical rotation between the minimum angle cap and your upper limit
                        mouseY = Mathf.Clamp(mouseY, minimumVerticalAngle, verticalLimit);
                    }
                    break; // Only use first valid touch for camera
                }
            }
        }
        // Mouse input for Editor testing
        else if (Input.GetMouseButton(0) && !IsPointerOverUIMouse())
        {
            mouseX += Input.GetAxis("Mouse X") * sensitivity * 5f;
            mouseY -= Input.GetAxis("Mouse Y") * sensitivity * 5f;
            
            // FIX: Applies the exact same lower angle block protection inside the Unity Editor
            mouseY = Mathf.Clamp(mouseY, minimumVerticalAngle, verticalLimit);
        }
        
        transform.rotation = Quaternion.Euler(mouseY, mouseX, 0f);
    }

    // Check if touch position is over UI
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;
        
        var pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;
        
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        return results.Count > 0;
    }
    
    // Check if mouse is over UI (for Editor)
    private bool IsPointerOverUIMouse()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void UpdateSensitivity(float normalizedValue)
    {
        sensitivity = 0.5f * normalizedValue;
    }
}