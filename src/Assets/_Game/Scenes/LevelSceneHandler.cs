using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneHandler : MonoBehaviour
{
    private bool isPaused = false;
    
    [SerializeField]
    private GameObject pauseMenuUI;
    
    public void WinGame()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void LooseGame()
    {
        SceneManager.LoadSceneAsync(3);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TryToPauseGame();
        }
    }

    public void ContinueGame()
    {
        TryToPauseGame();
    }
    
    public void RetryGame()
    {
        SceneManager.LoadScene(1);
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void TryToPauseGame()
    {
        
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0;
            pauseMenuUI.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            pauseMenuUI.SetActive(false);
        }
    }
}
