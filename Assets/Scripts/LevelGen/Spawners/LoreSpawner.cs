using System.Collections.Generic;
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
            LevelMap.OnLevelLoaded += SpawnDocuments;
            LevelMap.OnLevelUnloaded += DeleteDocuments;
        }

        private void SpawnDocuments(LevelMap levelMap)
        {
            if (dialogueManager == null) return;
            //list of rooms
            List<int> roomIndices = new List<int>();
            for(int i = 0; i < levelMap.rooms.Count; i++) roomIndices.Add(i);

            for (int j = 0; j < dialogueManager.Dialogues.Length && j < levelMap.rooms.Count; j++)
            {
                //temporary index for index array
                int tempIndex = Random.Range(0, roomIndices.Count);
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
