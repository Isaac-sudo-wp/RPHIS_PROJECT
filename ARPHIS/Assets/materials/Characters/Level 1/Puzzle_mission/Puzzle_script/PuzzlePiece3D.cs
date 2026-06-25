using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzlePiece3D : MonoBehaviour
{
    [Header("Snap Settings")]
    public float snapDistance = 0.5f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;

    [Header("System Links")]
    public PuzzleManager manager;

    [HideInInspector] public ArtifactFragment fragmentData;
    [HideInInspector] public bool isSnapped = false;

    [Header("Snap Points (Drag from Hierarchy)")]
    public Transform correctSnapPoint;

    [HideInInspector] public Transform currentSnapPoint;

    private Vector3 startLocalPos;
    private Quaternion startLocalRot;
    private Transform startParent;
    private Vector3 mOffset;
    private float mZCoord;
    private GameObject spawned3DModel;

    void Start()
    {
        startLocalPos = transform.localPosition;
        startLocalRot = transform.localRotation;
        startParent = transform.parent;

        if (correctSnapPoint == null)
            Debug.LogWarning($"⚠️ {gameObject.name} has NO correctSnapPoint assigned!");
        
        // 🔥 Auto-find artifact container if manager doesn't have it
        if (manager != null && manager.artifactContainer == null)
        {
            // Try to find PaeteFragMissing in the scene
            GameObject container = GameObject.Find("PaeteFragMissing");
            if (container != null)
            {
                manager.artifactContainer = container.transform;
                Debug.Log($"✅ Auto-found artifactContainer: {container.name}");
            }
        }
    }

    public int GetFragmentNumber()
    {
        if (fragmentData == null) return 0;
        string gameObjectName = gameObject.name;
        if (gameObjectName.Contains("1")) return 1;
        else if (gameObjectName.Contains("2")) return 2;
        else if (gameObjectName.Contains("3")) return 3;
        else if (gameObjectName.Contains("4")) return 4;
        else return 0;
    }

    public bool IsFragmentReal()
    {
        if (fragmentData == null) return false;
        return fragmentData.isRealArtifact;
    }

    public string GetFragmentName()
    {
        if (fragmentData == null) return "Unknown";
        return fragmentData.fragmentName;
    }

    void OnMouseDown()
    {
        if (isSnapped) return;

        mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        mOffset = transform.position - GetMouseAsWorldPoint();

        if (manager != null && fragmentData != null)
        {
            Debug.Log($"🖱️ Clicked: {gameObject.name} - {GetFragmentName()} (#{GetFragmentNumber()}, Real: {IsFragmentReal()})");
            manager.InspectFragment(fragmentData);
        }
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

        if (Input.GetMouseButton(1))
        {
            float rotationInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, -rotationInput, Space.World);
        }

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            transform.Rotate(Vector3.up, scrollInput * 100f * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    void OnMouseUp()
    {
        if (isSnapped) return;

        if (correctSnapPoint == null)
        {
            Debug.LogError($"❌ {gameObject.name} has NO correctSnapPoint! Returning.");
            ResetPosition();
            return;
        }

        Transform closestSnap = null;
        float minDistance = snapDistance;

        if (manager != null)
        {
            foreach (Transform snap in manager.GetAllSnapPoints())
            {
                if (snap == null) continue;
                float distance = Vector3.Distance(transform.position, snap.position);
                if (distance <= minDistance && !manager.IsSnapPointOccupied(snap))
                {
                    minDistance = distance;
                    closestSnap = snap;
                }
            }
        }

        if (closestSnap != null)
        {
            if (closestSnap == correctSnapPoint)
            {
                transform.position = closestSnap.position;
                transform.rotation = closestSnap.rotation;
                isSnapped = true;
                currentSnapPoint = closestSnap;

                // 🔥 FIX: Parent to artifact container so it rotates with PaeteFragMissing
                Transform container = GetArtifactContainer();
                if (container != null)
                {
                    transform.SetParent(container);
                    Debug.Log($"✅ {gameObject.name} snapped and joined {container.name} rotation!");
                }
                else
                {
                    Debug.LogWarning("⚠️ artifactContainer is NULL! Fragment will NOT rotate with container.");
                }

                if (manager != null) manager.CheckWinCondition();
            }
            else
            {
                Debug.Log($"❌ Wrong snap point! Returning to tray.");
                ResetPosition();
            }
        }
        else
        {
            Debug.Log($"↩️ Dropped too far. Returning to tray.");
            ResetPosition();
        }
    }

    // 🔥 Helper to find artifact container
    private Transform GetArtifactContainer()
    {
        if (manager != null && manager.artifactContainer != null)
            return manager.artifactContainer;

        // Try to find by name
        GameObject container = GameObject.Find("PaeteFragMissing");
        if (container != null)
            return container.transform;

        // Try to find by tag
        GameObject tagged = GameObject.FindGameObjectWithTag("ArtifactContainer");
        if (tagged != null)
            return tagged.transform;

        return null;
    }

    public void ResetPosition()
    {
        if (startParent != null)
            transform.SetParent(startParent);

        transform.localPosition = startLocalPos;
        transform.localRotation = startLocalRot;
        isSnapped = false;
        currentSnapPoint = null;
        Debug.Log($"🔄 {gameObject.name} reset to original position.");
    }

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

            StartCoroutine(ForceVisibilityRoutine(spawned3DModel));
        }
    }

    private IEnumerator ForceVisibilityRoutine(GameObject model)
    {
        yield return new WaitForEndOfFrame();

        if (model != null)
        {
            MonoBehaviour[] allScripts = model.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour script in allScripts) Destroy(script);

            MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mesh in renderers) mesh.enabled = true;
        }
    }
}