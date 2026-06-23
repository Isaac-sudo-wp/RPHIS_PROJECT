using UnityEngine;

public class FragmentClickManager : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (IsAnyPanelOpen()) return;

        if (Camera.main == null)
        {
            Debug.LogError("❌ No Main Camera!");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 🔥 REMOVE LAYER MASK - Hit ANYTHING
        if (Physics.Raycast(ray, out hit, 100f))
        {
            Debug.Log($"🎯 Clicked: {hit.collider.gameObject.name}");

            // Check if clicked object has ArtifactFragment
            ArtifactFragment fragment = hit.collider.GetComponent<ArtifactFragment>();
            
            // If not, check parent
            if (fragment == null && hit.collider.transform.parent != null)
            {
                fragment = hit.collider.transform.parent.GetComponent<ArtifactFragment>();
            }

            if (fragment != null)
            {
                Debug.Log($"✅ Found fragment: {fragment.fragmentName}");
                fragment.TryPickup();
            }
            else
            {
                Debug.Log($"❌ No fragment on {hit.collider.gameObject.name}");
            }
        }
    }

    private bool IsAnyPanelOpen()
    {
        GameObject dialoguePanel = GameObject.Find("DialoguePanel");
        if (dialoguePanel != null && dialoguePanel.activeInHierarchy) return true;

        GameObject callPanel = GameObject.Find("InCallBackground");
        if (callPanel != null && callPanel.activeInHierarchy) return true;

        GameObject tabletPanel = GameObject.Find("TabletPanel");
        if (tabletPanel != null && tabletPanel.activeInHierarchy) return true;

        return false;
    }
}