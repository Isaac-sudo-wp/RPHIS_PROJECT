using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject inGamePanel;

    public void PauseGame()
    {
        // 1. Switch the UI panels just like your button was doing
        if (pausePanel != null) pausePanel.SetActive(true);
        if (inGamePanel != null) inGamePanel.SetActive(false);

        // 2. Freeze the game world time matrix completely
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        // 1. Switch the UI panels back to standard gameplay
        if (pausePanel != null) pausePanel.SetActive(false);
        if (inGamePanel != null) inGamePanel.SetActive(true);

        // 2. Unfreeze time back to normal running speed
        Time.timeScale = 1f;
    }
}