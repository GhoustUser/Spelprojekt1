using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelGen
{
    public class FogOfWar : MonoBehaviour
    {
        public Tile tileUnexplored;
        public Tile tileExplored;

        public Player player;
    
        [HideInInspector] public LevelMap levelMap;
        private bool hasInitialized = false;
        private Tilemap tilemap;
        private int prevPlayerRoomId = -1;
    
        // Start is called before the first frame update
        void Start()
        {
            tilemap = GetComponent<Tilemap>();
        }

        // Update is called once per frame
        void Update()
        {
            //if map is not generated
            if (!levelMap.isGenerated)
            {
                //if tilemap is not generated but fog is, clear fog
                if (hasInitialized)
                {
                    hasInitialized = false;
                    prevPlayerRoomId = -1;
                    tilemap.ClearAllTiles();
                }
                //wait until map has been generated
                return;
            }
            //if map is generated but fog is not initialized
            else if (!hasInitialized)
            {
                hasInitialized = true;
                for (int x = 0; x < levelMap.Width; x++)
                {
                    for (int y = 0; y < levelMap.Height; y++)
                    {
                        Vector3Int tilePos = new(x + levelMap.Position.x, y + levelMap.Position.y, 0);
                        tilemap.SetTile(tilePos, tileUnexplored);
                    }
                }
            }
            
            //if player has moved to a new room
            if (player.room != prevPlayerRoomId)
            {
                SetFogInRoom(player.room, null);
                if(prevPlayerRoomId != -1) SetFogInRoom(prevPlayerRoomId, tileExplored);
                prevPlayerRoomId = player.room;
            }
        }
        
        //set fog tiles in room
        public void SetFogInRoom(int roomId, Tile tile)
        {
            foreach (Vector2Int pos in levelMap.rooms[roomId].shape)
            {
                Vector3Int tilePos = new(pos.x, pos.y, 0);
                tilemap.SetTile(tilePos, tile);
            }

            foreach (BorderNode door in levelMap.rooms[roomId].doors)
            {
                Vector3Int tilePos = new(door.position.x, door.position.y, 0);
                tilemap.SetTile(tilePos, tile);
            }
        }
    }
}
