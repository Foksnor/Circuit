using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeChunk : MonoBehaviour
{
    [Range(1, 100)] public int ChunkAppareanceChance = 10;
    [SerializeField] private GameObject grid;
    public int biomeID { get; set; } = 0;
    [SerializeField]
    private List<CharacterSpawnpoint> characterSpawnPoints = new();

    private void Awake()
    {
        // Add this chunk to the biome list, used for save/loading game state
        Grid.GridPositions.ActiveBiomeChunks.Add(this);
    }

    public void SpawnCharactersInChunk()
    {
        for (int i = 0; i < characterSpawnPoints.Count; i++)
            characterSpawnPoints[i].SpawnCharacter();
    }

    public GridCube GetFurthestGridCube()
    {
        GridCube lastCubeFromChunk = null;
        float dist = 0;
        foreach (Transform grid in grid.transform)
        {
            if (dist < grid.transform.position.y)
            {
                dist = grid.transform.position.y;
                lastCubeFromChunk = grid.GetComponent<GridCube>();
            }
        }
        return lastCubeFromChunk;
    }
}
