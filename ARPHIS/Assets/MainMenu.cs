using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // ==========================================
    // MAIN MENU BUTTON LOGIC
    // ==========================================

    // Link this to your "Begin Mission" or "Start" button!
    public void StartGame()
    {
        // Async loading safely opens the 3D level in the background without freezing the game.
        // (Make sure "SampleScene" perfectly matches your game scene's name!)
        SceneManager.LoadSceneAsync("SampleScene");
    }

    // Link this to your "Exit" or "Quit" button!
    public void QuitGame()
    {
        Debug.Log("Quit button was clicked!");

        // This closes the game when someone is playing the fully published version.
        Application.Quit();

        // 🔥 This tells the Unity Editor to stop Play Mode while you are testing!
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}