using System.Collections.Generic;
using UnityEngine;

namespace LevelGen
{
    public class LoreSpawner : MonoBehaviour
    {
        private LevelMap levelMap;
        public GameObject DocumentBenchPrefab;
        public DialogueManager dialogueManager;
        public GameObject player;

        private bool hasSpawned = false;
        // Start is called before the first frame update
        void Start()
        {
            levelMap = GetComponent<LevelMap>();
        }

        // Update is called once per frame
        void Update()
        {
            //if map has not generated
            if (!levelMap.isGenerated)
            {
                //if benches have spawned
                if (hasSpawned)
                {
                    hasSpawned = false;
                    //delete benches
                    GameObject[] objectsToDelete = GameObject.FindGameObjectsWithTag("DocumentBench");

                    foreach (GameObject obj in objectsToDelete)
                    {
                        Destroy(obj);
                    }
                }
            }
            //if map has generated, but benches have not spawned
            else if (!hasSpawned)
            {
                //spawn benches
                hasSpawned = true;
                
                if(player == null) print("please add reference to player in LoreSpawner");
                
                //list of rooms
                List<int> roomIndices = new List<int>();
                for(int i = 0; i < levelMap.rooms.Count; i++) roomIndices.Add(i);

                for (int j = 0; j < dialogueManager.Dialogues.Length; j++)
                {
                    //temporary index for index array
                    int tempIndex = Random.Range(0, roomIndices.Count);
                    //guarantee first dialogue in first room
                    if (j == 0) tempIndex = 0;
                    
                    //room index
                    int roomIndex = roomIndices[tempIndex];

                    //select random tile in room
                    int tileIndex = Random.Range(0, levelMap.rooms[roomIndex].shape.Count);
                    //calculate position
                    Vector2Int tilePos = levelMap.rooms[roomIndex].shape[tileIndex];
                    Vector3 objectPos = new(tilePos.x + 0.5f, tilePos.y + 0.5f, 0f);
                    
                    //retry if too close to spawn
                    if (player != null && Vector3.Distance(objectPos, player.transform.position) < 4f)
                    {
                        j--;
                        continue;
                    }
                    
                    //remove room index from array to prevent multiple benches in the same room
                    roomIndices.RemoveAt(tempIndex);

                    //spawn bench
                    GameObject bench = Instantiate(DocumentBenchPrefab, objectPos, Quaternion.identity);
                    DialogueTrigger dt = bench.GetComponentInChildren<DialogueTrigger>();
                    dt.dialogue = dialogueManager.Dialogues[j];
                }
            }
        }
    }
}
