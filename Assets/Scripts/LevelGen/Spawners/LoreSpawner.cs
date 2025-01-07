using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelGen
{
    public class LoreSpawner : MonoBehaviour
    {
        public GameObject DocumentBenchPrefab;
        public DialogueManager dialogueManager;
        public Player player;

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
            for(int i = 0; i<  indicesEnumerable.Count(); i++) roomIndices.Add(indicesEnumerable[i]);

            for (int j = 0; j < dialogueManager.Dialogues.Length && roomIndices.Count > 0; j++)
            {
                //temporary index for index array
                int tempIndex = Random.Range(0, roomIndices.Count - 1);
                //guarantee first dialogue in first room
                if (j == 0) tempIndex = 0;
                    
                //room index
                int roomIndex = roomIndices[tempIndex];

                //select random tile in room
                int tileIndex = 0;
                
                //try to generate it next to a wall, but not a door
                bool valid = false;
                for (int k = 0; k < 100 && !valid; k++)
                {
                    valid = true;
                    tileIndex = Random.Range(0, levelMap.rooms[roomIndex].Floor.Count);
                    bool isAdjacentToWall = false;
                    for (int d = 0; d < TileManager.directions.Length && valid; d++)
                    {
                        Vector2Int newPosition = levelMap.rooms[roomIndex].Floor[tileIndex] + TileManager.directions[d];
                        TileType newTile = levelMap.GetTileWorldSpace(newPosition);
                        //check if next to a door
                        if (TileManager.IsDoor(newTile)) valid = false;
                        //check if next to wall
                        if(newTile == TileType.Wall) isAdjacentToWall = true;
                    }

                    if (!isAdjacentToWall) valid = false;
                    //print(k);
                }
                
                //calculate position
                Vector2Int tilePos = levelMap.rooms[roomIndex].Floor[tileIndex];
                Vector3 objectPos = new(tilePos.x + 0.5f, tilePos.y + 0.5f, 0f);
                    
                //retry if too close to player spawn
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
