using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzlePiece3D : MonoBehaviour
{
    // ==========================================
    // ⚙️ INSPECTOR SETTINGS
    // ==========================================
    [Header("Snap Settings")]
    [Tooltip("How close the player must drag the piece before it locks into the hole.")]
    public float snapDistance = 0.5f;

    [Header("System Links")]
    [Tooltip("Drag the PuzzleManager object here so this piece can send data to the UI!")]
    public PuzzleManager manager;

    // ==========================================
    // 🧠 HIDDEN DATA & NEW MEMORY SYSTEM
    // ==========================================
    [HideInInspector] public ArtifactFragment fragmentData;
    [HideInInspector] public bool isSnapped = false;

    // 🔥 NEW: Tracks where this piece ACTUALLY belongs based on the Manager's sorting
    [HideInInspector] public Transform correctSnapPoint;

    // 🔥 NEW: Tracks where the player decided to drop it on the board
    [HideInInspector] public Transform currentSnapPoint;

    private Vector3 startLocalPos;
    private Quaternion startLocalRot;
    private Vector3 mOffset;
    private float mZCoord;
    private GameObject spawned3DModel;

    // ==========================================
    // 🎬 STARTUP LOGIC
    // ==========================================
    void Start()
    {
        // Memorize exactly where this tray slot is so the piece can snap back here if dropped incorrectly.
        startLocalPos = transform.localPosition;
        startLocalRot = transform.localRotation;
    }

    // ==========================================
    // 🖱️ MOUSE INTERACTION: CLICK
    // ==========================================
    void OnMouseDown()
    {
        if (isSnapped) return;

        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseAsWorldPoint();

        if (manager != null && fragmentData != null)
        {
            manager.InspectFragment(fragmentData);
        }
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    // ==========================================
    // 🖱️ MOUSE INTERACTION: DRAG
    // ==========================================
    void OnMouseDrag()
    {
        if (isSnapped) return;
        transform.position = GetMouseAsWorldPoint() + mOffset;
    }

    // ==========================================
    // 🖱️ MOUSE INTERACTION: DROP (🔥 THE UPGRADE)
    // ==========================================
    void OnMouseUp()
    {
        if (isSnapped) return;

        Transform closestSnap = null;
        float minDistance = snapDistance;

        if (manager != null)
        {
            // 🔥 SEARCH THE BOARD: Ask the Manager for a list of ALL available holes
            foreach (Transform snap in manager.GetAllSnapPoints())
            {
                if (snap == null) continue;

                // Measure the distance between this piece and the hole we are currently checking
                float distance = Vector3.Distance(transform.position, snap.position);

                // Is it close enough? AND is the hole currently empty?
                if (distance <= minDistance && !manager.IsSnapPointOccupied(snap))
                {
                    minDistance = distance;
                    closestSnap = snap;
                }
            }
        }

        // If we successfully dropped it near an empty hole...
        if (closestSnap != null)
        {
            // Magnetically snap into place!
            transform.position = closestSnap.position;
            transform.rotation = closestSnap.rotation;
            isSnapped = true;

            // 🔥 GRADE ME: Tell the manager exactly which hole the player chose!
            currentSnapPoint = closestSnap;

            // Tell the manager to check if the game is over
            if (manager != null) manager.CheckWinCondition();
        }
        else
        {
            // Dropped too far away from ANY hole, or the hole was full? Send it back to the tray.
            ResetPosition();
        }
    }

    public void ResetPosition()
    {
        transform.localPosition = startLocalPos;
        transform.localRotation = startLocalRot;
        isSnapped = false;

        // 🔥 Clear the player's choice if the piece is sent back to the tray
        currentSnapPoint = null;
    }

    // ==========================================
    // 🔥 DYNAMIC 3D SPAWNING SYSTEM
    // ==========================================
    public void Spawn3DModel(GameObject newPrefab)
    {
        if (spawned3DModel != null) Destroy(spawned3DModel);

        if (newPrefab != null)
        {
            spawned3DModel = Instantiate(newPrefab, transform);

            spawned3DModel.transform.localPosition = Vector3.zero;
            spawned3DModel.transform.localRotation = Quaternion.identity;

            Collider[] childColliders = spawned3DModel.GetComponentsInChildren<Collider>();
            foreach (Collider col in childColliders) Destroy(col);

            // 🔥 RESTORED OUR COROUTINE FIX: This prevents the models from turning invisible!
            StartCoroutine(ForceVisibilityRoutine(spawned3DModel));
        }
    }

    // ==========================================
    // 🔥 THE ONE-FRAME DELAY ASSASSIN 
    // ==========================================
    private IEnumerator ForceVisibilityRoutine(GameObject model)
    {
        // WAIT 1 FRAME: This allows the rogue scripts to run their "hide" code first.
        yield return new WaitForEndOfFrame();

        if (model != null)
        {
            // NOW we assassinate all scripts on the model so they can never run again
            MonoBehaviour[] allScripts = model.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour script in allScripts)
            {
                Destroy(script);
            }

            // NOW we force the meshes back on, getting the final word!
            MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mesh in renderers)
            {
                mesh.enabled = true;
            }
        }
    }
}