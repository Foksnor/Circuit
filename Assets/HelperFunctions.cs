using System.Collections;
using System.Collections.Generic;
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

    private static List<Vector2Int> AdjustTowardsClosestTarget(
        Character instigator, List<Vector2Int> targetPositions, _CardAction action, int maxRange, List<Character> targetTeam)
    {
        Vector3 closestTargetPosition = GetClosestTargetPosition(instigator, targetTeam, maxRange);
        return GetRotationAngleTowardsTarget(instigator, closestTargetPosition, action, targetPositions);
    }

    private static List<Vector2Int> AdjustTowardsClosestCorpse(Character instigator, List<Vector2Int> targetPositions, int maxRange)
    {
        Vector3 closestCorpsePosition = GetClosestCorpsePosition(instigator, maxRange);
        if (closestCorpsePosition != Vector3.zero)
        {
            targetPositions = GetRotationAngleTowardsTarget(instigator, closestCorpsePosition, _CardAction.Move, targetPositions);
        }
        return targetPositions;
    }

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

    public static List<Vector2Int> GetRotationAngleTowardsTarget(Character instigator, Vector3 closestTargetPosition, _CardAction action, List<Vector2Int> targetPositions)
    {
        // Calculates the angle between two grids, used for changing the angle of the previs if needed
        Vector3 pos1 = instigator.transform.position;
        Vector3 pos2 = closestTargetPosition;
        float angle = Vector3.Angle(pos1 - pos2, Vector3.up);

        // Vector3.Angle always returns an absolute number, so checking whether pos1 is left or right of pos2 to see if the angle needs to be set to a negative        
        Vector3 cross = pos1 - pos2;
        if (cross.x > 0)
            angle = -angle;

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
}
