using System.Collections;
using System.Collections.Generic;
using LevelGen;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [Header("Enemies")] [Tooltip("Each arena room will contain between x and y amount of enemies")] [SerializeField]
    private Vector2Int enemyAmountRangeArena1 = new(3, 5);
    private Vector2Int enemyAmountRangeArena2 = new(5, 7);
    private Vector2Int enemyAmountRangeArena3 = new(8, 10);

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
                    GameObject go = Instantiate(MeleeEnemyPrefab, enemyPosition, Quaternion.identity);
                    Enemy e = go.GetComponent<Enemy>();
                    e.room = r;
                    EnemyGetCount.enemyCount++;
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
