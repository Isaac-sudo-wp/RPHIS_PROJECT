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
    public GameObject documentPanelToShow;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI sentenceDisplay;

    [Header("Gameplay UI Reference")]
    public GameObject inGamePanel;

    [Header("Optional Notification Triggers")]
    public GameObject tabletNotificationBadge;
    private bool shouldUnlockNotificationOnEnd = false;

    // --- NEW: Variable to hold the Puzzle UI so it opens when dialogue ends ---
    private GameObject panelToOpenAfterDialogue;

    private Queue<DialogueLine> dialogueQueue;
    private bool isDocumentOpen = false;

    void OnEnable()
    {
        if (dialogueQueue == null)
        {
            dialogueQueue = new Queue<DialogueLine>();
        }
    }

    public void StartDialogue(DialogueLine[] fullConversation, bool triggerNotificationAtEnd)
    {
        shouldUnlockNotificationOnEnd = triggerNotificationAtEnd;
        StartDialogue(fullConversation);
    }

    // --- NEW: Call this from Paeng's script to open the puzzle when he finishes talking! ---
    public void StartDialogueAndOpenPanel(DialogueLine[] fullConversation, GameObject panelToOpen)
    {
        panelToOpenAfterDialogue = panelToOpen;
        StartDialogue(fullConversation); // Calls the normal start function below
    }

    public void StartDialogue(DialogueLine[] fullConversation)
    {
        if (dialogueQueue == null)
        {
            dialogueQueue = new Queue<DialogueLine>();
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
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
        if (isDocumentOpen) return;

        if (dialogueQueue == null || dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = dialogueQueue.Dequeue();

        if (nameDisplay != null) nameDisplay.text = currentLine.speakerName;
        if (sentenceDisplay != null) sentenceDisplay.text = currentLine.sentenceText;

        if (currentLine.documentPanelToShow != null)
        {
            TriggerDocument(currentLine.documentPanelToShow);
        }
    }

    private void TriggerDocument(GameObject docPanel)
    {
        isDocumentOpen = true;
        if (docPanel != null) docPanel.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void ResumeDialogueAfterDocument(GameObject docPanel)
    {
        if (docPanel != null) docPanel.SetActive(false);
        isDocumentOpen = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        DisplayNextSentence();
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Default: bring back the gameplay UI so the player can walk around
        if (inGamePanel != null) inGamePanel.SetActive(true);

        if (shouldUnlockNotificationOnEnd && tabletNotificationBadge != null)
        {
            tabletNotificationBadge.SetActive(true);
        }
        shouldUnlockNotificationOnEnd = false;

        // --- NEW: If a puzzle panel is waiting, open it now! ---
        if (panelToOpenAfterDialogue != null)
        {
            panelToOpenAfterDialogue.SetActive(true);

            // Turn the joystick/HUD back OFF so they can't walk away during the puzzle!
            if (inGamePanel != null) inGamePanel.SetActive(false);

            panelToOpenAfterDialogue = null; // Clear it out for the next conversation
        }

        Debug.Log("Dialogue interaction finished completely.");
    }
}