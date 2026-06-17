using UnityEngine;

public class ArtifactFragment : MonoBehaviour
{
    [Header("UI Connection")]
    public InspectUIManager uiManager;

    [Header("Fragment Data")]
    public string fragmentName = "FRAGMENT 1";
    
    [TextArea(4, 10)]
    public string descriptionText = "Sample description here..."; 

    [Header("3D Model Setup")]
    public GameObject artifactPrefab;
    public bool isRealArtifact = false;

    [Header("Inventory Graphic")]
    public Sprite inventoryIcon;

    [Header("Story Progression Lock")]
    [Tooltip("Drag Rex Barragan here so this fragment knows when to reveal itself!")]
    public NPCInteraction rexBarraganNPC;

    private MeshRenderer meshRenderer;
    private Collider fragmentCollider; // FIXED: Changed '3dCollider' to a legal identifier string name!
    private bool isRevealed = false;

    private void Awake()
    {
        // Capture the renderer handling the 3D meshes visual data matrix
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Failsafe: If your low-poly model graphics sit inside a child object, search there
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        // Capture the 3D physics collider automatically
        fragmentCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        // If Rex Barragan is linked, BOTH real and fake items hide instantly at boot.
        if (rexBarraganNPC != null)
        {
            // 1. Turn off the mesh renderer immediately so it is hidden from the player's eyes
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            // 2. Disable the physics collider shape too so players can't click an invisible asset box
            if (fragmentCollider != null)
            {
                fragmentCollider.enabled = false;
            }

            isRevealed = false;
        }
        else
        {
            // If no NPC is linked at all, default to visible and clickable
            isRevealed = true;
        }
    }

    private void Update()
    {
        // If the item has already materialized, stop processing tracking checks
        if (isRevealed) return;

        // Poll if Rex Barragan's story progression flag has flipped to true
        if (rexBarraganNPC != null && rexBarraganNPC.hasCompletedConversation)
        {
            RevealFragment();
        }
    }

    private void RevealFragment()
    {
        isRevealed = true;
        
        // Materialize visual mesh grids
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true; 
        }

        // Reactivate 3D click bounds cleanly
        if (fragmentCollider != null)
        {
            fragmentCollider.enabled = true;
        }

        Debug.Log($"✨ Fragment Materialized: {fragmentName} (Real: {isRealArtifact}) is now fully visible and interactable.");
    }

    private void OnMouseDown()
    {
        // BLOCK INPUT: If the fragment hasn't been officially revealed yet, ignore clicks!
        if (!isRevealed) return;

        // --- 🔥 DIALOGUE & PHONE CALL INPUT PROTECTION ---
        // 1. Check if the central Dialogue Subtitle Box panel layout is active
        GameObject dialoguePanel = GameObject.Find("DialoguePanel");
        if (dialoguePanel != null && dialoguePanel.activeInHierarchy)
        {
            Debug.Log($"🚫 Interaction Blocked: Cannot pickup {fragmentName} while an NPC dialogue subtitle box is active on screen!");
            return; // Exit instantly
        }

        // 2. Check if the Tablet's active incoming phone call background screen layout is open
        GameObject callPanel = GameObject.Find("InCallBackground");
        if (callPanel != null && callPanel.activeInHierarchy)
        {
            Debug.Log($"🚫 Interaction Blocked: Cannot pickup {fragmentName} during active phone transmission stream layouts!");
            return; // Exit instantly
        }

        // --- EXISTING TABLET LOCK STRATEGIES ---
        // 1. CHOOSE A TABLET LOCK STRATEGY:
        GameObject tabletPanel = GameObject.Find("TabletPanel");

        if (tabletPanel != null && tabletPanel.activeInHierarchy)
        {
            return; 
        }

        // 2. ALTERNATIVE STRATEGY:
        if (uiManager != null && uiManager.gameObject.activeInHierarchy)
        {
            // return;
        }

        // 3. Run your authentic item pickup execution sequence safely
        if (uiManager != null)
        {
            uiManager.OpenInspectWindow(fragmentName, descriptionText, artifactPrefab, isRealArtifact, this, false);
        }
        else
        {
            Debug.LogWarning("UI Manager is not linked on fragment: " + gameObject.name);
        }
    }
}