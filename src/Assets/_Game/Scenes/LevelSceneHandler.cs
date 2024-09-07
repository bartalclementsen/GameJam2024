using System;
using Core.Mediators;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneHandler : MonoBehaviour
{
    private bool isPaused = false;
    
    [SerializeField]
    private TextMeshProUGUI _scoreText;
    
    [SerializeField]
    private TextMeshProUGUI _hitPointsText;
    
    [SerializeField]
    private GameObject pauseMenuUI;

    private IMessenger _messenger;
    private IDisposable _subscription;
    private IDisposable _subscription2;

    private int totalKills = 0;
    private DateTime _timeStart;

    private void Start()
    {
        _messenger = Game.Container.Resolve<IMessenger>();

        _subscription = _messenger.Subscribe<EnemyKilledMessage>((m) =>
        {
            totalKills++;
            
            // CROWD GOES WILD!!
        });
        
        _subscription2 = _messenger.Subscribe<PlayerTookDamageMessage>((m) =>
        {
            _hitPointsText.text = "HP " + Math.Max(0, m.PlayerHandler.CurrentHitPonts) + "/" + 5;
            if (m.PlayerHandler.CurrentHitPonts < 1)
            {
                LooseGame();  
            }
        });
        
        _timeStart = DateTime.Now;
    }

    public void WinGame()
    {
        SceneManager.LoadSceneAsync(2);
        Time.timeScale = 1;
    }

    public void LooseGame()
    {
        SceneManager.LoadSceneAsync(3);
        Time.timeScale = 1;
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
        Time.timeScale = 1;
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
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



    private void FixedUpdate()
    {
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        var timeElapsed = (DateTime.Now - _timeStart);
        _scoreText.text = "Kills " + totalKills.ToString("00") + Environment.NewLine + "Time " + ((int)timeElapsed.TotalMinutes).ToString("00") + ":" + timeElapsed.Seconds.ToString("00");
    }
}
