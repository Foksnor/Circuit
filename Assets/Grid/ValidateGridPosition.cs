using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValidateGridPosition
{
    public static bool CanStep(Character targetCharacter, GridCube startingCube, GridCube destinationCube, int cardNumber)
    {
        if (startingCube == null ||
            destinationCube == null)
            return false;

        // Can move 1 step if destination is 1 step away
        if ((startingCube.Position - destinationCube.Position).magnitude > 1)
            return false;

        // QQQ TODO: add this later when you get to it
        // Cannot move when a different character has priority to move on the same grid during the same action cycle
        //if (!destinationCube.GetCharacterMovementPriority(targetCharacter, cardNumber))
        //    return false;

        if (IsDesitionationTooHigh(startingCube, destinationCube))
            return false;

        // Cannot move when there is a character that would still be alive at this part of the card sequence
        if (destinationCube.CharacterOnThisGrid != null)
            if (destinationCube.CharacterOnThisGrid.isPotentialKill)
                return true;

        if (IsDestinationOccupiedByARelative(targetCharacter, startingCube, destinationCube))
            return false;
        
        return true;
    }

    public static bool CanAttack(GridCube startingCube, GridCube destinationCube, int attackOffsetY)
    {
        if (IsDesitionationTooHigh(startingCube, destinationCube))
            return false;
        return true;
    }

    private static bool IsDestinationOccupiedByARelative(Character targetCharacter, GridCube startingCube, GridCube destinationCube)
    {
        // Cannot move when a character occupies destination
        if (destinationCube.CharacterOnThisGrid != null)
            if (!destinationCube.CharacterOnThisGrid.IsCharacterRelatedToMe(targetCharacter))
                return true;

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

