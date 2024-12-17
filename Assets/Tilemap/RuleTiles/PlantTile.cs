using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Props/PlantTile")]
public class PlantTile : RuleTile<PlantTile.Neighbor>
{
    // Define some constants for different rules
    public class Neighbor
    {
        public const int Plant = 1;
        public const int Empty = 2;
    }


    // Override the RuleMatch method to check tile types
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.Plant:
                return tile is PlantTile;
            case Neighbor.Empty:
                return !(tile is PlantTile);
            default:
                return true;
        }
    }
}