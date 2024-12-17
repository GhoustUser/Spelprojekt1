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
    }
    
    public class Props : MonoBehaviour
    {
        public static readonly List<PropType>[] PropRules = new[]
        {
            //default
            new List<PropType> { PropType.Plant1 },
            //lab
            new List<PropType> { PropType.Plant1 },
            //lounge
            new List<PropType> { PropType.Couch, PropType.Plant2 },
        };
        
        /* -------- Settings --------*/
        [Header("Settings")]
        [Tooltip("minimum amount of props that will attempt to generate per room")] [SerializeField]
        [Range(0, 10)]private int PropAmountMin = 2;
        [Tooltip("maximum amount of props that will attempt to generate per room")] [SerializeField]
        [Range(0, 20)]private int PropAmountMax = 5;
        [Tooltip("Amount of times it will attempt to place props again after failing")] [SerializeField]
        [Range(0, 500)]private int AttemptsPerRoom = 100;
        
        
        /* -------- Object references --------*/
        private Tilemap tilemap;
        [Header("RuleTiles")]
        public SeatTile seatTile;
        public TableTile tableTile;
        public PlantTile plantTile1;
        public PlantTile plantTile2;


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
        private void GenerateProps(LevelMap map)
        {
            foreach (Room room in map.rooms)
            {
                bool hasGeneratedCouch = false;
                
                //place n amount of tiles
                int remainingAttempts = AttemptsPerRoom;
                
                //select random prop type
                PropType propType;
                void RandomizePropType() {propType = PropRules[(int)room.style][Random.Range(0, PropRules[(int)room.style].Count)];}
                RandomizePropType();
                
                //place random amount of props
                for (int n = 0; n < Random.Range(PropAmountMin, PropAmountMax); n++)
                {
                    //pick a random tile
                    Vector2Int originPos = room.Floor[Random.Range(0, room.Floor.Count)];
                    Vector3Int tilePosition = new Vector3Int(originPos.x, originPos.y, 0);


                    switch (propType)
                    {
                        //couch
                        case PropType.Couch:
                            //if room already contains a couch
                            if (hasGeneratedCouch) break;
                            if (
                                //check floor space
                                room.IsAreaFloor(originPos + new Vector2Int(-1, -1), originPos + new Vector2Int(1, 0)) &&
                                //make sure there is a wall above
                                !room.IsAreaFloor(originPos + new Vector2Int(-1, 1), originPos + new Vector2Int(1, 1)) &&
                                //make sure it is not connected to wall on both left and right side
                                (room.Floor.Contains(originPos + new Vector2Int(-2,0)) || room.Floor.Contains(originPos + new Vector2Int(2,0))) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-2, -1), originPos + new Vector2Int(2, 1))
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
                                room.IsAreaFloor(originPos, originPos) &&
                                //make sure it is next to a wall
                                !room.IsAreaFloor(originPos + new Vector2Int(-1, -1), originPos + new Vector2Int(1, 1)) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-1, -1), originPos + new Vector2Int(1, 1))
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
                                room.IsAreaFloor(originPos, originPos) &&
                                //make sure it is next to a wall
                                !room.IsAreaFloor(originPos + new Vector2Int(-1, -1), originPos + new Vector2Int(1, 1)) &&
                                //make sure it is not blocking a door
                                !room.BoundsContainDoor(originPos + new Vector2Int(-1, -1), originPos + new Vector2Int(1, 1))
                            )
                            {
                                tilemap.SetTile(tilePosition, plantTile2);
                                tilemap.SetTile(tilePosition + new Vector3Int(0, 1, 0), plantTile2);
                                RandomizePropType();
                            }

                            break;
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