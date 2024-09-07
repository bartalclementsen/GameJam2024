using System;
using System.Collections;
using System.Collections.Generic;
using Core.Loggers;
using UnityEngine;
using UnityEngine.InputSystem;
using ILogger = Core.Loggers.ILogger;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 movement;
    private Vector2 rotate;
    public InputAction lookInput;
    private ILogger _logger;
    PlayerControls _playerControls;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.Gameplay.Rotate.performed += ctx => rotate = ctx.ReadValue<Vector2>();
        _playerControls.Gameplay.Rotate.canceled += ctx => rotate = Vector2.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        ILoggerFactory factory = Game.Container.Resolve<ILoggerFactory>();
        _logger = factory.Create(this);
    }

    void Update()
    {
        // Get input from the keyboard (WASD or Arrow keys)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");


        
        _logger.Log(rotate);
        Vector2 r = new Vector2(rotate.x, rotate.y) * 100f * Time.deltaTime;
        transform.Rotate(r, Space.World);
        _logger.Log("-----------------");

        // Normalize the vector to prevent faster diagonal movement
        movement.Normalize();
        
    }

    void FixedUpdate()
    {
        // Move the player
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}
