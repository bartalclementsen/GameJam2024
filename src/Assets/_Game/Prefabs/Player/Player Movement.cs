using System;
using System.Collections;
using System.Collections.Generic;
using Core.Loggers;
using UnityEngine;
using UnityEngine.InputSystem;
using ILogger = Core.Loggers.ILogger;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] 
    private float moveSpeed = 5f;
    
    [SerializeField] 
    private float rotationSpeed = 10f;

    [SerializeField] 
    private Transform _modelTransform;
    
    private ILogger _logger;
    private Vector2 movement;
    private Vector2 aimInput;
    public Rigidbody2D rb; // Assuming 2D physics

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ILoggerFactory factory = Game.Container.Resolve<ILoggerFactory>();
        _logger = factory.Create(this);
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
            _modelTransform.rotation = Quaternion.Slerp(_modelTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void FixedUpdate()
    {
        Vector2 movement2 = movement.normalized * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + movement2);
    }
}
