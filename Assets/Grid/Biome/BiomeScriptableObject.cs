using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeScriptableObject", menuName = "ScriptableObject/New Biome")]

public class BiomeScriptableObject : ScriptableObject
{
    // List is populated through the inspector
    [SerializeField] private BiomeChunk BiomeStartingChunk;
    [SerializeField] private List<BiomeChunk> BiomeChunks = new List<BiomeChunk>();

    public BiomeChunk GetStartingChunk()
    {
        return BiomeStartingChunk;
    }

    public BiomeChunk GetChunkByName(string name)
    {
        BiomeChunk chunk = BiomeChunks.FirstOrDefault(obj => obj.name == name);
        return chunk;
    }

    public BiomeChunk GetRandomChunk()
    {
        // Add all odds to a totalroll number
        BiomeChunk chosenChunk = BiomeChunks[0];
        float totalRoll = 0;
        for (int i = 0; i < BiomeChunks.Count; i++)
        {
            totalRoll += BiomeChunks[i].ChunkAppareanceChance;
        }
        float rng = Random.Range(0, totalRoll);

        // That totalroll can be used for a dice roll to determine which chunk should be used
        float curChance = 0;
        for (int i = 0; i < BiomeChunks.Count; i++)
        {
            curChance += BiomeChunks[i].ChunkAppareanceChance;
            if (curChance > rng)
            {
                chosenChunk = BiomeChunks[i];
                return chosenChunk;
            }
        }
        return chosenChunk;
    }
}
