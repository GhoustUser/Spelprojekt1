using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Props/SeatTile")]
public class SeatTile : RuleTile<SeatTile.Neighbor>
{
    // Define some constants for different rules
    public class Neighbor
    {
        public const int Seat = 1;
        public const int Empty = 2;
    }


    // Override the RuleMatch method to check tile types
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.Seat:
                return tile is SeatTile;
            case Neighbor.Empty:
                return !(tile is SeatTile);
            default:
                return true;
        }
    }
}