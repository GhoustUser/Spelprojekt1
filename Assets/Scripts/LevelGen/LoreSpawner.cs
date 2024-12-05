using System.Collections.Generic;
using UnityEngine;

namespace LevelGen
{
    public class LoreSpawner : MonoBehaviour
    {
        private LevelMap levelMap;
        public GameObject DocumentBenchPrefab;
        public DialogueManager dialogueManager;

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
                
                //list of rooms
                List<int> roomIndices = new List<int>();
                for(int i = 0; i < levelMap.rooms.Count; i++) roomIndices.Add(i);

                for (int j = 0; j < dialogueManager.Dialogues.Length; j++)
                {
                    int tempIndex = Random.Range(0, roomIndices.Count);
                    int roomIndex = roomIndices[tempIndex];
                    roomIndices.RemoveAt(tempIndex);

                    int tileIndex = Random.Range(0, levelMap.rooms[roomIndex].shape.Count);
                    Vector2Int tilePos = levelMap.rooms[roomIndex].shape[tileIndex];
                    Vector3 objectPos = new(tilePos.x + 0.5f, tilePos.y + 0.5f, 0f);

                    GameObject bench = Instantiate(DocumentBenchPrefab, objectPos, Quaternion.identity);
                    DialogueTrigger dt = bench.GetComponentInChildren<DialogueTrigger>();
                    dt.dialogue = dialogueManager.Dialogues[j];
                }
            }
        }
    }
}
