using System.Collections.Generic;
using LevelGen;
using UnityEngine;

public class PowerupGen : MonoBehaviour
{
    [SerializeField] private List<GameObject> powerupList;
    private HashSet<int> takenPowerups;

    void Start()
    {
        takenPowerups = new HashSet<int>();

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
            for (int i = 0; i < 100; i++)
            {
                a = room.Floor[Random.Range(0, room.Floor.Count)];
                bool isValid = true;
                foreach (Door door in room.Doors)
                {
                    if (Vector2Int.Distance(a, door.Position) < 4)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid) break;
            }
            Vector2Int b = room.Floor[Random.Range(0, room.Floor.Count)];
            for (int i = 0; i < 100; i++)
            {
                b = room.Floor[Random.Range(0, room.Floor.Count)];
                bool isValid = true;
                foreach (Door door in room.Doors)
                {
                    if (Vector2Int.Distance(b, door.Position) < 4)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (Vector2Int.Distance(a, b) < 4) isValid = false;

                if (isValid) break;
            }

            int availablePowerups = 0;

            for (int i = 0; i < powerupList.Count; i++)
            {
                if (!takenPowerups.Contains(i)) availablePowerups++;
            }

            if (availablePowerups < 2) takenPowerups = new HashSet<int>();

            Vector3 powerPos1 = new Vector3(a.x + 0.5f, a.y + 0.5f, 0);
            Vector3 powerPos2 = new Vector3(b.x + 0.5f, b.y + 0.5f, 0);

            int powerId1 = -1;
            int powerId2 = -1;

            do
            {
                powerId1 = Random.Range(0, powerupList.Count);
            } while (takenPowerups.Contains(powerId1));

            do
            {
                powerId2 = Random.Range(0, powerupList.Count);
            } while (takenPowerups.Contains(powerId2) || powerId1 == powerId2);

            takenPowerups.Add(powerId1);
            takenPowerups.Add(powerId2);

            GameObject powerUp1 = Instantiate(powerupList[powerId1], powerPos1, Quaternion.identity);
            GameObject powerUp2 = Instantiate(powerupList[powerId2], powerPos2, Quaternion.identity);

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