using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValidateGridPosition
{
    public static bool CanStepX(int targetNum1, int targetNum2)
    {
        if (targetNum1 >= GridPositions._GridCubes.Count ||
            targetNum2 >= GridPositions._GridCubes.Count ||
            targetNum1 < 0 ||
            targetNum2 < 0)
            return false;

        GridCube pos1 = GridPositions._GridCubes[targetNum1];
        GridCube pos2 = GridPositions._GridCubes[targetNum2];

        if ((pos1.transform.position - pos2.transform.position).magnitude <= 1)
            return true;

        if (pos1.transform.position.y == pos2.transform.position.y &&
            pos1 != pos2)
            return true;

        return false;
    }

    public static bool CanStepY(int targetNum1, int targetNum2)
    {
        if (targetNum1 >= GridPositions._GridCubes.Count ||
            targetNum2 >= GridPositions._GridCubes.Count ||
            targetNum1 < 0 ||
            targetNum2 < 0)
            return false;

        GridCube pos1 = GridPositions._GridCubes[targetNum1];
        GridCube pos2 = GridPositions._GridCubes[targetNum2];

        if ((pos1.transform.position - pos2.transform.position).magnitude <= 1)
            return true;

        if (pos1.transform.position.y != pos2.transform.position.y &&
            pos1 != pos2)
            return true;

        return false;
    }

    public static bool CanAttack(int targetNum1, int targetNum2, int attackOffsetY)
    {
        if (targetNum1 >= GridPositions._GridCubes.Count ||
            targetNum2 >= GridPositions._GridCubes.Count ||
            targetNum1 < 0 ||
            targetNum2 < 0)
            return false;

        GridCube pos1 = GridPositions._GridCubes[targetNum1];
        GridCube pos2 = GridPositions._GridCubes[targetNum2];

        int yDiff = (int)Mathf.Abs(pos1.transform.position.y - pos2.transform.position.y);
        if (yDiff == attackOffsetY)
            return true;

        return false;
    }
}

