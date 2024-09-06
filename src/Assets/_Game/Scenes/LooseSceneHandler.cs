using UnityEngine;
using UnityEngine.SceneManagement;

public class LooseSceneHandler : MonoBehaviour
{
    public void RetryGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
