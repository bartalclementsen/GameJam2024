using System;
using _Game;
using Core.Mediators;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class LevelSceneHandler : MonoBehaviour
{
    private bool isPaused = false;

    public void SubmitHighScore()
    {
        string name = _highScoreNameField.text;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Unknown";
        }
        
        var timeElapsed = (DateTime.Now - _timeStart);
        highScoreService.AddHighScore(name, totalKills, timeElapsed);
        
        _highScoreNameField.gameObject.SetActive(false);
        _highScoreSubmitButton.SetActive(false);
    }
    
    [SerializeField]
    private TMP_InputField _highScoreNameField;
    
    
    [SerializeField]
    private GameObject _highScoreSubmitButton;
    
    [SerializeField]
    private TextMeshProUGUI _scoreText;
    
    [SerializeField]
    private TextMeshProUGUI _hitPointsText;
    
    [SerializeField]
    private GameObject _finishedMenu;
    
    [SerializeField]
    private TextMeshProUGUI _finishedKilldText;
    
    [SerializeField]
    private TextMeshProUGUI _finishedTimeText;
    
    [SerializeField]
    private GameObject retryButton;
    
    [SerializeField]
    private EventSystem eventSystem;
    
    [SerializeField]
    private GameObject pauseMenuUI;

    [SerializeField]
    private AudioSource crowdAudioSource;
    
    [SerializeField]
    private AudioSource crowdSwellAudioSource;
    
    [SerializeField]
    private AudioClip[] crowdSwellAudioClips;
    
    [SerializeField]
    private AudioSource mainAudioSource;
    
    [SerializeField]
    private AudioClip themeMusic;
    
    [SerializeField]
    private Image RedFlashImage;
    
    [SerializeField]
    private Image RedFlashImage2;
    
    [SerializeField]
    private Image RedFlashImage3;
    
    [SerializeField]
    private Image RedFlashImage4;

    private void TryPlayRandomCrowCheer()
    {
        if (Time.time - lastKillTime < 1.0f || random.Next(10) != 0)
        {
            return;
        }
        
        if (crowdSwellAudioSource.isPlaying)
        {
            return;
        }
        
        var clip = crowdSwellAudioClips[random.Next(crowdSwellAudioClips.Length)];
        crowdSwellAudioSource.clip = clip;
        crowdSwellAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        crowdSwellAudioSource.Play();
    }
    
    private IMessenger _messenger;
    private IDisposable _subscription;
    private IDisposable _subscription2;

    private int totalKills = 0;
    private DateTime _timeStart;
    private PlayerControls _playerControls;
    private InputAction navigate;
    private InputAction click;
    private bool isDead = false;
    private Random random = new Random();
    private IHighScoreService highScoreService;
    private bool _shouldShowImage;
    private DateTime _sinceShowedImage;

    private void ShowFinishedScreen()
    {
        _finishedMenu.SetActive(true);
        eventSystem.SetSelectedGameObject(retryButton);
        
        var timeElapsed = (DateTime.Now - _timeStart);
        
        _finishedKilldText.text = totalKills + " kills";
        _finishedTimeText.text =
            ((int)timeElapsed.TotalMinutes).ToString("00") + ":" + timeElapsed.Seconds.ToString("00");

        mainAudioSource.Stop();
        mainAudioSource.clip = themeMusic;
        mainAudioSource.volume = 0.6f;
        mainAudioSource.Play();
        
        crowdAudioSource.volume = 0.3f;
        
        Time.timeScale = 0;
    }

    private void OnEnable()
    {
        click = _playerControls.UI.Pause;
        navigate = _playerControls.UI.Navigate;
        navigate.Enable();
        click.Enable();
        
        click.performed += context =>
        {
            TryToPauseGame();
        };
    }

    private void OnDisable()
    {
        navigate.Disable();
        click.Disable();
    }

    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    private void OnDestroy()
    {
        _subscription.Dispose();
        _subscription2.Dispose();
    }

    private float lastKillTime = 0;
    private void Start()
    {
        _messenger = Game.Container.Resolve<IMessenger>();
        highScoreService = Game.Container.Resolve<IHighScoreService>();
        _subscription = _messenger.Subscribe<EnemyKilledMessage>((m) =>
        {
            
            totalKills++;

            TryPlayRandomCrowCheer();
            
            lastKillTime = Time.time;
        });
        
        _subscription2 = _messenger.Subscribe<PlayerTookDamageMessage>((m) =>
        {
            _hitPointsText.text = "HP " + Math.Max(0, m.PlayerHandler.CurrentHitPonts) + "/" + 10;

            _shouldShowImage = true;
            _sinceShowedImage = DateTime.Now;
            
            if (m.PlayerHandler.CurrentHitPonts < 1)
            {
                isDead = true;
                ShowFinishedScreen();
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

    public void ContinueGame()
    {
        TryToPauseGame();
    }
    
    public void RetryGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
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
        if (isDead == false)
        {
            UpdateScoreText();

            UpdateDamageImage();
        }
    }

    private void UpdateScoreText()
    {
        var timeElapsed = (DateTime.Now - _timeStart);
        _scoreText.text = "Kills " + totalKills.ToString("00") + Environment.NewLine + "Time " + ((int)timeElapsed.TotalMinutes).ToString("00") + ":" + timeElapsed.Seconds.ToString("00");
    }

    private void UpdateDamageImage()
    {
        if (_shouldShowImage)
        {
            RedFlashImage.enabled = true;
            RedFlashImage2.enabled = true;
            RedFlashImage3.enabled = true;
            RedFlashImage4.enabled = true;

            if (_sinceShowedImage.AddSeconds(1) < DateTime.Now)
            {
                _shouldShowImage = false;
                RedFlashImage.enabled = false;
                RedFlashImage2.enabled = false;
                RedFlashImage3.enabled = false;
                RedFlashImage4.enabled = false;
            }
        }
    }
}
