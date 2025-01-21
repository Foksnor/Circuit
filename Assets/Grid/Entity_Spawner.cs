using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Entity_Spawner : MonoBehaviour, ITurnSequenceTriggerable
{
    [SerializeField] private List<BiomeScriptableObject> biomes = new();
    [SerializeField] private Character player;
    private GridCube furthestGridCubeSpawned;
    [SerializeField] float distanceBetweenPlayerAndLastGridCubeBeforeNewChunkSpawns = 11;
    public List<Character> AllPossibleCharacters;

    // Player respawn
    private Vector3 savedPlayerSpawnLocation = Vector3.zero;
    private bool spawnPlayerAtEndTurn = false;

    private void Awake()
    {
        SpawnerFunctions.Instance = this;
    }

    public void InitiateFirstChunk()
    {
        // Spawn the first start area in the game
        BiomeChunk firstChunk = biomes[0].BiomeStartingChunk;
        InstantiateChunk(firstChunk, 0, Vector3.zero, true);
    }

    public void InstantiateChunkByName(string name, int biomeID, Vector3 spawnPosition, bool enableCharacterSpawners)
    {
        // Get both the chunks from the biome as well as the starting chunk
        // Add them both to a new list
        List<BiomeChunk> chunksFromBiomeID = new();
        chunksFromBiomeID = biomes[biomeID].BiomeChunks;
        chunksFromBiomeID.Add(biomes[biomeID].BiomeStartingChunk);

        // Use that list to search for a matching name in the chunks and then spawn that chunk
        BiomeChunk chunk = chunksFromBiomeID.FirstOrDefault(obj => obj.name == name);
        if (chunk != null)
            InstantiateChunk(chunk, biomeID, spawnPosition, enableCharacterSpawners);
        else
        {
            Debug.LogError($"Cannot find chunk '{name}' to instantiate!");
        }
    }

    public void InstantiateChunk(BiomeChunk chunk, int biomeID, Vector3 spawnPosition, bool enableCharacterSpawners)
    {
        BiomeChunk newChunk = Instantiate(chunk, spawnPosition, transform.rotation, transform);
        newChunk.name = chunk.name;
        newChunk.biomeID = biomeID;
        furthestGridCubeSpawned = newChunk.GetFurthestGridCube();
        if (enableCharacterSpawners)
            newChunk.SpawnCharactersInChunk();
    }

    private void Update()
    {
        // Spawn a new level chunk when a player character is nearing the end of a chunk
        if (Teams.CharacterTeams.PlayerTeamKing != null)
        {
            float dist = furthestGridCubeSpawned.Position.y - Teams.CharacterTeams.PlayerTeamKing.transform.position.y;
            if (dist <= distanceBetweenPlayerAndLastGridCubeBeforeNewChunkSpawns)
                InstantiateChunk(biomes[0].GetRandomChunk(), 0, furthestGridCubeSpawned.transform.position, true);
        }
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
        GridCube cubeCharacterSpawnsOnTopOff = Grid.GridPositions.GetGridByPosition(spawnPosition);
        c.transform.position = cubeCharacterSpawnsOnTopOff.transform.position;
        c.ChangeDestinationGrid(cubeCharacterSpawnsOnTopOff, 1);
        c.TeamType = teamType;
        switch (teamType)
        {
            case Character._TeamType.Player:
                Teams.CharacterTeams.PlayerTeamCharacters.Add(c);
                Teams.CharacterTeams.SetPlayerKingIfNoneActive(c);
                break;
            case Character._TeamType.Enemy:
                Teams.CharacterTeams.EnemyTeamCharacters.Add(c);
                break;
            case Character._TeamType.Neutral:
                Teams.CharacterTeams.EnemyTeamCharacters.Add(c);
                break;
        }
        return c;
    }

    public void SpawnPlayerAtEndTurn(Vector3 spawnPosition)
    {
        savedPlayerSpawnLocation = spawnPosition;
        spawnPlayerAtEndTurn = true;
        TurnSequence.TransitionTurns.TurnSequenceTriggerables.Add(this);
    }

    public void InstantiatePlayer()
    {
        // Remove character on this grid incase an enemy walked on the spawn location after killing previous player
        GridCube spawngrid = Grid.GridPositions.GetGridByPosition(savedPlayerSpawnLocation);
        spawngrid.KillCharacterOnGrid();

        Character c = SpawnSpecificCharacter(player, savedPlayerSpawnLocation, Character._TeamType.Player);
        c.PlaySummonAnimations();
    }

    public void SpawnCorpse(GameObject corpseType, GridCube gridpos)
    {
        // Only spawn a corpse when there is space for it
        if (gridpos.CorpseOnThisGrid == null)
        {
            GameObject corpse = Instantiate(corpseType);
            corpse.name = corpseType.name;
            gridpos.SetCorpseReferenceOnGrid(corpse);
            corpse.transform.position = gridpos.transform.position;
        }
    }


    // ITurnSequenceTriggerable interface
    public void OnStartPlayerTurn()
    {
    }

    public void OnStartEnemyTurn()
    {
    }

    public void OnEndstep()
    {
    }

    public void OnEndTurn()
    {
        // Player respawn
        if (spawnPlayerAtEndTurn)
        {
            InstantiatePlayer();
            spawnPlayerAtEndTurn = false;
        }
    }

    public void OnUpkeep()
    {
    }
}

public static class SpawnerFunctions
{
    public static Entity_Spawner Instance { get; set; } = null;
}