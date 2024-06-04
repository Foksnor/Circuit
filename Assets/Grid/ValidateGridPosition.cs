using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValidateGridPosition
{
    public static bool CanStepX(Character targetCharacter, int startingNum, int destinationNum)
    {
        if (startingNum >= GridPositions._GridCubes.Count ||
            destinationNum >= GridPositions._GridCubes.Count ||
            startingNum < 0 ||
            destinationNum < 0)
            return false;

        GridCube startingPos = GridPositions._GridCubes[startingNum];
        GridCube destinationPos = GridPositions._GridCubes[destinationNum];

        bool checkForSimulation = false;
        if (targetCharacter.CharacterSimulation == null)
            checkForSimulation = true; // Target character is a simulation

        if (!checkForSimulation)
        {
            // Cannot move when a character occupies destination
            if (destinationPos.CharacterOnThisGrid != null)
                if (!destinationPos.CharacterOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return false;
        }
        else
        {
            // Cannot move when a simulation occupies destination
            if (destinationPos.SimulationOnThisGrid != null)
                if (!destinationPos.SimulationOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return false;
        }

        // Can move 1 step if destination is 1 step away
        if ((startingPos.transform.position - destinationPos.transform.position).magnitude <= 1)
            return true;

        // Can move horizontally when both positions share the same y positions
        if (startingPos.transform.position.y == destinationPos.transform.position.y &&
            startingPos != destinationPos)
            return true;

        return false;
    }

    public static bool CanStepY(Character targetCharacter, int startingNum, int destinationNum)
    {
        if (startingNum >= GridPositions._GridCubes.Count ||
            destinationNum >= GridPositions._GridCubes.Count ||
            startingNum < 0 ||
            destinationNum < 0)
            return false;

        GridCube startingPos = GridPositions._GridCubes[startingNum];
        GridCube destinationPos = GridPositions._GridCubes[destinationNum];

        bool checkForSimulation = false;
        if (targetCharacter.CharacterSimulation == null)
            checkForSimulation = true; // Target character is a simulation

        if (!checkForSimulation)
        {
            // Cannot move when a character occupies destination
            if (destinationPos.CharacterOnThisGrid != null)
                if (!destinationPos.CharacterOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return false;
        }
        else
        {
            // Cannot move when a simulation occupies destination
            if (destinationPos.SimulationOnThisGrid != null)
                if (!destinationPos.SimulationOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return false;
        }

        // Can move 1 step if destination is 1 step away
        if ((startingPos.transform.position - destinationPos.transform.position).magnitude <= 1)
            return true;

        // Can move vertically when both positions have different y positions
        if (startingPos.transform.position.y != destinationPos.transform.position.y &&
            startingPos != destinationPos)
            return true;

        return false;
    }

    public static bool CanAttack(int startingNum, int destinationNum, int attackOffsetY)
    {
        if (startingNum >= GridPositions._GridCubes.Count ||
            destinationNum >= GridPositions._GridCubes.Count ||
            startingNum < 0 ||
            destinationNum < 0)
            return false;

        GridCube startingPos = GridPositions._GridCubes[startingNum];
        GridCube destinationPos = GridPositions._GridCubes[destinationNum];

        int yDiff = (int)Mathf.Abs(startingPos.transform.position.y - destinationPos.transform.position.y);
        if (yDiff == attackOffsetY)
            return true;

        return false;
    }
}

