using UnityEngine;

public class Cell
{
    public Vector2 position;

    public int fCost;
    public int gCost;
    public int hCost;

    public Vector2 connection;

    public bool walkable;

    public Cell(Vector2 pos, bool walkable)
    {
        position = pos;
        this.walkable = walkable;
    }
}
