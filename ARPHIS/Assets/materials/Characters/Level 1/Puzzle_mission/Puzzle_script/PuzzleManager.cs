using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Setup")]
    public GameObject puzzleUIPanel;
    public PuzzlePiece3D[] puzzlePieces; // Drag your 4 fragments here

    [Header("Victory Event")]
    [Tooltip("What happens when the puzzle is finished?")]
    public UnityEvent onPuzzleComplete;

    private int snappedCount = 0;

    // The puzzle pieces will call this every time one snaps into place
    public void CheckWinCondition()
    {
        snappedCount = 0;
        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            if (piece.isSnapped) snappedCount++;
        }

        // Did they snap all 4 pieces?
        if (snappedCount >= puzzlePieces.Length)
        {
            Debug.Log("Diorama Restored! Puzzle Complete!");
            onPuzzleComplete.Invoke(); // Triggers the victory event!
        }
    }

    // Link this to your UI "Reset" Button
    public void ResetPuzzle()
    {
        foreach (PuzzlePiece3D piece in puzzlePieces)
        {
            piece.ResetPosition();
        }
    }

    // Link this to your UI "Exit" Button
    public void ExitPuzzle()
    {
        if (puzzleUIPanel != null) puzzleUIPanel.SetActive(false);

        // If you need to turn the player's joystick/HUD back on manually, 
        // you can do that here!
    }
}