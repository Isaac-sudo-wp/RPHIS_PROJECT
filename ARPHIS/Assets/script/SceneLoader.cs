using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad = "MainMenu";  // Name of your main menu scene
    
    [Header("UI References - Drag your objects here")]
    public Slider loadingBar;        // Drag your "LoadingBar" Slider here
    public GameObject loadingPanel;  // Drag your "Panel" here
    public GameObject imgLogo;       // Drag your "imgLogo" here
    
    [Header("Loading Settings")]
    public float minimumLoadTime = 10f;  // Minimum 10 seconds to show loading screen
    public float smoothSpeed = 0.5f;     // Speed of smooth loading animation
    
    private AsyncOperation asyncOperation;
    private float targetProgress = 0f;
    private float currentProgress = 0f;
    private float startTime;
    private bool isLoadingComplete = false;
    
    void Start()
    {
        // Reset loading bar
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
            loadingBar.gameObject.SetActive(true);
        }
        
        if (imgLogo != null)
            imgLogo.SetActive(true);
        
        startTime = Time.time;
        
        // Start loading
        StartCoroutine(LoadSceneAsync());
    }
    
    void Update()
    {
        // Smoothly animate the loading bar
        if (loadingBar != null && !isLoadingComplete)
        {
            // Smoothly interpolate current progress towards target progress
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * smoothSpeed);
            loadingBar.value = currentProgress;
            
            // WHEN THE FILL REACHES 100% (or very close), PROCEED TO MAIN MENU
            if (currentProgress >= 0.99f && !isLoadingComplete)
            {
                StartCoroutine(ProceedToMainMenu());
            }
        }
    }
    
    IEnumerator LoadSceneAsync()
    {
        // Start loading the scene asynchronously
        asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncOperation.allowSceneActivation = false;  // Don't switch immediately
        
        // While the scene is loading
        while (!asyncOperation.isDone)
        {
            // Get actual loading progress (0 to 0.9, then jumps to 1)
            float realProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            
            // Calculate time-based progress (for smooth fill)
            float elapsedTime = Time.time - startTime;
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            
            // Use the MAX of real progress and time progress
            targetProgress = Mathf.Max(realProgress, timeProgress);
            
            yield return null;
        }
    }
    
    IEnumerator ProceedToMainMenu()
    {
        if (isLoadingComplete) yield break;
        
        isLoadingComplete = true;
        
        // Fill the bar completely
        targetProgress = 1f;
        loadingBar.value = 1f;
        
        // Small delay for visual feedback
        yield return new WaitForSeconds(0.2f);
        
        // PROCEED TO MAIN MENU SCENE
        if (asyncOperation != null)
        {
            asyncOperation.allowSceneActivation = true;
        }
        else
        {
            // Fallback if asyncOperation is null
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}