using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelGen
{
    public class FogOfWar : MonoBehaviour
    {
        public Tile tileUnexplored;
        public Tile tileExplored;

        private Player player;
        private LevelMap levelMap;

        private Tilemap tilemap;
        private int prevPlayerRoomId = -1;

        // Start is called before the first frame update
        void Start()
        {
            tilemap = GetComponent<Tilemap>();
            levelMap = FindObjectOfType<LevelMap>();
            player = FindObjectOfType<Player>();
            LevelMap.OnLevelGenerated += Initialize;
        }

        private void Initialize(LevelMap lm)
        {
            prevPlayerRoomId = -1;
            tilemap.ClearAllTiles();
            for (int x = 0; x < levelMap.Width; x++)
            {
                for (int y = 0; y < levelMap.Height; y++)
                {
                    Vector3Int tilePos = new(x + levelMap.Position.x, y + levelMap.Position.y, 0);
                    tilemap.SetTile(tilePos, tileUnexplored);
                }
            }
            //print("fog of war intialized");
        }

        // Update is called once per frame
        void Update()
        {
            if (!LevelMap.IsLoaded) return;

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
            foreach (Vector2Int pos in levelMap.rooms[roomId].Floor)
            {
                Vector3Int tilePos = new(pos.x, pos.y, 0);
                tilemap.SetTile(tilePos, tile);
            }

            foreach (Door door in levelMap.rooms[roomId].Doors)
            {
                Vector3Int tilePos = new(door.Position.x + door.direction.x, door.Position.y + door.direction.y, 0);
                tilemap.SetTile(tilePos, tile);
                foreach (Vector2Int direction in TileManager.directions8)
                {
                    tilePos = new(
                        door.Position.x + door.direction.x + direction.x,
                        door.Position.y + door.direction.y + direction.y, 0);
                    tilemap.SetTile(tilePos, tile);
                }
            }

            foreach (BorderNode node in levelMap.rooms[roomId].border)
            {
                Vector3Int tilePos = new(node.position.x, node.position.y, 0);
                tilemap.SetTile(tilePos, tile);
            }
        }
    }
}