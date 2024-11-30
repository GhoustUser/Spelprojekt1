using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelGen;

public class TileRules
{
    public class TileRule
    {
        private TileType[] rule;
        private int tileId;
        public int TileId => tileId;

        public TileRule(TileType[] rule, int tileId)
        {
            this.rule = rule;
            this.tileId = tileId;
        }

        public bool CheckRule(LevelMap map, Vector2Int position)
        {
            for (int y = 1; y >= -1; y--)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int index = -y * 3 + x + 4;
                    //if rule is "any", continue
                    if (rule[index] == TileType.Any) continue;
                    //position of neighboring tile
                    Vector2Int newPosition = new Vector2Int(position.x + x, position.y + y);
                    //get tile at position
                    TileType newTile = map.GetTile(newPosition);
                    //TEMPORARY: treats neighboring doors as floor
                    if (newTile == TileType.Door && !(x == 0 && y == 0)) newTile = TileType.Floor;
                    //compare tile with rule
                    if (newTile != rule[index]) return false;
                }
            }

            return true;
        }
        public bool CheckRule(LevelMap map, int x, int y) { return CheckRule(map, new Vector2Int(x, y)); }
    }

    public readonly TileRule[] rules = new TileRule[]
    {
        //floor
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Any, TileType.Any, 
                TileType.Any, TileType.Floor, TileType.Any, 
                TileType.Any, TileType.Any, TileType.Any
            },
            0
        ),
        //wall right
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Any, 
                TileType.Floor, TileType.Wall, TileType.Any, 
                TileType.Any, TileType.Wall, TileType.Any
            },
            11
        ),
        //wall left
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Any, 
                TileType.Any, TileType.Wall, TileType.Floor, 
                TileType.Any, TileType.Wall, TileType.Any
            },
            13
        ),
        //inverted wall top left corner
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Floor, TileType.Any, 
                TileType.Floor, TileType.Wall, TileType.Wall, 
                TileType.Any, TileType.Wall, TileType.Any
            },
            1
        ),
        //inverted wall top right corner
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Floor, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Floor, 
                TileType.Any, TileType.Wall, TileType.Any
            },
            3
        ),
        //wall up left corner
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Any, TileType.Any, 
                TileType.Any, TileType.Wall, TileType.Wall, 
                TileType.Any, TileType.Wall, TileType.Floor
            },
            4
        ),
        //wall up left
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Any, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Wall, 
                TileType.Wall, TileType.Floor, TileType.Any
            },
            5
        ),
        //wall up middle
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Any, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Wall, 
                TileType.Floor, TileType.Floor, TileType.Floor
            },
            6
        ),
        //wall up right
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Any, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Wall, 
                TileType.Any, TileType.Floor, TileType.Wall
            },
            7
        ),
        //wall up right corner
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Any, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Any, 
                TileType.Floor, TileType.Wall, TileType.Any
            },
            8
        ),
        //inverted wall up left corner
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Any, 
                TileType.Floor, TileType.Wall, TileType.Wall, 
                TileType.Any, TileType.Floor, TileType.Any
            },
            21
        ),
        //inverted wall up left corner
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Floor, 
                TileType.Any, TileType.Floor, TileType.Any
            },
            23
        ),
        //wall down left corner
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Floor, 
                TileType.Any, TileType.Wall, TileType.Wall, 
                TileType.Any, TileType.Any, TileType.Any
            },
            24
        ),
        //wall down
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Floor, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Wall, 
                TileType.Any, TileType.Any, TileType.Any
            },
            25
        ),
        //wall down right corner
        new TileRule(
            new TileType[]
            {
                TileType.Floor, TileType.Wall, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Any, 
                TileType.Any, TileType.Any, TileType.Any
            },
            28
        ),
        //wall right blocked
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Any, 
                TileType.Any, TileType.Wall, TileType.Any
            },
            11
        ),
        //wall left blocked
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Any, 
                TileType.Any, TileType.Wall, TileType.Wall, 
                TileType.Any, TileType.Wall, TileType.Any
            },
            13
        ),
        //door vertical
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Any, TileType.Any, 
                TileType.Wall, TileType.Door, TileType.Wall, 
                TileType.Any, TileType.Any, TileType.Any
            },
            32
        ),
        //door horizontal
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Any, 
                TileType.Any, TileType.Door, TileType.Any, 
                TileType.Any, TileType.Wall, TileType.Any
            },
            35
        ),
        //test tube top wall
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Any, 
                TileType.Floor, TileType.Wall, TileType.Floor, 
                TileType.Any, TileType.Floor, TileType.Any
            },
            39
        ),
        //test tube bottom wall
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Wall, TileType.Any, 
                TileType.Floor, TileType.Wall, TileType.Wall, 
                TileType.Any, TileType.Floor, TileType.Any
            },
            39
        ),
        //test tube left wall
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Floor, TileType.Any, 
                TileType.Wall, TileType.Wall, TileType.Floor, 
                TileType.Any, TileType.Floor, TileType.Any
            },
            39
        ),
        //test tube right wall
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Floor, TileType.Any, 
                TileType.Floor, TileType.Wall, TileType.Wall, 
                TileType.Any, TileType.Floor, TileType.Any
            },
            39
        ),
        //test tube no wall
        new TileRule(
            new TileType[]
            {
                TileType.Any, TileType.Floor, TileType.Any, 
                TileType.Floor, TileType.Wall, TileType.Floor, 
                TileType.Any, TileType.Floor, TileType.Any
            },
            39
        ),
    };

}