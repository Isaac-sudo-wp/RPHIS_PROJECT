using UnityEngine;

public class FindMapBounds : MonoBehaviour
{
    void Start()
    {
        GameObject fragments = GameObject.Find("Fragments");
        if (fragments == null) return;
        
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        
        foreach (Transform frag in fragments.transform)
        {
            if (frag.name.Contains("(Fake)")) continue;
            if (!frag.name.Contains("paete_fragment")) continue;
            
            if (frag.position.x < minX) minX = frag.position.x;
            if (frag.position.x > maxX) maxX = frag.position.x;
            if (frag.position.z < minZ) minZ = frag.position.z;
            if (frag.position.z > maxZ) maxZ = frag.position.z;
            
            Debug.Log($"Fragment {frag.name}: X={frag.position.x}, Z={frag.position.z}");
        }
        
        float width = maxX - minX;
        float height = maxZ - minZ;
        float centerX = (minX + maxX) / 2;
        float centerZ = (minZ + maxZ) / 2;
        
        Debug.Log($"=== MAP BOUNDS ===");
        Debug.Log($"Min X: {minX}, Max X: {maxX}");
        Debug.Log($"Min Z: {minZ}, Max Z: {maxZ}");
        Debug.Log($"Width: {width}, Height: {height}");
        Debug.Log($"Center: X={centerX}, Z={centerZ}");
        Debug.Log($"Plane Scale: X={width}, Z={height}");
        Debug.Log($"Plane Position: X={centerX}, Z={centerZ}");
    }
}