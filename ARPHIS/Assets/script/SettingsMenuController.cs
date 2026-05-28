using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuController : MonoBehaviour
{
    [Header("UI Components")]
    public Slider sensitivitySlider;       // Drag sldrSensitivity here
    public TextMeshProUGUI percentageText; // Drag txtPercentage here

    [Header("Camera Configuration")]
    public CameraOrbit cameraOrbitScript;  // Drag CameraPivot here

void Start()
{
    // 1. Setup Slider Bounds
    sensitivitySlider.minValue = 0.01f;
    sensitivitySlider.maxValue = 2f;
    
    // 2. Load preference safely (default to 100% -> 1f)
    float savedSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", 1f);
    
    // 3. Register the listener FIRST before setting the value
    // This ensures that when we set the value next, it triggers the change perfectly
    sensitivitySlider.onValueChanged.AddListener(OnSliderChanged);
    
    // 4. Set the value, which automatically fires OnSliderChanged smoothly!
    sensitivitySlider.value = savedSensitivity;
}

    void OnSliderChanged(float value)
    {
        // Convert the float value (0f to 2f) into an integer percentage string (0% to 200%)
        int percentage = Mathf.RoundToInt(value * 100f);
        if (percentageText != null)
        {
            percentageText.text = percentage + "%";
        }

        // Pass the updated multiplier over to your CameraOrbit processing loop
        if (cameraOrbitScript != null)
        {
            cameraOrbitScript.UpdateSensitivity(value);
        }

        // Commit the raw setting value into persistent local storage matrix
        PlayerPrefs.SetFloat("CameraSensitivity", value);
        PlayerPrefs.Save(); 
    }
}