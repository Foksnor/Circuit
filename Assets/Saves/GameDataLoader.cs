using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class GameDataLoader : MonoBehaviour
{
    private string filePath;

    private void Awake()
    {
        filePath = $"{Application.persistentDataPath}/savefile.json";

        // Check if the save file exists and is not empty
        if (File.Exists(filePath))
        {
            // Read the content of the file
            string fileContent = File.ReadAllText(filePath);

            // Check if the file content is not empty or just white spaces
            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                // The file has valid content, so load the game state
                LoadGameState();
            }
            else
            {
                // The file does not exist, so start a new game
                StartNewGame();
            }
        }
        else
        {
            // The file does not exist, so start a new game
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        TurnSequence.TransitionTurns.InitiateFirstTurn();
    }

    public void SaveGameState()
    {
        GameObjectDataCollection dataCollection = new GameObjectDataCollection();

        // Add Player Team Characters
        for (int i = 0; i < CharacterTeams._PlayerTeamCharacters.Count; i++)
        {
            dataCollection.characterDataList.Add(new CharacterData(CharacterTeams._PlayerTeamCharacters[i]));
        }

        // Add Enemy Team Characters
        for (int i = 0; i < CharacterTeams._EnemyTeamCharacters.Count; i++)
        {
            dataCollection.characterDataList.Add(new CharacterData(CharacterTeams._EnemyTeamCharacters[i]));
        }

        // Add Biome Chunks
        for (int i = 0; i < Grid.GridPositions.ActiveBiomeChunks.Count; i++)
        {
            dataCollection.biomeDataList.Add(new BiomeData(Grid.GridPositions.ActiveBiomeChunks[i]));
        }

        // Add only GridCubes that are effected by either status effects or surface effects
        for (int i = 0; i < Grid.GridPositions.GridCubes.Count; i++)
        {
            if (Grid.GridPositions.GridCubes[i].SurfaceType != _SurfaceType.None ||
                Grid.GridPositions.GridCubes[i].StatusType != _StatusType.None)
                dataCollection.gridCubeDataList.Add(new GridCubeData(Grid.GridPositions.GridCubes[i]));
        }

        // Convert the entire collection to JSON and save it
        string jsonData = JsonUtility.ToJson(dataCollection, true); // 'true' for pretty formatting
        System.IO.File.WriteAllText(filePath, jsonData);
    }

    public void LoadGameState()
    {
        if (File.Exists(filePath))
        {
            // Read the JSON file
            string jsonData = File.ReadAllText(filePath);

            // Deserialize the data back into your GameObjectDataCollection (CharacterDataCollection)
            GameObjectDataCollection dataCollection = JsonUtility.FromJson<GameObjectDataCollection>(jsonData);

            // Spawn biomes
            // This has to be called before the others load in so there's ground to alter and spawn characters on
            foreach (BiomeData biomeData in dataCollection.biomeDataList)
            {
                SpawnBiome(biomeData);
            }

            // Update changed grid cubes
            foreach (GridCubeData gridCubeData in dataCollection.gridCubeDataList)
            {
                UpdateGridCubes(gridCubeData);
            }

            // Spawn characters on the grid
            foreach (CharacterData characterData in dataCollection.characterDataList)
            {
                SpawnCharacter(characterData);
            }
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }

    public void DeleteGameState()
    {
        if (File.Exists(filePath))
        {
            // Delete the save file
            File.Delete(filePath);
            Debug.Log("Save file deleted.");
        }
    }

    private void SpawnCharacter(CharacterData characterData)
    {
        // Instantiate a new character from the prefab
        // Apply the loaded data to the instantiated character
        Character characterComponent = SpawnerFunctions.Instance.SpawnSpecificCharacterByName(characterData.GetName(), characterData.GetPosition());

        if (characterComponent != null)
        {
            characterComponent.Health = characterData.GetHealth();
            characterComponent.SetStatus(characterData.GetStatus());

            Debug.Log($"Spawned character: {characterComponent.name}, Health: {characterComponent.Health}, Status: {characterComponent.StatusType}");
        }
        else
        {
            Debug.LogError("Character component not found on the spawned prefab!");
        }
    }

    private void SpawnBiome(BiomeData biomeData)
    {
        SpawnerFunctions.Instance.InstantiateChunkByName(biomeData.GetName(), biomeData.GetBiomeID(), biomeData.GetPosition(), false);
    }

    private void UpdateGridCubes(GridCubeData gridCubeData)
    {
        GridCube gridCube = Grid.GridPositions.GetGridByPosition(gridCubeData.GetPosition());
        gridCube.UpdateGridCubeToSaveState(gridCubeData.Status, gridCubeData.Surface);
    }

    public void RestartScene()
    {
        // Get the current active scene and reload it
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
