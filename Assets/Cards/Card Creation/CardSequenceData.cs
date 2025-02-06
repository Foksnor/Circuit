using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSequenceData
{
    private static int serializationDepth = 0;
    private const int MAX_SERIALIZATION_DEPTH = 5;

    public List<CardActionData> Actions;
    public GridSelector TargetRequirement;

    public CardSequenceData()
    {
        if (serializationDepth >= MAX_SERIALIZATION_DEPTH)
        {
            // Prevent further recursion
            Actions = null; 
            return;
        }

        serializationDepth++;
    }

    ~CardSequenceData()
    {
        serializationDepth--;
    }
}