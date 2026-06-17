using UnityEngine;

public class BlinkingTracker : MonoBehaviour
{
    public float blinkSpeed = 2f;
    public Color color1 = Color.red;
    public Color color2 = Color.yellow;
    
    private Renderer rend;
    private Material mat;
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
    }
    
    void Update()
    {
        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        Color currentColor = Color.Lerp(color1, color2, t);
        mat.SetColor("_Color", currentColor);
        mat.SetColor("_EmissionColor", currentColor);
    }
}