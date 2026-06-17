using UnityEngine;
using TMPro;

public class ItemDisplayManager : MonoBehaviour
{
    [Header("UI Connections")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText; // CHANGED: From itemLoreText to itemDescriptionText

    [Header("Photo Studio Setup")]
    public Transform studioSpawnPoint;
    private GameObject current3DModel;
    public float spinSpeed = 30f;

    [Header("System Links")]
    public InventoryManager inventoryManager;
    public InspectUIManager inspectWindow;

    [Header("System UI Canvas")]
    // NEW: We need a link to the main inventory screen so we can hide it!
    public GameObject inventoryCanvas;

    private ArtifactFragment currentItemData;

    void Update()
    {
        if (studioSpawnPoint != null && current3DModel != null)
        {
            studioSpawnPoint.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
        }
    }

    public void UpdateDisplayPanel(ArtifactFragment itemData)
    {
        if (itemData == null) return;

        currentItemData = itemData;

        if (itemNameText != null) itemNameText.text = itemData.fragmentName;
        
        // FIXED: Changed from .lore to .descriptionText
        if (itemDescriptionText != null) itemDescriptionText.text = itemData.descriptionText;

        if (current3DModel != null) Destroy(current3DModel);

        if (itemData.artifactPrefab != null)
        {
            current3DModel = Instantiate(itemData.artifactPrefab, studioSpawnPoint);
            current3DModel.SetActive(true);
            current3DModel.transform.localPosition = Vector3.zero;
        }
    }

    public void OnDropClicked()
    {
        if (currentItemData != null && inventoryManager != null)
        {
            inventoryManager.DropItem(currentItemData);

            currentItemData = null;
            if (itemNameText != null) itemNameText.text = "";
            if (itemDescriptionText != null) itemDescriptionText.text = ""; // FIXED: Clear description text
            if (current3DModel != null) Destroy(current3DModel);
        }
    }

    public void OnInspectClicked()
    {
        if (currentItemData != null && inspectWindow != null)
        {
            // FIXED: Removed .category and .weight parameters, changed .lore to .descriptionText
            inspectWindow.OpenInspectWindow(
                currentItemData.fragmentName,
                currentItemData.descriptionText,
                currentItemData.artifactPrefab,
                currentItemData.isRealArtifact,
                currentItemData,
                true
            );

            // NEW: Hide the inventory screen so it stops blocking the Inspect Window!
            if (inventoryCanvas != null)
            {
                inventoryCanvas.SetActive(false);
            }
        }
    }
}