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
    public class LevelMap
    {
        private Vector2Int position;
        private int width, height;
        
        private List<List<TileType>> grid = new List<List<TileType>>();
        public List<Door> doors = new List<Door>();

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

    public class Door
    {
        public Vector2Int position;
        public DoorDirection direction;
        //value between 0 and 1, where 0 means closed and 1 means open
        private float progress = 0.0f;

        public float Progress
        {
            get => progress;
            set
            {
                progress = Mathf.Clamp(value, 0.0f, 1.0f);
            }
        }

        public DoorState State
        {
            get
            {
                if(progress < 0.5f) return DoorState.Closed;
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