using System.Collections.Generic;
using Unity.VisualScripting;
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
        CoffeeCup,
        CoffeeMachine,
        WaterDispenser,
    }

    public class Props : MonoBehaviour
    {
        //what props generate in each room type
        public static readonly List<PropType>[] PropRules = new[]
        {
            //default
            new List<PropType> { PropType.Plant1, PropType.CoffeeCup, PropType.WaterDispenser },
            //lab
            new List<PropType> { PropType.Plant1, PropType.Counter, PropType.Beakers, PropType.CoffeeCup },
            //lounge
            new List<PropType>
                { PropType.Couch, PropType.Table, PropType.Plant2, PropType.CoffeeCup, PropType.CoffeeMachine },
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
        public TableTile counterTileUL;
        public TableTile counterTileDR;
        public PlantTile plantTile1;
        public PlantTile plantTile2;
        public Tile beakersTile;
        public Tile coffeeCupTile;
        public DispenserTile coffeeMachineTile;
        public DispenserTile waterDispenserTile;


        /* -------- Variables --------*/
        private List<TileBase> tiles;

        /* -------- Events --------*/
        public static event LevelLoaded OnPropsLoaded = delegate { };

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
            // bool isAreaValid = true;

            for (int x = bottomLeft.x; x <= topRight.x; x++)
            {
                for (int y = bottomLeft.y; y <= topRight.y; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    if (tilemap != null && tilemap.GetTile(position) != null) return false;
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
            if (tilemap == null) tilemap = GetComponent<Tilemap>();

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
                    for (int c = 0; c < 3; c++)
                    {
                        Vector2Int counterOrigin = Vector2Int.zero;
                        for (int i = 0; i < 100; i++)
                        {
                            if (i == 99)
                            {
                                counterOrigin = Vector2Int.zero;
                                break;
                            }

                            counterOrigin = room.Floor[Random.Range(0, room.Floor.Count - 1)];

                            //check if tile is adjacent to a wall
                            int wallCount = room.WallCountInBounds(counterOrigin + new Vector2Int(-1, -1),
                                counterOrigin + new Vector2Int(1, 1));
                            if (wallCount <= 1) continue;

                            //check if tile is adjacent to a door
                            if (room.BoundsContainDoor(counterOrigin + new Vector2Int(-2, -2),
                                    counterOrigin + new Vector2Int(2, 2))) continue;
                            break;
                        }

                        if (counterOrigin != Vector2Int.zero)
                        {
                            List<bool> isUlOpen = new List<bool>(){true};
                            List<bool> isUlClosed = new List<bool>();
                            List<Vector2Int> openSet = new List<Vector2Int>() { counterOrigin };
                            List<Vector2Int> closedSet = new List<Vector2Int>() { };

                            for (int i = 0; i < Random.Range(4, 8) && openSet.Count > 0; i++)
                            {
                                closedSet.Add(openSet[0]);
                                isUlClosed.Add(isUlOpen[0]);
                                Vector2Int prevNode = openSet[0];
                                openSet.RemoveAt(0);
                                isUlOpen.RemoveAt(0);

                                for (int d = 0; d < TileManager.directions.Length; d++)
                                {
                                    Vector2Int newPos = prevNode + TileManager.directions[d];

                                    //check if tile is already in list
                                    if (openSet.Contains(newPos)) continue;
                                    if (closedSet.Contains(newPos)) continue;

                                    //check if tile is a floor tile
                                    if (!room.Floor.Contains(newPos)) continue;

                                    //check if tile is adjacent to a wall
                                    int wallCountUl = room.WallCountInBounds(newPos + new Vector2Int(-1, 0),
                                        newPos + new Vector2Int(0, 1));
                                    int wallCountDr = room.WallCountInBounds(newPos + new Vector2Int(0, -1),
                                        newPos + new Vector2Int(1, 0));
                                    if (wallCountUl + wallCountDr < 1) continue;

                                    bool ul = wallCountUl >= wallCountDr;

                                    //check if tile is adjacent to a door
                                    if (room.BoundsContainDoor(newPos + new Vector2Int(-2, -2),
                                            newPos + new Vector2Int(2, 2))) continue;

                                    openSet.Add(newPos);
                                    isUlOpen.Add(ul);
                                    if (isUlClosed.Count == 1) isUlClosed[0] = ul;
                                    if (closedSet.Count > 0 || openSet.Count > 1) break;
                                }
                            }

                            if (closedSet.Count >= 3)
                            {
                                for (int i = 0; i < closedSet.Count; i++)
                                {
                                    room.counterTops.Add(closedSet[i]);
                                    Vector3Int tilePosition = new Vector3Int(closedSet[i].x, closedSet[i].y, 0);
                                    tilemap.SetTile(tilePosition, isUlClosed[i] ? counterTileUL : counterTileDR);
                                }
                            }
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
                            if (tilemap == null) break;
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
                            if (tilemap == null) break;
                            if (
                                tilemap.GetTile(new Vector3Int(originPos.x, originPos.y, 0)) == null &&
                                tilemap.GetTile(new Vector3Int(originPos.x, originPos.y + 1, 0)) == null &&
                                //check floor space
                                room.IsAreaFloor(originPos, originPos) &&
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
                            if (tilemap == null) break;
                            if (
                                tilemap.GetTile(new Vector3Int(originPos.x, originPos.y, 0)) == null &&
                                tilemap.GetTile(new Vector3Int(originPos.x, originPos.y + 1, 0)) == null &&
                                //check floor space
                                room.IsAreaFloor(originPos, originPos) &&
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

                        //waterDispenser
                        case PropType.WaterDispenser:
                            if (tilemap == null) break;
                            if (
                                tilemap.GetTile(new Vector3Int(originPos.x, originPos.y, 0)) == null &&
                                tilemap.GetTile(new Vector3Int(originPos.x, originPos.y + 1, 0)) == null &&
                                //check floor space
                                room.IsAreaFloor(originPos, originPos) &&
                                //make sure it is next to a wall
                                IsAdjacentToWall(room, originPos, originPos) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-1, -1),
                                    originPos + new Vector2Int(1, 1))
                            )
                            {
                                tilemap.SetTile(tilePosition, waterDispenserTile);
                                tilemap.SetTile(tilePosition + new Vector3Int(0, 1, 0), waterDispenserTile);
                                RandomizePropType();
                            }

                            break;

                        //coffeeMachine
                        case PropType.CoffeeMachine:
                            if (tilemap == null) break;
                            if (
                                tilemap.GetTile(new Vector3Int(originPos.x, originPos.y, 0)) == null &&
                                tilemap.GetTile(new Vector3Int(originPos.x, originPos.y + 1, 0)) == null &&
                                //check floor space
                                room.IsAreaFloor(originPos, originPos) &&
                                //make sure it is next to a wall
                                IsAdjacentToWall(room, originPos, originPos) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-1, -1),
                                    originPos + new Vector2Int(1, 1))
                            )
                            {
                                tilemap.SetTile(tilePosition, coffeeMachineTile);
                                tilemap.SetTile(tilePosition + new Vector3Int(0, 1, 0), coffeeMachineTile);
                                RandomizePropType();
                            }

                            break;

                        //coffee cup
                        case PropType.CoffeeCup:
                            if (tilemap == null) break;
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
                            if (tilemap == null) break;
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
            OnPropsLoaded.Invoke(map);
        }

        private void ClearProps()
        {
            if (tilemap != null) tilemap.ClearAllTiles();
        }
    }
}