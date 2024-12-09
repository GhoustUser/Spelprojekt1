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

        [Header("Doors")] [Tooltip("Percent chance for extra doors to generate")] [SerializeField]
        private uint extraDoorChance = 0;

        [Tooltip("Distance between extra doors")] [SerializeField]
        private float extraDoorDistance = 7f;

        [Header("Room Types")] [SerializeField]
        private uint arena1Size = 60;

        [SerializeField] private uint arena2Size = 100;
        [SerializeField] private uint arena3Size = 140;
        [SerializeField] private uint hallwaySize = 50;

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
            BorderNode originNode = new(new(0, 0), Vector2Int.up, false, RoomType.Default);
            GenerateRoomShape(map, originNode, (int)roomSize, RoomType.Start, out room, false);

            //generate rest of rooms
            for (int r = 0; r < 1000 && roomsLeftToGenerate > 0; r++)
            {
                if (borderNodes.Count > 0)
                {
                    //pick a random tile to generate new room from
                    int index = Random.Range(0, borderNodes.Count - 1);
                    RoomType prevRoomType = borderNodes[index].roomType;
                    RoomType nextRoomType;
                    switch (prevRoomType)
                    {
                        case RoomType.Hallway:
                            nextRoomType = new RoomType[] { RoomType.Arena1, RoomType.Arena2 }[Random.Range(0, 2)];
                            break;
                        case RoomType.Arena1:
                            nextRoomType = new RoomType[]
                                { RoomType.Arena1, RoomType.Arena2, RoomType.Arena3, RoomType.Hallway }[
                                Random.Range(0, 4)];
                            break;
                        case RoomType.Arena2:
                            nextRoomType = new RoomType[] { RoomType.Arena2, RoomType.Hallway }[Random.Range(0, 2)];
                            break;

                        default:
                            nextRoomType = RoomType.Hallway;
                            break;
                    }

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
                    if (GenerateRoomShape(map, borderNodes[index], nextRoomSize, nextRoomType, out room))
                    {
                        //reduce counter if room was generated successfully
                        roomsLeftToGenerate--;
                        
                    }
                    //remove nearby borderNodes
                    Vector2Int nodePosition = borderNodes[index].position;
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
                foreach (BorderNode node in room.border)
                {
                    if (!node.isAdjacentToFloor) continue;
                    if (map.GetTile(node.position - bottomLeft) != TileType.Empty) continue;
                    map.SetTile(node.position - bottomLeft, TileType.Wall);
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
                RecalculateRoomShape(room, map);
            }

            map.ApplyToTilemap();

            EnemyGetCount.gameWin = true;
        }

        private void CleanMapShape(LevelMap map)
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
                        
                        //remove portruding walls
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
                            float distance = Vector2.Distance(door.Position, tilePos);
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
                            map.GetTile(x + 1, y) == TileType.Empty &&
                            map.GetTile(x - 1, y) == TileType.Empty &&
                            map.GetTile(x + 2, y) == TileType.Empty &&
                            map.GetTile(x - 2, y) == TileType.Empty
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
                                 map.GetTile(x, y + 1) == TileType.Empty &&
                                 map.GetTile(x, y - 1) == TileType.Empty &&
                                 map.GetTile(x, y + 2) == TileType.Empty &&
                                 map.GetTile(x, y - 2) == TileType.Empty
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
        }

        //regenerate room shape after finalized map
        public void RecalculateRoomShape(Room room, LevelMap map, bool isProcedural = true)
        {
            Vector2Int startPos = room.Floor[(isProcedural ? (int)roomSpacing : 0)];
            room.Floor.Clear();
            room.border.Clear();
            room.Doors.Clear();

            List<Vector2Int> openSet = new List<Vector2Int>() { startPos };
            List<Vector2Int> closedSet = new List<Vector2Int>();

            for (int i = 0; i < 1000 && openSet.Count > 0; i++)
            {
                //close first node
                closedSet.Add(openSet[0]);
                openSet.RemoveAt(0);
                Vector2Int prevPos = closedSet[^1];

                foreach (Vector2Int direction in TileManager.directions8)
                {
                    Vector2Int nextPos = prevPos + direction;
                    TileType nextTile = map.GetTile(nextPos - map.Position);
                    //if(TileRules.IsDoor(nextTile)) closedSet.Add(nextPos);
                    bool valid = true;
                    foreach (Vector2Int node in openSet)
                    {
                        if (node == nextPos)
                        {
                            valid = false;
                            break;
                        }
                    }

                    //add walls
                    if (nextTile == TileType.Wall) room.border.Add(new BorderNode(nextPos, direction, true, room.type));
                    //ignore if diagonal
                    if (direction.x != 0 && direction.y != 0) continue;
                    //add doors
                    if (TileManager.IsDoor(nextTile)) room.Doors.Add(new Door(nextPos, direction));
                    //ignore if not floor
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

            room.Floor = closedSet;
        }

        bool GenerateRoomShape(LevelMap map, BorderNode origin, int area, RoomType roomType, out Room room,
            bool hasDoor = true)
        {
            //create room
            room = new Room();
            room.type = roomType;
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
                room.Floor.Add(prevTile.position);
                //Debug.Log($"Placed tile on {prevTile.position}");

                //add new positions to open set
                foreach (Vector2Int direction in TileManager.directions)
                {
                    Vector2Int newPos = prevTile.position + direction;

                    bool isValid = true;
                    if (Vector2Int.Distance(origin.position, newPos) <= 1) isValid = false;
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

                    else openSet.Add(new(newPos, 0, closedSet.Count() - 1));
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

                if (!borderNode.isAdjacentToFloor) doPlaceNode = false;
                /*
                for (int i = 0; i < borderNodes.Count; i++)
                {
                    if (borderNodes[i].position == borderNode.position)
                    {
                        borderNodes.Remove(borderNodes[i]);
                        doPlaceNode = false;
                        break;
                    }
                }
                */

                if (doPlaceNode) borderNodes.Add(borderNode);
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
        public RoomType roomType;

        public BorderNode(Vector2Int position, Vector2Int direction, bool isAdjacentToFloor, RoomType roomType)
        {
            this.position = position;
            this.direction = direction;
            this.isAdjacentToFloor = isAdjacentToFloor;
            this.roomType = roomType;
        }
    }
}