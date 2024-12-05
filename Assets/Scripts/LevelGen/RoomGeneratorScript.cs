using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace LevelGen
{
    public class RoomGeneratorScript : MonoBehaviour
    {
        /* -------- Settings --------*/

        [Header("Rooms")] [Tooltip("Amount of rooms to generate")] [SerializeField]
        private uint roomAmount = 10;

        [Tooltip("How random rooms shapes will be")] [SerializeField]
        private uint roomShapeRandomness = 0;

        [Tooltip("Space between rooms")] [SerializeField]
        private uint roomSpacing = 2;

        [Tooltip("Space between rooms")] [SerializeField]
        private uint roomSize = 80;

        [Header("Doors")] [Tooltip("Doors open when player is within this distance")] [SerializeField]
        private float doorOpenDistance = 1.5f;

        [Tooltip("Higher number = door opens faster")] [SerializeField]
        private float doorOpenSpeed = 3.0f;

        [Tooltip("Percent chance for extra doors to generate")] [SerializeField]
        private uint extraDoorChance = 0;

        [Tooltip("Distance between extra doors")] [SerializeField]
        private float extraDoorDistance = 7f;

        [Header("Actions")] [Tooltip("Generates a new map")] [SerializeField]
        private bool RegenerateMap = false;

        [Header("Enemies")] [Tooltip("Each arena room will contain between x and y amount of enemies")] [SerializeField]
        private Vector2Int enemyAmountRange = new(3, 5);

        public GameObject MeleeEnemyPrefab;

        /* -------- -------- --------*/

        private bool doPrintLogs = false;

        private LevelMap map;
        private TileManager tileManager;
        private TileRules tileRules;


        private Tilemap tilemap;

        //list of positions where new rooms can spawn
        private List<BorderNode> BorderNodes = new List<BorderNode>();

        //list of tiles adjacent to existing rooms
        private List<Vector2Int> roomAdjacentTiles = new List<Vector2Int>();

        private int roomsLeftToGenerate;

        private Vector2Int bottomLeft;
        private Vector2Int topRight;
        private int mapWidth, mapHeight;

        [HideInInspector] public GameObject[] doorOpeners;

        [HideInInspector] public static Dictionary<Vector2, Cell> cells = new Dictionary<Vector2, Cell>();


        public int FindRoom(Vector2Int v)
        {
            if (map == null) return -1;

            for (int i = 0; i < map.rooms.Count; i++)
            {
                if (!map.rooms[i].bounds.Contains(new(v.x, v.y, 0))) continue;
                if (map.rooms[i].shape.Contains(v)) return i;
            }

            return -1;
        }

        private void GenerateGrid()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector2Int worldPos = bottomLeft + gridPos;

                    if (cells.ContainsKey(worldPos))
                    {
                        cells[worldPos] = new Cell(worldPos,
                            map.GetTile(gridPos) == TileType.Floor && !TileRules.isDoor(map.GetTile(gridPos))
                            //tilemap.GetTile(new Vector3Int(x, y, 0)) == tileManager.tiles[0]
                        );
                        continue;
                    }

                    cells.Add(worldPos,
                        new Cell(worldPos,
                            map.GetTile(gridPos) == TileType.Floor && !TileRules.isDoor(map.GetTile(gridPos))
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

            foreach (Room room in map.rooms)
            {
                foreach (Vector2Int shape in room.shape)
                {
                    Gizmos.color = Color.red;

                    Gizmos.DrawCube((Vector2)shape + new Vector2(0.5f, 0.5f),
                        new Vector3(1, 1));
                }
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

            map = GetComponent<LevelMap>();
            map.Reset(Vector2Int.zero, 0, 0);
            RegenerateMap = true;
        }


        // Update is called once per frame
        void Update()
        {
            if (RegenerateMap)
            {
                //clear lists
                map.Clear();
                map.isGenerated = false;
                map.rooms.Clear();
                roomAdjacentTiles.Clear();
                BorderNodes.Clear();

                //delete enemies
                GameObject[] objectsToDelete = GameObject.FindGameObjectsWithTag("Enemy");

                foreach (GameObject obj in objectsToDelete)
                {
                    Destroy(obj);
                }

                //reset values
                roomsLeftToGenerate = (int)roomAmount;

                //generate first rectangle room shape
                Room room;
                GenerateRoomShape(new(0, 0), Vector2Int.up, roomSize, RoomType.Start, out room, false);
                RegenerateMap = false;
            }

            //generate room
            if (roomsLeftToGenerate > 0)
            {
                if (BorderNodes.Count > 0)
                {
                    //pick a random tile to generate new room from
                    int index = Random.Range(0, BorderNodes.Count - 1);
                    RoomType roomType = (Random.Range(0, 2) == 0 ? RoomType.Arena : RoomType.Hallway);
                    if (BorderNodes[index].roomType == RoomType.Hallway) roomType = RoomType.Arena;
                    //generate room
                    Room room;
                    if (GenerateRoomShape(BorderNodes[index].position, BorderNodes[index].direction, roomSize,
                            roomType,
                            out room))
                    {
                        //reduce counter if room was generated successfully
                        roomsLeftToGenerate--;
                    }
                }
                else if (doPrintLogs) print("No space left to generate rooms");
            }
            //when all rooms have been generated
            else if (BorderNodes.Count > 0)
            {
                tilemap.ClearAllTiles();
                FinalizeMap();
                map.isGenerated = true;
                roomAdjacentTiles.Clear();
                BorderNodes.Clear();
                GenerateGrid();

                //reset player position
                Vector2Int playerPositionTile = map.rooms[0].shape[Random.Range(0, map.rooms[0].shape.Count - 1)];
                Vector3 playerPosition = new Vector3(playerPositionTile.x + 0.5f, playerPositionTile.y + 0.5f, 0);
                GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("CanOpenDoors");
                foreach (GameObject obj in playerObjects)
                {
                    obj.transform.position = playerPosition;
                }

                if (doPrintLogs) print("Room generation finished");

                doorOpeners = GameObject.FindGameObjectsWithTag("CanOpenDoors");
            }

            //update doors
            foreach (Door door in map.doors)
            {
                bool doOpen = false;
                foreach (GameObject doorOpener in doorOpeners)
                {
                    Vector2Int openerPos = -bottomLeft + new Vector2Int(
                        Mathf.FloorToInt(doorOpener.transform.position.x),
                        Mathf.FloorToInt(doorOpener.transform.position.y));

                    if (Vector2Int.Distance(openerPos, door.position) < doorOpenDistance)
                    {
                        doOpen = true;
                        break;
                    }
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

                tilemap.SetTile(new Vector3Int(door.position.x + bottomLeft.x, door.position.y + bottomLeft.y, 0),
                    tileManager.tiles[tileId]);
            }
        }

        private void FinalizeMap()
        {
            //calculate map size
            bottomLeft = new(map.rooms[0].bounds.xMin, map.rooms[0].bounds.yMin);
            topRight = new(map.rooms[0].bounds.xMax, map.rooms[0].bounds.yMax);
            for (int i = 1; i < map.rooms.Count; i++)
            {
                bottomLeft.x = Mathf.Min(bottomLeft.x, map.rooms[i].bounds.xMin);
                bottomLeft.y = Mathf.Min(bottomLeft.y, map.rooms[i].bounds.yMin);
                topRight.x = Mathf.Max(topRight.x, map.rooms[i].bounds.xMax);
                topRight.y = Mathf.Max(topRight.y, map.rooms[i].bounds.yMax);
            }

            bottomLeft.x -= (int)roomSpacing;
            bottomLeft.y -= (int)roomSpacing;
            topRight.x += (int)roomSpacing;
            topRight.y += (int)roomSpacing;

            if (doPrintLogs) print($"bottomLeft: {bottomLeft}, topRight: {topRight}");
            mapWidth = topRight.x - bottomLeft.x;
            mapHeight = topRight.y - bottomLeft.y;
            //initiate map grid
            map.Reset(bottomLeft, mapWidth, mapHeight);

            //retrieve tiles from rooms
            for (int r = 0; r < map.rooms.Count; r++)
            {
                Room room = map.rooms[r];
                //place floor tiles
                foreach (Vector2Int tilePos in room.shape)
                {
                    map.SetTile(tilePos - bottomLeft, TileType.Floor);
                }

                //place wall tiles
                foreach (BorderNode node in room.border)
                {
                    if (node.distance >= 2f) continue;
                    if (map.GetTile(node.position - bottomLeft) != TileType.Empty) continue;
                    map.SetTile(node.position - bottomLeft, TileType.Wall);
                }

                //place doors
                foreach (BorderNode node in room.doors)
                {
                    TileType doorTileType;
                    if (node.direction.x > 0)
                    {
                        doorTileType = TileType.DoorLeft;
                    }
                    else if (node.direction.x < 0)
                    {
                        doorTileType = TileType.DoorRight;
                    }
                    else
                    {
                        doorTileType = TileType.DoorVertical;
                    }

                    map.doors.Add(new Door(node.position - bottomLeft, node.direction));

                    map.SetTile(node.position - bottomLeft, doorTileType);
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
                        foreach (Vector2Int direction in TileManager.directions)
                        {
                            TileType tile = map.GetTile(tilePos + direction);
                            if (tile == TileType.Wall) neighborCount++;
                            else if (tile == TileType.Floor || TileRules.isDoor(tile)) hasFloor = true;
                        }

                        if (neighborCount < 2 && hasFloor)
                        {
                            map.SetTile(tilePos, TileType.Floor);
                            doRemoveWalls = true;
                        }
                    }
                }
            }

            //add extra doors
            if (extraDoorChance > 0)
            {
                for (int x = 3; x < mapWidth - 3; x++)
                {
                    for (int y = 3; y < mapHeight - 3; y++)
                    {
                        //random chance to run
                        if (Random.Range(0, 100) > extraDoorChance) continue;
                        Vector2Int tilePos = new Vector2Int(x, y);
                        //check if tile is wall
                        if (map.GetTile(x, y) != TileType.Empty) continue;

                        //get distance to nearest door
                        float distanceToDoorVertical = Mathf.Infinity;
                        float distanceToDoorHorizontal = Mathf.Infinity;
                        foreach (Door door in map.doors)
                        {
                            float distance = Vector2.Distance(door.position, tilePos);
                            if (door.DoorDirection == DoorDirection.Left || door.DoorDirection == DoorDirection.Right)
                                distanceToDoorHorizontal = Mathf.Min(distance,
                                    distanceToDoorHorizontal);
                            else
                                distanceToDoorVertical = Mathf.Min(distance,
                                    distanceToDoorVertical);
                        }

                        //vertical
                        if (distanceToDoorVertical >= extraDoorDistance &&
                            map.GetTile(x, y + 1) == TileType.Wall &&
                            map.GetTile(x, y + 2) == TileType.Floor &&
                            map.GetTile(x, y - 1) == TileType.Wall &&
                            map.GetTile(x, y - 2) == TileType.Floor &&
                            map.GetTile(x + 1, y) != TileType.Floor &&
                            map.GetTile(x - 1, y) != TileType.Floor &&
                            map.GetTile(x + 2, y) != TileType.Floor &&
                            map.GetTile(x - 2, y) != TileType.Floor
                           )
                        {
                            map.doors.Add(new Door(tilePos + Vector2Int.up, Vector2Int.up));
                            map.doors.Add(new Door(tilePos + Vector2Int.down, Vector2Int.down));
                            map.SetTile(x, y, TileType.Floor);
                            map.SetTile(x, y + 1, TileType.DoorVertical);
                            map.SetTile(x, y - 1, TileType.DoorVertical);
                            map.SetTile(x + 1, y, TileType.Wall);
                            map.SetTile(x - 1, y, TileType.Wall);
                        }
                        //horizontal
                        else if (distanceToDoorHorizontal >= extraDoorDistance &&
                                 map.GetTile(x + 1, y) == TileType.Wall &&
                                 map.GetTile(x + 2, y) == TileType.Floor &&
                                 map.GetTile(x - 1, y) == TileType.Wall &&
                                 map.GetTile(x - 2, y) == TileType.Floor &&
                                 map.GetTile(x, y + 1) != TileType.Floor &&
                                 map.GetTile(x, y - 1) != TileType.Floor &&
                                 map.GetTile(x, y + 2) != TileType.Floor &&
                                 map.GetTile(x, y - 2) != TileType.Floor
                                )
                        {
                            map.doors.Add(new Door(tilePos + Vector2Int.left, Vector2Int.left));
                            map.doors.Add(new Door(tilePos + Vector2Int.right, Vector2Int.right));
                            map.SetTile(x - 1, y, TileType.DoorLeft);
                            map.SetTile(x + 1, y, TileType.DoorRight);
                            map.SetTile(x, y, TileType.Floor);
                            map.SetTile(x, y + 1, TileType.Wall);
                            map.SetTile(x, y - 1, TileType.Wall);
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

            //regenerate room shape lists
            for (int r = 0; r < map.rooms.Count; r++)
            {
                Room room = map.rooms[r];
                FixRoomShape(room);

                //place enemies
                if (room.type == RoomType.Arena)
                {
                    for (int i = 0; i < Random.Range(enemyAmountRange.x, enemyAmountRange.y); i++)
                    {
                        Vector2Int enemyPositionTile = room.shape[Random.Range(0, room.shape.Count - 1)];
                        Vector3 enemyPosition = new Vector3(enemyPositionTile.x + 0.5f, enemyPositionTile.y + 0.5f, 0);
                        GameObject go = Instantiate(MeleeEnemyPrefab, enemyPosition, Quaternion.identity);
                        Enemy e = go.GetComponent<Enemy>();
                        e.room = r;
                        EnemyGetCount.enemyCount++;
                    }
                }
            }

            EnemyGetCount.gameWin = true;
        }

        //regenerate room shape after finalized map
        void FixRoomShape(Room room)
        {
            Vector2Int startPos = room.shape[(int)roomSpacing];
            room.shape.Clear();
            room.border.Clear();

            List<Vector2Int> openSet = new List<Vector2Int>() { startPos };
            List<Vector2Int> closedSet = new List<Vector2Int>();

            for (int i = 0; i < 1000 && openSet.Count > 0; i++)
            {
                //close first node
                closedSet.Add(openSet[0]);
                openSet.RemoveAt(0);
                Vector2Int prevPos = closedSet[^1];

                foreach (Vector2Int direction in TileManager.directions)
                {
                    Vector2Int nextPos = prevPos + direction;
                    TileType nextTile = map.GetTile(nextPos - bottomLeft);
                    //if(TileRules.isDoor(nextTile)) closedSet.Add(nextPos);
                    bool valid = true;
                    foreach (Vector2Int node in openSet)
                    {
                        if (node == nextPos)
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (nextTile == TileType.Wall) room.border.Add(new BorderNode(nextPos, direction, 1f, room.type));
                    //else if (TileRules.isDoor(nextTile)) room.doors.Add(new Door(nextPos, direction));
                    if (nextTile != TileType.Floor) continue;

                    foreach (Vector2Int node in closedSet)
                    {
                        if (node == nextPos && !valid)
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid) openSet.Add(nextPos);
                }
            }

            room.shape = closedSet;
        }

        bool GenerateRoomShape(Vector2Int origin, Vector2Int roomDirection, uint area, RoomType roomType, out Room room,
            bool hasDoor = true)
        {
            //create room
            room = new Room();
            room.type = roomType;

            List<RoomGenTile> openSet = new List<RoomGenTile>() { };
            List<RoomGenTile> closedSet = new List<RoomGenTile>() { new(origin, 0) };
            room.shape.Add(closedSet[^1].position);


            if (hasDoor)
            {
                room.doors.Add(new(origin, roomDirection, 0f, roomType));
                for (int i = 1; i < roomSpacing; i++)
                {
                    closedSet.Add(new(closedSet[^1].position + roomDirection, 0));
                    room.shape.Add(closedSet[^1].position);
                }

                room.doors.Add(new(closedSet[^1].position, -roomDirection, 0f, roomType));
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
                foreach (RoomGenTile node in openSet) node.value = GetTileValue(node.position, closedSet, roomType);

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
                foreach (Vector2Int direction in TileManager.directions)
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
            room.GenerateBorder((int)roomSpacing);
            map.rooms.Add(room);
            foreach (BorderNode borderNode in room.border)
            {
                bool doPlaceNode = !(borderNode.direction == Vector2Int.zero);
                if (!roomAdjacentTiles.Contains(borderNode.position))
                {
                    roomAdjacentTiles.Add(borderNode.position);
                }
                else doPlaceNode = false;

                if (borderNode.distance > 1) doPlaceNode = false;
                for (int i = 0; i < BorderNodes.Count; i++)
                {
                    if (BorderNodes[i].position == borderNode.position)
                    {
                        BorderNodes.Remove(BorderNodes[i]);
                        doPlaceNode = false;
                        break;
                    }
                }

                if (doPlaceNode) BorderNodes.Add(borderNode);
            }

            return true;
        }

        //returns value of how good a new tile is
        float GetTileValue(Vector2Int position, List<RoomGenTile> closedSet, RoomType roomType)
        {
            float value = 0;
            //get neighbor count
            int neighborCount = 0;
            int cornerNeighborCount = 0;
            for (int d = 0; d < 8; d++)
            {
                Vector2Int direction = TileManager.directions8[d];
                for (int i = 0; i < closedSet.Count; i++)
                {
                    if (closedSet[i].position == position + direction)
                    {
                        if (d < 4) neighborCount++;
                        else cornerNeighborCount++;
                        break;
                    }
                }
            }

            int[] HallwayWeights = { 0, 2, 4, -5, -5 };

            switch (roomType)
            {
                case RoomType.Hallway:
                    value += HallwayWeights[neighborCount];
                    if (cornerNeighborCount > 1) value -= cornerNeighborCount * 0.8f;
                    value -= (Vector2Int.Distance(closedSet[0].position, position) * 0.05f);
                    break;
                default:
                    value += neighborCount;
                    break;
            }

            if (neighborCount == 2 && cornerNeighborCount == 3) value -= 5;

            //decrease value by distance
            //value -= Mathf.RoundToInt(position.magnitude * 0.5f);
            //value -= Mathf.FloorToInt(new Vector2(position.x * 0.2f, (float)position.y * 0.1f).magnitude);

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
            foreach (BorderNode node in room.border)
            {
                Vector3Int tilemapPosition = new(node.position.x, node.position.y, 0);

                if (node.distance < 2f && tilemap.GetTile(tilemapPosition) == null)
                {
                    tilemap.SetTile(tilemapPosition, tileManager.tiles[6]);
                }
            }

            foreach (BorderNode node in room.doors)
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
            public float value;

            public RoomGenTile(Vector2Int position, int value)
            {
                this.position = position;
                this.value = value;
            }
        }
    }

    public class BorderNode
    {
        public Vector2Int position;
        public Vector2Int direction;
        public float distance;
        public RoomType roomType;

        public BorderNode(Vector2Int position, Vector2Int direction, float distance, RoomType roomType)
        {
            this.position = position;
            this.direction = direction;
            this.distance = distance;
            this.roomType = roomType;
        }
    }
}