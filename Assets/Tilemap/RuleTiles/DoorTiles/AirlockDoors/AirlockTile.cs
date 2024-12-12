using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Doors/Airlock Door")]
public class AirlockTile : RuleTile<AirlockTile.Neighbor>
{
    // Define some constants for different rules
    public class Neighbor
    {
        public const int Floor = 1;
        public const int Wall = 2;
        public const int Void = 3;
        public const int Airlock = 4;
    }


    // Override the RuleMatch method to check tile types
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.Floor:
                return tile is FloorTile;
            case Neighbor.Wall:
                return tile is WallTile;
            case Neighbor.Airlock:
                return tile is AirlockTile;
            case Neighbor.Void:
                return tile is VoidTile || tile is null;
            default:
                return false;
        }
    }
}