using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("System Link")]
    public DialogueManager dialogueManager;

    [Header("Main Conversation Thread")]
    public DialogueLine[] conversation;

    [Header("Quest Progression Triggers")]
    public bool triggerTabletNotificationOnEnd = false;

    [Header("Quest Dependency Settings")]
    [Tooltip("Check this ON for Rex Barragan so he requires an NPC roadblock check.")]
    public bool requiresPreviousNPCDone = false;
    
    [Tooltip("Drag the first NPC (Almeda) here so this NPC can check if she's done talking!")]
    public NPCInteraction preRequisiteNPC;

    [Header("Locked Feedback Settings")]
    [Tooltip("If this has text (Size > 0), the NPC is CLICKABLE and plays this text. If Empty (Size 0), the NPC is completely UNCLICKABLE until Almeda is done.")]
    public DialogueLine[] lockedFallbackConversation;

    // --- NEW PHONE CALL OVERRIDE SETTINGS ---
    [Header("--- PHONE CALL OVERRIDE SETTINGS ---")]
    [Tooltip("Check this box true ONLY on your Caller_NPC object!")]
    public bool isPhoneCaller = false; 

    [Tooltip("Type the customized display name for this caller right here.")]
    public string callerName = "HQ Dispatch";

    [Tooltip("Type the customized display phone number for this caller right here.")]
    public string callerPhoneNumber = "09XX-XXX-XXXX";

    [Tooltip("Drag the CallerName TextMeshPro UI component here (the one currently saying 'New Text').")]
    public TMPro.TextMeshProUGUI phoneCallerNameText;

    [Tooltip("Drag the CallerNumber TextMeshPro UI component here.")]
    public TMPro.TextMeshProUGUI phoneCallerNumberText;

    [Tooltip("Drag the empty Profile Circle or CallerAvatar UI Image component here.")]
    public UnityEngine.UI.Image phoneProfileImageDisplay;

    [Tooltip("Put the customized portrait picture sprite for this specific caller right here!")]
    public Sprite callerProfileSprite;

    [Header("Runtime State Debugger")]
    public bool hasCompletedConversation = false;

    /// <summary>
    /// Call this function via the UI Button components (On Click) for the btnAnswer button!
    /// </summary>
    public void StartPhoneCallDialogue()
    {
        Debug.Log("📞 btnAnswer clicked! Activating dialogue manager subtitle feed stream...");

        // TRIGGER THE NARRATIVE CONVERSATION SUBTITLES (Your old system)
        if (dialogueManager != null)
        {
            if (conversation != null && conversation.Length > 0)
            {
                dialogueManager.gameObject.SetActive(true);
                dialogueManager.StartDialogue(conversation, triggerTabletNotificationOnEnd);
                hasCompletedConversation = true; 
            }
        }
    }

    private void OnMouseDown()
    {
        // --- 1. TABLET PANEL INPUT PROTECTION ---
        GameObject tabletPanel = GameObject.Find("TabletPanel");
        if (tabletPanel != null && tabletPanel.activeInHierarchy)
        {
            return; 
        }

        // --- 🔥 ONE-TIME CONVERSATION LOCK ---
        // If the interaction loop has already finished cleanly once, ignore all subsequent clicks
        if (hasCompletedConversation)
        {
            Debug.Log($"🔒 Click Ignored: Conversation thread for {gameObject.name} has already been completed. Locked to a single-use interaction state!");
            return; // Halts execution frame immediately so the dialogue system isn't invoked again!
        }

        // --- 2. QUEST FLOW DEPENDENCY VALIDATION ---
        if (requiresPreviousNPCDone && preRequisiteNPC != null)
        {
            // If Dr. Almeda has NOT finished her conversation yet...
            if (!preRequisiteNPC.hasCompletedConversation)
            {
                // DYNAMIC CLICKABLE TOGGLE:
                if (lockedFallbackConversation != null && lockedFallbackConversation.Length > 0)
                {
                    // Size is greater than 0 -> NPC is clickable! Play the fallback message.
                    Debug.Log($"🔒 Interaction restricted on {gameObject.name}. Activating fallback.");
                    
                    if (dialogueManager != null)
                    {
                        // FORCE WAKE UP: Ensures the Dialogue Manager's UI box is set to Active on screen!
                        dialogueManager.gameObject.SetActive(true);
                        
                        // Fire the fallback lines
                        dialogueManager.StartDialogue(lockedFallbackConversation, false);
                    }
                    else
                    {
                        Debug.LogError($"🚨 {gameObject.name} is missing the DialogueManager reference link in the Inspector slot!");
                    }
                }
                else
                {
                    // Size is 0 -> NPC is completely UNCLICKABLE! Ignore the click entirely.
                    Debug.Log($"🔒 Click ignored. {gameObject.name} is unclickable because Locked Fallback Conversation is empty.");
                }
                
                return; // Stop execution here so the main conversation doesn't run!
            }
        }

        // --- 3. NORMAL INTERACTION SEQUENCE (Unlocked state or no dependencies) ---
        if (dialogueManager != null)
        {
            if (conversation != null && conversation.Length > 0)
            {
                dialogueManager.gameObject.SetActive(true); // Force wake up for main dialogue too
                dialogueManager.StartDialogue(conversation, triggerTabletNotificationOnEnd);
                hasCompletedConversation = true; 
                Debug.Log($"✅ Story Flag Updated: Started main thread with {gameObject.name}.");
            }
        }
    }
}