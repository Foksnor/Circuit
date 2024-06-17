using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridPositions
{
    public static List<GridCube> _GridCubes = new List<GridCube>();
    public static Vector2 _GridSize = Vector2.zero;

    public static GridCube GetGridByPosition(Vector2 GridPosition)
    {
        GridCube result = _GridCubes.Find(x => x.Position == GridPosition);
        return result;
    }
}
