using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    public float Speed = 40f;  // Enemy's movement speed
    public float StoppingDistance = 0f; // Minimum distance to stop near the player
    
    private GameObject _player;  // Reference to the player's position
    private Rigidbody2D _rb2D;
    
    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
        _rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the distance to the player
        float distance = Vector2.Distance(transform.position, _player.transform.position);

        // Move toward the player if the distance is greater than the stoppingDistance
        if (distance > StoppingDistance)
        {
            Vector2 direction = (_player.transform.position - transform.position).normalized;

            // Move the enemy towards the player
            _rb2D.MovePosition((Vector2)transform.position + direction * Speed * Time.deltaTime);
        }
    }

    public void TakeDamage(int num)
    {
    }
}
