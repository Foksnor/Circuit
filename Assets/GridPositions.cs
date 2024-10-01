using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPositions : MonoBehaviour
{
    public List<GridCube> GridCubes { get; set; } = new();
    public List<BiomeChunk> ActiveBiomeChunks { get; set; } = new();

    public void Awake()
    {
        Grid.GridPositions = this;
    }

    public void ResetPositions()
    {
        GridCubes.Clear();
        ActiveBiomeChunks.Clear();
    }

    public GridCube GetGridByPosition(Vector2 GridPosition)
    {
        GridCube result = GridCubes.Find(x => x.Position == GridPosition);
        return result;
    }
}

public static class Grid
{
    public static GridPositions GridPositions = null;
}
