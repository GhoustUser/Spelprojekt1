using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlbinEnemySpawner : MonoBehaviour
{
    private Transform playerPosition;
    [SerializeField] 
    private GameObject enemyPrefab1;
    [SerializeField] 
    private GameObject enemyPrefab2;
    private Vector3 spawnPoint;
    private GameObject[] enemyList;
    [SerializeField]
    private float difficulty = 1;

    private float checkFrequency = 1;

    private SpriteRenderer sr;

    private Transform spawn;
    [SerializeField]
    private bool countCheck; 
    // Start is called before the first frame update
    void Start()
    {
        spawn = transform.parent; 
        spawnPoint = spawn.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        checkFrequency = checkFrequency - Time.deltaTime;
        if(checkFrequency < 0)
        {
            EnemySpawn();
            checkFrequency = 10;
        }

    }

    void EnemySpawn()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length <= 0 && countCheck)
        {
            Instantiate(enemyPrefab1, spawnPoint, Quaternion.identity);
            Instantiate(enemyPrefab2, spawnPoint, Quaternion.identity);
        }
        else
        {
            Instantiate(enemyPrefab1, spawnPoint, Quaternion.identity);
            Instantiate(enemyPrefab2, spawnPoint, Quaternion.identity);
        }
        
    }
}
