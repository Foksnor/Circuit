using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridPositions
{
    public static List<GridCube> _GridCubes = new List<GridCube>();

    public static GridCube GetGridByPosition(Vector2 GridPosition)
    {
        GridCube result = _GridCubes.Find(x => x.Position == GridPosition);
        return result;
    }
}
