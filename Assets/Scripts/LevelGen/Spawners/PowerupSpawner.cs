using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LevelGen;
using UnityEngine;

public class PowerupGen : MonoBehaviour
{
    [SerializeField] private List<GameObject> powerupList;
    // Start is called before the first frame update
    void Start()
    {
        LevelMap.OnLevelLoaded += SpawnPowerups;
        LevelMap.OnLevelUnloaded += UnloadPowerups;
    }

    private void SpawnPowerups(LevelMap map)
    {
        foreach (Room room in map.rooms)
        {
            if(room.type != RoomType.RewardRoom) continue;

            //select 2 random tiles
            Vector2Int a = room.Floor[Random.Range(0, room.Floor.Count)];
            Vector2Int b = room.Floor[Random.Range(0, room.Floor.Count)];
            for (int i = 0; i < 100 && b == a; i++)
            {
                b = room.Floor[Random.Range(0, room.Floor.Count)];
            }

            Vector3 powerPos = new Vector3(a.x + 0.5f, a.y + 0.5f, 0);
            int powerId1 = Random.Range(0, powerupList.Count);
            GameObject powerUp1 = Instantiate(powerupList[powerId1], powerPos,
                Quaternion.identity);
            
            powerPos = new Vector3(b.x + 0.5f, b.y + 0.5f, 0);
            int powerId2 = Random.Range(0, powerupList.Count);
            for (int i = 0; i < 100 && powerId1 == powerId2; i++)
            {
                powerId2 = Random.Range(0, powerupList.Count);
            }
            GameObject powerUp2 = Instantiate(powerupList[powerId2], powerPos,
                Quaternion.identity);

            Powerup p1 = powerUp1.GetComponent<Powerup>();
            Powerup p2 = powerUp2.GetComponent<Powerup>();

            p1.OnPowerupDestroyed += p2.OtherPowerupDestroyed;
            p2.OnPowerupDestroyed += p1.OtherPowerupDestroyed;
        }
    }

    private void UnloadPowerups()
    {
        GameObject[] objectsToDelete = GameObject.FindGameObjectsWithTag("Powerup");

        foreach (GameObject obj in objectsToDelete)
        {
            Destroy(obj);
        }
    }
}