using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public static class HelperFunctions
{
    private static readonly float referenceResolutionWidth = 1920f;
    private static float lastResolutionScale = 0;

    // Magic numbers for distance and angle checks
    private const float DefaultClosestDistance = 99f;
    private const int DefaultAngleBreakpoint = 45;
    private const int MovementAngleBreakpoint = 8;

    // Cycles through every transform in a gameobject and sets the same layer throughout
    public static void SetGameLayerRecursive(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
        {
            SetGameLayerRecursive(child.gameObject, layer);
        }
    }

    // Only returns true if boths lists have the exact same cards AND order
    public static bool AreCardListsDifferent(List<Card> list1, List<Card> list2)
    {
        bool areListsDifferent = false;

        if (list1.Count != list2.Count)
            return false;

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i])
            {
                areListsDifferent = true;
            }
        }

        return areListsDifferent;
    }

    public static List<GridCube> GetVicinityGridCubes(GridCube startingCube, int radiusSize)
    {
        List<GridCube>vicinityCubes = new();
        // Double the range so it can be centered around the grid, and add 1 for the center point
        int diameterSize = radiusSize + radiusSize + 1;

        for (int x = -radiusSize; x < diameterSize; x++)
        {
            for (int y = -radiusSize; y < diameterSize; y++)
            {
                Vector2 nearbyPosition = new(startingCube.Position.x - x, startingCube.Position.y - y);
                GridCube nearbyCube = Grid.GridPositions.GetGridByPosition(nearbyPosition);
                if (nearbyCube != null)
                    vicinityCubes.Add(nearbyCube);
            }
        }

        return vicinityCubes;
    }

    // Shorter way of adding a triggerable to the turn sequence and filtering duplicates
    public static void AddToTurnTrigger(ITurnSequenceTriggerable triggerable)
    {
        if (!TurnSequence.TransitionTurns.TurnSequenceTriggerables.Contains(triggerable))
            TurnSequence.TransitionTurns.TurnSequenceTriggerables.Add(triggerable);
    }

    // Shorter way of removing a triggerable
    public static void RemoveFromTurnTrigger(ITurnSequenceTriggerable triggerable)
    {
        TurnSequence.TransitionTurns.TurnSequenceTriggerables.Remove(triggerable);
    }

    public static Vector3 GetDirectionVector(Vector2 startPos, Vector2 endPos)
    {
        Vector3 dir = startPos - endPos;
        Vector3 unitDir = Vector3.Normalize(dir);

        return unitDir;
    }

    public static Vector2 GetAveragePosition(List<Vector2Int> positions)
    {
        Vector2 totalVector = Vector2.zero;
        foreach (Vector2Int pos in positions)
        {
            totalVector += pos;
        }

        return totalVector / positions.Count;
    }

    public static float GetResolutionScale()
    {
        // Normalize scaling
        float resolutionScale = Screen.width / referenceResolutionWidth;

        return resolutionScale;
    }

    public static bool HasResolutionChanged()
    {
        if (lastResolutionScale != GetResolutionScale())
        {
            lastResolutionScale = GetResolutionScale();
            return true;
        }

        return false;
    }

    // Handles any adjustments to the target positions according to their _AutoTargetType
    public static List<Vector2Int> AdjustTargetPositions(
        Character instigator, List<Vector2Int> targetPositions, _CardAction action, _AutoTargetType autoTargetType, int maxRange)
    {
        switch (autoTargetType)
        {
            case _AutoTargetType.None:
                return targetPositions;

            case _AutoTargetType.ClosestSelfTeam:
                targetPositions = AdjustTowardsClosestTarget(instigator, targetPositions, action, maxRange, GetSelfTeam(instigator));
                break;

            case _AutoTargetType.ClosestAllyNoSelf:
                var teamWithoutSelf = GetSelfTeam(instigator);
                teamWithoutSelf.Remove(instigator);
                targetPositions = AdjustTowardsClosestTarget(instigator, targetPositions, action, maxRange, teamWithoutSelf);
                break;

            case _AutoTargetType.ClosestEnemyTeam:
                targetPositions = AdjustTowardsClosestTarget(instigator, targetPositions, action, maxRange, GetEnemyTeam(instigator));
                break;

            case _AutoTargetType.ClosestCorpse:
                targetPositions = AdjustTowardsClosestCorpse(instigator, targetPositions, maxRange);
                break;
        }

        return targetPositions;
    }

    // Adjusts the list of positions towards the closest target reference
    private static List<Vector2Int> AdjustTowardsClosestTarget(
        Character instigator, List<Vector2Int> targetPositions, _CardAction action, int maxRange, List<Character> targetTeam)
    {
        Vector3 closestTargetPosition = GetClosestTargetPosition(instigator, targetTeam, maxRange);
        return RotatePositionsTowardsTarget(instigator.transform.position, closestTargetPosition, action, targetPositions);
    }

    // Adjusts the list of positions towards the closest corpse
    private static List<Vector2Int> AdjustTowardsClosestCorpse(Character instigator, List<Vector2Int> targetPositions, int maxRange)
    {
        Vector3 closestCorpsePosition = GetClosestCorpsePosition(instigator, maxRange);
        if (closestCorpsePosition != Vector3.zero)
        {
            targetPositions = RotatePositionsTowardsTarget(instigator.transform.position, closestCorpsePosition, _CardAction.Move, targetPositions);
        }
        return targetPositions;
    }

    // Gets the closest position of a valid target team
    public static Vector3 GetClosestTargetPosition(Character instigator, List<Character> listOfCharacters, int maxRange)
    {
        Vector3 closestTargetPosition = Vector3.zero;
        float closestTargetDistance = 99;

        // Cycle through the list of characters and see who's closest
        foreach (Character character in listOfCharacters)
        {
            float dist = Vector3.Distance(instigator.transform.position, character.transform.position);
            if (dist <= closestTargetDistance && dist <= maxRange)
            {
                // Set closest position, this gets updated once the cycle found a match that's even closer
                closestTargetDistance = dist;
                closestTargetPosition = character.transform.position;
            }
        }

        return closestTargetPosition;
    }

    // Gets the closest position of a valid corpse
    private static Vector3 GetClosestCorpsePosition(Character instigator, int maxRange)
    {
        List<GridCube> vicinityCubes = GetVicinityGridCubes(instigator.AssignedGridCube, maxRange);
        float closestTargetDistance = DefaultClosestDistance;
        Vector3 closestCorpsePosition = Vector3.zero;

        foreach (GridCube cube in vicinityCubes)
        {
            if (cube.CorpseOnThisGrid != null)
            {
                float dist = Vector3.Distance(instigator.transform.position, cube.transform.position);
                if (dist < closestTargetDistance)
                {
                    closestTargetDistance = dist;
                    closestCorpsePosition = cube.CorpseOnThisGrid.transform.position;
                }
            }
        }

        return closestCorpsePosition;
    }

    public static List<Vector2Int> RotatePositionsTowardsTarget(Vector3 instigatorPosition, Vector3 targetPosition, _CardAction action, List<Vector2Int> targetPositions)
    {
        float angle = GetDirectionAngle(instigatorPosition, targetPosition);
        int angleBreakpoint = action == _CardAction.Move ? MovementAngleBreakpoint : DefaultAngleBreakpoint;

        // Check if angle is not front facing
        if (Mathf.Abs(angle) > angleBreakpoint)
        {
            if (angle < 0)
            {
                // Rotate right side
                targetPositions = RotatePositions(targetPositions, -90);
            }
            else
            {
                // Rotate left side
                targetPositions = RotatePositions(targetPositions, 90);
            }
        }

        return targetPositions;
    }

    public static float GetDirectionAngle(Vector3 instigatorPosition, Vector3 targetPosition)
    {
        // Calculates the angle between two grids
        // E.G. for changing the angle of the previs if needed
        float angle = Vector3.Angle(instigatorPosition - targetPosition, Vector3.up);

        // Vector3.Angle always returns an absolute number, so checking whether pos1 is left or right of pos2 to see if the angle needs to be set to a negative        
        Vector3 cross = instigatorPosition - targetPosition;
        if (cross.x > 0)
            angle = -angle;

        return angle;
    }

    public static List<Vector2Int> RotatePositions(List<Vector2Int> positions, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cosTheta = Mathf.Cos(angleRadians);
        float sinTheta = Mathf.Sin(angleRadians);

        // Round numbers
        return positions.ConvertAll(pos => new Vector2Int(
            Mathf.RoundToInt(pos.x * cosTheta - pos.y * sinTheta),
            Mathf.RoundToInt(pos.x * sinTheta + pos.y * cosTheta)
        ));
    }


    private static List<Character> GetSelfTeam(Character instigator)
    {
        return instigator.TeamType == _TeamType.Player ? Teams.CharacterTeams.PlayerTeamCharacters : Teams.CharacterTeams.EnemyTeamCharacters;
    }

    private static List<Character> GetEnemyTeam(Character instigator)
    {
        return instigator.TeamType == _TeamType.Player ? Teams.CharacterTeams.EnemyTeamCharacters : Teams.CharacterTeams.PlayerTeamCharacters;
    }

    public static readonly Dictionary<_CardAction, string> actionDescriptions = new()
    {
        { _CardAction.Damage, "Deal {x} <style=Highlight>damage</style> to each red indicator." },
        { _CardAction.Heal, "Restore {x} <style=Highlight>health</style> to a target." },
        { _CardAction.Move, "<style=Highlight>Move</style> {x} steps." },
        { _CardAction.DrawCard, "<style=Highlight>Draw</style> {x} cards." },
        { _CardAction.DiscardThisCard, "<style=Highlight>Discard</style> this card." },
        { _CardAction.DiscardOtherCard, "<style=Highlight>Discard</style> {x} random card" },
        { _CardAction.DestroyThisCard, "<style=Highlight>BURN</style> this card!<br>Remove this card from your deck." },
        { _CardAction.DestroyOtherCard, "<style=Highlight>BURN</style> {x} random card!<br>Remove that card from your deck." },
        { _CardAction.EnhanceSlotFire, "<style=Highlight>{x} fire</style><br>Enhances this slot with <style=Highlight>fire</style>. Cards played in this slot will use <style=Highlight>fire</style>." },
        { _CardAction.EnhanceSlotShock, "<style=Highlight>{x} shock</style><br>Enhances this <style=Highlight>shock</style> with shock. Cards played in this slot will use <style=Highlight>shock</style>." },
        { _CardAction.EnhanceSlotRetrigger, "<style=Highlight>{x} echo</style><br>Enhances this slot with <style=Highlight>echo</style>. Cards played in this slot will trigger twice." },
        { _CardAction.AddLife, "Add {x} life." },
        { _CardAction.SubtractLife, "Playing cost:<br>Subtract {x} life!" },
        { _CardAction.ConsumeCorpse, "Playing cost:<br>Consume {x} nearby corpse!" }
    };

    public static string FormatDescription(_CardAction action, int value)
    {
        if (!actionDescriptions.TryGetValue(action, out string description))
            return "";

        string actionTitle = GetIcon(action);

        // If no description, return only the action title
        if (string.IsNullOrWhiteSpace(description))
            return actionTitle;

        // Ensure {x} is styled correctly inside the sentence
        string styledValue = $"<style=Value>{value}</style>";
        string formattedDescription = description.Replace("{x}", styledValue);

        return $"{actionTitle} {formattedDescription}";
    }

    private static string GetIcon(_CardAction action)
    {
        return $"<sprite name=\"{action}\">";
    }

    private static readonly Dictionary<_CardAction, string> actionHexColors = new()
    {
        { _CardAction.Damage, "#FF0032" },
        { _CardAction.Heal, "#FFFFFF" },
        { _CardAction.Move, "#3C96C8" },
        { _CardAction.DrawCard, "#FFFFFF" },
        { _CardAction.DiscardThisCard, "#83ACA8" },
        { _CardAction.DiscardOtherCard, "#FFFFFF" },
        { _CardAction.DestroyThisCard, "#075951" },
        { _CardAction.DestroyOtherCard, "#FFFFFF" },
        { _CardAction.EnhanceSlotFire, "#F14A00" },
        { _CardAction.EnhanceSlotShock, "#F1D600" },
        { _CardAction.EnhanceSlotRetrigger, "#5E00F1" },
        { _CardAction.AddLife, "#FFFFFF" },
        { _CardAction.SubtractLife, "#FFFFFF" },
        { _CardAction.ConsumeCorpse, "#FFFFFF" }
    };

    public static Color GetActionColor(_CardAction action)
    {
        Color color = Color.white;

        if (actionHexColors.TryGetValue(action, out string hexColor))
        {
            if (UnityEngine.ColorUtility.TryParseHtmlString(hexColor, out Color newColor))
            {
                color = newColor;
            }
            else
            {
                Debug.LogError($"Invalid hex color for action {action}: {hexColor}");
            }
        }
        else
        {
            Debug.LogError($"Action {action} not found in dictionary!");
        }

        return color;
    }
}
