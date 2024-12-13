using System.Collections.Generic;
using UnityEngine;

namespace LevelGen
{
    public enum RoomType
    {
        End = -2,
        Default = -1,
        Start = 0,
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
        Lounge = 1,
        Office = 2,
        LabMedium = 3,
        LabLarge = 4,
    }

    public class RoomRules
    {
        //how many connections each room type can have
        public static readonly int[] MaxConnections = new[]
        {
            2, //start
            4, //hallway
            2, //arena 1
            2, //arena 2
            2, //arena 3
            2, //lore room
            1, //reward room
        };
        
        //room colors
        public static readonly Color[] RoomGizmoColors = new[]
        {
            Color.white, //start
            Color.blue, //hallway
            Color.cyan, //arena 1
            Color.magenta, //arena 2
            Color.red, //arena 3
            Color.yellow, //lore room
            Color.green, //reward room
        };

        public static List<RoomType> ChooseRoomType(LevelMap map, int prevRoomId)
        {
            List<RoomType> roomTypes = new List<RoomType>();

            void AddToList(RoomType[] types)
            {
                foreach(var type in types) roomTypes.Add(type);
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
            
            switch (prevRoom.type)
            {
                /* -------- start room -------- */
                case RoomType.Start:
                    roomTypes.Add(RoomType.Arena1);
                    break;
                /* -------- hallway -------- */
                case RoomType.Hallway:
                    roomTypes.Add(RoomType.Arena1);
                    break;

                /* -------- small arena -------- */
                case RoomType.Arena1:
                    //if next to start room
                    if (prevRoomNeighborTypes.Contains(RoomType.Start))
                    {
                        roomTypes.Add(RoomType.Arena1);
                    }
                    //if next to medium arena
                    else if (prevRoomNeighborTypes.Contains(RoomType.Arena2))
                    {
                        AddToList(new RoomType[]
                        {
                            RoomType.Arena1, RoomType.Arena2, RoomType.Arena3
                        });
                    }
                    //if next to large arena
                    else if (prevRoomNeighborTypes.Contains(RoomType.Arena3))
                    {
                        AddToList(new RoomType[]
                        {
                            RoomType.Arena1, RoomType.Arena2, RoomType.LoreRoom, RoomType.RewardRoom
                        });
                    }
                    //default
                    else
                    {
                        AddToList(new RoomType[]
                        {
                            RoomType.Arena1, RoomType.Arena2, RoomType.Hallway
                        });
                    }
                    break;
                /* -------- medium arena -------- */
                case RoomType.Arena2:
                    AddToList(new RoomType[]
                    {
                        RoomType.Arena1, RoomType.LoreRoom, RoomType.RewardRoom
                    });
                    break;
                /* -------- large arena -------- */
                case RoomType.Arena3:
                    AddToList(new RoomType[]
                    {
                        RoomType.Arena1, RoomType.LoreRoom
                    });
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
            };
            return roomTypes;
        }
    }
}
