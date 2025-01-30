using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardActions
{
    public static CardBehaviour Instance = null;
}

public class CardBehaviour : MonoBehaviour
{
    private HashSet<string> triggeredCards = new HashSet<string>();
    private _CardAction associatedEnhancement;

    private void Awake()
    {
        CardActions.Instance = this;
    }

    public bool CallAction(Character instigator, Card card, _CardAction action, object value, GridSelector targets)
    {
        if (value == null)
            Debug.LogError($"{instigator.name} tried to use {action} action. But the value hasn't been assigned yet.");
        List<Vector2Int> targetPositions = targets.RelativeSelectedPositions;

        // Targets are based on the facing direction the team is moving
        if (instigator.TeamType == _TeamType.Enemy)
        {
            // Invert each vector's direction
            for (int i = 0; i < targetPositions.Count; i++)
            {
                targetPositions[i] = -targetPositions[i];
            }
        }

        if (!triggeredCards.Contains(card.CardId))
        {
            // Use the enhancement associated with the connected socket
            associatedEnhancement = card.ConnectedSocket.UseSlotEnhancement();
            triggeredCards.Add(card.CardId);
        }
        ExecuteCardEnhancement(instigator, card, action, value, targets, associatedEnhancement);

        // ACTIONS THAT TRIGGER ON EACH TARGET LOCATION
        for (int i = 0; i < targetPositions.Count; i++)
        {
            // Cycle through every target location
            Vector2 targetPos = instigator.AssignedGridCube.Position + targetPositions[i];
            GridCube targetGrid = Grid.GridPositions.GetGridByPosition(targetPos);

            // Apply slot enhancement on terrain
            ExecuteTerrainEnhancement(instigator, targetGrid, associatedEnhancement);

            switch (action)
            {
                case _CardAction.Damage:
                    // Check for each location if attack can happen or not
                    if (ValidateGridPosition.CanAttack(instigator.AssignedGridCube, targetGrid))
                    {
                        if (targetGrid.CharacterOnThisGrid != null)
                            // No friendly damage allowed
                            if (targetGrid.CharacterOnThisGrid.TeamType != instigator.TeamType)
                                targetGrid.CharacterOnThisGrid.SubtractHealth((int)value, instigator);
                    }
                    break;
                case _CardAction.Heal:
                    if (targetGrid.CharacterOnThisGrid != null)
                        targetGrid.CharacterOnThisGrid.SubtractHealth(-(int)value, instigator);
                    break;
                case _CardAction.SpawnParticleOnTarget:
                    // Spawn particles on each target position
                    GameObject particle = Instantiate((GameObject)value, targetGrid.Position, instigator.transform.rotation);

                    // Sets the scale of the particle relative to the direction angle of the instigator
                    Vector3 dir = HelperFunctions.GetDirectionVector(targetPos, instigator.AssignedGridCube.Position);
                    particle.transform.localScale = new Vector3(dir.y, 1, 1); // x-axis get's flipped based on character y-axis face direction
                    break;
            }
        }

        // ACTIONS THAT TRIGGER ONLY ONCE
        switch (action)
        {
            case _CardAction.DrawCard:
                PlayerUI.HandPanel.DrawCards((int)value);
                break;
            case _CardAction.DiscardThisCard:
                card.ConnectedCircuitboard.DiscardedACard();
                card.ConnectedCircuitboard.RemoveFromSocket(card);
                PlayerUI.HandPanel.SentCardToDiscard(card);
                PlayerUI.PlayerCircuitboard.UpdateCardsInPlay();
                break;
            case _CardAction.DiscardOtherCard:
                break;
            case _CardAction.DestroyThisCard:
                break;
            case _CardAction.DestroyOtherCard:
                break;
            case _CardAction.EnhanceSlotFire:
            case _CardAction.EnhanceSlotShock:
            case _CardAction.EnhanceSlotRetrigger:
                card.ConnectedSocket.SetSlotEnhancement(action, (int)value);
                break;
            case _CardAction.AddLife:
                PlayerUI.LifePanel.AdjustLifeCount((int)value);
                break;
            case _CardAction.SubtractLife:
                PlayerUI.LifePanel.AdjustLifeCount(-(int)value);
                break;
            case _CardAction.ConsumeCorpse:
                List<GridCube> vicinityCubes = new();
                vicinityCubes.AddRange(HelperFunctions.GetVicinityGridCubes(instigator.AssignedGridCube, targets.MaxRange));
                for (int i = 0; i < vicinityCubes.Count; i++)
                    if (vicinityCubes[i].CorpseOnThisGrid != null)
                    {
                        Destroy(vicinityCubes[i].CorpseOnThisGrid);
                        return true;
                    }
                // If no corpse can be consumed, then the action can not be completed
                return false;
            case _CardAction.SpawnParticleOnSelf:
                // Spawn particle on self, with the scale converted to the direction based of the average target positions
                if (targets != null)
                {
                    GameObject particle = Instantiate((GameObject)value, instigator.AssignedGridCube.Position, instigator.transform.rotation);

                    // Sets the scale of the particle relative to the direction angle of the instigator
                    Vector3 averageTargetPos = instigator.AssignedGridCube.Position + HelperFunctions.GetAveragePosition(targets.RelativeSelectedPositions);
                    Vector3 dir = HelperFunctions.GetDirectionVector(averageTargetPos, instigator.AssignedGridCube.Position);
                    particle.transform.localScale = new Vector3(dir.y, 1, 1); // x-axis get's flipped based on character y-axis face direction
                }
                // Spawn particle on self, with the default scale
                else
                    Instantiate((GameObject)value, instigator.transform.position, instigator.transform.rotation);
                break;
        }
        return true;
    }

    private void ExecuteTerrainEnhancement(Character instigator, GridCube gridCube, _CardAction enhancement)
    {
        switch (enhancement)
        {
            case _CardAction.EnhanceSlotFire:
                gridCube.ToggleStatus(instigator, _StatusType.Fire, true);
                break;
            case _CardAction.EnhanceSlotShock:
                gridCube.ToggleStatus(instigator, _StatusType.Shocked, true);
                break;
            case _CardAction.EnhanceSlotRetrigger:
                break;
        }
    }

    private void ExecuteCardEnhancement(Character instigator, Card card, _CardAction action, object value, GridSelector targets, _CardAction enhancement)
    {
        switch (enhancement)
        {
            case _CardAction.EnhanceSlotRetrigger:
                CallAction(instigator, card, action, value, targets);
                break;
        }
    }

    public void ResetTriggeredCardsTracking()
    {
        triggeredCards.Clear();
    }
}