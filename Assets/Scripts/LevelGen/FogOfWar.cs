using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelGen
{
    public class FogOfWar : MonoBehaviour
    {
        public Tile tileUnexplored;
        public Tile tileExplored;

        public Player player;

        public LevelMap levelMap;
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
                if (prevPlayerRoomId != -1) SetFogInRoom(prevPlayerRoomId, tileExplored);
                SetFogInRoom(player.room, null);
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

            foreach (Door door in levelMap.rooms[roomId].doors)
            {
                Vector3Int tilePos = new(door.position.x + door.direction.x, door.position.y + door.direction.y, 0);
                tilemap.SetTile(tilePos, tile);
                foreach (Vector2Int direction in TileManager.directions8)
                {
                    tilePos = new(
                        door.position.x + door.direction.x + direction.x,
                        door.position.y + door.direction.y + direction.y, 0);
                    tilemap.SetTile(tilePos, tile);
                }
                /*
                tilemap.SetTile(tilePos, tile);
                tilePos.x += door.direction.x;
                tilePos.y += door.direction.y;
                tilemap.SetTile(tilePos, tile);
                tilePos.x += door.direction.x;
                tilePos.y += door.direction.y;
                tilemap.SetTile(tilePos, tile);
                */
            }

            foreach (BorderNode node in levelMap.rooms[roomId].border)
            {
                Vector3Int tilePos = new(node.position.x, node.position.y, 0);
                tilemap.SetTile(tilePos, tile);
            }
        }
    }
}