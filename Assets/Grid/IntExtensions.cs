using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IntExtensions
{
    public static int ConvertIntToIncrement(this ref int number)
    {
        // Used to make a number that is larger than 1 or -1 into 1 or -1.
        if (number != 0)
            return number > 0 ? number = 1 : number = -1;
        return number;
    }

    public static Vector2Int ConvertVector2IntToIncrement(this ref Vector2Int vector2)
    {
        // Used to make a number that is larger than 1 or -1 into 1 or -1.
        int numX = 0;
        int numY = 0;

        if (vector2.x != 0)
            numX = vector2.x > 0 ? numX = 1 : numX = -1;
        if (vector2.y != 0)
            numY = vector2.y > 0 ? numY = 1 : numY = -1;

        return new Vector2Int(numX, numY);
    }
}
