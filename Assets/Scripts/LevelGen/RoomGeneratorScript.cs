using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace LevelGen
{
    public class LevelMap
    {
        private List<List<TileType>> grid = new List<List<TileType>>();
        private Vector2Int position;
        private int width, height;

        public Vector2Int Position => position;
        public int Width => width;
        public int Height => height;

        public LevelMap(Vector2Int position, int width, int height)
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
        }

        public TileType GetTile(Vector2Int position)
        {
            //out of bounds
            /*
            if (position.x < this.position.x || 
                position.y < this.position.y || 
                position.x >= this.position.x + width ||
                position.y >= this.position.y + height)
            {
                return TileType.Empty;
            }
            */
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
        public TileType GetTile(int x, int y) {return GetTile(new Vector2Int(x, y));}

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
        public bool SetTile(int x, int y, TileType tile) { return SetTile(new Vector2Int(x, y), tile); }
    }

    public class RoomGeneratorScript : MonoBehaviour
    {
        /* -------- Settings --------*/

        public int roomAmount = 10;
        public float roomGenDelay = 0.0f;
        public int roomShapeRandomness = 0;
        public int roomSpacing = 2;
        public int roomSize = 80;
        public bool doPrintLogs = false;

        /* -------- -------- --------*/

        private LevelMap map;
        private TileManager tileManager;
        private TileRules tileRules;


        private static Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        private Tilemap tilemap;
        private List<Room> rooms = new List<Room>();

        //list of positions where new rooms can spawn
        private List<NewRoomNode> newRoomNodes = new List<NewRoomNode>();

        //list of tiles adjacent to existing rooms
        private List<Vector2Int> roomAdjacentTiles = new List<Vector2Int>();

        public static Dictionary<Vector2, Cell> cells = new Dictionary<Vector2, Cell>();

        private int roomsLeftToGenerate;
        private float roomTimer;

        private Vector2Int bottomLeft;
        private Vector2Int topRight;
        private int mapWidth, mapHeight;
        
        public bool RegenerateMap = false;


        private void GenerateGrid()
        {
            /*
            Vector3Int halfSize = tilemap.size / 2;
            for (int i = -tilemap.size.x; i < tilemap.size.x; i++)
            {
                for (int j = -tilemap.size.y; j < tilemap.size.y; j++)
                {
                    if (tilemap.GetTile(new Vector3Int(i, j, 0)) == null) continue;

                    if (cells.ContainsKey(new Vector2(i, j)))
                    {
                        cells[new Vector2(i, j)] = new Cell(new Vector2(i, j),
                            tilemap.GetTile(new Vector3Int(i, j, 0)) == tileManager.tiles[0]);
                        continue;
                    }

                    cells.Add(new Vector2(i, j),
                        new Cell(new Vector2(i, j),
                            tilemap.GetTile(new Vector3Int(i, j, 0)) == tileManager.tiles[0]));
                }
            }
            */
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector2Int worldPos = bottomLeft + gridPos;
                    
                    if(cells.ContainsKey(worldPos))
                    {
                        cells[worldPos] = new Cell(worldPos,
                            map.GetTile(gridPos) == TileType.Floor || map.GetTile(gridPos) == TileType.Door
                            //tilemap.GetTile(new Vector3Int(x, y, 0)) == tileManager.tiles[0]
                            );
                        continue;
                    }

                    cells.Add(worldPos,
                        new Cell(worldPos,
                            map.GetTile(gridPos) == TileType.Floor || map.GetTile(gridPos) == TileType.Door
                        ));
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (KeyValuePair<Vector2, Cell> pair in cells)
            {
                Gizmos.color = pair.Value.walkable ? Color.white : Color.green;

                Gizmos.DrawCube(pair.Key + (Vector2)transform.position + new Vector2(0.5f, 0.5f), new Vector3(1, 1));
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            //find tilemap component
            tilemap = GetComponent<Tilemap>();
            tileManager = GetComponent<TileManager>();
            tileManager.LoadTiles();
            tileRules = new();

            map = new LevelMap(Vector2Int.zero, 0, 0);
            RegenerateMap = true;
        }


        // Update is called once per frame
        void Update()
        {
            if (RegenerateMap)
            {
                //clear lists
                map.Clear();
                rooms.Clear();
                roomAdjacentTiles.Clear();
                newRoomNodes.Clear();
                
                //reset values
                roomsLeftToGenerate = roomAmount;
                roomTimer = roomGenDelay;
                
                //generate first rectangle room shape
                Room room;
                GenerateRoomShape(new(0, 0), Vector2Int.up, roomSize, out room, false);
                RegenerateMap = false;
            }
            
            roomTimer -= Time.deltaTime;
            if (roomTimer > 0) return;
            else roomTimer += roomGenDelay;

            //generate room
            if (roomsLeftToGenerate > 0)
            {
                if (newRoomNodes.Count > 0)
                {
                    //pick a random tile to generate new room from
                    int index = Random.Range(0, newRoomNodes.Count - 1);
                    //generate room
                    Room room;
                    if (GenerateRoomShape(newRoomNodes[index].position, newRoomNodes[index].direction, roomSize, out room))
                    {
                        //reduce counter if room was generated successfully
                        roomsLeftToGenerate--;
                    }
                }
                else if (doPrintLogs) print("No space left to generate rooms");
            }
            //when all rooms have been generated
            else if (newRoomNodes.Count > 0)
            {
                tilemap.ClearAllTiles();
                FinalizeMap();
                roomAdjacentTiles.Clear();
                newRoomNodes.Clear();
                GenerateGrid();
                if (doPrintLogs) print("Room generation finished");
            }
        }

        private void FinalizeMap()
        {
            //calculate map size
            bottomLeft = new(rooms[0].bounds.xMin, rooms[0].bounds.yMin);
            topRight = new(rooms[0].bounds.xMax, rooms[0].bounds.yMax);
            for (int i = 1; i < rooms.Count; i++)
            {
                bottomLeft.x = Mathf.Min(bottomLeft.x, rooms[i].bounds.xMin);
                bottomLeft.y = Mathf.Min(bottomLeft.y, rooms[i].bounds.yMin);
                topRight.x = Mathf.Max(topRight.x, rooms[i].bounds.xMax);
                topRight.y = Mathf.Max(topRight.y, rooms[i].bounds.yMax);
            }

            bottomLeft.x -= roomSpacing;
            bottomLeft.y -= roomSpacing;
            topRight.x += roomSpacing;
            topRight.y += roomSpacing;

            if (doPrintLogs) print($"bottomLeft: {bottomLeft}, topRight: {topRight}");
            mapWidth = topRight.x - bottomLeft.x;
            mapHeight = topRight.y - bottomLeft.y;
            //initiate map grid
            map = new LevelMap(bottomLeft, mapWidth, mapHeight);

            //retrieve tiles from rooms
            foreach (Room room in rooms)
            {
                //place floor tiles
                foreach (Vector2Int tilePos in room.shape)
                {
                    map.SetTile(tilePos - bottomLeft, TileType.Floor);
                }
                
                //place wall tiles
                foreach (NewRoomNode node in room.border)
                {
                    if (node.distance >= 2f) continue;
                    if (map.GetTile(node.position - bottomLeft) != TileType.Empty) continue;
                    map.SetTile(node.position - bottomLeft, TileType.Wall);
                }

                //place doors
                foreach (NewRoomNode node in room.doors)
                {
                    map.SetTile(node.position - bottomLeft, TileType.Door);
                }
            }
            
            //remove disconnected walls
            bool doRemoveWalls = true;
            for (int a = 0; a < 10 && doRemoveWalls; a++)
            {
                doRemoveWalls = false;
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        Vector2Int tilePos = new Vector2Int(x, y);
                        if (map.GetTile(tilePos) != TileType.Wall) continue;
                        int neighborCount = 0;
                        bool hasFloor = false;
                        foreach (Vector2Int direction in directions)
                        {
                            TileType tile = map.GetTile(tilePos + direction);
                            if (tile == TileType.Wall) neighborCount++;
                            else if (tile == TileType.Floor || tile == TileType.Door) hasFloor = true;
                        }

                        if (neighborCount < 2 && hasFloor)
                        {
                            map.SetTile(tilePos, TileType.Floor);
                            doRemoveWalls = true;
                        }
                    }
                }
            }

            //place tiles on tilemap
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int tileId = 12;
                    /*
                    switch (map.GetTile(x, y))
                    {
                        case TileType.Floor:
                            tileId = 0;
                            break;
                        case TileType.Wall:
                            tileId = 22;
                            break;
                        case TileType.Door:
                            tileId = 32;
                            break;
                        //empty
                        default:
                            tileId = 12;
                            break;
                    }
                    */
                    foreach (TileRules.TileRule rule in tileRules.rules)
                    {
                        if (rule.CheckRule(map, x, y))
                        {
                            tileId = rule.TileId;
                            break;
                        }
                    }
                    tilemap.SetTile(new Vector3Int(x + bottomLeft.x, y + bottomLeft.y, 0), tileManager.tiles[tileId]);
                }
            }
        }

        bool GenerateRoomShape(Vector2Int origin, Vector2Int roomDirection, int area, out Room room,
            bool hasDoor = true)
        {
            room = new Room();
            List<RoomGenTile> openSet = new List<RoomGenTile>() { };
            List<RoomGenTile> closedSet = new List<RoomGenTile>() { new(origin, 0) };
            room.shape.Add(closedSet[^1].position);

            if (hasDoor)
            {
                room.doors.Add(new(origin, roomDirection, 0f));
                for (int i = 1; i < roomSpacing; i++)
                {
                    closedSet.Add(new(closedSet[^1].position + roomDirection, 0));
                    room.shape.Add(closedSet[^1].position);
                }

                room.doors.Add(new(closedSet[^1].position, -roomDirection, 0f));
                openSet.Add(new(closedSet[^1].position + roomDirection, 0));

                //check validity of room starting point
                foreach (Vector2Int node in roomAdjacentTiles)
                {
                    if (node == openSet[0].position) return false;
                }
            }
            else
            {
                openSet.Add(new(origin + Vector2Int.up, 0));
                openSet.Add(new(origin + Vector2Int.down, 0));
                openSet.Add(new(origin + Vector2Int.left, 0));
                openSet.Add(new(origin + Vector2Int.right, 0));
            }

            for (int i = 0; i < area; i++)
            {
                //if openSet is empty, room is invalid
                //abort room generation and return false
                if (openSet.Count == 0) return false;

                //update neighbor count
                foreach (RoomGenTile node in openSet) node.value = GetTileValue(node.position, closedSet);

                //find tile with most neighbors
                List<int> indices = new List<int>() { 0 };
                for (int j = 1; j < openSet.Count; j++)
                {
                    if (openSet[j].value > openSet[indices[0]].value)
                    {
                        indices = new List<int>() { j };
                    }
                    else if (openSet[j].value == openSet[indices[0]].value)
                    {
                        indices.Add(j);
                    }
                }

                //if multiple have equal value, pick a random one
                int index = indices[Random.Range(0, indices.Count - 1)];
                //close node
                closedSet.Add(openSet[index]);
                openSet.Remove(openSet[index]);
                RoomGenTile prevTile = closedSet[closedSet.Count - 1];
                room.shape.Add(prevTile.position);
                //Debug.Log($"Placed tile on {prevTile.position}");

                //add new positions to open set
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int newPos = prevTile.position + direction;

                    bool isValid = true;
                    if (Vector2Int.Distance(origin, newPos) <= 1) isValid = false;
                    foreach (RoomGenTile node in openSet)
                    {
                        if (node.position == newPos) isValid = false;
                    }

                    foreach (RoomGenTile node in closedSet)
                    {
                        if (node.position == newPos) isValid = false;
                    }

                    foreach (Vector2Int node in roomAdjacentTiles)
                    {
                        if (node == newPos) isValid = false;
                    }

                    //if position is not valid, do not add to openSet
                    if (!isValid) continue;

                    else openSet.Add(new(newPos, 0));
                }
            }

            // Debug.Log($"Room shape generated with {room.shape.Count} tiles.");

            //add positions to roomAdjacentTiles
            room.GenerateBounds();
            room.GenerateBorder(roomSpacing);
            rooms.Add(room);
            foreach (NewRoomNode borderNode in room.border)
            {
                bool doPlaceNode = !(borderNode.direction == Vector2Int.zero);
                if (!roomAdjacentTiles.Contains(borderNode.position))
                {
                    roomAdjacentTiles.Add(borderNode.position);
                }
                else doPlaceNode = false;

                if (borderNode.distance > 1) doPlaceNode = false;
                for (int i = 0; i < newRoomNodes.Count; i++)
                {
                    if (newRoomNodes[i].position == borderNode.position)
                    {
                        newRoomNodes.Remove(newRoomNodes[i]);
                        doPlaceNode = false;
                        break;
                    }
                }

                if (doPlaceNode) newRoomNodes.Add(borderNode);
            }

            return true;
        }

        //returns value of how good a new tile is
        int GetTileValue(Vector2Int position, List<RoomGenTile> closedSet)
        {
            int value = 0;
            //get neighbor count
            for (int i = 0; i < closedSet.Count; i++)
            {
                if (closedSet[i].position == position + Vector2Int.up ||
                    closedSet[i].position == position + Vector2Int.down ||
                    closedSet[i].position == position + Vector2Int.left ||
                    closedSet[i].position == position + Vector2Int.right)
                {
                    value++;
                }
            }

            //decrease value by distance
            //value -= Mathf.RoundToInt(position.magnitude * 0.5f);
            //value += Math.Abs(position.x);

            //add randomness
            if (roomShapeRandomness > 0) value += Random.Range(0, roomShapeRandomness);

            return value;
        }

        void PlaceFloorTiles(Room room)
        {
            foreach (Vector2Int shapePoint in room.shape)
            {
                tilemap.SetTile(new Vector3Int(shapePoint.x, shapePoint.y, 0), tileManager.tiles[0]);
            }
        }

        void PlaceWallTiles(Room room)
        {
            foreach (NewRoomNode node in room.border)
            {
                Vector3Int tilemapPosition = new(node.position.x, node.position.y, 0);

                if (node.distance < 2f && tilemap.GetTile(tilemapPosition) == null)
                {
                    tilemap.SetTile(tilemapPosition, tileManager.tiles[6]);
                }
            }

            foreach (NewRoomNode node in room.doors)
            {
                Vector3Int tilemapPosition = new(node.position.x, node.position.y, 0);
                if (Math.Abs(node.direction.x) > Math.Abs(node.direction.y))
                {
                    if (node.direction.x > 0) tilemap.SetTile(tilemapPosition, tileManager.tiles[36]);
                    else tilemap.SetTile(tilemapPosition, tileManager.tiles[35]);
                }
                else
                {
                    tilemap.SetTile(tilemapPosition, tileManager.tiles[32]);
                }
            }
        }

        private class RoomGenTile
        {
            public Vector2Int position;
            public int value;

            public RoomGenTile(Vector2Int position, int value)
            {
                this.position = position;
                this.value = value;
            }
        }

        public class NewRoomNode
        {
            public Vector2Int position;
            public Vector2Int direction;
            public float distance;

            public NewRoomNode(Vector2Int position, Vector2Int direction, float distance)
            {
                this.position = position;
                this.direction = direction;
                this.distance = distance;
            }
        }

        public class Room
        {
            public BoundsInt bounds;

            //list of tiles
            public List<Vector2Int> shape = new List<Vector2Int>();

            //list of positions making the border
            public List<NewRoomNode> border = new List<NewRoomNode>();

            //positions of doors
            public List<NewRoomNode> doors = new List<NewRoomNode>();

            public void GenerateBounds()
            {
                if (shape.Count == 0)
                {
                    //Debug.Log("Cannot generate bounds for empty room");
                    return;
                }

                bounds.xMin = bounds.xMax = shape[0].x;
                bounds.yMin = bounds.yMax = shape[0].y;
                foreach (Vector2Int shapePoint in shape)
                {
                    bounds.xMin = Mathf.Min(bounds.xMin, shapePoint.x);
                    bounds.yMin = Mathf.Min(bounds.yMin, shapePoint.y);
                    bounds.xMax = Mathf.Max(bounds.xMax, shapePoint.x);
                    bounds.yMax = Mathf.Max(bounds.yMax, shapePoint.y);
                }
            }

            public void GenerateBorder(int spacing)
            {
                border.Clear();
                for (int y = bounds.yMin - spacing; y <= bounds.yMax + spacing; y++)
                {
                    for (int x = bounds.xMin - spacing; x <= bounds.xMax + spacing; x++)
                    {
                        Vector2Int position = new Vector2Int(x, y);
                        bool isTile = false;
                        int neighborCount = 0;
                        Vector2Int direction = Vector2Int.zero;
                        float distance = 999f;
                        //go through list of tiles
                        foreach (Vector2Int tile in shape)
                        {
                            //check if current position has tile
                            if (position == tile)
                            {
                                isTile = true;
                                break;
                            }

                            //check surrounding area for tiles
                            for (int y2 = -spacing; y2 <= spacing; y2++)
                            {
                                for (int x2 = -spacing; x2 <= spacing; x2++)
                                {
                                    if (x2 == 0 && y2 == 0) continue;
                                    Vector2Int relativePosition = new Vector2Int(x2, y2);

                                    //if a tile is found withing search area
                                    if (position == tile + relativePosition)
                                    {
                                        neighborCount++;
                                        distance = MathF.Min(distance, relativePosition.magnitude);
                                        //set direction of border
                                        if (x2 == 0 || y2 == 0)
                                        {
                                            if (x2 == 0) direction = new(0, y2 > 0 ? 1 : -1);
                                            else if (y2 == 0) direction = new(x2 > 0 ? 1 : -1, 0);
                                        }
                                    }
                                }
                            }
                        }

                        if (!isTile && neighborCount > 0)
                        {
                            border.Add(new NewRoomNode(position, direction, distance));
                        }
                    }
                }
            }
        }
    }
}