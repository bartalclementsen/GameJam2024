using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneHandler : MonoBehaviour
{
    public void WinGame()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void LooseGame()
    {
        SceneManager.LoadSceneAsync(3);
    }
}
