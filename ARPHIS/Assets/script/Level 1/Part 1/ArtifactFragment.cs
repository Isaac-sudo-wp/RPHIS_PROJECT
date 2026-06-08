using UnityEngine;

public class ArtifactFragment : MonoBehaviour
{
    [Header("UI Connection")]
    public InspectUIManager uiManager;

    [Header("Fragment Data")]
    public string fragmentName = "FRAGMENT 1";
    public string category = "Wood Carving";
    public string weight = "1.2 kg";
    [TextArea(4, 10)]
    public string lore = "Sample lore here...";

    [Header("3D Model Setup")]
    public GameObject artifactPrefab;
    public bool isRealArtifact = false;

    [Header("Inventory Graphic")]
    public Sprite inventoryIcon;

    private void OnMouseDown()
    {
        if (uiManager != null)
        {
            // 'this' tells Unity to hand over this ENTIRE script to the UI Manager.
            // 'false' tells the UI that this is being picked up from the street.
            uiManager.OpenInspectWindow(fragmentName, category, weight, lore, artifactPrefab, isRealArtifact, this, false);
        }
        else
        {
            Debug.LogWarning("UI Manager is not linked!");
        }
    }
}