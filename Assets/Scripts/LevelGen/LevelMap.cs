using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace LevelGen
{
    public class LevelMap : MonoBehaviour
    {
        /* -------- Object references --------*/
        private Tilemap tilemap;
        private TileManager tileManager;

        /* -------- Variables --------*/
        private Vector2Int position;
        private int width, height;
        [HideInInspector] public bool isGenerated = false;

        private List<List<TileType>> grid = new List<List<TileType>>();
        public List<Door> doors = new List<Door>();
        public List<Room> rooms = new List<Room>();

        private TileRules tileRules = new TileRules();

        /* -------- Properties --------*/
        public Vector2Int Position => position;
        public int Width => width;
        public int Height => height;

        /* -------- Constructor --------*/
        public LevelMap()
        {
            tilemap = GetComponent<Tilemap>();
            tileManager = GetComponent<TileManager>();
        }

        /* -------- Functions --------*/
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