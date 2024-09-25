using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawnpoint : MonoBehaviour
{
    [SerializeField] private BiomeChunk biomeChunk;
    // TODO QQQ: Gradualy spawn more enemies when difficulty should increase
    [SerializeField] private Character[] charactersToSpawnHere;
    [Range(1, 100)] public int appareanceChance = 50;
    [SerializeField] private List<Transform> positions = new List<Transform>();
    private Rect gizmoRect;

    private void Start()
    {
        // Only execute the spawn sequence if the biome chunk is verified and not spawned through a save load
        if (biomeChunk.IsSpawnedThroughLoadingSave)
            return;

        // If no positions are defined, default to the transform where this component is attached to
        if (positions.IsNullOrEmpty())
            positions.Add(transform);

        // Cycles through a rotation of spawn functions for the charactersToSpawnHere array
        for (int characterNumber = 0; characterNumber < charactersToSpawnHere.Length; characterNumber++)
        {
            // Ignore any positions that already have a character on top of it
            for (int i = 0; i < positions.Count; i++)
            {
                GridCube cubeCharacterSpawnsOnTopOff = GridPositions.GetGridByPosition(positions[i].position);

                // Remove this position from the list if (another) character is on top of the position already
                if (cubeCharacterSpawnsOnTopOff.CharacterOnThisGrid != null)
                    positions.RemoveAt(i);
            }

            // Select a random position from the list of positions
            int chosenPositionIndex = UnityEngine.Random.Range(1, positions.Count) - 1;

            // Spawn character if the odds meet at the selected position
            if (UnityEngine.Random.Range(0, 100) <= appareanceChance)
            {
                SpawnerFunctions.Instance.SpawnSpecificCharacter(charactersToSpawnHere[characterNumber], positions[chosenPositionIndex].position, charactersToSpawnHere[characterNumber].TeamType);
                // After spawning that character, remove that position from the available spawn points
                positions.RemoveAt(chosenPositionIndex);
            }
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Draws the sprite of the character as a gizmo on this transform
        if (!charactersToSpawnHere.IsNullOrEmpty())
        {
            gizmoRect = new Rect(transform.position, Vector2.zero);
            Gizmos.DrawGUITexture(gizmoRect, charactersToSpawnHere[0].CharacterSpriteRenderer.sprite.texture, 1, 1, 1, 1);
        }
    }
}
