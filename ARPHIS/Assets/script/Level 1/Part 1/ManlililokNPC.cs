using UnityEngine;

public class ManlililokNPC : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 4f;
    private Transform playerTransform;

    [Header("Dialogue System Links")]
    public DialogueManager dialogueManager;
    public GameObject puzzleCutsceneUI;

    [Header("Quest Objects to Reveal")]
    [Tooltip("Drag the Chisel 3D object from the Hierarchy here.")]
    public GameObject chiselObject;
    [Tooltip("Drag the Wood Glue 3D object from the Hierarchy here.")]
    public GameObject woodGlueObject;

    [Header("Conversations")]
    public DialogueLine[] missingFragmentsConversation;
    public DialogueLine[] introConversation;
    public DialogueLine[] missingToolsReminderConversation;
    public DialogueLine[] successConversation;

    private bool hasGivenToolQuest = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void OnMouseDown()
    {
        if (playerTransform == null || dialogueManager == null) return;

        float distance = Vector3.Distance(playerTransform.position, transform.position);
        if (distance > interactionRange)
        {
            Debug.Log("Too far away!");
            return;
        }

        InventoryManager inventory = FindObjectOfType<InventoryManager>();

        if (inventory != null)
        {
            bool hasFragments = inventory.HasAllFragments();
            bool hasTools = inventory.HasChiselAndGlue();

            // STAGE 2 & 3: The player brought back 4 fragments...
            if (hasFragments)
            {
                // STAGE 3: Player has Fragments AND Tools
                if (hasTools)
                {
                    // 🔥 FIX: Clear any existing callbacks before starting
                    dialogueManager.ClearCallback();
                    
                    // Open the puzzle UI
                    dialogueManager.StartDialogueAndOpenPanel(successConversation, puzzleCutsceneUI);
                }
                // STAGE 2: Player has Fragments, but NO Tools yet
                else
                {
                    if (hasGivenToolQuest == false)
                    {
                        // 🔥 FIX: Clear any existing callbacks
                        dialogueManager.ClearCallback();
                        
                        // 1. Play the long story about Baticulin wood
                        dialogueManager.StartDialogue(introConversation);

                        // 2. REVEAL THE TOOLS!
                        if (chiselObject != null) chiselObject.SetActive(true);
                        if (woodGlueObject != null) woodGlueObject.SetActive(true);

                        hasGivenToolQuest = true;
                    }
                    else
                    {
                        // 🔥 FIX: Clear any existing callbacks
                        dialogueManager.ClearCallback();
                        
                        // Reminder
                        dialogueManager.StartDialogue(missingToolsReminderConversation);
                    }
                }
            }
            // STAGE 1: Player DOES NOT have 4 fragments yet
            else
            {
                dialogueManager.StartDialogue(missingFragmentsConversation);
            }
        }
    }
}