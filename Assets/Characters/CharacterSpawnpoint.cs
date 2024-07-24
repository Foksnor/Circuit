using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawnpoint : MonoBehaviour
{
    // TODO QQQ: Gradualy spawn more enemies when difficulty should increase
    [SerializeField] private Character characterToSpawnHere;
    [Range(1, 100)] public int appareanceChance = 50;
    [SerializeField] private Transform[] positions;
    private Rect gizmoRect;

    private void Start()
    {
        // If no positions are defined, default to the transform where this component is attached to
        if (positions.IsNullOrEmpty())
        {
            positions = new Transform[1];
            positions[0] = transform;
        }

        // Select a random position
        int i = UnityEngine.Random.Range(1, positions.Length) - 1;

        // Spawn character if the odds meet at the selected position
        if (UnityEngine.Random.Range(0, 100) <= appareanceChance)
        {
            SpawnerFunctions.Instance.SpawnSpecificCharacter(characterToSpawnHere, positions[i].position, characterToSpawnHere.TeamType);
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        // Draws the sprite of the character as a gizmo
        if (characterToSpawnHere != null)
        {
            gizmoRect = new Rect(transform.position, Vector2.zero);
            Gizmos.DrawGUITexture(gizmoRect, characterToSpawnHere.CharacterSpriteRenderer.sprite.texture, 1, 1, 1, 1);
        }
    }
}
