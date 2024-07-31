using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class CharacterSimulation : Character
{
    [SerializeField] private Animator VisualRootAnimator;
    private Character ownerOfThisSimulation;

    private void Awake()
    {
        // Character sim cannot be dealt damage
        //isInvulnerable = true;
    }

    protected override void Die(Character instigator)
    {
        ownerOfThisSimulation.isSimulationMarkedForDeath = true;
        base.Die(instigator);
    }

    public void SetCharacterSimInfo(Character owner, SpriteRenderer spriteRenderer)
    {
        ownerOfThisSimulation = owner;
        isSimulation = true;
        health = owner.Health;
        TeamType = owner.TeamType;
        CharacterSpriteRenderer.sprite = spriteRenderer.sprite;
    }

    public void ToggleCharacterSimHighlight(bool isHighlighted)
    {
        VisualRootAnimator.SetBool("isHighlighted", isHighlighted);
    }

    public override void RefreshCharacterSimulation()
    {    
    }

    public override void InstantiateCharacterSimulation()
    {    
    }
}
