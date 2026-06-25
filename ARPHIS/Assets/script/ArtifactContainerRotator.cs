using UnityEngine;

public class ArtifactContainerRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 3f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 15f;

    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private Camera mainCamera;
    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("❌ Camera.main not found!");

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if click hit this object OR any of its children (snapped fragments)
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    isDragging = true;
                    lastMousePosition = Input.mousePosition;
                    yaw = transform.eulerAngles.y;
                    pitch = transform.eulerAngles.x;
                    Debug.Log($"🔄 Started rotating {gameObject.name}");
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            yaw -= delta.x * rotationSpeed * Time.deltaTime;
            pitch += delta.y * rotationSpeed * Time.deltaTime;

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleZoom()
    {
        if (mainCamera == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * scroll * zoomSpeed;
            float distance = Vector3.Distance(newPosition, transform.position);

            if (distance >= minZoom && distance <= maxZoom)
                mainCamera.transform.position = newPosition;
        }
    }

    // 🔥 OnMouseDown on the container itself (not children)
    void OnMouseDown()
    {
        isDragging = true;
        lastMousePosition = Input.mousePosition;
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void OnMouseDrag()
    {
        Vector3 delta = Input.mousePosition - lastMousePosition;

        yaw -= delta.x * rotationSpeed * 0.02f;
        pitch += delta.y * rotationSpeed * 0.02f;

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        lastMousePosition = Input.mousePosition;
    }
}