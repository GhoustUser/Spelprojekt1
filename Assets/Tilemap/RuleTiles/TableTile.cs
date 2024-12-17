using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Props/TableTile")]
public class TableTile : RuleTile<TableTile.Neighbor>
{
    // Define some constants for different rules
    public class Neighbor
    {
        public const int Table = 1;
        public const int Empty = 2;
    }


    // Override the RuleMatch method to check tile types
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.Table:
                return tile is TableTile;
            case Neighbor.Empty:
                return !(tile is TableTile);
            default:
                return true;
        }
    }
}