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
    public NPCInteraction rexBarraganNPC;

    [Header("Mobile Proximity Optimization (Backup Scan)")]
    [Tooltip("If standard touch detection drops, this radius guarantees interaction when close to the target.")]
    public float directInteractRange = 3.5f;

    private MeshRenderer meshRenderer;
    private Collider fragmentCollider;
    private Transform cachedPlayerTransform;
    private bool isRevealed = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = GetComponentInChildren<MeshRenderer>();
        
        fragmentCollider = GetComponent<Collider>();
    }

    void Start()
    {
        if (fragmentCollider != null)
        {
            fragmentCollider.isTrigger = false;
            fragmentCollider.enabled = true;
            Debug.Log($"✅ Collider set up on {gameObject.name}");
        }

        // Automatic runtime lookup engine assignment
        if (uiManager == null)
            uiManager = FindObjectOfType<InspectUIManager>();

        // Cache the active player transform geometry matrix profile
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            cachedPlayerTransform = playerObj.transform;
        }

        if (rexBarraganNPC != null)
        {
            if (meshRenderer != null) meshRenderer.enabled = false;
            if (fragmentCollider != null) fragmentCollider.enabled = false;
            isRevealed = false;
            Debug.Log($"🔒 {fragmentName} is HIDDEN");
        }
        else
        {
            isRevealed = true;
            Debug.Log($"🔓 {fragmentName} is REVEALED");
        }
    }

    void Update()
    {
        if (!isRevealed && rexBarraganNPC != null && rexBarraganNPC.hasCompletedConversation)
        {
            RevealFragment();
        }

        // 🔥 CRITICAL DIRECT TOUCH FAILSAFE PIPELINE
        // Bypasses broken OnMouseDown events when UI layers mask screen pixel layouts
        if (Input.GetMouseButtonDown(0) && isRevealed)
        {
            EvaluateScreenTouchInput();
        }
    }

    private void EvaluateScreenTouchInput()
    {
        if (Camera.main == null) return;

        Ray interactionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;

        // Forceful physics layer test
        if (Physics.Raycast(interactionRay, out raycastHit))
        {
            if (raycastHit.collider.gameObject == this.gameObject || raycastHit.transform.IsChildOf(this.transform))
            {
                Debug.Log($"🎯 Direct Raycast Hit Verified on Target Mesh Component: {fragmentName}");
                TryPickup();
            }
        }
    }

    void RevealFragment()
    {
        isRevealed = true;
        if (meshRenderer != null) meshRenderer.enabled = true;
        if (fragmentCollider != null)
        {
            fragmentCollider.enabled = true;
            fragmentCollider.isTrigger = false;
        }
        Debug.Log($"✨ {fragmentName} REVEALED!");
    }

    // This method handles interaction routing safely
    public void TryPickup()
    {
        Debug.Log($" TryPickup called on {fragmentName} - isRevealed: {isRevealed}");
        
        if (!isRevealed)
        {
            Debug.LogWarning($"[{fragmentName}] Interaction blocked: Asset is not yet officially materialized.");
            return;
        }

        // Distance validation to prevent distant picking exploits
        if (cachedPlayerTransform != null)
        {
            float playerTargetDistance = Vector3.Distance(cachedPlayerTransform.position, transform.position);
            if (playerTargetDistance > directInteractRange)
            {
                Debug.LogWarning($"❌ Interaction Denied: Player coordinate distance evaluation ({playerTargetDistance:F2}) is out of target range thresholds.");
                return;
            }
        }

        if (IsAnyPanelOpen())
        {
            Debug.Log("⚠️ A subsystem panel interface configuration is active, ignoring input framework execution.");
            return;
        }

        if (uiManager != null)
        {
            uiManager.OpenInspectWindow(fragmentName, descriptionText, artifactPrefab, isRealArtifact, this, false);
            Debug.Log($"✅ Opening inspect for {fragmentName}");
        }
        else
        {
            Debug.LogError("❌ uiManager target initialization link reference is NULL!");
        }
    }

    private bool IsAnyPanelOpen()
    {
        GameObject dialoguePanel = GameObject.Find("DialoguePanel");
        if (dialoguePanel != null && dialoguePanel.activeInHierarchy) return true;

        GameObject callPanel = GameObject.Find("InCallBackground");
        if (callPanel != null && callPanel.activeInHierarchy) return true;

        GameObject tabletPanel = GameObject.Find("TabletPanel");
        if (tabletPanel != null && tabletPanel.activeInHierarchy) return true;

        return false;
    }

    // Retained for editor workspace click testing loops
    void OnMouseDown()
    {
        Debug.Log($" OnMouseDown on {fragmentName}");
        TryPickup();
    }
}