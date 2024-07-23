using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Entity_Spawner : MonoBehaviour
{
    [SerializeField] private List<BiomeScriptableObject> biomes = new List<BiomeScriptableObject>();
    [SerializeField] private Character player;
    private GridCube furthestGridCubeSpawned;
    [SerializeField] float distanceBetweenPlayerAndLastGridCubeBeforeNewChunkSpawns = 11;
    [SerializeField] private Vector2 startingPosition;
    [SerializeField] private Character enemyWeakMelee, enemyWeakRanged;

    private void Awake()
    {
        SpawnerFunctions.Instance = this;
    }

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
        GridCube cubePlayerSpawnsOnTopOff = GridPositions.GetGridByPosition(startingPosition);
        player.transform.position = cubePlayerSpawnsOnTopOff.transform.position;
        player.ChangeDestinationGrid(cubePlayerSpawnsOnTopOff, 1);
        CharacterTeams._PlayerTeamCharacters.Add(player);
        return player;
    }

    public Character SpawnEnemyAtLevelEdge()
    {
        Character e = Instantiate(enemyWeakMelee);
        int rngRow = (int)Random.Range(1, 5);
        GridCube cubeEnemySpawnsOnTopOff = GridPositions._GridCubes[GridPositions._GridCubes.Count - rngRow];
        e.transform.position = cubeEnemySpawnsOnTopOff.transform.position;
        e.ChangeDestinationGrid(cubeEnemySpawnsOnTopOff, 1);
        CharacterTeams._EnemyTeamCharacters.Add(e);
        return e;
    }

    public Character SpawnSpecificCharacter(Character character, Vector2 position, Character._TeamType teamType)
    {
        Character c = Instantiate(character);
        GridCube cubeCharacterSpawnsOnTopOff = GridPositions.GetGridByPosition(position);
        c.transform.position = cubeCharacterSpawnsOnTopOff.transform.position;
        c.ChangeDestinationGrid(cubeCharacterSpawnsOnTopOff, 1);
        c.TeamType = teamType;
        switch (teamType)
        {
            case Character._TeamType.Player:
                CharacterTeams._PlayerTeamCharacters.Add(c);
                break;
            case Character._TeamType.Enemy:
                CharacterTeams._EnemyTeamCharacters.Add(c);
                break;
            case Character._TeamType.Neutral:
                CharacterTeams._EnemyTeamCharacters.Add(c);
                break;
        }
        return c;
    }
}

public static class SpawnerFunctions
{
    public static Entity_Spawner Instance { get; set; } = null;
}