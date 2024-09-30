using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPositions : MonoBehaviour
{
    public List<GridCube> GridCubes { get; set; }
    public List<BiomeChunk> ActiveBiomeChunks { get; set; }

    public void Awake()
    {
        Grid.GridPositions = this;
        GridCubes = new();
        ActiveBiomeChunks = new();
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
