using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Misc;

namespace LevelGen
{
    public class RoomGeneratorScript : MonoBehaviour
    {
        /* -------- Settings --------*/

        [Header("Rooms")] [Tooltip("Amount of rooms to generate")] [SerializeField] [Range(1, 100)]
        private uint roomAmount = 10;

        [Tooltip("How random rooms shapes will be")] [SerializeField] [Range(0, 100)]
        private uint roomShapeRandomness = 0;

        [Tooltip("Space between rooms")] [SerializeField]
        private uint roomSpacing = 2;

        [Tooltip("Space between rooms")] [SerializeField]
        private uint roomSize = 80;

        [Header("Doors")] [Tooltip("Percent chance for extra doors to generate")] [SerializeField] [Range(0, 100)]
        private uint extraDoorChance = 0;

        [Header("Room Types")] [SerializeField]
        private uint arena1Size = 60;

        [SerializeField] private uint arena2Size = 100;
        [SerializeField] private uint arena3Size = 140;
        [SerializeField] private uint hallwaySize = 50;

        [SerializeField] private GameObject ElevatorObject;
        [SerializeField] private GameObject generatorObject;

        /* -------- -------- --------*/

        //list of positions where new rooms can spawn
        private List<BorderNode> borderNodes = new List<BorderNode>();

        //list of tiles adjacent to existing rooms
        private List<Vector2Int> roomAdjacentTiles = new List<Vector2Int>();

        private int roomsLeftToGenerate;

        private Vector2Int bottomLeft;
        private Vector2Int topRight;
        private int mapWidth, mapHeight;

        [HideInInspector] public GameObject[] doorOpeners;

        private void Reset()
        {
            roomAdjacentTiles.Clear();
            borderNodes.Clear();
            EnemyGetCount.enemyCount = 0;
            EnemyGetCount.gameWin = false;
            roomsLeftToGenerate = (int)roomAmount;
        }

        public bool GenerateMap(LevelMap map)
        {
            //reset values
            Reset();
            Room room = new Room();

            //generate first room
            BorderNode originNode = new(new(0, 0), Vector2Int.up, false, -1);
            GenerateRoomShape(map, originNode, (int)roomSize, RoomType.Start, out room, false);

            //generate rest of rooms
            for (int r = 0; r < 1000 && roomsLeftToGenerate > 0; r++)
            {
                if (borderNodes.Count > 0)
                {
                    //pick a random tile to generate new room from
                    int nodeIndex = Random.Range(0, borderNodes.Count - 1);
                    //decide what room type to generate next
                    List<RoomType> roomTypes = RoomRules.ChooseRoomType(map, borderNodes[nodeIndex].roomId);

                    //invalid room
                    if (roomTypes.Count == 0)
                    {
                        borderNodes.RemoveAt(nodeIndex);
                        continue;
                    }

                    RoomType nextRoomType = roomTypes[Random.Range(0, roomTypes.Count)];


                    /*
                    if (nextRoomType == RoomType.RewardRoom)
                    {
                        room = new Room();
                        room.type = RoomType.RewardRoom;
                        //room.walls = PremadeRoomHandler.customRooms[RoomType.RewardRoom].walls;
                        room.Floor = PremadeRoomHandler.customRooms[RoomType.RewardRoom].floor;
                        room.doors = PremadeRoomHandler.customRooms[RoomType.RewardRoom].doors;
                        room.GenerateBounds();
                        map.rooms.Add(room);
                        foreach (Wall wall in room.walls)
                        {
                            roomAdjacentTiles.Add(wall.Position);
                        }
                        print("reward room");
                    }
                    */
                    //room size
                    int nextRoomSize;
                    switch (nextRoomType)
                    {
                        case RoomType.Hallway:
                            nextRoomSize = (int)hallwaySize;
                            break;
                        case RoomType.Arena1:
                            nextRoomSize = (int)arena1Size;
                            break;
                        case RoomType.Arena2:
                            nextRoomSize = (int)arena2Size;
                            break;
                        case RoomType.Arena3:
                            nextRoomSize = (int)arena3Size;
                            break;

                        default:
                            nextRoomSize = (int)roomSize;
                            break;
                    }


                    //generate room
                    if (GenerateRoomShape(map, borderNodes[nodeIndex], nextRoomSize, nextRoomType, out room))
                    {
                        //reduce counter if room was generated successfully
                        //don't count hallways for total room count
                        if (nextRoomType != RoomType.Hallway) roomsLeftToGenerate--;
                        //add neighbor id's
                        map.rooms[borderNodes[nodeIndex].roomId].neighborIds.Add(map.rooms.Count - 1);
                        map.rooms[^1].neighborIds.Add(borderNodes[nodeIndex].roomId);
                    }

                    //remove nearby borderNodes
                    Vector2Int nodePosition = borderNodes[nodeIndex].position;
                    for (int i = borderNodes.Count - 1; i >= 0; i--)
                    {
                        if (Vector2Int.Distance(nodePosition, borderNodes[i].position) <= roomSpacing)
                        {
                            borderNodes.RemoveAt(i);
                        }
                    }
                }
                //unable to generate more rooms
                else
                {
                    print("No space left to generate rooms");
                    break;
                }
            }

            //failed to generate rooms
            if (roomsLeftToGenerate > 0)
            {
                Debug.LogError("Failed to generate all rooms");
                return false;
            }
            //successfully generated rooms
            else
            {
                //generate end room, limit attempts
                bool hasGeneratedEndRoom = false;
                for (int attempts = 0; attempts < 50; attempts++)
                {
                    //find nodes furthest away
                    List<int> nodeIndices = ListUtils.GetHighestValuesIndices(borderNodes,
                        (a, b) => map.rooms[a.roomId].distanceFromStart - map.rooms[b.roomId].distanceFromStart);

                    //choose one random valid node
                    int nodeIndex = nodeIndices[Random.Range(0, nodeIndices.Count)];

                    //successfully generated end room
                    int roomId = borderNodes[nodeIndex].roomId;
                    if (GenerateRoomShape(map, borderNodes[nodeIndex], (int)roomSize, RoomType.End, out room))
                    {
                        map.rooms[roomId].neighborIds.Add(map.rooms.Count - 1);
                        map.rooms[^1].neighborIds.Add(roomId);
                        hasGeneratedEndRoom = true;
                        
                        //place generator
                        Vector3 GeneratorPos = map.rooms[Random.Range(1, map.rooms.Count - 2)].bounds.center;
                        GameObject generator = Instantiate(generatorObject,
                            GeneratorPos, Quaternion.identity);
                        
                        //place elevator
                        Vector3 ElevatorPos = room.bounds.center;
                        GameObject go = Instantiate(ElevatorObject,
                            ElevatorPos, Quaternion.identity);
                        go.GetComponent<BoxCollider2D>().enabled = false;
                        generator.GetComponent<Generator>().OnGeneratorDestroyed += () => { go.GetComponent<BoxCollider2D>().enabled = true; };
                        
                        break;
                    }
                    //end room could not be generated from this node
                    else
                    {
                        borderNodes.RemoveAt(nodeIndex);
                    }
                }

                //failed to generate end room
                if (!hasGeneratedEndRoom)
                {
                    Debug.LogError("Failed to generate end room");
                }

                //finalize map
                FinalizeMap(map);

                //reset player position
                Vector2Int playerPositionTile = map.rooms[0].Floor[Random.Range(0, map.rooms[0].Floor.Count - 1)];
                Vector3 playerPosition = new Vector3(playerPositionTile.x + 0.5f, playerPositionTile.y + 0.5f, 0);
                GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject obj in playerObjects)
                {
                    obj.transform.position = playerPosition;
                }

                return true;
            }
        }

        private void CalculateBounds(LevelMap map)
        {
            //calculate map size
            bottomLeft = new(int.MaxValue, int.MaxValue);
            topRight = new(int.MinValue, int.MinValue);
            for (int i = 0; i < map.rooms.Count; i++)
            {
                bottomLeft.x = Mathf.Min(bottomLeft.x, map.rooms[i].bounds.xMin - (int)roomSpacing);
                bottomLeft.y = Mathf.Min(bottomLeft.y, map.rooms[i].bounds.yMin - (int)roomSpacing);
                topRight.x = Mathf.Max(topRight.x, map.rooms[i].bounds.xMax + (int)roomSpacing);
                topRight.y = Mathf.Max(topRight.y, map.rooms[i].bounds.yMax + (int)roomSpacing);
            }

            mapWidth = topRight.x - bottomLeft.x;
            mapHeight = topRight.y - bottomLeft.y;
        }

        private void FinalizeMap(LevelMap map)
        {
            //initiate map grid
            CalculateBounds(map);
            map.ResetGrid(bottomLeft, mapWidth, mapHeight);

            //retrieve tiles from rooms
            for (int r = 0; r < map.rooms.Count; r++)
            {
                Room room = map.rooms[r];
                //place floor tiles
                foreach (Vector2Int tilePos in room.Floor)
                {
                    map.SetTile(tilePos - bottomLeft, TileType.Floor);
                }

                //place wall tiles
                foreach (Wall wall in room.walls)
                {
                    if (!wall.IsAdjacentToFloor) continue;
                    if (map.GetTile(wall.Position - bottomLeft) != TileType.Empty) continue;
                    map.SetTile(wall.Position - bottomLeft, TileType.Wall);
                }

                //place doors
                foreach (Door node in room.Doors)
                {
                    TileType doorTileType;
                    if (node.direction.x > 0) doorTileType = TileType.DoorLeft;
                    else if (node.direction.x < 0) doorTileType = TileType.DoorRight;
                    else doorTileType = TileType.DoorVertical;

                    map.doors.Add(new Door(node.Position - bottomLeft, -node.direction));

                    map.SetTile(node.Position - bottomLeft, doorTileType);
                }
            }

            //clean up room geometry
            CleanMapShape(map);

            //regenerate room shape lists
            for (int r = 0; r < map.rooms.Count; r++)
            {
                Room room = map.rooms[r];
                AddProps(map, room);
                map.RecalculateRoomShape(room, (int)roomSpacing);
                room.GenerateBounds();
            }

            map.ApplyToTilemap();

            EnemyGetCount.gameWin = true;
        }

        private void CleanMapShape(LevelMap map)
        {
            void RemoveDisconnectedWalls()
            {
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

                            //remove protruding walls
                            foreach (Vector2Int direction in TileManager.directions)
                            {
                                TileType tile = map.GetTile(tilePos + direction);
                                if (tile == TileType.Wall) neighborCount++;
                                else if (tile == TileType.Floor || TileManager.IsDoor(tile)) hasFloor = true;
                            }

                            if (neighborCount < 2 && hasFloor)
                            {
                                map.SetTile(tilePos, TileType.Floor);
                                doRemoveWalls = true;
                            }

                            //remove walls not adjacent to floor
                            hasFloor = false;
                            foreach (Vector2Int direction in TileManager.directions8)
                            {
                                TileType tile = map.GetTile(tilePos + direction);
                                if (tile == TileType.Floor || TileManager.IsDoor(tile))
                                {
                                    hasFloor = true;
                                    break;
                                }
                            }

                            if (!hasFloor)
                            {
                                map.SetTile(tilePos, TileType.Empty);
                                doRemoveWalls = true;
                            }
                        }
                    }
                }
            }

            RemoveDisconnectedWalls();

            //add extra walls
            foreach (Room room in map.rooms)
            {
                if (Random.Range(0, 5) != 0) continue;

                //Vector2Int a = room.Floor[Random.Range(room.Floor)]
            }

            //add extra doors
            if (extraDoorChance > 0)
            {
                for (int x = 3; x < mapWidth - 3; x++)
                {
                    for (int y = 3; y < mapHeight - 3; y++)
                    {
                        //random chance to run
                        if (extraDoorChance < 100 && Random.Range(0, 100) > extraDoorChance) continue;
                        Vector2Int tilePos = new Vector2Int(x, y);
                        //check if tile is wall
                        if (map.GetTile(x, y) != TileType.Empty) continue;

                        //vertical
                        if (
                            map.GetTile(x, y + 1) == TileType.Wall &&
                            map.GetTile(x, y + 2) == TileType.Floor &&
                            map.GetTile(x, y - 1) == TileType.Wall &&
                            map.GetTile(x, y - 2) == TileType.Floor &&
                            map.GetTile(x + 1, y) == TileType.Empty &&
                            map.GetTile(x - 1, y) == TileType.Empty &&
                            map.GetTile(x + 2, y) == TileType.Empty &&
                            map.GetTile(x - 2, y) == TileType.Empty
                        )
                        {
                            //check rooms
                            int roomId1 = map.FindRoom(map.Position + new Vector2Int(x, y + 2));
                            int roomId2 = map.FindRoom(map.Position + new Vector2Int(x, y - 2));
                            if (roomId1 != -1 && roomId2 != -1 && roomId1 != roomId2)
                            {
                                Room room1 = map.rooms[roomId1];
                                Room room2 = map.rooms[roomId2];

                                if (
                                    room1.neighborIds.Count <= RoomRules.MaxConnections[(int)room1.type] &&
                                    room2.neighborIds.Count <= RoomRules.MaxConnections[(int)room2.type] &&
                                    !room1.neighborIds.Contains(roomId2) &&
                                    RoomRules.ChooseRoomType(map, roomId1).Contains(room2.type)
                                )
                                {
                                    //add neighbor ids
                                    room1.neighborIds.Add(roomId2);
                                    room2.neighborIds.Add(roomId1);

                                    //add door
                                    map.doors.Add(new Door(tilePos + Vector2Int.up, Vector2Int.up));
                                    map.doors.Add(new Door(tilePos + Vector2Int.down, Vector2Int.down));
                                    map.SetTile(x, y, TileType.Floor);
                                    map.SetTile(x, y + 1, TileType.DoorVertical);
                                    map.SetTile(x, y - 1, TileType.DoorVertical);
                                    map.SetTile(x + 1, y, TileType.Wall);
                                    map.SetTile(x - 1, y, TileType.Wall);
                                }
                            }
                        }
                        //horizontal
                        else if (
                            map.GetTile(x + 1, y) == TileType.Wall &&
                            map.GetTile(x + 2, y) == TileType.Floor &&
                            map.GetTile(x - 1, y) == TileType.Wall &&
                            map.GetTile(x - 2, y) == TileType.Floor &&
                            map.GetTile(x, y + 1) == TileType.Empty &&
                            map.GetTile(x, y - 1) == TileType.Empty &&
                            map.GetTile(x, y + 2) == TileType.Empty &&
                            map.GetTile(x, y - 2) == TileType.Empty
                        )
                        {
                            //check rooms 
                            int roomId1 = map.FindRoom(map.Position + new Vector2Int(x + 2, y));
                            int roomId2 = map.FindRoom(map.Position + new Vector2Int(x - 2, y));
                            if (roomId1 != -1 && roomId2 != -1 && roomId1 != roomId2)
                            {
                                Room room1 = map.rooms[roomId1];
                                Room room2 = map.rooms[roomId2];

                                if (
                                    room1.neighborIds.Count <= RoomRules.MaxConnections[(int)room1.type] &&
                                    room2.neighborIds.Count <= RoomRules.MaxConnections[(int)room2.type] &&
                                    !room1.neighborIds.Contains(roomId2) &&
                                    RoomRules.ChooseRoomType(map, roomId1).Contains(room2.type)
                                )
                                {
                                    //add neighbor ids
                                    room1.neighborIds.Add(roomId2);
                                    room2.neighborIds.Add(roomId1);

                                    //add door
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
                }
            }

            //unblock doors
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (TileManager.IsDoor(map.GetTile(x, y)))
                    {
                        Vector2Int tilePos = new Vector2Int(x, y);
                        Vector2Int floorDirection = Vector2Int.zero;
                        Vector2Int wallDirection = Vector2Int.zero;
                        int floorTiles = 0;
                        foreach (Vector2Int direction in TileManager.directions)
                        {
                            Vector2Int newPos = tilePos + direction;
                            if (map.GetTile(newPos) == TileType.Floor)
                            {
                                floorTiles++;
                                floorDirection = direction;
                            }
                            else if (map.GetTile(newPos) == TileType.Wall)
                            {
                                wallDirection = direction;
                            }
                        }

                        //if only one floor is found
                        if (floorTiles == 1)
                        {
                            map.SetTile(tilePos - floorDirection, TileType.Floor);
                            //print("fixed blocked door");
                        }
                        else if (floorTiles == 3)
                        {
                            map.SetTile(tilePos - wallDirection, TileType.Wall);
                            map.SetTile(tilePos - wallDirection * 2, TileType.Wall);
                            //print("fixed exposed door");
                        }
                    }
                }
            }

            RemoveDisconnectedWalls();
        }

        bool GenerateRoomShape(LevelMap map, BorderNode origin, int area, RoomType roomType, out Room room,
            bool hasDoor = true)
        {
            //create room
            room = new Room();
            room.type = roomType;
            room.style =
                RoomRules.StyleRules[(int)roomType][Random.Range(0, RoomRules.StyleRules[(int)roomType].Count)];
            room.distanceFromStart = map.rooms.Count() - 1;

            List<RoomGenTile> openSet = new List<RoomGenTile>() { };
            List<RoomGenTile> closedSet = new List<RoomGenTile>() { new(origin.position, 0, -1) };
            room.Floor.Add(closedSet[^1].position);


            if (hasDoor)
            {
                room.Doors.Add(new(origin.position, origin.direction));
                for (int i = 1; i < roomSpacing; i++)
                {
                    closedSet.Add(new(closedSet[^1].position + origin.direction, 0, closedSet.Count() - 1));
                    room.Floor.Add(closedSet[^1].position);
                }

                room.Doors.Add(new(closedSet[^1].position, -origin.direction));
                closedSet.Add(new(closedSet[^1].position + origin.direction, 0, closedSet.Count() - 1));
                openSet.Add(new(closedSet[^1].position + origin.direction, 0, closedSet.Count() - 1));

                //check validity of room starting point
                foreach (Vector2Int node in roomAdjacentTiles)
                {
                    if (node == openSet[0].position) return false;
                }
            }
            else
            {
                openSet.Add(new(origin.position + Vector2Int.up, 0, closedSet.Count() - 1));
                openSet.Add(new(origin.position + Vector2Int.down, 0, closedSet.Count() - 1));
                openSet.Add(new(origin.position + Vector2Int.left, 0, closedSet.Count() - 1));
                openSet.Add(new(origin.position + Vector2Int.right, 0, closedSet.Count() - 1));
            }

            //generate floor layout
            for (int i = 0; i < area; i++)
            {
                //if openSet is empty, room is invalid
                //abort room generation and return false
                if (openSet.Count == 0) return false;

                //update value
                foreach (RoomGenTile node in openSet) node.value = GetTileValue(node, closedSet, roomType);

                //find tile with most neighbors
                List<int> indices = ListUtils.GetHighestValuesIndices(openSet,
                    (a, b) => a.value - b.value);

                //if multiple have equal value, pick a random one
                int index = indices[Random.Range(0, indices.Count - 1)];
                //close node
                closedSet.Add(openSet[index]);
                openSet.Remove(openSet[index]);
                RoomGenTile prevTile = closedSet[^1];
                room.Floor.Add(prevTile.position);
                //Debug.Log($"Placed tile on {prevTile.position}");

                //add new positions to open set
                foreach (Vector2Int direction in TileManager.directions)
                {
                    Vector2Int newPos = prevTile.position + direction;

                    bool isValid = true;

                    //check if tile is already occupied
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

                    else openSet.Add(new RoomGenTile(newPos, 0, closedSet.Count() - 1));
                }
            }

            // Debug.Log($"Room shape generated with {room.shape.Count} tiles.");

            //add positions to roomAdjacentTiles
            room.GenerateBounds();
            room.GenerateBorder((int)roomSpacing);
            map.rooms.Add(room);
            foreach (Wall wall in room.walls)
            {
                if (!roomAdjacentTiles.Contains(wall.Position))
                {
                    roomAdjacentTiles.Add(wall.Position);
                }
                else continue;

                if (!Mathf.Approximately(wall.Direction.magnitude, 1f)) continue;

                borderNodes.Add(new(wall.Position, wall.Direction, wall.IsAdjacentToFloor, map.rooms.Count() - 1));
            }

            return true;
        }

        //returns value of how good a new tile is
        float GetTileValue(RoomGenTile node, List<RoomGenTile> closedSet, RoomType roomType)
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
                    if (closedSet[i].position == node.position + direction)
                    {
                        if (d < 4) neighborCount++;
                        else cornerNeighborCount++;
                        break;
                    }
                }
            }

            int[] HallwayWeights = { 0, 3, 6, 2, -5 };
            int sameAxisTileCount = GetTileCountOnSameAxis(node.position, closedSet,
                node.position.x == closedSet[node.parentIndex].position.x);

            switch (roomType)
            {
                case RoomType.Hallway:
                    value += HallwayWeights[neighborCount];
                    if (cornerNeighborCount > 1) value -= cornerNeighborCount * 0.8f;
                    value -= (Vector2Int.Distance(closedSet[0].position, node.position) * 0.05f);
                    value += GetTileCountInRange(node.position, closedSet, 3) * 0.2f;
                    value -= GetTileCountInRange(node.position, closedSet, 8) * 0.2f;
                    break;
                case RoomType.Start:
                    value -= Vector2Int.Distance(closedSet[0].position, node.position);
                    value += neighborCount * 0.5f;
                    break;
                default:
                    value += neighborCount;
                    break;
            }

            if (neighborCount == 2 && cornerNeighborCount == 3) value -= 5;

            //decrease value by distance
            //value -= Mathf.RoundToInt(position.magnitude * 0.5f);
            //value -= Mathf.FloorToInt(new Vector2(position.x * 0.2f, (float)position.y * 0.1f).magnitude);
            //value -= GetAverageDistance(position, closedSet) * 0.1f;

            //add randomness
            if (roomShapeRandomness > 0) value += Random.Range(0, roomShapeRandomness);

            return value;
        }

        float GetAverageDistance(Vector2Int origin, List<RoomGenTile> positions)
        {
            float totalDistance = 0f;
            foreach (RoomGenTile node in positions)
            {
                totalDistance += Vector2Int.Distance(node.position, origin);
            }

            return totalDistance / positions.Count;
        }

        private int GetTileCountInRange(Vector2Int origin, List<RoomGenTile> positions, float range)
        {
            int tileCount = 0;
            foreach (RoomGenTile node in positions)
            {
                if (Vector2Int.Distance(node.position, origin) < range) tileCount++;
            }

            return tileCount;
        }

        private int GetTileCountOnSameAxis(Vector2Int origin, List<RoomGenTile> positions, bool isXAxis)
        {
            int tileCount = 0;
            foreach (RoomGenTile node in positions)
            {
                if (isXAxis && origin.x == node.position.x) tileCount++;
                else if (origin.y == node.position.y) tileCount++;
            }

            return tileCount;
        }

        public void AddProps(LevelMap map, Room room)
        {
            //print(room.type);
            /*
            if (room.type == RoomType.RewardRoom)
            {
                Vector2Int tilePos = new Vector2Int(Mathf.FloorToInt(room.bounds.center.x), Mathf.FloorToInt(room.bounds.center.y)) + map.Position;
                map.SetTile(tilePos, TileType.Door);
            }
            */
        }

        private class RoomGenTile
        {
            public Vector2Int position;
            public float value;
            public int parentIndex;

            public RoomGenTile(Vector2Int position, int value, int parentIndex)
            {
                this.position = position;
                this.value = value;
                this.parentIndex = parentIndex;
            }
        }
    }

    public class BorderNode
    {
        public Vector2Int position;
        public Vector2Int direction;
        public bool isAdjacentToFloor;
        public int roomId;

        public BorderNode(Vector2Int position, Vector2Int direction, bool isAdjacentToFloor, int roomId)
        {
            this.position = position;
            this.direction = direction;
            this.isAdjacentToFloor = isAdjacentToFloor;
            this.roomId = roomId;
        }
    }
}