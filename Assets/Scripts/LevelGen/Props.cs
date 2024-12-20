using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelGen
{
    public enum PropType
    {
        Couch = 0,
        Plant1,
        Plant2,
        Table,
        Counter,
        Beakers,
        CoffeeCup
    }

    public class Props : MonoBehaviour
    {
        //what props generate in each room type
        public static readonly List<PropType>[] PropRules = new[]
        {
            //default
            new List<PropType> { PropType.Plant1, PropType.CoffeeCup },
            //lab
            new List<PropType> { PropType.Plant1, PropType.Counter, PropType.Beakers, PropType.CoffeeCup },
            //lounge
            new List<PropType> { PropType.Couch, PropType.Table, PropType.Plant2, PropType.CoffeeCup },
        };

        /* -------- Settings --------*/
        [Header("Settings")]
        [Tooltip("minimum amount of props that will attempt to generate per room")]
        [SerializeField]
        [Range(0, 10)]
        private int PropAmountMin = 2;

        [Tooltip("maximum amount of props that will attempt to generate per room")] [SerializeField] [Range(0, 20)]
        private int PropAmountMax = 5;

        [Tooltip("Amount of times it will attempt to place props again after failing")] [SerializeField] [Range(0, 500)]
        private int AttemptsPerRoom = 100;


        /* -------- Object references --------*/
        private Tilemap tilemap;
        [Header("RuleTiles")] public SeatTile seatTile;
        public TableTile tableTile;
        public TableTile counterTile;
        public PlantTile plantTile1;
        public PlantTile plantTile2;
        public Tile beakersTile;
        public Tile coffeeCupTile;


        /* -------- Variables --------*/
        private List<TileBase> tiles;


        /* -------- Start --------*/
        void Start()
        {
            //get object reference to tilemap
            tilemap = GetComponent<Tilemap>();

            //check if map has loaded
            if (LevelMap.IsLoaded)
            {
                LevelMap levelMap = FindObjectOfType<LevelMap>();
                GenerateProps(levelMap);
            }

            //subscribe to events
            LevelMap.OnLevelLoaded += GenerateProps;
            LevelMap.OnLevelUnloaded += ClearProps;
        }


        /* -------- Functions --------*/
        private bool IsAreaValid(Room room, Vector2Int bottomLeft, Vector2Int topRight)
        {
            if (!room.IsAreaFloor(bottomLeft, topRight)) return false;
            bool isAreaValid = true;

            for (int x = bottomLeft.x; x <= topRight.x; x++)
            {
                for (int y = bottomLeft.y; y <= topRight.y; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    if (tilemap.GetTile(position) != null) return false;
                }
            }

            return true;
        }

        private bool IsAdjacentToWall(Room room, Vector2Int bottomLeft, Vector2Int topRight)
        {
            return !room.IsAreaFloor(bottomLeft + new Vector2Int(-1, -1), topRight + new Vector2Int(1, 1));
        }

        private void GenerateProps(LevelMap map)
        {
            foreach (Room room in map.rooms)
            {
                bool hasGeneratedCouch = false;

                //place n amount of tiles
                int remainingAttempts = AttemptsPerRoom;
                bool placedProp = false;

                //select random prop type
                PropType propType;

                void RandomizePropType()
                {
                    propType = PropRules[(int)room.style][Random.Range(0, PropRules[(int)room.style].Count)];
                    placedProp = true;
                }

                RandomizePropType();

                //place counter
                if (room.style == RoomStyle.Lab)
                {
                    Vector2Int topLeft = room.Floor[0];
                    for (int i = 1; i < room.Floor.Count; i++)
                    {
                        if (room.Floor[i].y > topLeft.y ||
                            (room.Floor[i].y == topLeft.y && room.Floor[i].x < topLeft.x))
                        {
                            topLeft = room.Floor[i];
                        }
                    }

                    int leftHeight = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (
                            room.Floor.Contains(new Vector2Int(topLeft.x, topLeft.y - leftHeight)) &&
                            !room.Floor.Contains(new Vector2Int(topLeft.x - 1, topLeft.y - leftHeight)) &&
                            !room.BoundsContainDoor(new Vector2Int(topLeft.x - 1, topLeft.y - leftHeight - 1),
                                new Vector2Int(topLeft.x + 1, topLeft.y - leftHeight + 1))
                        ) leftHeight++;
                        else break;
                    }

                    int width = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        if (
                            room.Floor.Contains(new Vector2Int(topLeft.x + width, topLeft.y)) &&
                            !room.Floor.Contains(new Vector2Int(topLeft.x + width, topLeft.y + 1)) &&
                            !room.BoundsContainDoor(new Vector2Int(topLeft.x + width - 1, topLeft.y - 1),
                                new Vector2Int(topLeft.x + width + 1, topLeft.y + 1))
                        ) width++;
                        else break;
                    }

                    int rightHeight = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (
                            room.Floor.Contains(new Vector2Int(topLeft.x + width, topLeft.y - rightHeight)) &&
                            !room.Floor.Contains(new Vector2Int(topLeft.x + width - 1, topLeft.y - rightHeight)) &&
                            !room.BoundsContainDoor(new Vector2Int(topLeft.x + width - 1, topLeft.y - rightHeight - 1),
                                new Vector2Int(topLeft.x + width + 1, topLeft.y - rightHeight + 1))
                        ) rightHeight++;
                        else break;
                    }
                    //print($"width: {width}");
                    //print($"left: {leftHeight}");
                    //print($"right: {rightHeight}");

                    //apply tiles
                    if (width >= 4)
                    {
                        Vector3Int tilePosition = new Vector3Int(topLeft.x, topLeft.y, 0);
                        for (int i = 0; i < width; i++)
                        {
                            tilemap.SetTile(tilePosition + new Vector3Int(i, 0, 0), counterTile);
                        }

                        for (int i = 0; i < leftHeight; i++)
                        {
                            tilemap.SetTile(tilePosition + new Vector3Int(0, -i, 0), counterTile);
                        }

                        for (int i = 0; i < rightHeight; i++)
                        {
                            tilemap.SetTile(tilePosition + new Vector3Int(width, -i, 0), counterTile);
                        }
                    }
                }

                //place random amount of props
                for (int n = 0; n < Random.Range(PropAmountMin, PropAmountMax); n++)
                {
                    placedProp = false;
                    //pick a random tile
                    Vector2Int originPos = room.Floor[Random.Range(0, room.Floor.Count)];
                    Vector3Int tilePosition = new Vector3Int(originPos.x, originPos.y, 0);


                    switch (propType)
                    {
                        //couch
                        case PropType.Couch:
                            //if room already contains a couch
                            if (hasGeneratedCouch)
                            {
                                RandomizePropType();
                                break;
                            }
                            if (
                                //check floor space
                                IsAreaValid(room, originPos + new Vector2Int(-1, -1),
                                    originPos + new Vector2Int(1, 0)) &&
                                //make sure there is a wall above
                                !room.IsAreaFloor(originPos + new Vector2Int(-1, 1),
                                    originPos + new Vector2Int(1, 1)) &&
                                //make sure it is not connected to wall on both left and right side
                                (room.Floor.Contains(originPos + new Vector2Int(-2, 0)) ||
                                 room.Floor.Contains(originPos + new Vector2Int(2, 0))) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-2, -1),
                                    originPos + new Vector2Int(2, 1))
                            )
                            {
                                for (int x = -1; x <= 1; x++)
                                {
                                    for (int y = -1; y <= 0; y++)
                                    {
                                        tilemap.SetTile(tilePosition + new Vector3Int(x, y, 0), seatTile);
                                    }
                                }

                                RandomizePropType();
                                hasGeneratedCouch = true;
                            }

                            break;
                        //table
                        case PropType.Table:
                            if (
                                //check floor space
                                IsAreaValid(room, originPos, originPos + new Vector2Int(1, 0)) &&
                                //make sure it is next to a wall
                                !room.IsAreaFloor(originPos + new Vector2Int(-1, -1),
                                    originPos + new Vector2Int(2, 1)) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-1, -1),
                                    originPos + new Vector2Int(2, 1))
                            )
                            {
                                tilemap.SetTile(tilePosition, tableTile);
                                tilemap.SetTile(tilePosition + new Vector3Int(1, 0, 0), tableTile);
                                RandomizePropType();
                            }
                            else if (remainingAttempts > 0)
                            {
                                remainingAttempts--;
                                n--;
                            }

                            break;

                        //plant 1
                        case PropType.Plant1:
                            if (
                                //check floor space
                                IsAreaValid(room, originPos, originPos) &&
                                //make sure it is next to a wall
                                IsAdjacentToWall(room, originPos, originPos) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-1, -1),
                                    originPos + new Vector2Int(1, 1))
                            )
                            {
                                tilemap.SetTile(tilePosition, plantTile1);
                                tilemap.SetTile(tilePosition + new Vector3Int(0, 1, 0), plantTile1);
                                RandomizePropType();
                            }

                            break;

                        //plant 2
                        case PropType.Plant2:
                            if (
                                //check floor space
                                IsAreaValid(room, originPos, originPos) &&
                                //make sure it is next to a wall
                                IsAdjacentToWall(room, originPos, originPos) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-1, -1),
                                    originPos + new Vector2Int(1, 1))
                            )
                            {
                                tilemap.SetTile(tilePosition, plantTile2);
                                tilemap.SetTile(tilePosition + new Vector3Int(0, 1, 0), plantTile2);
                                RandomizePropType();
                            }

                            break;

                        //coffee cup
                        case PropType.CoffeeCup:
                            if (
                                //check floor space
                                IsAreaValid(room, originPos, originPos) &&
                                //make sure it is next to a wall
                                IsAdjacentToWall(room, originPos, originPos) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-1, -1),
                                    originPos + new Vector2Int(1, 1))
                            )
                            {
                                tilemap.SetTile(tilePosition, coffeeCupTile);
                                RandomizePropType();
                            }

                            break;
                        //beaker
                        case PropType.Beakers:
                            if (
                                //check floor space
                                IsAreaValid(room, originPos, originPos) &&
                                //make sure it is next to a wall
                                IsAdjacentToWall(room, originPos, originPos) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-1, -1),
                                    originPos + new Vector2Int(1, 1))
                            )
                            {
                                tilemap.SetTile(tilePosition, beakersTile);
                                RandomizePropType();
                            }

                            break;
                    }

                    if (!placedProp && remainingAttempts > 0)
                    {
                        remainingAttempts--;
                        n--;
                    }
                }
            }
        }

        private void ClearProps()
        {
            tilemap.ClearAllTiles();
        }
    }
}