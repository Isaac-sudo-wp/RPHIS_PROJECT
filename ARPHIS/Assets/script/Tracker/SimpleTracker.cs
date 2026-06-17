using UnityEngine;
using System.Collections.Generic;

public class SimpleTracker : MonoBehaviour
{
    public Transform hologramPlane;
    
    void Start()
    {
        GameObject fragments = GameObject.Find("Fragments");
        if (fragments == null) return;
        
        float planeY = hologramPlane.position.y;
        
        foreach (Transform frag in fragments.transform)
        {
            if (frag.name.Contains("(Fake)")) continue;
            if (!frag.name.Contains("paete_fragment")) continue;
            
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.transform.position = new Vector3(frag.position.x, planeY + 0.1f, frag.position.z);
            dot.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            dot.GetComponent<Renderer>().material.color = Color.red;
            dot.transform.SetParent(hologramPlane);
        }
    }
}