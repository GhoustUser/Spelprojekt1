using LevelGen;
using System.Collections.Generic;
using UnityEngine;

public class PremadeRoom
{
    public List<Wall> walls;
    public List<Vector2Int> floor;
    public List<Door> doors;

    public GameObject roomDecorations;
    
    public PremadeRoom (List<Wall> walls, List<Vector2Int> floor, List<Door> doors)
    {
        this.walls = walls;
        this.floor = floor;
        this.doors = doors;
    }
}

public class PremadeRoomHandler
{
    public static readonly Dictionary<RoomType, PremadeRoom> customRooms = new Dictionary<RoomType, PremadeRoom>() {
        { RoomType.RewardRoom,
            new PremadeRoom(new List<Wall>() { new Wall(new Vector2Int(6, 10)), new Wall(new Vector2Int(7, 10)), new Wall(new Vector2Int(8, 10)), new Wall(new Vector2Int(9, 10)), new Wall(new Vector2Int(10, 10)), new Wall(new Vector2Int(11, 10)), new Wall(new Vector2Int(12, 10)), new Wall(new Vector2Int(13, 10)), new Wall(new Vector2Int(4, 11)), new Wall(new Vector2Int(5, 11)), new Wall(new Vector2Int(6, 11)), new Wall(new Vector2Int(13, 11)), new Wall(new Vector2Int(3, 12)), new Wall(new Vector2Int(4, 12)), new Wall(new Vector2Int(13, 12)), new Wall(new Vector2Int(14, 12)), new Wall(new Vector2Int(15, 12)), new Wall(new Vector2Int(3, 13)), new Wall(new Vector2Int(4, 13)), new Wall(new Vector2Int(3, 14)), new Wall(new Vector2Int(13, 14)), new Wall(new Vector2Int(14, 14)), new Wall(new Vector2Int(15, 14)), new Wall(new Vector2Int(3, 15)), new Wall(new Vector2Int(13, 15)), new Wall(new Vector2Int(3, 16)), new Wall(new Vector2Int(4, 16)), new Wall(new Vector2Int(11, 16)), new Wall(new Vector2Int(12, 16)), new Wall(new Vector2Int(13, 16)), new Wall(new Vector2Int(4, 17)), new Wall(new Vector2Int(5, 17)), new Wall(new Vector2Int(6, 17)), new Wall(new Vector2Int(7, 17)), new Wall(new Vector2Int(8, 17)), new Wall(new Vector2Int(9, 17)), new Wall(new Vector2Int(10, 17)), new Wall(new Vector2Int(11, 17)) },
            new List<Vector2Int>() { new Vector2Int(7, 11), new Vector2Int(8, 11), new Vector2Int(9, 11), new Vector2Int(10, 11), new Vector2Int(11, 11), new Vector2Int(12, 11), new Vector2Int(5, 12), new Vector2Int(6, 12), new Vector2Int(7, 12), new Vector2Int(8, 12), new Vector2Int(9, 12), new Vector2Int(10, 12), new Vector2Int(11, 12), new Vector2Int(12, 12), new Vector2Int(5, 13), new Vector2Int(6, 13), new Vector2Int(7, 13), new Vector2Int(8, 13), new Vector2Int(9, 13), new Vector2Int(10, 13), new Vector2Int(11, 13), new Vector2Int(12, 13), new Vector2Int(14, 13), new Vector2Int(4, 14), new Vector2Int(5, 14), new Vector2Int(6, 14), new Vector2Int(7, 14), new Vector2Int(8, 14), new Vector2Int(9, 14), new Vector2Int(10, 14), new Vector2Int(11, 14), new Vector2Int(12, 14), new Vector2Int(4, 15), new Vector2Int(5, 15), new Vector2Int(6, 15), new Vector2Int(7, 15), new Vector2Int(8, 15), new Vector2Int(9, 15), new Vector2Int(10, 15), new Vector2Int(11, 15), new Vector2Int(12, 15), new Vector2Int(5, 16), new Vector2Int(6, 16), new Vector2Int(7, 16), new Vector2Int(8, 16), new Vector2Int(9, 16), new Vector2Int(10, 16) },
            new List<Door>() { new Door(new Vector2Int(13, 13)), new Door(new Vector2Int(15, 13))}) 
        }
    };
}
