using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class RoomGeneratorScript : MonoBehaviour
{
    /* -------- Settings --------*/

    public int roomAmount = 10;
    public float roomGenDelay = 0.2f;
    public int roomShapeRandomness = 0;
    public int roomSpacing = 2;
    public bool doPrintLogs = false;

    /* -------- -------- --------*/
    

    private static Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private Tilemap tilemap;
    public Tile floorTile, wallTile, crackedWallTile;
    private List<Room> rooms = new List<Room>();
    private TileManager tileManager;

    //list of positions where new rooms can spawn
    private List<NewRoomNode> newRoomNodes = new List<NewRoomNode>();

    //list of tiles adjacent to existing rooms
    private List<Vector2Int> roomAdjacentTiles = new List<Vector2Int>();

    public static Dictionary<Vector2, Cell> cells = new Dictionary<Vector2, Cell>();

    private int roomsLeftToGenerate;
    private float roomTimer;


    private void GenerateGrid()
    {
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
        roomsLeftToGenerate = roomAmount;
        roomTimer = roomGenDelay;
        //find tilemap component
        tilemap = GetComponent<Tilemap>();
        tileManager = GetComponent<TileManager>();
        //tilemap.SetTile(new Vector3Int(-4,-5,0), wallTile);

        //generate first rectangle room shape
        Room room;
        GenerateRoomShape(new(0, 0), Vector2Int.up, 80, out room, false);
        PlaceWallTiles(room);
    }


    // Update is called once per frame
    void Update()
    {
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
                if (GenerateRoomShape(newRoomNodes[index].position, newRoomNodes[index].direction, 80, out room))
                {
                    //reduce counter if room was generated successfully
                    roomsLeftToGenerate--;
                    PlaceWallTiles(room);
                }

                //remove tile from pool
                //newRoomNodes.RemoveAt(index);
            }
            else if(doPrintLogs) print("No space left to generate rooms");
        }
        //when all rooms have been generated
        else if(newRoomNodes.Count > 0)
        {
            roomAdjacentTiles.Clear();
            newRoomNodes.Clear();
            GenerateGrid();
            if(doPrintLogs) print("Room generation finished");
        }
    }

    bool GenerateRoomShape(Vector2Int origin, Vector2Int roomDirection, int area, out Room room, bool hasDoor = true)
    {
        room = new Room();
        List<RoomGenTile> openSet = new List<RoomGenTile>() { };
        List<RoomGenTile> closedSet = new List<RoomGenTile>() { new(origin, origin + roomDirection, 0) };
        room.shape.Add(closedSet[^1].position);

        if (hasDoor)
        {
            room.doors.Add(new(origin, roomDirection, 0f));
            for (int i = 1; i < roomSpacing; i++)
            {
                closedSet.Add(new(closedSet[^1].position + roomDirection, closedSet[^1].position, 0));
                room.shape.Add(closedSet[^1].position);
            }

            room.doors.Add(new(closedSet[^1].position, -roomDirection, 0f));
            openSet.Add(new(closedSet[^1].position + roomDirection, closedSet[^1].position, 0));

            //check validity of room starting point
            foreach (Vector2Int node in roomAdjacentTiles)
            {
                if (node == openSet[0].position) return false;
            }
        }
        else
        {
            openSet.Add(new(origin + Vector2Int.up, origin, 0));
            openSet.Add(new(origin + Vector2Int.down, origin, 0));
            openSet.Add(new(origin + Vector2Int.left, origin, 0));
            openSet.Add(new(origin + Vector2Int.right, origin, 0));
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

                else openSet.Add(new(newPos, prevTile.position, 0));
            }
        }

        //place room tiles
        PlaceFloorTiles(room);

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

        //add randomness
        if(roomShapeRandomness > 0) value += Random.Range(0, roomShapeRandomness);
        
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
                if(node.direction.x > 0) tilemap.SetTile(tilemapPosition, tileManager.tiles[36]);
                else tilemap.SetTile(tilemapPosition, tileManager.tiles[35]);
            }
            else 
            {
                tilemap.SetTile(tilemapPosition, tileManager.tiles[32]);
            }
        }
    }

    public class RoomGenTile
    {
        public Vector2Int position;
        public int value;
        public Vector2Int direction;

        public RoomGenTile(Vector2Int position, Vector2Int parentPosition, int neighborCount)
        {
            this.position = position;
            this.value = neighborCount;
            this.direction = -(parentPosition - position);
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