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
    public InputAction lookInput;
    private ILogger _logger;

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

        
        System.Numerics.Vector2 test = lookInput.ReadValue<System.Numerics.Vector2>();
        _logger.Log(test.X*100);
        _logger.Log(test.Y*100);
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
