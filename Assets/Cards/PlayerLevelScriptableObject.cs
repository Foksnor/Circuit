using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlayerLevelScriptableObject", menuName = "ScriptableObject/New Level")]
public class PlayerLevelScriptableObject : ScriptableObject
{
    public int unlocksLevel = 1;
    public float experienceRequirement = 1;
    public CardScriptableObject[] possibleNewCardRewards;
}
