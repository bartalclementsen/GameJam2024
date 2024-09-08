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

public class EnemyBar : MonoBehaviour
{
    private float Speed = new Random().Next(3, 6);  // Enemy's movement speed
    public float StoppingDistance = 0f; // Minimum distance to stop near the player
    
    [SerializeField]
    private AudioClip[] _damageSounds;
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
        PlayRandomDamageSound();
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

    private void PlayRandomBarSounds()
    {
        if (_audioSource.isPlaying)
        {
            return;
        }

        _audioSource.clip = _barSounds[_random.Next(_barSounds.Length)];
        _audioSource.Play();
    }

    // Calculate the distance to the player
    private void AdjustVolumeBasedOnDistance()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Calculate volume based on distance (closer to the player means louder)
        float volume = Mathf.Clamp01(1 - ((distanceToPlayer - maxVolumeDistance) / (minVolumeDistance - maxVolumeDistance)));

        // Set the audio volume, ensuring it's between 0 and 1
        _audioSource.volume = Mathf.Clamp(volume, 0f, 1f);
    }
}
