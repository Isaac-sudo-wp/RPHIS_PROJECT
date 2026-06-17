using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARPHIS.Settings
{
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("UI Components")]
        public Slider sensitivitySlider;       // Drag sldrSensitivity here
        public TextMeshProUGUI percentageText; // Drag txtPercentage here

        [Header("Camera Configuration")]
        public CameraOrbit cameraOrbitScript;  // Drag CameraPivot here

        void Start()
        {
            // 1. Setup Slider Bounds (Min 1% to Max 200%)
            if (sensitivitySlider != null)
            {
                sensitivitySlider.minValue = 0.01f;
                sensitivitySlider.maxValue = 2f;
            }
            
            // 2. Load preference safely (default to 100% -> 1f)
            float savedSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", 1f);
            
            // 3. Try to auto-find CameraOrbit if slot was left empty (Great for your In-Game Scene!)
            if (cameraOrbitScript == null)
            {
                cameraOrbitScript = Object.FindFirstObjectByType<CameraOrbit>();
            }

            // 4. Register the listener FIRST before setting the value
            if (sensitivitySlider != null)
            {
                // Remove any existing listeners first to prevent potential duplicate call stacking bugs
                sensitivitySlider.onValueChanged.RemoveListener(OnSliderChanged);
                sensitivitySlider.onValueChanged.AddListener(OnSliderChanged);
                
                // 5. Set the value, which automatically fires OnSliderChanged smoothly!
                sensitivitySlider.value = savedSensitivity;
            }
        }

        void OnSliderChanged(float value)
        {
            // Convert the float value (0.01f to 2f) into an integer percentage string (1% to 200%)
            int percentage = Mathf.RoundToInt(value * 100f);
            if (percentageText != null)
            {
                percentageText.text = percentage + "%";
            }

            // Pass the updated multiplier over to your CameraOrbit processing loop safely
            if (cameraOrbitScript != null)
            {
                cameraOrbitScript.UpdateSensitivity(value);
            }

            // Commit the raw setting value into persistent local storage matrix
            PlayerPrefs.SetFloat("CameraSensitivity", value);
            PlayerPrefs.Save(); 
        }
    }
}