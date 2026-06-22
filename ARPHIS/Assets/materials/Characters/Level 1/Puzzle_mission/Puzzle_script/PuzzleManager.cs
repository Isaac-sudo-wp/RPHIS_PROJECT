using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    // ==========================================
    // ⚙️ SYSTEM SETTINGS
    // ==========================================
    [Header("Puzzle Setup")]
    [Tooltip("The main Canvas panel that holds all the UI for the puzzle.")]
    public GameObject puzzleUIPanel;
    [Tooltip("Drag the 4 empty paete containers from your Hierarchy here!")]
    public PuzzlePiece3D[] puzzlePieces;

    // ==========================================
    // 🔥 NEW: SMART SNAP POINTS
    // ==========================================
    [Header("Dynamic Snap Points")]
    [Tooltip("Drag your actual SnapPoint objects from the hierarchy here so the code knows where the holes are!")]
    public Transform snapPoint1;
    public Transform snapPoint2;
    public Transform snapPoint3;
    public Transform snapPoint4;

    // ==========================================
    // 🏆 WIN/LOSS EVENTS
    // ==========================================
    [Header("End Game Events")]
    [Tooltip("What happens when the puzzle is finished perfectly (All REAL fragments)?")]
    public UnityEvent onPuzzleComplete;

    [Tooltip("What happens when the puzzle is finished, but with FAKE fragments?")]
    public UnityEvent onPuzzleFailed;

    // ==========================================
    // 🖥️ UI & SCENE CONNECTIONS
    // ==========================================
    [Header("Button Screen Links")]
    public GameObject missionFailedPanel;
    public GameObject puzzle3DContainer;
    public GameObject inGamePanel;
    public string mainMenuSceneName = "MainMenu";

    // ==========================================
    // 🔍 INSPECTION PANEL UI
    // ==========================================
    [Header("Inspection Panel")]
    [Tooltip("The dark background panel that holds the text and image.")]
    public GameObject inspectPanel;
    [Tooltip("The UI Image component that will show the 2D sprite.")]
    public Image inspectIcon;
    [Tooltip("The TextMeshPro text that will show the fragment's name.")]
    public TextMeshProUGUI inspectName;
    [Tooltip("The TextMeshPro text that will show the fragment's lore.")]
    public TextMeshProUGUI inspectDesc;

    private int snappedCount = 0;

    // ==========================================
    // 🔥 NEW: SNAP POINT HELPERS
    // ==========================================
    // This allows the 3D pieces to look at the board and find all available holes!
    public Transform[] GetAllSnapPoints()
    {
        return new Transform[] { snapPoint1, snapPoint2, snapPoint3, snapPoint4 };
    }

    // This prevents the player from cheating and putting two fragments in the same hole!
    public bool IsSnapPointOccupied(Transform pointToCheck)
    {
        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            if (piece.isSnapped && piece.currentSnapPoint == pointToCheck) return true;
        }
        return false;
    }

    // ==========================================
    // 🎬 STARTUP & DATA SYNC 
    // ==========================================
    void OnEnable()
    {
        // 1. THE BROOM: Clean out any old clones from the tray!
        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            foreach (Transform child in piece.transform)
            {
                Destroy(child.gameObject);
            }
            piece.gameObject.SetActive(false);
        }

        if (puzzle3DContainer != null) puzzle3DContainer.SetActive(true);

        InventoryManager inventory = FindObjectOfType<InventoryManager>();

        if (inventory != null && inventory.collectedItems.Count > 0)
        {
            for (int i = 0; i < puzzlePieces.Length; i++)
            {
                if (i < inventory.collectedItems.Count)
                {
                    ArtifactFragment currentItem = inventory.collectedItems[i];

                    // SYNC DATA
                    puzzlePieces[i].fragmentData = currentItem;

                    // ==========================================
                    // 🔥 THE FIX: Turn the slot ON *before* spawning!
                    // This allows the visibility Coroutine to run perfectly.
                    // ==========================================
                    puzzlePieces[i].gameObject.SetActive(true);

                    // NOW we can safely spawn the 3D model
                    puzzlePieces[i].Spawn3DModel(currentItem.artifactPrefab);

                    // We only extract the clean name you wrote in the Inspector
                    string fragName = currentItem.fragmentName;

                    // ==========================================
                    // 🔥 THE AI FIX: Ignore the 3D model name completely!
                    // By ignoring "modelName", we stop the computer from reading 
                    // random AI gibberish like "tripo_node_5dac3694".
                    // ==========================================
                    if (fragName.Contains("1"))
                        puzzlePieces[i].correctSnapPoint = snapPoint1;
                    else if (fragName.Contains("2"))
                        puzzlePieces[i].correctSnapPoint = snapPoint2;
                    else if (fragName.Contains("3"))
                        puzzlePieces[i].correctSnapPoint = snapPoint3;
                    else if (fragName.Contains("4"))
                        puzzlePieces[i].correctSnapPoint = snapPoint4;
                }
                else
                {
                    // If the player doesn't have an item for this slot, keep it hidden
                    puzzlePieces[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // ==========================================
    // 🖱️ INSPECTION TRIGGER
    // ==========================================
    public void InspectFragment(ArtifactFragment data)
    {
        if (data == null) return;

        if (inspectPanel != null) inspectPanel.SetActive(true);

        if (inspectIcon != null)
        {
            inspectIcon.sprite = data.inventoryIcon;
            inspectIcon.enabled = true;
        }

        if (inspectName != null) inspectName.text = data.fragmentName;
        if (inspectDesc != null) inspectDesc.text = data.descriptionText;
    }

    // ==========================================
    // 🧩 PUZZLE LOGIC & NEW WIN/FAIL GRADER
    // ==========================================
    public void CheckWinCondition()
    {
        snappedCount = 0;
        int correctPositionCount = 0; // Track how many are in the right spot

        // 1. First, check the board to see what the player did
        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            if (piece.isSnapped)
            {
                snappedCount++;

                // ==========================================
                // 🔍 THE DETECTIVE LOG: Prints the grading rubric to your console!
                // ==========================================
                Debug.Log("🔍 GRADING LOG: [" + piece.gameObject.name + "] was placed in [" + piece.currentSnapPoint.name + "] | Computer expects: [" + piece.correctSnapPoint.name + "]");

                // Compare where they placed it vs. where it actually belongs!
                if (piece.currentSnapPoint == piece.correctSnapPoint)
                {
                    correctPositionCount++;
                }
            }
        }

        // 2. Once all 4 pieces are placed somewhere on the board...
        if (snappedCount >= puzzlePieces.Length)
        {
            InventoryManager inventory = FindObjectOfType<InventoryManager>();

            if (inventory != null)
            {
                // 🔥 CONDITION 1: Did they bring a Fake item to the table? -> FAIL
                if (inventory.AreAllFragmentsReal() == false)
                {
                    Debug.Log("❌ FAKE FRAGMENT DETECTED! Mission Failed!");
                    onPuzzleFailed.Invoke();
                }
                // 🔥 CONDITION 2: Items are real, but put in the WRONG holes! -> FAIL
                else if (correctPositionCount < puzzlePieces.Length)
                {
                    Debug.Log("❌ REAL FRAGMENTS, BUT WRONG POSITIONS! Mission Failed!");
                    onPuzzleFailed.Invoke();
                }
                // 🔥 CONDITION 3: All items real, and placed perfectly! -> WIN
                else
                {
                    Debug.Log("🎉 Diorama Restored perfectly! Puzzle Complete!");
                    onPuzzleComplete.Invoke();
                }
            }
            else
            {
                Debug.LogWarning("No InventoryManager found, defaulting to Win.");
                onPuzzleComplete.Invoke();
            }
        }
    }

    // ==========================================
    // 🔁 BUTTON FUNCTIONS
    // ==========================================
    public void ResetPuzzle()
    {
        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            piece.ResetPosition();
        }
    }

    public void ExitPuzzle()
    {
        if (puzzleUIPanel != null) puzzleUIPanel.SetActive(false);
        if (puzzle3DContainer != null) puzzle3DContainer.SetActive(false);
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