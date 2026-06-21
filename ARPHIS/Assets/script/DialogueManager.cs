using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string sentenceText;

    [Header("Optional Event")]
    public GameObject documentPanelToShow; // Leave empty unless you want a document to pop up after this line!
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI sentenceDisplay;

    [Header("Gameplay UI Reference")]
    public GameObject inGamePanel; // Drag your 'InGamePanel' from the Hierarchy into this slot!

    [Header("Optional Notification Triggers")]
    public GameObject tabletNotificationBadge; // Drag your red notification circle here!
    private bool shouldUnlockNotificationOnEnd = false;

    private Queue<DialogueLine> dialogueQueue;
    private bool isDocumentOpen = false;

    // Using OnEnable ensures the queue initializes the very second the UI wakes up
    void OnEnable()
    {
        if (dialogueQueue == null)
        {
            dialogueQueue = new Queue<DialogueLine>();
        }
    }

    // Overloaded method: Call this from the NPC script to pass a notification trigger flag
    public void StartDialogue(DialogueLine[] fullConversation, bool triggerNotificationAtEnd)
    {
        shouldUnlockNotificationOnEnd = triggerNotificationAtEnd;
        StartDialogue(fullConversation);
    }

    public void StartDialogue(DialogueLine[] fullConversation)
    {
        // Failsafe: Instantly creates the queue if Unity lifecycle execution skipped Awake/OnEnable
        if (dialogueQueue == null)
        {
            dialogueQueue = new Queue<DialogueLine>();
        }

        // 1. Turn ON the dialogue UI box
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        // 2. Turn OFF the standard gameplay UI (joystick, HUD buttons)
        if (inGamePanel != null) inGamePanel.SetActive(false);

        isDocumentOpen = false;
        dialogueQueue.Clear();

        foreach (DialogueLine line in fullConversation)
        {
            dialogueQueue.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Block the next button if the player is currently reading a document
        if (isDocumentOpen) return;

        // Failsafe check to ensure the queue exists and has lines remaining
        if (dialogueQueue == null || dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = dialogueQueue.Dequeue();

        if (nameDisplay != null) nameDisplay.text = currentLine.speakerName;
        if (sentenceDisplay != null) sentenceDisplay.text = currentLine.sentenceText;

        // If this specific line has a document assigned, trigger it!
        if (currentLine.documentPanelToShow != null)
        {
            TriggerDocument(currentLine.documentPanelToShow);
        }
    }

    private void TriggerDocument(GameObject docPanel)
    {
        isDocumentOpen = true;

        // Open the multi-page registry document overlay
        if (docPanel != null) docPanel.SetActive(true);

        // HIDE the dialogue UI panel box so it doesn't clutter or block the screen view!
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    // Call this function when the player clicks the Exit/Close button on the Document Panel
    public void ResumeDialogueAfterDocument(GameObject docPanel)
    {
        if (docPanel != null) docPanel.SetActive(false);
        isDocumentOpen = false;

        // Bring back the dialogue UI panel box so they can read the next lines!
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        // Automatically progress to the next line of dialogue smoothly
        DisplayNextSentence();
    }

    public void EndDialogue()
    {
        // 1. Turn OFF the dialogue UI box completely
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // 2. Bring back the gameplay UI safely so the player can walk around again!
        if (inGamePanel != null) inGamePanel.SetActive(true);

        // 3. NEW: If this specific conversation requested it, trigger the notification badge on now!
        if (shouldUnlockNotificationOnEnd && tabletNotificationBadge != null)
        {
            tabletNotificationBadge.SetActive(true);
        }

        // Reset the runtime track tracker flag safely
        shouldUnlockNotificationOnEnd = false;

        Debug.Log("Dialogue interaction finished completely.");
    }
}
