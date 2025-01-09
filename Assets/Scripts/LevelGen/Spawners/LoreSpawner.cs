using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelGen
{
    public class LoreSpawner : MonoBehaviour
    {
        public GameObject DocumentBenchPrefab;
        public DialogueManager dialogueManager;
        [HideInInspector] public Player player;

        // Start is called before the first frame update
        void Start()
        {
            player = FindObjectOfType<Player>();
            Props.OnPropsLoaded += SpawnDocuments;
            LevelMap.OnLevelUnloaded += DeleteDocuments;
        }

        private void SpawnDocuments(LevelMap levelMap)
        {
            if (dialogueManager == null) return;
            //list of rooms
            IEnumerable<int> roomIndicesEnumerable = levelMap.rooms
                .Select((room, index) => new { room, index }) // Project both room and its index
                .Where(x => x.room.type == RoomType.LoreRoom) // Filter based on the condition
                .Select(x => x.index); // Select only the indices
            List<int> roomIndices = new List<int>();
            var indicesEnumerable = roomIndicesEnumerable as int[] ?? roomIndicesEnumerable.ToArray();
            
            //dynamic list of rooms to place logs in
            for(int i = 0; i<  indicesEnumerable.Count(); i++) roomIndices.Add(indicesEnumerable[i]);
            
            //list of locations for logs
            List<Vector2Int> logPositions = new List<Vector2Int>();

            //place logs
            for (int j = 0; j < 100; j++)
            {
                if (logPositions.Count >= dialogueManager.Dialogues.Length) break;
                if (roomIndices.Count == 0)
                {
                    for (int i = 0; i < indicesEnumerable.Count(); i++) roomIndices.Add(indicesEnumerable[i]);
                }
                
                Vector2Int tilePos;
                if (levelMap.rooms.Count == 0) continue;
                
                List<Vector2Int> counterTops = levelMap.rooms[roomIndices[0]].counterTops;
                if (counterTops.Count > 0 && Random.Range(0, 10) > 2)
                {
                    tilePos = counterTops[Random.Range(0, counterTops.Count - 1)];
                }
                else
                    tilePos = levelMap.rooms[roomIndices[0]]
                        .Floor[Random.Range(0, levelMap.rooms[roomIndices[0]].Floor.Count - 1)];
                
                //retry if too close to other document
                bool isValid = true;
                foreach (var pos in logPositions) 
                {
                    if (Vector2Int.Distance(pos, tilePos) < 3)
                    {
                        isValid = false;
                        break;
                    }
                }
                if(!isValid) continue;
                
                Vector3 objectPos = new(tilePos.x + 0.5f, tilePos.y + 0.5f, 0f);
                    
                //remove room index from array to prevent multiple benches in the same room
                roomIndices.RemoveAt(0);
                
                //add log location to list
                logPositions.Add(tilePos);

                //spawn bench
                GameObject bench = Instantiate(DocumentBenchPrefab, objectPos, Quaternion.identity);
                DialogueTrigger dt = bench.GetComponentInChildren<DialogueTrigger>();
                //dt.dialogue = dialogueManager.Dialogues[j];
                //print(dt.dialogue.name);
            }
        }

        private void DeleteDocuments()
        {
            //delete benches
            GameObject[] objectsToDelete = GameObject.FindGameObjectsWithTag("DocumentBench");

            foreach (GameObject obj in objectsToDelete)
            {
                Destroy(obj);
            }
        }
    }
}
