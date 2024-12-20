using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace LevelGen
{
    public enum RoomType
    {
        Default = -1,
        Start = 0,
        End,
        Hallway,
        Arena1,
        Arena2,
        Arena3,
        LoreRoom,
        RewardRoom,
    }

    public enum RoomStyle
    {
        Default = 0,
        Lab,
        Lounge,
    }

    public class RoomRules
    {
        //how many connections each room type can have
        public static readonly int[] MaxConnections = new[]
        {
            2, //start
            1, //end
            4, //hallway
            3, //arena 1
            2, //arena 2
            2, //arena 3
            2, //lore room
            1, //reward room
        };

        //what difficulty each room type has
        public static readonly int[] Difficulty = new[]
        {
            0, //start
            0, //end
            0, //hallway
            1, //arena 1
            3, //arena 2
            5, //arena 3
            0, //lore room
            0, //reward room
        };

        //what styles each room type can have
        public static readonly List<RoomStyle>[] StyleRules = new[]
        {
            //start
            new List<RoomStyle> { RoomStyle.Lab },
            //end
            new List<RoomStyle> { RoomStyle.Default },
            //hallway
            new List<RoomStyle> { RoomStyle.Lounge },
            //arena 1
            new List<RoomStyle> { RoomStyle.Lab, RoomStyle.Lounge },
            //arena 2
            new List<RoomStyle> { RoomStyle.Lab },
            //arena 3
            new List<RoomStyle> { RoomStyle.Lab },
            //lore room
            new List<RoomStyle> { RoomStyle.Lab },
            //reward room
            new List<RoomStyle> { RoomStyle.Default },
        };

        //room colors
        public static readonly Color[] RoomGizmoColors = new[]
        {
            Color.white, //start
            Color.gray, //end
            Color.blue, //hallway
            Color.cyan, //arena 1
            Color.magenta, //arena 2
            Color.red, //arena 3
            Color.yellow, //lore room
            Color.green, //reward room
        };

        public static List<RoomType> ChooseRoomType(LevelMap map, int prevRoomId)
        {
            //list of rooms that can be picked
            List<RoomType> roomTypes = new List<RoomType>();

            //local function for adding roomTypes to list to be picked from
            void AddToList(RoomType[] types)
            {
                foreach (var type in types) roomTypes.Add(type);
            }

            Room prevRoom = map.rooms[prevRoomId];
            List<RoomType> prevRoomNeighborTypes = new List<RoomType>();

            //check if previous room has max neighbors
            if (prevRoom.neighborIds.Count >= MaxConnections[(int)prevRoom.type])
            {
                return roomTypes;
            }

            //find previous rooms neighbors types
            foreach (int neighborId in prevRoom.neighborIds)
            {
                prevRoomNeighborTypes.Add(map.rooms[neighborId].type);
            }
            
            //list of all current room types
            List<RoomType> allRoomTypes = map.rooms.Select(obj => obj.type).ToList();

            //get path to previous room
            RoomPath path = new RoomPath();
            path.LoadPath(map, prevRoomId);

            //if max connections, make hallway
            if (prevRoomNeighborTypes.Count() >= MaxConnections[(int)prevRoom.type])
            {
                AddToList(new RoomType[]
                {
                    RoomType.Hallway
                });
                return roomTypes;
            }

            bool doDefaultCheck = true;

            /* -------- priority rules -------- */
            //lore room
            if (
                allRoomTypes.Count(obj => obj == RoomType.LoreRoom) < 4 &&
                path.Length >= 2 &&
                path.DistanceToType(RoomType.LoreRoom) > 2 &&
                path.Difficulty > 1
                )
            {
                roomTypes.Add(RoomType.LoreRoom);
                doDefaultCheck = false;
            }
            //reward room
            else if (
                allRoomTypes.Count(obj => obj == RoomType.RewardRoom) < 2 &&
                path.Length >= 2 &&
                path.DistanceToType(RoomType.RewardRoom) > 2 &&
                path.Difficulty > 2
                )
            {
                roomTypes.Add(RoomType.RewardRoom);
                doDefaultCheck = false;
            }
            //arena 3
            else if (
                allRoomTypes.Count(obj => obj == RoomType.Arena3) < 2 &&
                path.Length >= 4 &&
                path.DistanceToType(RoomType.Arena3) > 3 &&
                path.RoomTypes.Contains(RoomType.Arena2)
            )
            {
                roomTypes.Add(RoomType.Arena3);
                doDefaultCheck = false;
            }
            //arena 2
            else if (
                path.Length >= 2 &&
                path.DistanceToType(RoomType.Arena2) > 2 &&
                path.Difficulty > 1
            )
            {
                roomTypes.Add(RoomType.Arena2);
                doDefaultCheck = false;
            }

            if (!doDefaultCheck) return roomTypes;

            switch (prevRoom.type)
            {
                /* -------- start room -------- */
                case RoomType.Start:
                    roomTypes.Add(RoomType.Arena1);
                    break;
                /* -------- hallway -------- */
                case RoomType.Hallway:
                    AddToList(new RoomType[]
                    {
                        RoomType.Arena1, RoomType.Arena2
                    });
                    break;

                /* -------- small arena -------- */
                case RoomType.Arena1:
                    //can always lead to medium arena
                    AddToList(new RoomType[]
                    {
                        RoomType.Arena2
                    });
                    //if both previous rooms were not arena1, and difficulty of path is not too low
                    if (
                        !(prevRoom.type == RoomType.Arena1 && prevRoomNeighborTypes.Contains(RoomType.Arena1)) &&
                        (path.Length <= 3 || path.AverageDifficulty > 1)
                    )
                    {
                        AddToList(new RoomType[]
                        {
                            RoomType.Arena1
                        });
                    }

                    //if next to medium arena
                    if (prevRoomNeighborTypes.Contains(RoomType.Arena2))
                    {
                        AddToList(new RoomType[]
                        {
                            RoomType.Arena3
                        });
                    }
                    //if next to large arena
                    else if (prevRoomNeighborTypes.Contains(RoomType.Arena3))
                    {
                        AddToList(new RoomType[]
                        {
                            RoomType.Arena1, RoomType.Hallway
                        });

                        //if (allRoomTypes.Count(obj => obj == RoomType.LoreRoom) < 4) roomTypes.Add(RoomType.LoreRoom);
                    }

                    //if not next to hallway
                    if (!prevRoomNeighborTypes.Contains(RoomType.Hallway))
                    {
                        AddToList(new RoomType[]
                        {
                            RoomType.Hallway
                        });
                    }

                    break;
                /* -------- medium arena -------- */
                case RoomType.Arena2:
                    AddToList(new RoomType[]
                    {
                        RoomType.Arena1
                    });

                    //if (allRoomTypes.Count(obj => obj == RoomType.LoreRoom) < 4) roomTypes.Add(RoomType.LoreRoom);
                    break;
                /* -------- large arena -------- */
                case RoomType.Arena3:
                    AddToList(new RoomType[]
                    {
                        RoomType.Arena1
                    });

                    //if (allRoomTypes.Count(obj => obj == RoomType.LoreRoom) < 4) roomTypes.Add(RoomType.LoreRoom);
                    break;
                /* -------- lore room -------- */
                case RoomType.LoreRoom:
                    AddToList(new RoomType[]
                    {
                        RoomType.Arena1, RoomType.Arena2, RoomType.Arena3
                    });
                    break;
                /* -------- reward room -------- */
                case RoomType.RewardRoom:
                    roomTypes.Add(RoomType.Arena2);
                    break;
            }

            ;
            return roomTypes;
        }
    }

    public class RoomPath
    {
        //variables
        private List<RoomType> roomTypes = new List<RoomType>();
        private int difficulty;

        //properties
        public int Length => roomTypes.Count;
        public int Difficulty => difficulty;
        public float AverageDifficulty => difficulty / roomTypes.Count;
        public List<RoomType> RoomTypes => roomTypes;

        //functions
        public void LoadPath(LevelMap map, int roomId)
        {
            roomTypes.Clear();
            difficulty = 0;
            Room room = map.rooms[roomId];
            roomTypes.Add(room.type);

            //loop until first room is found, or limit is reached
            for (int i = 0; i < 100; i++)
            {
                //find previous room
                int previousId = -1;
                foreach (int neighborId in room.neighborIds)
                {
                    //find room with lower distance
                    if (map.rooms[neighborId].distanceFromStart < room.distanceFromStart)
                    {
                        previousId = neighborId;
                        break;
                    }
                }

                //when first room is reached, break the loop
                if (previousId == -1) break;

                //insert room type at the start of the list
                room = map.rooms[previousId];
                roomTypes.Insert(0, room.type);
            }

            //calculate total difficulty
            foreach (RoomType roomType in roomTypes)
            {
                difficulty += RoomRules.Difficulty[(int)roomType];
            }
        }

        public int DistanceToType(RoomType roomType)
        {
            for (int i = Length - 1; i >= 0; i--)
            {
                if (roomTypes[i] == roomType) return Length - 1 - i;
            }

            return Int32.MaxValue;
        }
    }
}