using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Spawner : MonoBehaviour
{
    [SerializeField] private GridCube cubeDefault, cubeWater, ct1, ct2, ct3, ct4, ct5;
    public Vector2 gridSize = new Vector2(5, 15);
    [SerializeField] private Character player;
    [SerializeField] private Vector2 startingPosition;
    private Vector2 currentPositionForNewCube = Vector2.zero;
    [SerializeField] private Character enemyWeakMelee, enemyWeakRanged;

    private void Awake()
    {
        // Set grid size for static positions
        GridPositions._GridSize = gridSize;
    }

    public void InitiateGrid()
    {
        for (int gridLength = 0; gridLength < gridSize.y; gridLength++)
        {
            for (int gridWidth = 0; gridWidth < gridSize.x; gridWidth++)
            {
                currentPositionForNewCube = new Vector2(gridWidth, gridLength);
                SpawnCube(cubeDefault, currentPositionForNewCube);
            }
        }
        SortGrid();
    }

    public void AddRow(int rowAmount)
    {
        for (int i = rowAmount; i > 0; i--)
        {
            currentPositionForNewCube = new Vector2(currentPositionForNewCube.x, currentPositionForNewCube.y + 1);
            for (int gridWidth = 0; gridWidth < gridSize.x; gridWidth++)
            {
                currentPositionForNewCube = new Vector2(gridWidth, currentPositionForNewCube.y);
                SpawnCube(cubeDefault, currentPositionForNewCube);
            }
        }
        SortGrid();
    }

    public void SpawnCube(GridCube cubeType, Vector2 position)
    {
        GridPositions._GridCubes.Add(Instantiate(cubeType, position, transform.rotation, transform));
        cubeType.SetGridReferenceNumber(GridPositions._GridCubes.Count);
    }

    void SortGrid()
    {
        /*
        // Position cubes on the right spot of the grid
        for (int i = 0; i < GridPositions._GridPositions.Count; i++)
        {
            GridPositions._GridCubes[i].SetHeight(GridPositions._GridPositions[i]);
        }
        */
    }

    public void SortCharacterHeight()
    {
    
    }

    public Character SpawnPlayer()
    {
        Character p = Instantiate(player);
        GridCube playerSpawnPos = GridPositions.GetGridByPosition(startingPosition);
        p.transform.position = playerSpawnPos.transform.position;
        p.ChangeDestinationGrid(playerSpawnPos, 1);
        return p;
    }

    public Character SpawnEnemy()
    {
        Character e = Instantiate(enemyWeakMelee);
        int rngRow = (int)Random.Range(1, GridPositions._GridSize.x);
        GridCube enemySpawnPos = GridPositions._GridCubes[GridPositions._GridCubes.Count - rngRow];
        e.transform.position = enemySpawnPos.transform.position;
        e.ChangeDestinationGrid(enemySpawnPos, 1);
        return e;
    }
}