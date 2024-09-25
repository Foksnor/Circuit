using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity_Spawner : MonoBehaviour
{
    [SerializeField] private List<BiomeScriptableObject> biomes = new List<BiomeScriptableObject>();
    [SerializeField] private Character player;
    private GridCube furthestGridCubeSpawned;
    [SerializeField] float distanceBetweenPlayerAndLastGridCubeBeforeNewChunkSpawns = 11;
    [SerializeField] private Vector2 startingPosition;
    public List<Character> AllPossibleCharacters;

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

    public void InitiateChunkByName(string name, Vector3 sp)
    {
        // Make chunk spawn

        public BiomeChunk GetChunkByName(string name)
        {
            BiomeChunk chunk = BiomeChunks.FirstOrDefault(obj => obj.name == name);
            return chunk;
        }
    }

    public void InitiateChunk(BiomeChunk chunk, Vector3 spawnPosition)
    {
        // Adds an offset to the y coordinate so that the new grid doesn't spawn right on top of the previous one
        spawnPosition = new Vector3(0, spawnPosition.y + 1, spawnPosition.z);
        BiomeChunk newChunk = Instantiate(chunk, spawnPosition, transform.rotation, transform);
        newChunk.name = chunk.name;
        furthestGridCubeSpawned = chunk.GetFurthestGridCube();
    }

    private void Update()
    {
        float dist = furthestGridCubeSpawned.Position.y - player.transform.position.y;
        if (dist <= distanceBetweenPlayerAndLastGridCubeBeforeNewChunkSpawns)
            InitiateChunk(biomes[0].GetRandomChunk(), furthestGridCubeSpawned.transform.position);
    }

    public Character SpawnPlayer()
    {
        Character p = Instantiate(player);
        p.name = player.name;
        player = p;
        player.CircuitBoard.CalculateAllCards(player, false);
        GridCube cubePlayerSpawnsOnTopOff = GridPositions.GetGridByPosition(startingPosition);
        player.transform.position = cubePlayerSpawnsOnTopOff.transform.position;
        player.ChangeDestinationGrid(cubePlayerSpawnsOnTopOff, 1);
        CharacterTeams._PlayerTeamCharacters.Add(player);
        return player;
    }

    public Character SpawnEnemyAtLevelEdge()
    {
        return null;
        // TODO QQQQ: Spawn a random Weak Enemy
        /*
        Character e = Instantiate(enemyWeakMelee);
        e.name = enemyWeakMelee.name;
		e.CircuitBoard.CalculateAllCards(e, false);
        int rngRow = (int)Random.Range(1, 5);
        GridCube cubeEnemySpawnsOnTopOff = GridPositions._GridCubes[GridPositions._GridCubes.Count - rngRow];
        e.transform.position = cubeEnemySpawnsOnTopOff.transform.position;
        e.ChangeDestinationGrid(cubeEnemySpawnsOnTopOff, 1);
        CharacterTeams._EnemyTeamCharacters.Add(e);
        return e;
        */
    }

    public Character SpawnSpecificCharacterByName(string name, Vector2 spawnPosition)
    {
        Character character = AllPossibleCharacters.FirstOrDefault(obj => obj.name == name);
        character = SpawnSpecificCharacter(character, spawnPosition, character.TeamType);
        return character;
    }

    public Character SpawnSpecificCharacter(Character character, Vector2 spawnPosition, Character._TeamType teamType)
    {
        Character c = Instantiate(character);
        c.name = character.name;
        c.CircuitBoard.CalculateAllCards(c, false);
        GridCube cubeCharacterSpawnsOnTopOff = GridPositions.GetGridByPosition(spawnPosition);
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