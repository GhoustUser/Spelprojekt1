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

    private MeleeEnemy enemy; 
    // Start is called before the first frame update
    void Start()
    {
        playerPosition = transform.parent;
        spawnPoint = playerPosition.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        checkFrequency = checkFrequency - Time.deltaTime;
        if(checkFrequency < 0)
        {
            EnemySpawn();
            checkFrequency = 1;
        }

    }

    void EnemySpawn()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length <= 0)
        {
            GameObject copy = Instantiate(enemyPrefab1, spawnPoint, Quaternion.identity);
            sr = copy.GetComponent<SpriteRenderer>();
            sr.color *= new Color(0.3f,0,0.3f,1 );
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(difficulty, difficulty);
            enemy = copy.GetComponent<MeleeEnemy>();
            
            GameObject copy2 = Instantiate(enemyPrefab2, spawnPoint, Quaternion.identity);
            sr = copy2.GetComponent<SpriteRenderer>();
            sr.color = new Color(0.3f,0,0.3f,1);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(difficulty, difficulty);
            
        }
        
    }
}
