using UnityEngine;

public class PuzzleVisibilityLink : MonoBehaviour
{
    [Tooltip("Drag your 3D_Puzzle_Container here!")]
    public GameObject puzzle3DContainer;

    // This runs the exact second the PuzzleUIPanel is turned ON by Paeng
    void OnEnable()
    {
        if (puzzle3DContainer != null)
        {
            puzzle3DContainer.SetActive(true);
        }
    }

    // This runs the exact second you click the EXIT button
    void OnDisable()
    {
        if (puzzle3DContainer != null)
        {
            puzzle3DContainer.SetActive(false);
        }
    }
}