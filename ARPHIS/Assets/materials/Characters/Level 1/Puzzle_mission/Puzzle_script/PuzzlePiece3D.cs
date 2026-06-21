using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzlePiece3D : MonoBehaviour
{
    [Header("Snap Settings")]
    public Transform targetSnapPoint;
    public float snapDistance = 1.5f;

    [Tooltip("Drag the PuzzleManager object here!")]
    public PuzzleManager manager;

    [HideInInspector] public bool isSnapped = false;

    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 mOffset;
    private float mZCoord;

    void Start()
    {
        // Remember where this piece started (the UI boxes at the bottom)
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void OnMouseDown()
    {
        if (isSnapped) return;
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag()
    {
        if (isSnapped) return;
        transform.position = GetMouseAsWorldPoint() + mOffset;
    }

    void OnMouseUp()
    {
        if (isSnapped) return;

        if (targetSnapPoint != null)
        {
            float distance = Vector3.Distance(transform.position, targetSnapPoint.position);

            if (distance <= snapDistance)
            {
                // SNAP!
                transform.position = targetSnapPoint.position;
                transform.rotation = targetSnapPoint.rotation;
                isSnapped = true;

                // Tell the manager to check if we won!
                if (manager != null) manager.CheckWinCondition();
            }
            else
            {
                // Missed! Snap back to the starting box at the bottom.
                ResetPosition();
            }
        }
    }

    // Called by the Reset button or if dropped in the wrong spot
    public void ResetPosition()
    {
        transform.position = startPos;
        transform.rotation = startRot;
        isSnapped = false;
    }
}