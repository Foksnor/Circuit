using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameDataLoader : MonoBehaviour
{
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
        for (int i = 0; i < GridPositions._ActiveBiomeChunks.Count; i++)
        {
            dataCollection.biomeDataList.Add(new BiomeData(GridPositions._ActiveBiomeChunks[i]));
        }

        // Add only GridCubes that are effected by either status effects or surface effects
        for (int i = 0; i < GridPositions._GridCubes.Count; i++)
        {
            if (GridPositions._GridCubes[i].SurfaceType != _SurfaceType.None ||
                GridPositions._GridCubes[i].StatusType != _StatusType.None)
                dataCollection.gridCubeDataList.Add(new GridCubeData(GridPositions._GridCubes[i]));
        }

        // Convert the entire collection to JSON and save it
        string jsonData = JsonUtility.ToJson(dataCollection, true); // 'true' for pretty formatting
        string filePath = filePath = $"{Application.persistentDataPath}/savefile.json";
        System.IO.File.WriteAllText(filePath, jsonData);
    }

    public void LoadGameState()
    {
        string filePath = filePath = $"{Application.persistentDataPath}/savefile.json";
        if (File.Exists(filePath))
        {
            // Read the JSON file
            string jsonData = File.ReadAllText(filePath);

            // Deserialize the data back into your GameObjectDataCollection (CharacterDataCollection)
            GameObjectDataCollection dataCollection = JsonUtility.FromJson<GameObjectDataCollection>(jsonData);

            // Spawn characters and apply the loaded data
            foreach (CharacterData characterData in dataCollection.characterDataList)
            {
                SpawnCharacter(characterData);
            }

            // Spawn biomes
            foreach (BiomeData biomeData in dataCollection.biomeDataList)
            {
                SpawnBiome(biomeData);
            }
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
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
        SpawnerFunctions.Instance.InitiateChunk(biomeData.GetPosition());
    }

    private void UpdateGridCubes(GridCubeData gridCubeData)
    {

    }
}
