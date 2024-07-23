using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawnpoint : MonoBehaviour
{
    [SerializeField] private Character characterToSpawnHere;
    [Range(1, 100)] public int appareanceChance = 50;
    private Rect gizmoRect;

    private void Awake()
    {
        if (UnityEngine.Random.Range(0, 100) <= appareanceChance)
            SpawnerFunctions.Instance.SpawnSpecificCharacter(characterToSpawnHere, transform.position, characterToSpawnHere.TeamType);
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
