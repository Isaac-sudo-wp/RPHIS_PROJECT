using UnityEngine;
using System.Collections.Generic;

public class MapTrackerManager : MonoBehaviour
{
    [Header("References")]
    public Transform hologramPlane;
    public GameObject trackerPrefab;
    
    [Header("Tracker Settings")]
    public float trackerHeightOffset = 0.5f;
    public Color trackerColor = Color.red;
    public Vector3 trackerScale = new Vector3(2f, 2f, 2f);
    
    [Header("Story Progression Lock")]
    [Tooltip("Drag Dr. Almeda's GameObject here to lock tracking initialization until she is talked to.")]
    public NPCInteraction almedaNPC;

    [Header("Tablet Notification UI Link")]
    [Tooltip("Drag the Red Notification Dot/Badge UI GameObject right here!")]
    public GameObject redNotificationDot;

    // --- EMERGENCY CALL OVERRIDE PARAMETERS ---
    [Header("Incoming Call System Link")]
    [Tooltip("Drag your InCallBackground UI panel parent right here!")]
    public GameObject inCallBackground;
    
    [Tooltip("Drag your normal ImgMapView or Map Panel here so we can hide it during an emergency!")]
    public GameObject imgMapView;

    private Dictionary<Transform, GameObject> activeTrackers = new Dictionary<Transform, GameObject>();
    private bool trackerHasUnlocked = false;
    private bool fragmentsCollectedPhaseTriggered = false;
    
    // The simplified target counter tracking system
    private int collectedCount = 0;

    void Start()
    {
        Debug.Log("=== MAP TRACKER MANAGER INITIALIZED (AWAITING ALMEDA PROGRESSION FLAG) ===");
        
        // Safety lock: Make sure call layout displays are hidden when the scene boots up
        if (inCallBackground != null) inCallBackground.SetActive(false);
        if (redNotificationDot != null) redNotificationDot.SetActive(false);
    }

    void Update()
    {
        // 1. If tracking system is active, update visual sphere tracker points to follow fragments on the grid
        if (trackerHasUnlocked)
        {
            if (hologramPlane == null) return;
            
            float planeY = hologramPlane.position.y;
            
            foreach (var pair in activeTrackers)
            {
                if (pair.Key != null && pair.Value != null)
                {
                    pair.Value.transform.position = new Vector3(pair.Key.position.x, planeY + trackerHeightOffset, pair.Key.position.z);
                }
            }
            return;
        }

        // 2. Poll if Dr. Almeda's conversation flag has turned true
        if (almedaNPC != null && almedaNPC.hasCompletedConversation)
        {
            trackerHasUnlocked = true;
            Debug.Log("🛰️ MapTrackerManager: Authorization confirmed. Spawning fragment tracking grid arrays...");
            CreateTrackers();
        }
    }

    /// <summary>
    /// Call this function via the UI Tablet Button component (On Click) event list layout!
    /// </summary>
    public void OnPlayerOpenedMap()
    {
        // Intercept standard panel opening routine if 5 fragments have been completely retrieved
        if (fragmentsCollectedPhaseTriggered)
        {
            if (inCallBackground != null)
            {
                inCallBackground.SetActive(true); // Force the call screen up automatically!
                Debug.Log("🚨 Emergency Transmission Override: Opening call window automatically inside Tablet viewport.");

                // 🔥 AUTOMATED DATA INJECTION: Push custom caller text attributes onto the UI screen immediately while ringing!
                NPCInteraction callerNPC = FindObjectOfType<NPCInteraction>();
                if (callerNPC != null && callerNPC.isPhoneCaller)
                {
                    // Hunt down the UI Text elements dynamically by their exact GameObject names
                    TMPro.TextMeshProUGUI nameText = GameObject.Find("CallerName")?.GetComponent<TMPro.TextMeshProUGUI>();
                    TMPro.TextMeshProUGUI numberText = GameObject.Find("CallerNumber")?.GetComponent<TMPro.TextMeshProUGUI>();

                    if (nameText != null)
                    {
                        nameText.gameObject.SetActive(true);
                        nameText.text = callerNPC.callerName; // Overwrites "New Text" with "Mr. Francisco" instantly!
                        nameText.ForceMeshUpdate(true);
                        Debug.Log($"🎯 Ringing Overrides Applied: Name UI mesh forced to display '{callerNPC.callerName}'");
                    }

                    if (numberText != null)
                    {
                        numberText.gameObject.SetActive(true);
                        numberText.text = callerNPC.callerPhoneNumber; // Overwrites template with your phone number string!
                        numberText.ForceMeshUpdate(true);
                        Debug.Log($"🎯 Ringing Overrides Applied: Number UI mesh forced to display '{callerNPC.callerPhoneNumber}'");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ MapTrackerManager: Call screen opened, but could not locate a valid 'Caller_NPC' or 'isPhoneCaller' is unchecked.");
                }
            }

            if (imgMapView != null)
            {
                imgMapView.SetActive(false); // Hide standard layout tracking maps
            }
        }

        // Wipe notification badge away upon interaction click
        if (redNotificationDot != null)
        {
            redNotificationDot.SetActive(false);
        }
    }

    void CreateTrackers()
    {
        GameObject fragmentsGroup = GameObject.Find("Fragments");
        if (fragmentsGroup == null)
        {
            Debug.LogError("❌ Cannot find 'Fragments' parent container object in the scene layout!");
            return;
        }

        if (hologramPlane == null)
        {
            Debug.LogError("❌ HologramPlane reference not assigned in the inspector!");
            return;
        }

        int hologramLayer = LayerMask.NameToLayer("HologramMap");
        float planeY = hologramPlane.position.y;

        foreach (Transform child in fragmentsGroup.transform)
        {
            // Track BOTH real and fake items container nodes
            if (!child.name.Contains("paete_fragment")) continue;

            Vector3 trackerPos = new Vector3(child.position.x, planeY + trackerHeightOffset, child.position.z);
            GameObject tracker;
            
            if (trackerPrefab != null)
            {
                tracker = Instantiate(trackerPrefab, trackerPos, Quaternion.identity);
                tracker.transform.localScale = trackerScale;
            }
            else
            {
                tracker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tracker.transform.localScale = trackerScale;
                
                Renderer rend = tracker.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = trackerColor;
                    rend.material.SetColor("_EmissionColor", trackerColor);
                }
            }
            
            tracker.layer = hologramLayer;
            tracker.name = "Tracker_" + child.name;
            tracker.transform.SetParent(hologramPlane);
            
            activeTrackers.Add(child, tracker);
        }
        
        Debug.Log($"🎯 Created {activeTrackers.Count} map tracking point meshes.");
    }
    
    /// <summary>
    /// Triggered directly via InspectUIManager when an item pickup transaction goes through successfully!
    /// </summary>
    public void RemoveTracker(Transform fragment)
    {
        if (fragment == null) return;

        // 1. Clean up visual hologram dots on your mini-map grid if tracking matches
        if (activeTrackers.ContainsKey(fragment))
        {
            if (activeTrackers[fragment] != null) Destroy(activeTrackers[fragment]);
            activeTrackers.Remove(fragment);
        }

        // 2. Increment global safe retrieval integer index tracker 
        collectedCount++;
        Debug.Log($"📥 Fragment collected! Tracking progress pool index: {collectedCount}/5.");

        // 3. TARGET RECOVERY GATE CHECK: Once index hits 5, initiate phone network override sequence!
        if (collectedCount >= 5)
        {
            TriggerIncomingCallSequence();
        }
    }

    private void TriggerIncomingCallSequence()
    {
        fragmentsCollectedPhaseTriggered = true;
        
        // Light up the red warning dot asset right over your btnTablet interface layer location cleanly
        if (redNotificationDot != null)
        {
            redNotificationDot.SetActive(true);
            Debug.Log("🎯 UI Alert: Red notification badge activated over btnTablet layout!");
        }
        else
        {
            Debug.LogWarning("⚠️ MapTrackerManager: Call triggered, but 'redNotificationDot' is missing from the inspector slot layout!");
        }

        Debug.Log("📞 ALERT SYSTEM RUNNING: All 5 tracking slots processed! Emergency call connection queued up...");
    }
    
    public Dictionary<Transform, GameObject> GetActiveTrackers()
    {
        return activeTrackers;
    }
}