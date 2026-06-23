using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InspectUIManager : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("UI Buttons")]
    public GameObject collectButton;
    public GameObject ignoreButton;
    public GameObject closeButton;

    [Header("3D Viewer Setup")]
    public Transform pedestal;
    private GameObject current3DModel;
    private bool currentIsReal;

    [Header("360 Observation Settings")]
    public float rotationSpeed = 0.5f; 
    private Vector3 previousMousePosition;

    [Header("Inventory Link")]
    public InventoryManager playerInventory;
    private ArtifactFragment currentFragment;

    public void OpenInspectWindow(string fName, string fDescription, GameObject artifactPrefab, bool isReal, ArtifactFragment collectedFragment, bool isFromInventory)
    {
        // Force-enable this component immediately so UI updates render instantly
        this.enabled = true;

        if (titleText != null) titleText.text = fName;
        if (descriptionText != null) descriptionText.text = fDescription;

        currentIsReal = isReal;
        currentFragment = collectedFragment;

        if (current3DModel != null) Destroy(current3DModel);

        if (artifactPrefab != null && pedestal != null)
        {
            current3DModel = Instantiate(artifactPrefab, pedestal);
            current3DModel.SetActive(true);

            current3DModel.transform.localPosition = Vector3.zero;
            current3DModel.transform.localScale = artifactPrefab.transform.localScale;

            // 🔥 CRITICAL FIX: Strip ALL colliders from children recursively!
            // This prevents duplicate physics bounds from floating in your UI space and breaking raycasts.
            Collider[] nestedColliders = current3DModel.GetComponentsInChildren<Collider>();
            foreach (Collider col in nestedColliders)
            {
                Destroy(col);
            }
        }

        // --- STATEFUL UI LOGIC ---
        if (isFromInventory)
        {
            if (collectButton != null) collectButton.SetActive(false);
            if (ignoreButton != null) ignoreButton.SetActive(false);
            if (closeButton != null) closeButton.SetActive(true);
        }
        else
        {
            if (collectButton != null) collectButton.SetActive(true);
            if (ignoreButton != null) ignoreButton.SetActive(true);
            if (closeButton != null) closeButton.SetActive(false);
        }

        gameObject.SetActive(true);
        Debug.Log($"[UI SUCCESS] Inspect window completely active for: {fName}");
    }

    void Update()
    {
        if (gameObject.activeSelf && current3DModel != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                previousMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - previousMousePosition;

                float rotateX = delta.x * rotationSpeed;
                float rotateY = delta.y * rotationSpeed;

                current3DModel.transform.Rotate(Vector3.up, -rotateX, Space.World);
                current3DModel.transform.Rotate(Vector3.right, rotateY, Space.World);

                previousMousePosition = Input.mousePosition;
            }
        }
    }

    public void OnCollectClicked()
    {
        if (playerInventory != null && currentFragment != null)
        {
            bool itemWasAccepted = playerInventory.AddItemToInventory(currentFragment);

            if (itemWasAccepted)
            {
                MapTrackerManager trackerManager = FindObjectOfType<MapTrackerManager>();
                if (trackerManager != null)
                {
                    trackerManager.RemoveTracker(currentFragment.transform);
                }

                if (currentFragment.gameObject != null)
                {
                    currentFragment.gameObject.SetActive(false);
                }

                if (currentIsReal)
                {
                    Debug.Log("SUCCESS: Real Artifact Collected!");
                }
                else
                {
                    Debug.Log("FAIL: Fake Artifact Collected!");
                }
            }
            else
            {
                Debug.LogWarning("Item left behind because backpack is full!");
            }
        }
        else
        {
            Debug.LogError("CRITICAL ERROR: 'currentFragment' or 'playerInventory' reference dropped.");
        }

        gameObject.SetActive(false);
    }

    public void OnIgnoreClicked()
    {
        gameObject.SetActive(false);
    }

    public void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }
}