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

    private GameObject panelToOpenAfterDialogue;
    private GameObject pendingResultPanel;
    private bool hasPendingResultPanel = false;
    private System.Action onDialogueEndCallback;
    private Queue<DialogueLine> dialogueQueue;
    private bool isDocumentOpen = false;
    private bool isEndingDialogue = false;
    private bool isWaitingForCallback = false;
    private bool callbackExecuted = false;
    private bool isDialogueActive = false;

    void OnEnable()
    {
        if (dialogueQueue == null)
            dialogueQueue = new Queue<DialogueLine>();
        isEndingDialogue = false;
        callbackExecuted = false;
        isDialogueActive = false;
    }

    public void ClearCallback()
    {
        onDialogueEndCallback = null;
        panelToOpenAfterDialogue = null;
        pendingResultPanel = null;
        hasPendingResultPanel = false;
        isEndingDialogue = false;
        isWaitingForCallback = false;
        callbackExecuted = false;
        isDialogueActive = false;
        Debug.Log("🧹 DialogueManager callbacks cleared.");
    }

    public void StartDialogue(DialogueLine[] fullConversation, bool triggerNotificationAtEnd)
    {
        shouldUnlockNotificationOnEnd = triggerNotificationAtEnd;
        StartDialogue(fullConversation);
    }

    public void StartDialogueAndOpenPanel(DialogueLine[] fullConversation, GameObject panelToOpen)
    {
        Debug.Log($"✅ StartDialogueAndOpenPanel called. Panel: {(panelToOpen != null ? panelToOpen.name : "NULL")}");
        panelToOpenAfterDialogue = panelToOpen;
        pendingResultPanel = panelToOpen;
        hasPendingResultPanel = (panelToOpen != null);
        onDialogueEndCallback = null;
        isEndingDialogue = false;
        isWaitingForCallback = false;
        callbackExecuted = false;
        StartDialogueInternal(fullConversation);
    }

    public void StartDialogueWithCallback(DialogueLine[] fullConversation, System.Action callback)
    {
        Debug.Log("✅ StartDialogueWithCallback called.");
        Debug.Log($"   Dialogue lines: {fullConversation.Length}");
        onDialogueEndCallback = callback;
        panelToOpenAfterDialogue = null;
        pendingResultPanel = null;
        hasPendingResultPanel = false;
        isEndingDialogue = false;
        isWaitingForCallback = true;
        callbackExecuted = false;
        StartDialogueInternal(fullConversation);
    }

    public void StartDialogue(DialogueLine[] fullConversation)
    {
        panelToOpenAfterDialogue = null;
        pendingResultPanel = null;
        hasPendingResultPanel = false;
        onDialogueEndCallback = null;
        isEndingDialogue = false;
        isWaitingForCallback = false;
        callbackExecuted = false;
        StartDialogueInternal(fullConversation);
    }

    private void StartDialogueInternal(DialogueLine[] fullConversation)
    {
        if (dialogueQueue == null)
            dialogueQueue = new Queue<DialogueLine>();

        // 🔥 HIDE INGAMEPANEL when dialogue starts
        if (inGamePanel != null) 
        {
            inGamePanel.SetActive(false);
            Debug.Log("📖 InGame panel HIDDEN.");
        }

        if (dialoguePanel != null) 
        {
            dialoguePanel.SetActive(true);
            Debug.Log("📖 Dialogue panel opened.");
        }

        isDialogueActive = true;
        isDocumentOpen = false;
        dialogueQueue.Clear();

        foreach (DialogueLine line in fullConversation)
            dialogueQueue.Enqueue(line);

        DisplayNextSentence();
    }

    // 🔥 This is called by the "Next" button
    public void OnNextButtonClicked()
    {
        Debug.Log("🖱️ Next button clicked!");
        
        if (isDocumentOpen)
        {
            Debug.Log("📄 Document is open. Please close it first.");
            return;
        }

        if (dialogueQueue != null && dialogueQueue.Count > 0)
        {
            DisplayNextSentence();
        }
        else
        {
            Debug.Log("📖 No more sentences. Ending dialogue.");
            EndDialogue();
        }
    }

    public void DisplayNextSentence()
    {
        if (isDocumentOpen) return;

        if (dialogueQueue == null || dialogueQueue.Count == 0)
        {
            Debug.Log("📖 No more sentences. Ending dialogue.");
            EndDialogue();
            return;
        }

        DialogueLine currentLine = dialogueQueue.Dequeue();

        if (nameDisplay != null) nameDisplay.text = currentLine.speakerName;
        if (sentenceDisplay != null) sentenceDisplay.text = currentLine.sentenceText;

        Debug.Log($"📖 Displaying: {currentLine.speakerName} - {currentLine.sentenceText}");

        if (currentLine.documentPanelToShow != null)
            TriggerDocument(currentLine.documentPanelToShow);
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
        if (isEndingDialogue) return;
        isEndingDialogue = true;
        isDialogueActive = false;

        Debug.Log($"🔚 EndDialogue called.");
        Debug.Log($"   onDialogueEndCallback = {(onDialogueEndCallback != null ? "SET" : "NULL")}");
        Debug.Log($"   isWaitingForCallback = {isWaitingForCallback}");
        Debug.Log($"   callbackExecuted = {callbackExecuted}");

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // ✅ PRIORITY 1: Callback for puzzle completion (success/fail panel)
        if (onDialogueEndCallback != null && !callbackExecuted)
        {
            Debug.Log("✅ FIRING onDialogueEndCallback!");
            
            callbackExecuted = true;
            System.Action cb = onDialogueEndCallback;
            onDialogueEndCallback = null;
            panelToOpenAfterDialogue = null;
            pendingResultPanel = null;
            hasPendingResultPanel = false;
            isWaitingForCallback = false;

            if (shouldUnlockNotificationOnEnd && tabletNotificationBadge != null)
                tabletNotificationBadge.SetActive(true);
            shouldUnlockNotificationOnEnd = false;

            // 🔥 INVOKE THE CALLBACK - Opens success/fail panel
            cb.Invoke();
            
            // 🔥 IMPORTANT: DO NOT restore InGamePanel when callback was executed
            isEndingDialogue = false;
            return;
        }

        // ✅ PRIORITY 2: Panel reference (for Manlililok NPC opening puzzle)
        GameObject resultPanel = panelToOpenAfterDialogue ?? (hasPendingResultPanel ? pendingResultPanel : null);

        if (resultPanel != null)
        {
            Debug.Log($"✅ OPENING result panel: {resultPanel.name}");
            resultPanel.SetActive(true);

            panelToOpenAfterDialogue = null;
            pendingResultPanel = null;
            hasPendingResultPanel = false;

            if (shouldUnlockNotificationOnEnd && tabletNotificationBadge != null)
                tabletNotificationBadge.SetActive(true);
            shouldUnlockNotificationOnEnd = false;

            isEndingDialogue = false;
            return;
        }

        // ✅ PRIORITY 3: Restore InGamePanel ONLY if no callback and no panel
        if (inGamePanel != null) 
        {
            inGamePanel.SetActive(true);
            Debug.Log("📖 InGame panel RESTORED.");
        }

        if (shouldUnlockNotificationOnEnd && tabletNotificationBadge != null)
            tabletNotificationBadge.SetActive(true);
        shouldUnlockNotificationOnEnd = false;

        Debug.Log("Dialogue finished — gameplay restored.");
        isEndingDialogue = false;
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}