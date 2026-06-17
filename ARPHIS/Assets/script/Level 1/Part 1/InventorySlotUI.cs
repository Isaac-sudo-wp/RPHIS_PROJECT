using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public ArtifactFragment storedFragment;
    public bool hasItem = false;

    private ItemDisplayManager displayManager;

    void Start()
    {
        // Connect to the TV Screen Brain
        displayManager = FindObjectOfType<ItemDisplayManager>();

        // BUG FIX: Search for the Button on this object AND its children
        Button slotButton = GetComponentInChildren<Button>();

        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClick);
        }
        else
        {
            Debug.LogError("CRITICAL ERROR: No Button component found on " + gameObject.name + " or its children!");
        }
    }

    public void OnSlotClick()
    {
        Debug.Log(">>> SLOT WAS CLICKED: " + gameObject.name);

        // THE FIX: Search for the TV screen right at the exact moment of the click!
        if (displayManager == null)
        {
            // The 'true' tells Unity to find it even if it's hiding!
            displayManager = FindObjectOfType<ItemDisplayManager>(true);
        }

        // Failsafes to prevent errors
        if (!hasItem)
        {
            Debug.Log("--- The slot is empty, ignoring click.");
            return;
        }

        if (storedFragment == null)
        {
            Debug.LogError("--- ERROR: The slot says it has an item, but the fragment data is missing!");
            return;
        }

        if (displayManager == null)
        {
            Debug.LogError("--- ERROR: Still cannot find the Item Display Manager! Is it attached to the GameManager?");
            return;
        }

        // If we pass all checks, send the data to the bottom panel!
        displayManager.UpdateDisplayPanel(storedFragment);
        Debug.Log("--- SUCCESS: Sent " + storedFragment.fragmentName + " to the bottom panel!");
    }
}