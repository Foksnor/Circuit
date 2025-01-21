using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSequenceData
{
    public GridSelector TargetRequirement;
    public List<CardActionData> Actions = new();
}