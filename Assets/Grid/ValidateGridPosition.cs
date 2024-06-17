using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValidateGridPosition
{
    public static bool CanStepX(Character targetCharacter, GridCube startingCube, GridCube destinationCube)
    {
        if (startingCube == null ||
            destinationCube == null)
            return false;

        bool checkForSimulation = false;
        if (targetCharacter.CharacterSimulation == null)
            checkForSimulation = true; // Target character is a simulation

        if (!checkForSimulation)
        {
            // Cannot move when a character occupies destination
            if (destinationCube.CharacterOnThisGrid != null)
                if (!destinationCube.CharacterOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return false;
        }
        else
        {
            // Cannot move when a simulation occupies destination
            if (destinationCube.SimulationOnThisGrid != null)
                if (!destinationCube.SimulationOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return false;
        }

        // Can move 1 step if destination is 1 step away
        if ((startingCube.transform.position - destinationCube.transform.position).magnitude <= 1)
            return true;

        // Can move horizontally when both positions share the same y positions
        if (startingCube.transform.position.y == destinationCube.transform.position.y &&
            startingCube != destinationCube)
            return true;

        return false;
    }

    public static bool CanStepY(Character targetCharacter, GridCube startingCube, GridCube destinationCube)
    {
        if (startingCube == null ||
            destinationCube == null)
            return false;

        bool checkForSimulation = false;
        if (targetCharacter.CharacterSimulation == null)
            checkForSimulation = true; // Target character is a simulation

        if (!checkForSimulation)
        {
            // Cannot move when a character occupies destination
            if (destinationCube.CharacterOnThisGrid != null)
                if (!destinationCube.CharacterOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return false;
        }
        else
        {
            // Cannot move when a simulation occupies destination
            if (destinationCube.SimulationOnThisGrid != null)
                if (!destinationCube.SimulationOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return false;
        }

        // Can move 1 step if destination is 1 step away
        if ((startingCube.transform.position - destinationCube.transform.position).magnitude <= 1)
            return true;

        // Can move vertically when both positions have different y positions
        if (startingCube.transform.position.y != destinationCube.transform.position.y &&
            startingCube != destinationCube)
            return true;

        return false;
    }

    public static bool CanAttack(GridCube startingCube, GridCube destinationCube, int attackOffsetY)
    {
        // QQQTODO: add checks to validate things like height, etc.
        return true;
    }
}

