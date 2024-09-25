using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridPositions
{
    public static List<GridCube> _GridCubes { get; set; } = new();
    public static List<BiomeChunk> _ActiveBiomeChunks { get; set; } = new();

    public static GridCube GetGridByPosition(Vector2 GridPosition)
    {
        GridCube result = _GridCubes.Find(x => x.Position == GridPosition);
        return result;
    }
}
