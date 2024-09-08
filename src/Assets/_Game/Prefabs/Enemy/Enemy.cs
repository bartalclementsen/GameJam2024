using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Loggers;
using Core.Mediators;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using ILogger = Core.Loggers.ILogger;
using Random = System.Random;

public class Enemy : MonoBehaviour
{
    private Random random = new Random();
    private float Speed = 2f;
    public float StoppingDistance = 0f; // Minimum distance to stop near the player
    
    [SerializeField]
    private AudioClip[] _damageSounds;
    
    [SerializeField]
    private TextMeshPro _topText;

    
    [SerializeField]
    private AudioSource _barAudioSource;
    
    [SerializeField]
    private AudioClip[] _barAudioClips;
    
    [SerializeField]
    private string[] _barText;
    
    [SerializeField]
    private AudioSource _audioSource;
    
    [SerializeField]
    private float timeBeingPushed = 0.5f;
    
    [SerializeField]
    private int pushStrength = 10;
    
    [SerializeField]
    public int CurrentHitPonts { get; set; } = 1;
    
    public bool IsDead { get; set; } = false;
    
    private DateTime _lastHitTime;
    
    private Vector2 _attackedFrom = Vector2.zero;
    private DateTime _pushedTime = DateTime.Now;
    private bool _isBeingPushed = false;
    
    private ILogger _logger;
    private GameObject _player;  // Reference to the player's position
    private Rigidbody2D _rb2D;
    private Collider2D _col;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _spriteRenderer_dead;
    private IMessenger _messenger;

    private Random _random = new Random();

    private float _lastBarTime;
    private float _nextBarIn = 0;
    private float _topTextShownOn = 0;
    private bool _topTextShown = false;
    private void TryPlayBarSound()
    {
        if (_barAudioSource.isPlaying || Time.time - _lastBarTime < _nextBarIn)
        {
            return;
        }

        int index = _random.Next(_barAudioClips.Length);
        AudioClip bar = _barAudioClips[index];
        string barText = _barText[index];
        _barAudioSource.PlayOneShot(bar);
        _barAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        _barAudioSource.volume = UnityEngine.Random.Range(0.6f, 1.0f);
        _nextBarIn = Time.time + UnityEngine.Random.Range(10.0f, 20.0f);

        _topText.text = barText;
        _topText.gameObject.SetActive(true);
        _topTextShownOn = Time.time;
        _topTextShown = true;
    }

    private void Awake()
    {
        _nextBarIn = Time.time + UnityEngine.Random.Range(10.0f, 20.0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        Speed = 1f + (float)(400f * random.NextDouble());
        
        ILoggerFactory factory = Game.Container.Resolve<ILoggerFactory>();
        _logger = factory.Create(this);
        
        _messenger = Game.Container.Resolve<IMessenger>();
        
        _player = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
        _rb2D = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(sr => sr.enabled);
        _spriteRenderer_dead = GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(sr => sr.enabled == false);

        if (random.Next(10) == 0)
        {
            MakeBig();
        }
    }

    private void MakeBig()
    {
        Speed = 1000f + (float)(2000f * random.NextDouble());
        _rb2D.mass = 10f;
        transform.localScale = new Vector3(2f, 2f, 2f);
        CurrentHitPonts = 2;
    }

    private void FixedUpdate()
    {
        if (_topTextShown && (Time.time - _topTextShownOn) > 1.5f)
        {
            _topTextShown = false;
            _topText.gameObject.SetActive(false);
        }
        TryPlayBarSound();
        
        // Calculate the distance to the player
        float distance = Vector2.Distance(transform.position, _player.transform.position);

        // Move toward the player if the distance is greater than the stoppingDistance
        if (distance > StoppingDistance && IsDead == false && _isBeingPushed == false)
        {
            Vector2 direction = (_player.transform.position - transform.position).normalized;

            // Move the enemy towards the player
            _rb2D.AddForce(direction * Speed * Time.deltaTime);
        }
        
        if (_isBeingPushed && DateTime.Now.AddSeconds(-timeBeingPushed) > _pushedTime)
        {
            _isBeingPushed = false;
        }

        if (IsDead && _isBeingPushed == false)
        {
            _col.enabled = false;
            _rb2D.bodyType = RigidbodyType2D.Static;
            _spriteRenderer.enabled = false;
            _spriteRenderer_dead.enabled = true;
        }
    }

    public void TakeDamage(Vector2 playerPosition)
    {
        if (IsDead) return;
        if (_lastHitTime.AddSeconds(1) > DateTime.Now) return;
        
        CurrentHitPonts--;
        _lastHitTime = DateTime.Now;
        
        Vector2 direction = ((Vector2)transform.position - playerPosition).normalized;
        _rb2D.AddForce(direction * pushStrength * _rb2D.mass);
        
        _pushedTime = DateTime.Now;
        _isBeingPushed = true;
        
        _messenger.Publish(new EnemyKilledMessage(this));
        PlayRandomDamageSound();

        if (CurrentHitPonts < 1)
        {
            IsDead = true;
        }
    }

    public void Undead()
    {
        IsDead = false;
        _col.enabled = enabled;
        _rb2D.bodyType = RigidbodyType2D.Dynamic;
        _spriteRenderer.enabled = true;
        _spriteRenderer_dead.enabled = false;
    }
    
    private void PlayRandomDamageSound()
    {
        if (_audioSource.isPlaying)
        {
            return;
        }
        
        _audioSource.clip = _damageSounds[_random.Next(_damageSounds.Length)];
        _audioSource.Play();
        _audioSource.volume = 0.8f + (float)(0.2f * _random.NextDouble());
        _audioSource.volume = 0.8f + (float)(0.4f * _random.NextDouble());
    }
}
