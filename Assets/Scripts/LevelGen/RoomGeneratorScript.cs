using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGeneratorScript : MonoBehaviour
{
    private static Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    private static Vector2Int[] directions8 = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new(1, 1), new(1, -1), new(-1, -1), new(-1, 1) };

    private Tilemap tilemap;
    public Tile floorTile, wallTile, crackedWallTile;

    //list of positions where new rooms can spawn
    public List<NewRoomNode> newRoomNodes = new List<NewRoomNode>();

    //list of tiles adjacent to existing rooms
    public List<Vector2Int> roomAdjacentTiles = new List<Vector2Int>();

    public static Dictionary<Vector2, Cell> cells = new Dictionary<Vector2, Cell>();

    private int roomsLeftToGenerate = 10;
    private float roomTimer = 1.0f;


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
                        tilemap.GetTile(new Vector3Int(i, j, 0)) == floorTile);
                    continue;
                }

                cells.Add(new Vector2(i, j),
                    new Cell(new Vector2(i, j),
                        tilemap.GetTile(new Vector3Int(i, j, 0)) == floorTile));
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
        //tilemap.SetTile(new Vector3Int(-4,-5,0), wallTile);

        //generate first rectangle room shape
        GenerateRoomShape(new(0, -1), Vector2Int.up, 80, false);
    }

    // Update is called once per frame
    void Update()
    {
        roomTimer -= Time.deltaTime;
        if (roomTimer > 0) return;
        else roomTimer += 1.0f;

        if (newRoomNodes.Count == 0)
        {
            Debug.Log("No spaces left to generate");
        }
        else if (roomsLeftToGenerate > 0)
        {
            //pick a random tile to generate new room from
            int index = Random.Range(0, newRoomNodes.Count - 1);
            //generate room
            if (GenerateRoomShape(newRoomNodes[index].position, newRoomNodes[index].direction, 80))
            {
                //reduce counter if room was generated successfully
                roomsLeftToGenerate--;
                GenerateGrid();
            }

            //remove tile from pool
            newRoomNodes.RemoveAt(index);
        }
    }

    bool GenerateRoomShape(Vector2Int origin, Vector2Int roomDirecion, int area, bool hasDoor = true)
    {
        Room room = new Room();
        List<RoomGenTile> openSet = new List<RoomGenTile>() { new(origin + roomDirecion, origin, 0) };
        List<RoomGenTile> closedSet = new List<RoomGenTile>() { new(origin, origin + roomDirecion, 0) };

        for (int i = 0; i < area; i++)
        {
            //if openSet is empty, room is invalid
            //abort room generation and return false
            if (openSet.Count == 0) return false;

            //update neighbor count
            foreach (RoomGenTile node in openSet) node.neighborCount = GetNeighborCount(node.position, closedSet);

            //find tile with most neighbors
            List<int> indices = new List<int>() { 0 };
            for (int j = 1; j < openSet.Count; j++)
            {
                if (openSet[j].neighborCount > openSet[indices[0]].neighborCount)
                {
                    indices = new List<int>() { j };
                }
                else if (openSet[j].neighborCount == openSet[indices[0]].neighborCount)
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

        //generate room bounds
        //room.GenerateBounds();
        //place room tiles
        room.PlaceTiles(tilemap, floorTile);
        //place door
        if (hasDoor)
        {
            tilemap.SetTile(new Vector3Int(origin.x, origin.y, 0), crackedWallTile);
        }
        else
        {
            tilemap.SetTile(new Vector3Int(origin.x, origin.y, 0), floorTile);
        }

        Debug.Log($"Room shape generated with {room.shape.Count} tiles.");

        //add positions to roomAdjacentTiles
        room.GenerateBounds();
        room.GenerateBorder();
        foreach (NewRoomNode borderNode in room.border)
        {
            if (!roomAdjacentTiles.Contains(borderNode.position))
            {
                roomAdjacentTiles.Add(borderNode.position);
            }

            bool doPlaceNode = !(borderNode.direction == Vector2Int.zero);
            //if (borderNode.direction.x != 0 && borderNode.direction.y != 0) doPlaceNode = false;
            for (int i = 0; i < newRoomNodes.Count; i++)
            {
                if (newRoomNodes[i].position == borderNode.position)
                {
                    newRoomNodes.Remove(newRoomNodes[i]);
                    doPlaceNode = false;
                    break;
                }
            }
            if(doPlaceNode) newRoomNodes.Add(borderNode);
        }

        return true;
    }

    int GetNeighborCount(Vector2Int position, List<RoomGenTile> closedSet)
    {
        int neighbors = 0;
        for (int i = 0; i < closedSet.Count; i++)
        {
            if (closedSet[i].position == position + Vector2Int.up ||
                closedSet[i].position == position + Vector2Int.down ||
                closedSet[i].position == position + Vector2Int.left ||
                closedSet[i].position == position + Vector2Int.right)
            {
                neighbors++;
            }
        }

        //neighbors += Random.Range(0, 3);
        return neighbors;
    }

    public class RoomGenTile
    {
        public Vector2Int position;
        public int neighborCount;
        public Vector2Int direction;

        public RoomGenTile(Vector2Int position, Vector2Int parentPosition, int neighborCount)
        {
            this.position = position;
            this.neighborCount = neighborCount;
            this.direction = -(parentPosition - position);
        }
    }

    public class NewRoomNode
    {
        public Vector2Int position;
        public Vector2Int direction;

        public NewRoomNode(Vector2Int position, Vector2Int direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }

    public class Room
    {
        public BoundsInt bounds;
        public List<Vector2Int> shape = new List<Vector2Int>();
        public List<NewRoomNode> border = new List<NewRoomNode>();

        public void GenerateBounds()
        {
            if (shape.Count == 0)
            {
                Debug.Log("Cannot generate bounds for empty room");
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

        public void GenerateBorder()
        {
            border.Clear();
            for (int y = bounds.yMin - 1; y <= bounds.yMax + 1; y++)
            {
                for (int x = bounds.xMin - 1; x <= bounds.xMax + 1; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    bool isTile = false;
                    int neighborCount = 0;
                    Vector2Int direction = Vector2Int.zero;
                    //check if tile is occupied or has neighbors
                    foreach (Vector2Int tile in shape)
                    {
                        if (position == tile)
                        {
                            isTile = true;
                            break;
                        }

                        for(int i = 0; i < directions8.Length; i++)
                        {
                            if (position == tile + directions8[i])
                            {
                                if(i <= 3 || neighborCount == 0) neighborCount++;
                                if(i <= 3) direction = directions8[i];
                            }
                        }
                    }

                    if (!isTile && neighborCount > 0)
                    {
                        if(neighborCount > 1) direction = Vector2Int.zero;
                        border.Add(new NewRoomNode(position, direction));
                    }
                }
            }

            Debug.Log($"Border length: {border.Count}");
        }

        public void PlaceTiles(Tilemap tilemap, Tile tile)
        {
            foreach (Vector2Int shapePoint in shape)
            {
                tilemap.SetTile(new Vector3Int(shapePoint.x, shapePoint.y, 0), tile);
            }
        }
    }
}