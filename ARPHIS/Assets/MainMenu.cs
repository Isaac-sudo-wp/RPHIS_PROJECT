using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  public void StartGame()
  {
    SceneManager.LoadSceneAsync("SampleScene");
  }
  public void QuitGame()
  {
    Debug.Log("Quit!");
    Application.Quit();

    #if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
    #endif
  }
}
