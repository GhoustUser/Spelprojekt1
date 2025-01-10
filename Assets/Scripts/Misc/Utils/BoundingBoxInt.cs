using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBoxInt : MonoBehaviour
{
    //position is bottom left corner
    private Vector2Int position, size;

    public int Top
    {
        get { return position.y + size.y; }
        set { position.y = value - size.y; }
    }
    public int Left
    {
        get { return position.x; }
        set { position.x = value; }
    }
    public int Bottom
    {
        get { return position.y; }
        set { position.y = value; }
    }
    public int Right
    {
        get { return position.x + size.x; }
        set { position.x = value - size.x; }
    }

    public int Width => size.x;
    public int Height => size.y;

    //constructor
    public BoundingBoxInt(int top, int left, int bottom, int right)
    {
        this.Top = top;
        this.Left = left;
        this.Bottom = bottom;
        this.Right = right;
    }
}
