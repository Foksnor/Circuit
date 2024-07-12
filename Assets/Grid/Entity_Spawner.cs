using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Spawner : MonoBehaviour
{
    [SerializeField] private List<BiomeScriptableObject> biomes = new List<BiomeScriptableObject>();
    [SerializeField] private Character player;
    private GridCube furthestGridCubeSpawned;
    [SerializeField] float distanceBetweenPlayerAndLastGridCubeBeforeNewChunkSpawns = 11;
    [SerializeField] private Vector2 startingPosition;
    [SerializeField] private Character enemyWeakMelee, enemyWeakRanged;


    public void InitiateFirstChunk()
    {
        BiomeChunk firstChunk = biomes[0].GetStartingChunk();
        firstChunk = Instantiate(firstChunk, transform.position, transform.rotation, transform);
        furthestGridCubeSpawned = firstChunk.GetFurthestGridCube();
    }

    public void InitiateChunk(Vector3 previousLocation)
    {
        // Adds an offset to the y coordinate so that the new grid doesn't spawn right on top of the previous one
        previousLocation = new Vector3(0, previousLocation.y + 1, previousLocation.z);
        BiomeChunk chunk = biomes[0].GetRandomChunk();
        chunk = Instantiate(chunk, previousLocation, transform.rotation, transform);
        furthestGridCubeSpawned = chunk.GetFurthestGridCube();
    }

    private void Update()
    {
        float dist = furthestGridCubeSpawned.Position.y - player.transform.position.y;
        if (dist <= distanceBetweenPlayerAndLastGridCubeBeforeNewChunkSpawns)
            InitiateChunk(furthestGridCubeSpawned.transform.position);
    }

    public void AddRow(int rowAmount)
    {

    }

    public void SortCharacterHeight()
    {
    
    }

    public Character SpawnPlayer()
    {
        player = Instantiate(player);
        GridCube playerSpawnPos = GridPositions.GetGridByPosition(startingPosition);
        player.transform.position = playerSpawnPos.transform.position;
        player.ChangeDestinationGrid(playerSpawnPos, 1);
        player.TeamType = Character._TeamType.Player;
        return player;
    }

    public Character SpawnEnemy()
    {
        Character e = Instantiate(enemyWeakMelee);
        int rngRow = (int)Random.Range(1, 5);
        GridCube enemySpawnPos = GridPositions._GridCubes[GridPositions._GridCubes.Count - rngRow];
        e.transform.position = enemySpawnPos.transform.position;
        e.ChangeDestinationGrid(enemySpawnPos, 1);
        e.TeamType = Character._TeamType.Enemy;
        return e;
    }
}