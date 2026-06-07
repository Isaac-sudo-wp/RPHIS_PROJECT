using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InspectUIManager : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI loreText;

    [Header("UI Buttons")]
    public GameObject collectButton;
    public GameObject ignoreButton;
    public GameObject closeButton; // The new button for Inventory mode!

    [Header("3D Viewer Setup")]
    public Transform pedestal;
    private GameObject current3DModel;
    private bool currentIsReal;

    [Header("Inventory Link")]
    public InventoryManager playerInventory;
    private ArtifactFragment currentFragment;

    // NEW: Added 'bool isFromInventory' to the very end of this list!
    public void OpenInspectWindow(string fName, string fCategory, string fWeight, string fLore, GameObject artifactPrefab, bool isReal, ArtifactFragment collectedFragment, bool isFromInventory)
    {
        titleText.text = fName;
        categoryText.text = fCategory;
        weightText.text = fWeight;
        loreText.text = fLore;
        currentIsReal = isReal;
        currentFragment = collectedFragment;

        if (current3DModel != null) Destroy(current3DModel);

        if (artifactPrefab != null)
        {
            current3DModel = Instantiate(artifactPrefab, pedestal);
            current3DModel.transform.localPosition = Vector3.zero;

            // Forces the scale to match perfectly (keeping our fix from earlier!)
            current3DModel.transform.localScale = artifactPrefab.transform.localScale;

            Collider col = current3DModel.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        // --- THE STATEFUL UI LOGIC ---
        if (isFromInventory == true)
        {
            // Inspecting from backpack: Hide Collect/Ignore, Show Close
            if (collectButton != null) collectButton.SetActive(false);
            if (ignoreButton != null) ignoreButton.SetActive(false);
            if (closeButton != null) closeButton.SetActive(true);
        }
        else
        {
            // Inspecting from street: Show Collect/Ignore, Hide Close
            if (collectButton != null) collectButton.SetActive(true);
            if (ignoreButton != null) ignoreButton.SetActive(true);
            if (closeButton != null) closeButton.SetActive(false);
        }

        gameObject.SetActive(true);
    }

    public void OnCollectClicked()
    {
        if (playerInventory != null && currentFragment != null)
        {
            // Ask the inventory to take it, and remember the answer (true or false)
            bool itemWasAccepted = playerInventory.AddItemToInventory(currentFragment);

            if (itemWasAccepted == true)
            {
                // THE FIX: ONLY disappear if the inventory actually accepted it!
                currentFragment.gameObject.SetActive(false);

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
                // If false, it skips the vanishing code entirely!
                Debug.LogWarning("Item left on the street because the backpack is full!");
            }
        }

        // Close the window so the player can keep walking
        gameObject.SetActive(false);
    }
    public void OnIgnoreClicked() { gameObject.SetActive(false); }

    // NEW: A simple function for your new Close button
    public void OnCloseClicked() { gameObject.SetActive(false); }

    public void RotateLeft() { if (current3DModel != null) current3DModel.transform.Rotate(0, 45f, 0, Space.World); }
    public void RotateRight() { if (current3DModel != null) current3DModel.transform.Rotate(0, -45f, 0, Space.World); }
    public void ZoomIn() { if (current3DModel != null) current3DModel.transform.localScale *= 1.2f; }
    public void ResetView() { if (current3DModel != null) { current3DModel.transform.localRotation = Quaternion.Euler(0, 0, 0); current3DModel.transform.localScale = new Vector3(1, 1, 1); } }
}