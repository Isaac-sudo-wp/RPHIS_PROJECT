using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Setup")]
    public GameObject puzzleUIPanel;
    public PuzzlePiece3D[] puzzlePieces;

    [Header("Artifact Container")]
    [Tooltip("Drag PaeteFragMissing here so snapped pieces rotate with it!")]
    public Transform artifactContainer;

    [Header("Dynamic Snap Points")]
    public Transform snapPoint1;
    public Transform snapPoint2;
    public Transform snapPoint3;
    public Transform snapPoint4;

    [Header("End Game Events")]
    public UnityEvent onPuzzleComplete;
    public UnityEvent onPuzzleFailed;

    [Header("Manlililok End Dialogue")]
    [Tooltip("Drag the DialogueManager here")]
    public DialogueManager dialogueManager;
    [Tooltip("Dialogue Manlililok says when puzzle is completed successfully")]
    public DialogueLine[] successDialogue;
    [Tooltip("Dialogue Manlililok says when puzzle fails")]
    public DialogueLine[] failDialogue;
    [Tooltip("Panel to open after success dialogue ends")]
    public GameObject successPanel;
    [Tooltip("Panel to open after fail dialogue ends")]
    public GameObject failPanel;

    [Header("Button Screen Links")]
    public GameObject missionFailedPanel;
    public GameObject puzzle3DContainer;
    public GameObject inGamePanel;
    public string mainMenuSceneName = "MainMenu";

    [Header("Inspection Panel")]
    public GameObject inspectPanel;
    public Image inspectIcon;
    public TextMeshProUGUI inspectName;
    public TextMeshProUGUI inspectDesc;

    private int snappedCount = 0;
    private bool isEndTriggered = false;

    // 🔥 Auto-clean puzzle pieces array
    private void CleanPuzzlePiecesArray()
    {
        if (puzzlePieces == null) return;
        
        var cleanList = new List<PuzzlePiece3D>();
        foreach (var piece in puzzlePieces)
        {
            if (piece != null && !string.IsNullOrEmpty(piece.gameObject.name))
            {
                if (piece.gameObject.name.Contains("paete_"))
                {
                    cleanList.Add(piece);
                }
            }
        }
        
        if (cleanList.Count < puzzlePieces.Length)
        {
            Debug.Log($"🧹 Cleaned puzzle pieces array: {puzzlePieces.Length} → {cleanList.Count}");
            puzzlePieces = cleanList.ToArray();
        }
    }

    public Transform[] GetAllSnapPoints()
    {
        return new Transform[] { snapPoint1, snapPoint2, snapPoint3, snapPoint4 };
    }

    public bool IsSnapPointOccupied(Transform pointToCheck)
    {
        if (pointToCheck == null) return false;

        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            if (piece != null && piece.isSnapped && piece.currentSnapPoint == pointToCheck)
                return true;
        }
        return false;
    }

    void OnEnable()
    {
        Debug.Log("🧩 PuzzleManager: OnEnable called!");
        isEndTriggered = false;

        // 🔥 Auto-clean the array
        CleanPuzzlePiecesArray();

        // Hide panels at start
        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);

        if (artifactContainer == null)
        {
            GameObject container = GameObject.Find("PaeteFragMissing");
            if (container != null)
            {
                artifactContainer = container.transform;
                Debug.Log($"✅ Auto-found artifactContainer: {container.name}");
            }
        }

        if (artifactContainer != null) artifactContainer.gameObject.SetActive(true);

        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            if (piece == null) continue;

            foreach (Transform child in piece.transform)
                Destroy(child.gameObject);

            piece.gameObject.SetActive(false);
            piece.isSnapped = false;
            piece.currentSnapPoint = null;
            piece.correctSnapPoint = null;
            piece.fragmentData = null;

            if (artifactContainer != null && piece.transform.parent == artifactContainer)
                piece.transform.SetParent(artifactContainer.parent);
        }

        if (puzzle3DContainer != null) puzzle3DContainer.SetActive(true);

        InventoryManager inventory = FindObjectOfType<InventoryManager>();

        if (inventory != null && inventory.collectedItems != null && inventory.collectedItems.Count > 0)
        {
            Debug.Log($"📦 Inventory has {inventory.collectedItems.Count} items");

            int slotIndex = 0;

            foreach (ArtifactFragment item in inventory.collectedItems)
            {
                if (item == null) continue;
                if (slotIndex >= puzzlePieces.Length) break;

                string gameObjectName = item.gameObject.name;
                Debug.Log($"🔍 Processing GameObject: {gameObjectName}");

                Transform targetSnapPoint = null;

                if (gameObjectName.Contains("1")) targetSnapPoint = snapPoint1;
                else if (gameObjectName.Contains("2")) targetSnapPoint = snapPoint2;
                else if (gameObjectName.Contains("3")) targetSnapPoint = snapPoint3;
                else if (gameObjectName.Contains("4")) targetSnapPoint = snapPoint4;
                else
                {
                    Debug.LogWarning($"⚠️ Could not determine number from GameObject: {gameObjectName}");
                    continue;
                }

                if (targetSnapPoint == null)
                {
                    Debug.LogWarning($"⚠️ Target snap point is null for: {gameObjectName}");
                    continue;
                }

                PuzzlePiece3D piece = puzzlePieces[slotIndex];
                piece.fragmentData = item;
                piece.correctSnapPoint = targetSnapPoint;
                piece.gameObject.SetActive(true);
                piece.Spawn3DModel(item.artifactPrefab);

                Debug.Log($"✅ Assigned '{gameObjectName}' to slot {slotIndex + 1} with snap point {targetSnapPoint.name}");

                slotIndex++;
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No inventory items found!");
        }
    }

    public void InspectFragment(ArtifactFragment data)
    {
        if (data == null) return;

        if (inspectPanel != null) inspectPanel.SetActive(true);

        if (inspectIcon != null && data.inventoryIcon != null)
        {
            inspectIcon.sprite = data.inventoryIcon;
            inspectIcon.enabled = true;
        }

        if (inspectName != null) inspectName.text = data.fragmentName;
        if (inspectDesc != null) inspectDesc.text = data.descriptionText;
    }

    public void CheckWinCondition()
    {
        if (isEndTriggered) return;

        snappedCount = 0;
        int correctPositionCount = 0;

        Debug.Log("🔍 Checking win condition...");

        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            if (piece == null) continue;

            if (piece.isSnapped)
            {
                snappedCount++;

                if (piece.correctSnapPoint == null)
                {
                    Debug.LogWarning($"⚠️ {piece.gameObject.name} has no correctSnapPoint!");
                    continue;
                }

                Debug.Log($"🔍 GRADING: [{piece.gameObject.name}] placed in [{piece.currentSnapPoint?.name}] | Expects: [{piece.correctSnapPoint?.name}]");

                if (piece.currentSnapPoint == piece.correctSnapPoint)
                    correctPositionCount++;
            }
        }

        Debug.Log($"📊 Snapped: {snappedCount}/{puzzlePieces.Length}, Correct: {correctPositionCount}/{puzzlePieces.Length}");

        if (snappedCount >= puzzlePieces.Length)
        {
            InventoryManager inventory = FindObjectOfType<InventoryManager>();

            if (inventory != null)
            {
                if (!inventory.AreAllFragmentsReal())
                {
                    Debug.Log("❌ FAKE FRAGMENT DETECTED! Mission Failed!");
                    TriggerEndDialogue(false);
                }
                else if (correctPositionCount < puzzlePieces.Length)
                {
                    Debug.Log("❌ REAL FRAGMENTS, BUT WRONG POSITIONS! Mission Failed!");
                    TriggerEndDialogue(false);
                }
                else
                {
                    Debug.Log("🎉 Diorama Restored perfectly! Puzzle Complete!");
                    TriggerEndDialogue(true);
                }
            }
            else
            {
                Debug.LogWarning("No InventoryManager found, defaulting to Win.");
                TriggerEndDialogue(true);
            }
        }
    }

    private void TriggerEndDialogue(bool isSuccess)
    {
        if (isEndTriggered) return;
        isEndTriggered = true;

        Debug.Log($"🎬 TriggerEndDialogue called. isSuccess: {isSuccess}");

        // Hide puzzle UI and container
        if (puzzleUIPanel != null) puzzleUIPanel.SetActive(false);
        if (puzzle3DContainer != null) puzzle3DContainer.SetActive(false);
        if (artifactContainer != null) artifactContainer.gameObject.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);

        if (dialogueManager == null)
        {
            Debug.LogWarning("⚠️ No DialogueManager! Showing panel directly.");
            if (isSuccess && successPanel != null) 
            {
                successPanel.SetActive(true);
                Debug.Log($"✅ Success panel opened directly: {successPanel.name}");
            }
            else if (!isSuccess && failPanel != null) 
            {
                failPanel.SetActive(true);
                Debug.Log($"✅ Fail panel opened directly: {failPanel.name}");
            }
            return;
        }

        // 🔥 Clear any existing callbacks FIRST
        dialogueManager.ClearCallback();

        if (isSuccess)
        {
            Debug.Log("🎉 Triggering success dialogue with callback.");

            if (successDialogue == null || successDialogue.Length == 0)
            {
                Debug.LogError("❌ successDialogue is empty!");
                if (successPanel != null) successPanel.SetActive(true);
                return;
            }

            // 🔥 Store panel reference in local variable for callback
            GameObject successPanelRef = successPanel;

            dialogueManager.StartDialogueWithCallback(successDialogue, () =>
            {
                Debug.Log("✅ SUCCESS CALLBACK FIRED! Opening success panel.");
                if (successPanelRef != null)
                {
                    successPanelRef.SetActive(true);
                    Debug.Log($"✅ Success panel opened: {successPanelRef.name}");
                }
                else
                {
                    Debug.LogError("❌ Success panel is NULL!");
                }
            });
        }
        else
        {
            Debug.Log("❌ Triggering fail dialogue with callback.");

            if (failDialogue == null || failDialogue.Length == 0)
            {
                Debug.LogError("❌ failDialogue is empty!");
                if (failPanel != null) failPanel.SetActive(true);
                return;
            }

            // 🔥 Store panel reference in local variable for callback
            GameObject failPanelRef = failPanel;

            dialogueManager.StartDialogueWithCallback(failDialogue, () =>
            {
                Debug.Log("✅ FAIL CALLBACK FIRED! Opening fail panel.");
                if (failPanelRef != null)
                {
                    failPanelRef.SetActive(true);
                    Debug.Log($"✅ Fail panel opened: {failPanelRef.name}");
                }
                else
                {
                    Debug.LogError("❌ Fail panel is NULL!");
                }
            });
        }
    }

    public void ResetPuzzle()
    {
        Debug.Log("🔄 Resetting puzzle...");
        isEndTriggered = false;

        if (artifactContainer != null) artifactContainer.gameObject.SetActive(true);

        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            if (piece != null)
            {
                if (artifactContainer != null && piece.transform.parent == artifactContainer)
                    piece.transform.SetParent(artifactContainer.parent);

                piece.ResetPosition();
            }
        }
        
        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);
    }

    public void ExitPuzzle()
    {
        Debug.Log("🚪 Exiting puzzle...");
        isEndTriggered = false;

        if (puzzleUIPanel != null) puzzleUIPanel.SetActive(false);
        if (puzzle3DContainer != null) puzzle3DContainer.SetActive(false);
        if (artifactContainer != null) artifactContainer.gameObject.SetActive(false);
        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);
        if (inGamePanel != null) inGamePanel.SetActive(true);
    }

    public void RetryMissionButton()
    {
        ResetPuzzle();
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);
        if (puzzleUIPanel != null) puzzleUIPanel.SetActive(false);
        if (puzzle3DContainer != null) puzzle3DContainer.SetActive(false);
        if (inGamePanel != null) inGamePanel.SetActive(true);
    }

    public void ReturnToMainMenuButton()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}