using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class CharacterSimulation : Character
{
    private void Awake()
    {
        // Character sim cannot be dealt damage
        //isInvulnerable = true;
    }

    public void SetCharacterSimInfo(SpriteRenderer spriteRenderer)
    {
        characterSpriteRenderer.sprite = spriteRenderer.sprite;
    }

    public override void RefreshCharacterSimulation()
    {    
    }

    public override void InstantiateCharacterSimulation()
    {    
    }
}
