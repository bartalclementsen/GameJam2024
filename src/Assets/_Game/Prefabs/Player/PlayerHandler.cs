using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Loggers;
using Core.Mediators;
using Unity.VisualScripting;
using UnityEngine;
using ILogger = Core.Loggers.ILogger;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private GameObject spear; // Assign the spear GameObject in the inspector
    
    [SerializeField] private float attackDistance = 1.5f; // How far the spear moves forward
    
    [SerializeField] private float attackSpeed = 10f; // Speed of spear thrust
    
    [SerializeField] private float attackDuration = 0.2f;
    
    [SerializeField] private AudioClip movementAudioClip;
    
    [SerializeField] private Transform _modelTransform;
    
    public int CurrentHitPonts { get; set; } = 5;

    private ILogger _logger;
    private Vector2 movement;
    private Vector2 aimInput;
    private Rigidbody2D rb;
    private Vector3 initialSpearPosition;
    private AudioSource audioSource;
    private bool isAttacking = false;
    private bool spearForward = true;
    private float attackTime;
    private bool isInDanger = false;
    private IMessenger _messenger;

    private void Awake()
    {
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
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        aimInput = new Vector2(Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical"));

        // Handle aiming
        if (aimInput.magnitude > 0.1f)
        {
            // Calculate target rotation angle
            float targetAngle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

            // Smoothly rotate towards the target rotation
            _modelTransform.rotation =
                Quaternion.Slerp(_modelTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Trigger attack on button press (left mouse button)
        if (Input.GetAxis("RT") == 1f & !isAttacking)
        {
            isAttacking = true;
        }

        // If attacking, count down the attack duration
        if (isAttacking)
        {
            MoveSpear();
        }
    }

    void FixedUpdate()
    {
        Vector2 movement2 = movement.normalized * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + movement2);
        
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
    }

    private float _lastDamageTakeTime;

    void MoveSpear()
    {
        // Move spear forward if spearForward is true, otherwise move back
        if (spearForward)
        {
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
            // Move the spear back to its original position
            spear.transform.localPosition = Vector3.MoveTowards(
                spear.transform.localPosition,
                initialSpearPosition,
                attackSpeed * Time.deltaTime
            );

            // Check if the spear has returned to its initial position
            if (Vector3.Distance(spear.transform.localPosition, initialSpearPosition) < 0.1f)
            {
                spearForward = true; // Ready for the next attack
                isAttacking = false; // Attack sequence ends
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        Enemy enemyCollider = collision.GetComponent<Enemy>();
        if ( enemyCollider is not null)
        {
            isInDanger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Enemy enemyCollider = other.GetComponent<Enemy>();
        if ( enemyCollider is not null)
        {
            isInDanger = false;
        }
    }
}