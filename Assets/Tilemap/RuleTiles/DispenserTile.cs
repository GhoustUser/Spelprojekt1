using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Props/DispenserTile")]
public class DispenserTile : RuleTile<DispenserTile.Neighbor>
{
    // Define some constants for different rules
    public class Neighbor
    {
        public const int Dispenser = 1;
        public const int Empty = 2;
    }


    // Override the RuleMatch method to check tile types
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.Dispenser:
                return tile is DispenserTile;
            case Neighbor.Empty:
                return !(tile is DispenserTile);
            default:
                return true;
        }
    }
}