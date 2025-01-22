using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardActions
{
    public static CardBehaviour Instance = null;
}

public class CardBehaviour : MonoBehaviour
{
    private void Awake()
    {
        CardActions.Instance = this;
    }

    public bool CallAction(Character instigator, _CardAction action, object value, GridSelector targets)
    {
        switch (action)
        {
            case _CardAction.Damage:
                for (int i = 0; i < targets.RelativeSelectedPositions.Count; i++)
                {
                    Vector2 targetPos = instigator.AssignedGridCube.Position + targets.RelativeSelectedPositions[i];
                    GridCube targetGrid = Grid.GridPositions.GetGridByPosition(targetPos);
                    if (ValidateGridPosition.CanAttack(instigator.AssignedGridCube, targetGrid))
                    {
                        if (targetGrid.CharacterOnThisGrid != null)
                            targetGrid.CharacterOnThisGrid.SubtractHealth((int)value, instigator);
                    }
                }
                break;
            case _CardAction.Heal:
                for (int i = 0; i < targets.RelativeSelectedPositions.Count; i++)
                {
                    Vector2 targetPos = instigator.AssignedGridCube.Position + targets.RelativeSelectedPositions[i];
                    GridCube targetGrid = Grid.GridPositions.GetGridByPosition(targetPos);
                    if (targetGrid.CharacterOnThisGrid != null)
                        targetGrid.CharacterOnThisGrid.SubtractHealth(-(int)value, instigator);
                }
                break;
            case _CardAction.DrawCard:
                PlayerUI.HandPanel.DrawCards((int)value);
                break;
            case _CardAction.DiscardThisCard:
                break;
            case _CardAction.DiscardOtherCard:
                break;
            case _CardAction.DestroyThisCard:
                break;
            case _CardAction.DestroyOtherCard:
                break;
            case _CardAction.EnhanceSlotFire:
                break;
            case _CardAction.EnhanceSlotShock:
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
            case _CardAction.SpawnParticleOnTarget:
                // Spawn particles on each target position
                for (int i = 0; i < targets.RelativeSelectedPositions.Count; i++)
                {
                    Vector2 targetPos = instigator.AssignedGridCube.Position + targets.RelativeSelectedPositions[i];
                    GridCube targetGrid = Grid.GridPositions.GetGridByPosition(targetPos);

                    GameObject particle = Instantiate((GameObject)value, targetGrid.Position, instigator.transform.rotation);

                    // Sets the scale of the particle relative to the direction angle of the instigator
                    Vector3 dir = HelperFunctions.GetDirectionVector(targetPos, instigator.AssignedGridCube.Position);
                    particle.transform.localScale = new Vector3(dir.y, 1, 1); // x-axis get's flipped based on character y-axis face direction
                }
                break;
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
}