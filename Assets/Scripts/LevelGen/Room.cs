using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace LevelGen
{
    /* -------- Room class --------*/
    public class Room
    {
        /* -------- Variables --------*/
        //room type
        public RoomType type;
        public RoomStyle style;
        
        //list of connected rooms id's
        public List<int> neighborIds = new List<int>();
        
        //distance from spawn room
        public int distanceFromStart = 0;
        
        //if room has been explored
        public bool hasBeenExplored = false;
        
        //bounding box
        public BoundsInt bounds;

        //list of tiles
        private List<Vector2Int> shape = new List<Vector2Int>();

        //list of positions making the border
        public List<Wall> walls = new List<Wall>();

        //positions of doors
        private List<Door> doors = new List<Door>();
        
        
        /* -------- Properties --------*/
        public List<Vector2Int> Floor
        {
            get => shape;
            set => shape = value;
        }

        public List<Door> Doors => doors;

        
        /* -------- Functions --------*/
        public void GenerateBounds()
        {
            if (shape.Count == 0)
            {
                Debug.Log("Cannot generate bounds for empty room");
                return;
            }

            bounds.xMin = shape[0].x - 1;
            bounds.yMin = shape[0].y - 1;
            bounds.xMax = shape[0].x + 2;
            bounds.yMax = shape[0].y + 2;
            foreach (Vector2Int shapePoint in shape)
            {
                bounds.xMin = Mathf.Min(bounds.xMin, shapePoint.x - 1);
                bounds.yMin = Mathf.Min(bounds.yMin, shapePoint.y - 1);
                bounds.xMax = Mathf.Max(bounds.xMax, shapePoint.x + 2);
                bounds.yMax = Mathf.Max(bounds.yMax, shapePoint.y + 2);
            }

            bounds.zMin = -1;
            bounds.zMax = 1;
        }

        public void GenerateBorder(int spacing)
        {
            walls.Clear();
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
                        walls.Add(new Wall(position, distance < 2f, direction));
                    }
                }
            }
        }
    }
    
    
    /* -------- Wall class --------*/
    public class Wall
    {
        private Vector2Int position;
        private bool isAdjacentToFloor;
        private Vector2Int direction;

        public Vector2Int Position => position;
        public bool IsAdjacentToFloor => isAdjacentToFloor;
        public Vector2Int Direction => direction;

        public Wall(Vector2Int position, bool isAdjacentToFloor)
        {
            this.position = position;
            this.isAdjacentToFloor = isAdjacentToFloor;
            this.direction = Vector2Int.zero;
        }
        public Wall(Vector2Int position, bool isAdjacentToFloor, Vector2Int direction)
        {
            this.position = position;
            this.isAdjacentToFloor = isAdjacentToFloor;
            this.direction = direction;
        }
    }
    
    
    /* -------- Door class --------*/
    public class Door
    {
        private Vector2Int position;
        public Vector2Int direction;
        public bool wasOpen;

        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }
        public Vector2Int Direction => direction;

        public DoorDirection DoorDirection
        {
            get
            {
                if (direction.x < 0) return DoorDirection.Left;
                if (direction.x > 0) return DoorDirection.Right;
                return DoorDirection.Vertical;
            }
        }

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
                if (progress < 0.2f) return DoorState.Closed;
                if (progress < 0.8f) return DoorState.Opening;
                return DoorState.Open;
            }
        }

        public Door(Vector2Int position, Vector2Int direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }
}
