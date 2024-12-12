using System.Collections;
using System.Collections.Generic;
using LevelGen;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [Header("Enemies")]  [Tooltip("If enabled, spawns new enemies. If disabled, initializes existing enemies")][SerializeField]
    private bool doSpawnEnemies;
    [Tooltip("Each arena room will contain between x and y amount of enemies")]
    [SerializeField]private Vector2Int enemyAmountRangeArena1 = new(3, 5);
    [SerializeField]private Vector2Int enemyAmountRangeArena2 = new(5, 7);
    [SerializeField]private Vector2Int enemyAmountRangeArena3 = new(8, 10);

    public GameObject MeleeEnemyPrefab;
    public GameObject RangedEnemyPrefab;
    // Start is called before the first frame update
    void Start()
    {
        LevelMap.OnLevelLoaded += SpawnEnemies;
        LevelMap.OnLevelUnloaded += DeleteEnemies;
    }

    private void SpawnEnemies(LevelMap levelMap)
    {
        if (doSpawnEnemies)
        {
            //regenerate room shape lists
            for (int r = 0; r < levelMap.rooms.Count; r++)
            {
                Room room = levelMap.rooms[r];

                //place enemies
                if (room.type == RoomType.Arena1 || room.type == RoomType.Arena2 || room.type == RoomType.Arena3)
                {
                    int enemyAmount;
                    switch (room.type)
                    {
                        case RoomType.Arena1:
                            enemyAmount = Random.Range(enemyAmountRangeArena1.x, enemyAmountRangeArena1.y);
                            break;
                        case RoomType.Arena2:
                            enemyAmount = Random.Range(enemyAmountRangeArena2.x, enemyAmountRangeArena2.y);
                            break;
                        case RoomType.Arena3:
                            enemyAmount = Random.Range(enemyAmountRangeArena3.x, enemyAmountRangeArena3.y);
                            break;
                        default:
                            enemyAmount = 1;
                            break;
                    }

                    for (int i = 0; i < enemyAmount; i++)
                    {
                        Vector2Int enemyPositionTile = room.Floor[Random.Range(0, room.Floor.Count - 1)];
                        Vector3 enemyPosition = new Vector3(enemyPositionTile.x + 0.5f, enemyPositionTile.y + 0.5f, 0);
                        GameObject go = Instantiate(Random.Range(0, 2) > 0 ? MeleeEnemyPrefab : RangedEnemyPrefab,
                            enemyPosition, Quaternion.identity);
                        Enemy e = go.GetComponent<Enemy>();
                        e.room = r;
                        EnemyGetCount.enemyCount++;
                    }
                }
            }
        }
        //assign room id to pre-placed enemies
        else
        {
            //find all game objects tagged "Enemy"
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Enemy");

            //loop through objects
            foreach (GameObject enemyObject in objects)
            {
                //check if object with enemy tag is an enemy object
                Enemy enemy = enemyObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Vector2Int tilePos = new Vector2Int(
                        Mathf.FloorToInt(transform.position.x),
                        Mathf.FloorToInt(transform.position.y));
                    
                    //check what room the enemy is in
                    int potentialRoom = levelMap.FindRoom(tilePos);
                    print(potentialRoom);
                    if (potentialRoom == -1) continue;
                    
                    //set room of enemy
                    enemy.room = potentialRoom;
                }
            }
        }
    }

    private void DeleteEnemies()
    {
        GameObject[] objectsToDelete = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject obj in objectsToDelete)
        {
            Destroy(obj);
        }
    }
}
