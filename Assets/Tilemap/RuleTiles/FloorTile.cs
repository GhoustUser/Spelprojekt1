using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/WallTile")]
public class FloorTile : RuleTile<FloorTile.Neighbor>
{
    // Define some constants for different rules
    public class Neighbor
    {
        public const int MyRule1 = 1;
        public const int MyRule2 = 2;
        public const int MyRule3 = 3;
    }


    // Override the RuleMatch method to check tile types
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        // Check if the neighboring tile is a wall
        if (tile is WallTile)
        {
            // Logic for wall
            return false;
        }
        // Check if the neighboring tile is a floor
        else if (tile is FloorTile)
        {
            // Logic for floor
            return true;
        }
        // Check if the neighboring tile is empty (null means no tile is placed here)
        else if (tile == null)
        {
            // Logic for empty tile
            return false;
        }

        // If none of the conditions are met, return false (default)
        return false;
    }
}