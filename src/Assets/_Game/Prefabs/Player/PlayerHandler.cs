using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Loggers;
using Core.Mediators;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using ILogger = Core.Loggers.ILogger;
using Random = System.Random;


public class PlayerScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private SpearHandler spear; // Assign the spear GameObject in the inspector
    
    [SerializeField] private float attackDistance = 1.5f; // How far the spear moves forward
    
    [SerializeField] private float attackSpeed = 12f;
    
    [SerializeField] private float attackRetractionSpeed = 2f; // Speed of spear thrust
    
    [SerializeField] private float attackDuration = 0.2f;
    
    [SerializeField] private float timeBeingPushed = 0.5f;
    
    [SerializeField] private int pushStrength = 10;
    
    [SerializeField] private AudioClip movementAudioClip;
    
    [SerializeField] private Transform _modelTransform;
    
    [SerializeField] private AudioClip[] _weaponSwingAudioClip;
    
    [SerializeField] private AudioSource _weaponAudioSource;
    
    public int CurrentHitPonts { get; set; } = 5;

    private Random _random = new Random();
    private ILogger _logger;
    private Vector2 movement = Vector2.zero;
    private Vector2 aimInput;
    private Rigidbody2D rb;
    private Vector3 initialSpearPosition;
    private AudioSource audioSource;
    private bool isAttacking = false;
    private bool spearForward = true;
    private float attackTime;
    private bool isInDanger = false;
    private Vector2 _attackedFrom = Vector2.zero;
    private DateTime _pushedTime = DateTime.Now;
    
    private bool _isBeingPushed = false;
    private IMessenger _messenger;
    
    private PlayerControls _playerControls;
    private InputAction move;
    private InputAction look;
    private InputAction fire;
    

    private void OnEnable()
    {
        move = _playerControls.Player.Move;
        look = _playerControls.Player.Look;
        fire = _playerControls.Player.Fire;
        move.Enable();
        fire.Enable();
        look.Enable();

        fire.performed += Fire;
    }

    private void OnDisable()
    {
        move.Disable();
        fire.Disable();
        look.Disable();
    }
    
    private void Awake()
    {
        _playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = movementAudioClip;
        audioSource.loop = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        ILoggerFactory factory = Game.Container.Resolve<ILoggerFactory>();
        _logger = factory.Create(this);

        initialSpearPosition = spear.transform.localPosition;

        _messenger = Game.Container.Resolve<IMessenger>();
    }
    
    void Update()
    {
        movement = move.ReadValue<Vector2>();
        aimInput = look.ReadValue<Vector2>();
        
        // Handle aiming
        if (aimInput.magnitude > 0.1f)
        {
            // Calculate target rotation angle
            float targetAngle = Mathf.Atan2(aimInput.x, aimInput.y) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

            // Smoothly rotate towards the target rotation
            _modelTransform.rotation =
                Quaternion.Slerp(_modelTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        
        if (isAttacking)
        {
            MoveSpear();
        }
    }
    
    

    private void Fire(InputAction.CallbackContext context)
    {
        if (isAttacking)
        {
            return;
        }
        isAttacking = true;
    }

    void FixedUpdate()
    {
        Vector2 movement2 = movement.normalized * (moveSpeed * Time.fixedDeltaTime);
        if (_isBeingPushed == false)
        {
            rb.MovePosition(rb.position + movement2);
        }
        
        bool isMoving = movement2 != Vector2.zero;
        
        if (isMoving == false && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else if (isMoving && audioSource.isPlaying == false)
        {
            audioSource.Play();
        }
        
        if (isInDanger && _lastDamageTakeTime + 1 < Time.time)
        {
            Vector2 direction = ((Vector2)transform.position - _attackedFrom).normalized;
            rb.velocity = direction * pushStrength;

            _pushedTime = DateTime.Now;
            _isBeingPushed = true;
            _attackedFrom = Vector2.zero;
            
            isInDanger = false;
            _lastDamageTakeTime = Time.time;
            CurrentHitPonts--;
            _messenger.Publish(new PlayerTookDamageMessage(this));
            
            
            if (CurrentHitPonts < 1)
            {
                // DIE
                _logger.Log("DIE");
            }
        }

        if (_isBeingPushed && DateTime.Now.AddSeconds(-timeBeingPushed) > _pushedTime)
        {
            _isBeingPushed = false;
            rb.velocity = Vector2.zero;
        }
    }

    private float _lastDamageTakeTime;

    void MoveSpear()
    {
        // Move spear forward if spearForward is true, otherwise move back
        if (spearForward)
        {
            PlayRandomWeaponSwingAudio();
            spear.IsDeadly = true;
            // Move the spear forward in local space
            spear.transform.localPosition = Vector3.MoveTowards(
                spear.transform.localPosition,
                initialSpearPosition + transform.up * attackDistance, // or forward in 3D
                attackSpeed * Time.deltaTime
            );

            // Check if the spear has reached the attack distance
            if (Vector3.Distance(spear.transform.localPosition, initialSpearPosition + transform.up * attackDistance) <
                0.1f)
            {
                spearForward = false; // Start retracting
            }
        }
        else
        {
            spear.IsDeadly = false;
            // Move the spear back to its original position
            spear.transform.localPosition = Vector3.MoveTowards(
                spear.transform.localPosition,
                initialSpearPosition,
                attackRetractionSpeed * Time.deltaTime
            );

            // Check if the spear has returned to its initial position
            if (Vector3.Distance(spear.transform.localPosition, initialSpearPosition) < 0.1f)
            {
                _weaponAudioSource.Stop();
                spearForward = true; // Ready for the next attack
                isAttacking = false; // Attack sequence ends
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemyCollider = collision.GetComponent<Enemy>();
            if ( enemyCollider is not null)
            {
                isInDanger = true;

                if (_attackedFrom == Vector2.zero)
                {
                    _attackedFrom = enemyCollider.transform.position;
                }
            }    
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemyCollider = other.GetComponent<Enemy>();
            if ( enemyCollider is not null)
            {
                isInDanger = false;
            }    
        }
    }

    private void PlayRandomWeaponSwingAudio()
    {
        if (_weaponAudioSource.isPlaying)
        {
            return;
        }

        _weaponAudioSource.clip = _weaponSwingAudioClip[_random.Next(_weaponSwingAudioClip.Length)];
        _weaponAudioSource.Play();

        _weaponAudioSource.volume = 0.05f + (float)(0.1 * _random.NextDouble());
        _weaponAudioSource.pitch = 0.7f + (float)(0.3 * _random.NextDouble());
    }
}