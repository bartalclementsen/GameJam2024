using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreneHandler : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
