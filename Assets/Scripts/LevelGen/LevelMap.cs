using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace LevelGen
{
    public delegate void LevelGenerated();
    public class LevelMap : MonoBehaviour
    {
        /* -------- Settings --------*/
        [Header("Doors")] [Tooltip("Doors open when player is within this distance")] [SerializeField]
        private float doorOpenDistance = 1.5f;

        [Tooltip("Higher number = door opens faster")] [SerializeField]
        private float doorOpenSpeed = 3.0f;
        
        [Header("Actions")] [Tooltip("Generates a new map")] [SerializeField]
        private bool GenerateMap = false;
        
        
        /* -------- Object references --------*/
        private Tilemap tilemap;
        private TileManager tileManager;
        private Player player;
        private RoomGeneratorScript roomGeneratorScript;

        
        /* -------- Variables --------*/
        private Vector2Int position;
        private int width, height;
        private static bool isLoaded = false;

        private List<List<TileType>> grid = new List<List<TileType>>();
        public List<Door> doors = new List<Door>();
        public List<Room> rooms = new List<Room>();

        private TileRules tileRules = new TileRules();

        public static Dictionary<Vector2, Cell> cells = new Dictionary<Vector2, Cell>();
        
        
        /* -------- Events --------*/
        public static event LevelGenerated OnLevelGenerated = delegate { };
        
        /* -------- Properties --------*/
        public static bool IsLoaded => isLoaded;
        public Vector2Int Position => position;
        public int Width => width;
        public int Height => height;

        
        /* -------- Start --------*/
        public void Start()
        {
            tilemap = GetComponent<Tilemap>();
            tileManager = GetComponent<TileManager>();
            tileManager.LoadTiles();
            roomGeneratorScript = GetComponent<RoomGeneratorScript>();
            GenerateMap = true;
        }

        /* -------- Update --------*/
        public void Update()
        {
            //regenerate map
            if (GenerateMap)
            {
                GenerateMap = false;
                Clear();
                isLoaded = roomGeneratorScript.GenerateMap(this);
                //map generated successfully
                if (isLoaded)
                {
                    print("Generated map successfully");
                    GenerateGrid();
                    OnLevelGenerated.Invoke();
                }
                //map failed to generate
                else
                {
                    print("Failed to generate map");
                }
            }
            
            //don't run until finished generating
            if (!isLoaded) return;
            
            //update doors
            foreach (Door door in doors)
            {
                bool doOpen = false;
                Vector2Int openerPos = -position + new Vector2Int(
                    Mathf.FloorToInt(player.transform.position.x),
                    Mathf.FloorToInt(player.transform.position.y));

                if (Vector2Int.Distance(openerPos, door.Position) < doorOpenDistance)
                {
                    doOpen = true;
                }

                door.Progress += (doOpen ? 1 : -1) * Time.deltaTime * doorOpenSpeed;

                int tileId = 30;
                switch (door.DoorDirection)
                {
                    case DoorDirection.Left:
                        tileId += 3;
                        break;
                    case DoorDirection.Right:
                        tileId += 6;
                        break;
                }

                switch (door.State)
                {
                    case DoorState.Opening:
                        tileId += 1;
                        break;
                    case DoorState.Open:
                        tileId += 2;
                        break;
                }

                tilemap.SetTile(new Vector3Int(door.Position.x + position.x, door.Position.y + position.y, 0),
                    tileManager.tiles[tileId]);
            }
        }

        /* -------- Functions --------*/
        private void GenerateGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector2Int worldPos = position + gridPos;

                    if (cells.ContainsKey(worldPos))
                    {
                        cells[worldPos] = new Cell(worldPos,
                            GetTile(gridPos) == TileType.Floor && !TileRules.IsDoor(GetTile(gridPos))
                            //tilemap.GetTile(new Vector3Int(x, y, 0)) == tileManager.tiles[0]
                        );
                        continue;
                    }

                    cells.Add(worldPos,
                        new Cell(worldPos,
                            GetTile(gridPos) == TileType.Floor && !TileRules.IsDoor(GetTile(gridPos))
                        ));
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Room room in rooms)
            {
                //floor
                foreach (Vector2Int shape in room.shape)
                {
                    Gizmos.color = Color.green;

                    Gizmos.DrawCube((Vector2)shape + new Vector2(0.5f, 0.5f),
                        new Vector3(1, 1));
                }

                foreach (BorderNode wall in room.border)
                {
                    Gizmos.color = Color.red;

                    Gizmos.DrawCube((Vector2)wall.position + new Vector2(0.5f, 0.5f),
                        new Vector3(1, 1));
                }

                foreach (Door door in room.doors)
                {
                    Gizmos.color = Color.blue;

                    Gizmos.DrawCube((Vector2)door.Position + new Vector2(0.5f, 0.5f),
                        new Vector3(1, 1));
                }
            }
        }
        public int FindRoom(Vector2Int v)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (!rooms[i].bounds.Contains(new(v.x, v.y, 0))) continue;
                if (rooms[i].shape.Contains(v)) return i;
                foreach (Door door in rooms[i].doors)
                {
                    if (v == door.Position) return i;
                }
            }

            return -1;
        }
        public void Reset(Vector2Int position, int width, int height)
        {
            this.position = position;
            this.width = width;
            this.height = height;
            grid = new List<List<TileType>>();
            for (int i = 0; i < width; i++)
            {
                grid.Add(Enumerable.Repeat(TileType.Empty, height).ToList());
            }
        }

        public void Clear()
        {
            grid.Clear();
            doors.Clear();
            rooms.Clear();
            tilemap.ClearAllTiles();
        }

        /* -------- Get tile --------*/
        public TileType GetTile(Vector2Int position)
        {
            //out of bounds
            if (position.x < 0 ||
                position.y < 0 ||
                position.x >= width ||
                position.y >= height)
            {
                return TileType.Empty;
            }
            //return tile
            else return grid[position.x][position.y];
        }

        public TileType GetTile(int x, int y)
        {
            return GetTile(new Vector2Int(x, y));
        }

        /* -------- Set tile --------*/
        public bool SetTile(Vector2Int position, TileType tile)
        {
            //out of bounds
            if (position.x < 0 ||
                position.y < 0 ||
                position.x >= width ||
                position.y >= height)
            {
                return false;
            }

            //tile is the same
            if (grid[position.x][position.y] == tile) return false;
            //set tile
            grid[position.x][position.y] = tile;
            return true;
        }

        public bool SetTile(int x, int y, TileType tile)
        {
            return SetTile(new Vector2Int(x, y), tile);
        }


        /* -------- Apply to tilemap --------*/
        public void ApplyToTilemap()
        {
            //place tiles on tilemap
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int tileId = 12;
                    foreach (TileRules.TileRule rule in tileRules.rules)
                    {
                        if (rule.CheckRule(this, x, y))
                        {
                            tileId = rule.TileId;
                            break;
                        }
                    }

                    tilemap.SetTile(new Vector3Int(x + position.x, y + position.y, 0), tileManager.tiles[tileId]);
                }
            }
        }
    }
}