using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Loggers;
using ILogger = Core.Loggers.ILogger;

public class SpearHandler : MonoBehaviour
{
    public bool IsDeadly { get; set; }
    
    private ILogger _logger;
    
    public SpearHandler()
    {
        
        //_logger.Log("cons!!!!");
    }


    // Start is called before the first frame update
    void Start()
    {
        ILoggerFactory factory = Game.Container.Resolve<ILoggerFactory>();
        _logger = factory.Create(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsDeadly && collision.CompareTag("Enemy"))
        {
            // Deal damage to the enemy
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(transform.position);
            }
        }
    }
}
