using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Loggers;
using Core.Mediators;
using UnityEngine;
using ILogger = Core.Loggers.ILogger;

public class EnemySpawnHandler : MonoBehaviour
{
    [SerializeField]
    private int MaxEnemies = 100;
    [SerializeField]
    private GameObject pipePrefab;
    [SerializeField]
    private int spawnRate;
    [SerializeField]
    private float spawnMaxPosX;
    [SerializeField]
    private float spawnMinPosX;
    [SerializeField]
    private float spawnMaxPosY;
    [SerializeField]
    private float spawnMinPosY;
    
    private ILogger _logger;
    private int _spawnCount = 0;
    private int _killCount = 0;
    private int _howManyEnemiesToSpawn = 5;
    private DateTime _lastSpawnTime;
    
    private IMessenger _messenger;
    private IDisposable _subscription;
    
    void Start()
    {
        ILoggerFactory factory = Game.Container.Resolve<ILoggerFactory>();
        _logger = factory.Create(this);
        
        //InvokeRepeating("SpawnRandomPipe", 2, spawnRate);
        _messenger = Game.Container.Resolve<IMessenger>();
        _subscription = _messenger.Subscribe<EnemyKilledMessage>((m) =>
        {
            _killCount++;
        });
    }

    private void FixedUpdate()
    {
        if (DateTime.Now > _lastSpawnTime.AddSeconds(spawnRate) && (_spawnCount < _killCount + MaxEnemies))
        {
            SpawnRandomPipe();
            _lastSpawnTime = DateTime.Now;
        }
    }

    private void SpawnRandomPipe()
    {
        Enemy enemy = pipePrefab.GetComponent<Enemy>();
        if (enemy.IsDead)
        {
            enemy.Undead();
        }
        
        for (int i = 0; i < _howManyEnemiesToSpawn; i++)
        {
            float spawnPosX = 0;
            float spawnPosY = 0;
            spawnPosX = UnityEngine.Random.Range(spawnMinPosX, spawnMaxPosX);
            spawnPosY = UnityEngine.Random.Range(spawnMaxPosY, spawnMinPosY);

            Vector3 spawnPos = new Vector3(spawnPosX, spawnPosY, 0);
            Quaternion spawnRotation = new Quaternion(0, 0, 0, 0);
            Instantiate(pipePrefab, spawnPos, spawnRotation);
            _spawnCount++;
            _howManyEnemiesToSpawn = CalcHowManyEnemiesToSpawn();
        }
    }

    private int CalcHowManyEnemiesToSpawn()
    {
        switch (_killCount)
        {
            case < 10:
                return 5;
                break;
            case < 20:
                return 7;
                break;
            case < 30:
                return 9;
                break;
            case < 40:
                return 11;
                break;
            case < 50:
                return 13;
                break;
            case < 60:
                return 20;
                break;
            case < 70:
                return 25;
                break;
            case < 80:
                return 30;
                break;
            case < 90:
                return 40;
                break;
            case < 100:
                return 50;
                break;
            case < 1000:
                return 100;
                break;
            default:
                return 5;
                break;
        }
    }
}
