using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Loggers;
using Core.Mediators;
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
    private AudioClip[] _barSounds;

    [SerializeField]
    private AudioSource _audioSource;
    
    public bool IsDead { get; set; } = false;
    
    private ILogger _logger;
    private GameObject _player;  // Reference to the player's position
    private Rigidbody2D _rb2D;
    private Collider2D _col;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _spriteRenderer_dead;
    private IMessenger _messenger;

    private Random _random = new Random();
    
    // Start is called before the first frame update
    void Start()
    {
        Speed = 1f + (float)(4f * random.NextDouble());
        
        ILoggerFactory factory = Game.Container.Resolve<ILoggerFactory>();
        _logger = factory.Create(this);
        
        _messenger = Game.Container.Resolve<IMessenger>();
        
        _player = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
        _rb2D = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(sr => sr.enabled);
        _spriteRenderer_dead = GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(sr => sr.enabled == false);
    }
    
    private void FixedUpdate()
    {
        // Calculate the distance to the player
        float distance = Vector2.Distance(transform.position, _player.transform.position);

        // Move toward the player if the distance is greater than the stoppingDistance
        if (distance > StoppingDistance && IsDead == false)
        {
            Vector2 direction = (_player.transform.position - transform.position).normalized;

            // Move the enemy towards the player
            _rb2D.MovePosition((Vector2)transform.position + direction * Speed * Time.deltaTime);
        }
        
        EnemyBarSounds();
    }

    public void TakeDamage(int num)
    {
        if (IsDead) return;
        
        IsDead = true;
        _col.enabled = false;
        _rb2D.bodyType = RigidbodyType2D.Static;
        _spriteRenderer.enabled = false;
        _spriteRenderer_dead.enabled = true;
        
        _messenger.Publish(new EnemyKilledMessage(this));
        PlayRandomCharacterSound(_damageSounds);
    }

    public void EnemyBarSounds()
    {
        PlayRandomCharacterSound(_barSounds);
    }

    public void Undead()
    {
        IsDead = false;
        _col.enabled = enabled;
        _rb2D.bodyType = RigidbodyType2D.Dynamic;
        _spriteRenderer.enabled = true;
        _spriteRenderer_dead.enabled = false;
    }
    
    private void PlayRandomCharacterSound(AudioClip[] _soundEffects)
    {
        if (_audioSource.isPlaying)
        {
            return;
        }
        
        _audioSource.clip = _soundEffects[_random.Next(_soundEffects.Length)];
        _audioSource.Play();
        _audioSource.volume = 0.8f + (float)(0.2f * _random.NextDouble());
        _audioSource.volume = 0.8f + (float)(0.4f * _random.NextDouble());
    }
}
