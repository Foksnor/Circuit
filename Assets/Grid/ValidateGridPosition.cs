using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValidateGridPosition
{
    public static bool CanStepX(Character targetCharacter, GridCube startingCube, GridCube destinationCube, bool checkForSimulation)
    {
        if (IsDestinationOccupiedByARelative(targetCharacter, startingCube, destinationCube, checkForSimulation))
            return false;

        if (IsDesitionationTooHigh(startingCube, destinationCube))
            return false;

        // Can move 1 step if destination is 1 step away
        if ((startingCube.Position - destinationCube.Position).magnitude <= 1)
            return true;

        // Can move horizontally when both positions share the same y positions
        if (startingCube.Position.y == destinationCube.Position.y &&
            startingCube != destinationCube)
            return true;

        return false;
    }

    public static bool CanStepY(Character targetCharacter, GridCube startingCube, GridCube destinationCube, bool checkForSimulation)
    {
        if (IsDestinationOccupiedByARelative(targetCharacter, startingCube, destinationCube, checkForSimulation))
            return false;

        if (IsDesitionationTooHigh(startingCube, destinationCube))
            return false;

        // Can move 1 step if destination is 1 step away
        if ((startingCube.Position - destinationCube.Position).magnitude <= 1)
            return true;

        // Can move vertically when both positions have different y positions
        if (startingCube.Position.y != destinationCube.Position.y &&
            startingCube != destinationCube)
            return true;

        return false;
    }

    public static bool CanAttack(GridCube startingCube, GridCube destinationCube, int attackOffsetY)
    {
        if (IsDesitionationTooHigh(startingCube, destinationCube))
            return false;
        return true;
    }

    private static bool IsDestinationOccupiedByARelative(Character targetCharacter, GridCube startingCube, GridCube destinationCube, bool checkForSimulation)
    {
        if (startingCube == null ||
            destinationCube == null)
            return true;

        if (!checkForSimulation)
        {
            // Cannot move when a character occupies destination
            if (destinationCube.CharacterOnThisGrid != null)
                if (!destinationCube.CharacterOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return true;
        }
        else if (targetCharacter.InstancedCharacterSimulation != null)
        {
            // Cannot move when a simulation occupies destination
            if (destinationCube.SimulationOnThisGrid != null)
                if (!destinationCube.SimulationOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                    return true;
        }

        return false;
    }

    private static bool IsDesitionationTooHigh(GridCube startingCube, GridCube destinationCube)
    {
        // Elevation in this project is determined by how far a gridcube goes to the front of the camera, which is a number that goes into the negative
        // A higher negative value means a higher elevation
        // Characters cannot walk up tiles that are higher than 1 meter
        if (Mathf.Abs(startingCube.Height - destinationCube.Height) >= 1 &&
            startingCube.Height >= destinationCube.Height)
            return true;
    
        return false; 
    }
}

