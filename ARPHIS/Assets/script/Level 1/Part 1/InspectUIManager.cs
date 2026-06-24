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
    public float rotationSpeed = 0.5f; // Bilis ng ikot depende sa swipe ng player
    private Vector3 previousMousePosition;

    [Header("Inventory Link")]
    public InventoryManager playerInventory;
    private ArtifactFragment currentFragment;

    public void OpenInspectWindow(string fName, string fDescription, GameObject artifactPrefab, bool isReal, ArtifactFragment collectedFragment, bool isFromInventory)
    {
        if (titleText != null) titleText.text = fName;
        if (descriptionText != null) descriptionText.text = fDescription;

        currentIsReal = isReal;
        currentFragment = collectedFragment;

        if (current3DModel != null) Destroy(current3DModel);

        if (artifactPrefab != null && pedestal != null)
        {
            current3DModel = Instantiate(artifactPrefab, pedestal);

            // THE FIX: Force the cloned model to be visible, even if the original is hidden in the backpack!
            current3DModel.SetActive(true);

            current3DModel.transform.localPosition = Vector3.zero;

            // Pinapanatili ang saktong default size ng iyong prefab
            current3DModel.transform.localScale = artifactPrefab.transform.localScale;

            Collider col = current3DModel.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        // --- THE STATEFUL UI LOGIC ---
        // Dynamically toggles buttons based on where the player clicked from
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
    }

    void Update()
    {
        // GUMAGANA PAREHO SA PC (MOUSE) AT MOBILE (TOUCH/SWIPE)
        if (gameObject.activeSelf && current3DModel != null)
        {
            // Kapag pinindot o hinawakan ang screen
            if (Input.GetMouseButtonDown(0))
            {
                previousMousePosition = Input.mousePosition;
            }

            // Habang kinakaladkad (dragging/swiping) ang artifact
            if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - previousMousePosition;

                // Math para sa 360 degree observation (Kaliwa-Kanan at Taas-Baba)
                float rotateX = delta.x * rotationSpeed;
                float rotateY = delta.y * rotationSpeed;

                // I-rotate ang model sa World space para hindi nakakalito ang axis kapag nakabaligtad na
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
                // --- CONNECT TO TRACKER SYSTEM DOWNGRADE TRIGGER ---
                // Dito natin hahanapin ang MapTrackerManager para sabihing nabura na ang fragment na ito
                MapTrackerManager trackerManager = FindObjectOfType<MapTrackerManager>();
                if (trackerManager != null)
                {
                    // Tinatanggal ang kaukulang hologram point bago itago ang fragment object
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
                    Debug.Log("FAIL: Fake Artifact Collected! They wasted a slot.");
                }
            }
            else
            {
                Debug.LogWarning("Item left on the street because the backpack is full!");
            }
        }
        else
        {
            Debug.LogError("CRITICAL ERROR: Cannot process collection! The passed 'currentFragment' or 'playerInventory' is null.");
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