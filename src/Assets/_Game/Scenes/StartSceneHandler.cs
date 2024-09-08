using System.Linq;
using System.Text;
using _Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _versionText;

    [SerializeField]
    private GameObject _mainMenu;
    
    [SerializeField]
    private GameObject _highScoreMenu;
    
    [SerializeField]
    private TextMeshProUGUI _highScoreText;
    
    [SerializeField]
    private Button _highScoreBackButton;
    
    [SerializeField]
    private Button _startButton;
    
    [SerializeField]
    private EventSystem _eventSystem;
    
    private IHighScoreService _highScoreService;
    
    private void Start()
    {
        _versionText.text = $"Brynleif, Eirikur, Ørvur og Bartal - Gladius V{Application.version}";
        _highScoreService = Game.Container.Resolve<IHighScoreService>();
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void ShowHighScoreMenu()
    {
        _mainMenu.SetActive(false);
        _highScoreMenu.SetActive(true);
        
        _eventSystem.SetSelectedGameObject(_highScoreBackButton.gameObject);

        var highScores = _highScoreService.GetHighScores();
        StringBuilder sb = new StringBuilder();
        if (highScores.Any() == false)
        {
            sb.AppendLine("No high scores");
        }
        else
        {
            for (int i = 0; i < highScores.Count(); i++)
            {
                var highScore = highScores.ElementAt(i);
                sb.AppendLine($"{(i +  1).ToString("00")}. {highScore.Name} kills {highScore.Kills} in {highScore.Time} ({highScore.Date}) ");
            }
        }

        _highScoreText.text = sb.ToString();
    }

    public void HideHighScoreMenu()
    {
        _highScoreMenu.SetActive(false);
        _mainMenu.SetActive(true);
        
        _eventSystem.SetSelectedGameObject(_startButton.gameObject);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
