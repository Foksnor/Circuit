using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
{
    private static readonly float referenceResolutionWidth = 1920f;
    private static float lastResolutionScale = 0;

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

    public static List<Vector2Int> AdjustTargetPositions(Character instigator, List<Vector2Int> targetPositions, _CardAction action, _AutoTargetType autoTargetType, int maxRange)
    {
        List<Character> targetTeam;
        Vector3 closestTargetPosition;
        switch (autoTargetType)
        {
            case _AutoTargetType.None:
                break;
            case _AutoTargetType.ClosestSelfTeam:
                if (instigator.TeamType == _TeamType.Player)
                    targetTeam = Teams.CharacterTeams.PlayerTeamCharacters;
                else
                    targetTeam = Teams.CharacterTeams.EnemyTeamCharacters;

                closestTargetPosition = GetClosestTargetPosition(instigator, targetTeam, maxRange);
                targetPositions = GetRotationAngleTowardsTarget(instigator, closestTargetPosition, action, targetPositions);
                break;
            case _AutoTargetType.ClosestAllyNoSelf:
                if (instigator.TeamType == _TeamType.Player)
                    targetTeam = Teams.CharacterTeams.PlayerTeamCharacters;
                else
                    targetTeam = Teams.CharacterTeams.EnemyTeamCharacters;

                // Remove self from possible targets
                targetTeam.Remove(instigator);

                closestTargetPosition = GetClosestTargetPosition(instigator, targetTeam, maxRange);
                targetPositions = GetRotationAngleTowardsTarget(instigator, closestTargetPosition, action, targetPositions);
                break;
            case _AutoTargetType.ClosestEnemyTeam:
                if (instigator.TeamType == _TeamType.Player)
                    targetTeam = Teams.CharacterTeams.EnemyTeamCharacters;
                else
                    targetTeam = Teams.CharacterTeams.PlayerTeamCharacters;

                closestTargetPosition = GetClosestTargetPosition(instigator, targetTeam, maxRange);
                targetPositions = GetRotationAngleTowardsTarget(instigator, closestTargetPosition, action, targetPositions);
                break;
            case _AutoTargetType.ClosestCorpse:
                // Get a list of all the cubes in the range of the card action
                List<GridCube> vicinityCubes = GetVicinityGridCubes(instigator.AssignedGridCube, maxRange);
                float closestTargetDistance = 99;
                GameObject closestCorpse;

                // Cycle through that list and assign closest corpse
                foreach(GridCube cube in vicinityCubes)
                {
                    if (cube.CorpseOnThisGrid != null)
                    {
                        float dist = Vector3.Distance(instigator.transform.position, cube.transform.position);
                        if (dist <= closestTargetDistance)
                        {
                            // Set target corpse as the closest, this gets updated once the cycle found a match that's even closer
                            closestTargetDistance = dist;
                            closestCorpse = cube.CorpseOnThisGrid;
                        }
                    }
                }
                break;
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

        // Default angle breakpoint to check if something is on the left or right of the instigator is 45 degrees
        int angleBreakpoint = 45;

        // Magic number for the angle breakpoint for movement is 8 degrees
        if (action == _CardAction.Move)
            angleBreakpoint = 8;

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
        List<Vector2Int> rotatedPositions = new List<Vector2Int>();
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cosTheta = Mathf.Cos(angleRadians);
        float sinTheta = Mathf.Sin(angleRadians);

        foreach (Vector2Int pos in positions)
        {
            int x = Mathf.RoundToInt(pos.x * cosTheta - pos.y * sinTheta);
            int y = Mathf.RoundToInt(pos.x * sinTheta + pos.y * cosTheta);
            rotatedPositions.Add(new Vector2Int(x, y));
        }

        return rotatedPositions;
    }
}
