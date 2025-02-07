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

    // Allows for enhancements or perks to have cards retrigger additional times
    private Dictionary<string, int> triggerCounts = new();
    private int BaseMaxTriggersPerTurn = 1;
    public int AdditionalRetriggers { get; private set; } = 0;
    public int MaxTriggersPerTurn => BaseMaxTriggersPerTurn + AdditionalRetriggers;

    private CardSocket savedCardSocket = null;
    private _CardAction savedEnhancement = default;

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

        // Track card trigger
        if (!triggeredCards.Contains(card.CardId))
        {
            if (card.ConnectedSocket != null)
            {
                // Save the card socket reference, in case the card gets discarded during the action, but still need the socket as reference for things like enhancements
                savedCardSocket = card.ConnectedSocket;

                // Use the enhancement associated with the connected socket
                savedEnhancement = savedCardSocket.UseSlotEnhancement(action);
                triggeredCards.Add(card.CardId);
            }
            else
                // QQQTODO: Needed as enemies don't have a socket
                savedCardSocket = null;
        }

        // ACTIONS THAT TRIGGER ON EACH TARGET LOCATION
        foreach (Vector2Int pos in targetPositions)
        {
            // Cycle through every target location
            Vector2 targetPos = instigator.AssignedGridCube.Position + pos;
            GridCube targetGrid = Grid.GridPositions.GetGridByPosition(targetPos);

            // Apply slot enhancement on terrain
            // QQQTODO: Needed as enemies don't have a socket
            if (savedCardSocket != null)
                ExecuteTerrainEnhancement(instigator, targetGrid, savedEnhancement);

            switch (action)
            {
                case _CardAction.Damage:
                    // Check for each location if attack can happen or not
                    // Needs a character to damage
                    // No friendly damamge allowed
                    if (ValidateGridPosition.CanAttack(instigator.AssignedGridCube, targetGrid) &&
                        targetGrid.CharacterOnThisGrid != null &&
                        targetGrid.CharacterOnThisGrid.TeamType != instigator.TeamType)
                    {
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
                // Only discard when max triggers are reached
                if (HasReachedMaxTriggers(card))
                    TriggerDiscard(card);
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
                savedCardSocket.SetSlotEnhancement(action, (int)value);
                break;
            case _CardAction.AddLife:
                PlayerUI.LifePanel.AdjustLifeCount((int)value);
                break;
            case _CardAction.SubtractLife:
                PlayerUI.LifePanel.AdjustLifeCount(-(int)value);
                break;
            case _CardAction.ConsumeCorpse:
                List<GridCube> vicinityCubes = HelperFunctions.GetVicinityGridCubes(instigator.AssignedGridCube, targets.MaxRange);
                foreach (var cube in vicinityCubes)
                {
                    if (cube.CorpseOnThisGrid != null)
                    {
                        Destroy(cube.CorpseOnThisGrid);
                        return true;
                    }
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

    private bool CardActionRequiredToTriggerOnlyOnce(_CardAction actionRequirement, Card card)
    {
        // Filters a unique card id with the action prefix, allowing this only to happen once
        // triggeredCards needs to be cleared at the end of round for this to work once a turn
        if (!triggeredCards.Contains($"{actionRequirement}-{card.CardId}"))
        {
            triggeredCards.Add($"{actionRequirement}-{card.CardId}");
            return true;
        }
        else
            return false;
    }

    private void TriggerDiscard(Card card)
    {
        card.ConnectedCircuitboard.DiscardedACard();
        card.ConnectedCircuitboard.RemoveFromSocket(card);
        PlayerUI.HandPanel.SentCardToDiscard(card, 0.5f);
        PlayerUI.PlayerCircuitboard.UpdateCardsInPlay();
    }

    public bool HasReachedMaxTriggers(Card card)
    {
        if (!triggerCounts.ContainsKey(card.CardId))
            triggerCounts[card.CardId] = 0;

        // Calculate all the bonus triggers
        // Using savedCardSocket as a reference, as a card can be discarded from a slot before this calculation happens in the same action sequence
        int bonusTriggers = savedCardSocket.GetSlotTriggers();

        return triggerCounts[card.CardId] >= MaxTriggersPerTurn + bonusTriggers;
    }

    public void IncrementTriggerCount(Card card)
    {
        if (!triggerCounts.ContainsKey(card.CardId))
            triggerCounts[card.CardId] = 0;

        triggerCounts[card.CardId]++;
    }

    public void DecrementTriggerCount(Card card)
    {
        if (!triggerCounts.ContainsKey(card.CardId))
            triggerCounts[card.CardId] = 0;

        triggerCounts[card.CardId]--;
    }

    public void ResetTriggeredCardsTracking()
    {
        triggeredCards.Clear();
        triggerCounts.Clear();
    }

    public void ApplyRetriggerPerk(int bonusRetriggers)
    {
        AdditionalRetriggers += bonusRetriggers;
    }
}