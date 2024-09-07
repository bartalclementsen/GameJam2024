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

public class Enemy : MonoBehaviour
{
    public float Speed = 2f;  // Enemy's movement speed
    public float StoppingDistance = 0f; // Minimum distance to stop near the player
    
    private ILogger _logger;
    private bool _isDead = false;
    private GameObject _player;  // Reference to the player's position
    private Rigidbody2D _rb2D;
    private Collider2D _col;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _spriteRenderer_dead;
    private IMessenger _messenger;
    
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
        if (distance > StoppingDistance && _isDead == false)
        {
            Vector2 direction = (_player.transform.position - transform.position).normalized;

            // Move the enemy towards the player
            _rb2D.MovePosition((Vector2)transform.position + direction * Speed * Time.deltaTime);
        }
    }

    public void TakeDamage(int num)
    {
        if (_isDead) return;
        
        _isDead = true;
        _col.enabled = false;
        _rb2D.bodyType = RigidbodyType2D.Static;
        _spriteRenderer.enabled = false;
        _spriteRenderer_dead.enabled = true;
        
        _messenger.Publish(new EnemyKilledMessage(this));
    }
}
