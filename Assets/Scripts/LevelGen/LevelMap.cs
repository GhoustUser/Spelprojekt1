using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace LevelGen
{
    public enum DoorState
    {
        Open,
        Opening,
        Closed
    }

    public enum DoorDirection
    {
        Left,
        Right,
        Vertical
    }

    public enum RoomType
    {
        Start,
        End,
        Hallway,
        Arena
    }

    public class LevelMap : MonoBehaviour
    {
        private Vector2Int position;
        private int width, height;
        public bool isGenerated = false;

        private List<List<TileType>> grid = new List<List<TileType>>();
        public List<Door> doors = new List<Door>();
        public List<Room> rooms = new List<Room>();

        public Vector2Int Position => position;
        public int Width => width;
        public int Height => height;

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
        }

        public TileType GetTile(Vector2Int position)
        {
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
    }

    public class Room
    {
        //room type
        public RoomType type;
        
        //if room has been explored
        public bool hasBeenExplored = false;
        
        //bounding box
        public BoundsInt bounds;

        //list of tiles
        public List<Vector2Int> shape = new List<Vector2Int>();

        //list of positions making the border
        public List<BorderNode> border = new List<BorderNode>();

        //positions of doors
        public List<BorderNode> doors = new List<BorderNode>();

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

            bounds.zMin = -1;
            bounds.zMax = 1;
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
                        border.Add(new(position, direction, distance, type));
                    }
                }
            }
        }
    }

    public class Door
    {
        public Vector2Int position;

        public DoorDirection direction;

        //value between 0 and 1, where 0 means closed and 1 means open
        private float progress = 0.0f;

        public float Progress
        {
            get => progress;
            set { progress = Mathf.Clamp(value, 0.0f, 1.0f); }
        }

        public DoorState State
        {
            get
            {
                if (progress < 0.5f) return DoorState.Closed;
                if (progress < 1.0f) return DoorState.Opening;
                return DoorState.Open;
            }
        }

        public Door(Vector2Int position, DoorDirection direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }
}