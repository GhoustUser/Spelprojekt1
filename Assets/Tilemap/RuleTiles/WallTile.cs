using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Room/WallTile")]
public class WallTile : RuleTile<WallTile.Neighbor>
{
    // Define some constants for different rules
    public class Neighbor
    {
        public const int Floor = 1;
        public const int Wall = 2;
        public const int Void = 3;
    }


    // Override the RuleMatch method to check tile types
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.Floor:
                return tile is FloorTile || tile is AirlockTile;
            case Neighbor.Wall:
                return tile is WallTile;
            case Neighbor.Void:
                return tile is VoidTile || tile is null;
            default:
                return false;
        }
    }
}