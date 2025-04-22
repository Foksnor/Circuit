using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.Utilities;

public class SetRotationFromList : MonoBehaviour
{
    [SerializeField] private List<Vector2> rotations = new();
    [SerializeField] private Transform targetTransform = null;

    private void Awake()
    {
        if (targetTransform == null || rotations.IsNullOrEmpty())
            return;

        int rngFromList = Random.Range(0, rotations.Count - 1);
        float rotValue = Random.Range(rotations[rngFromList].x, rotations[rngFromList].y);
        targetTransform.eulerAngles = new Vector3(targetTransform.eulerAngles.x, targetTransform.eulerAngles.y, rotValue);
    }
}
