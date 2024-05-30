using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
{
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
}
