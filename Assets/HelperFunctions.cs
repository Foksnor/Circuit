using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
{
    public static readonly float referenceResolutionWidth = 1920f;

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
}
