using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CardScriptableObject", menuName = "ScriptableObject/New Card")]
public class CardScriptableObject : ScriptableObject
{
    public _CardType CardType;
    public enum _CardType { Movement, Attack };
    public CardSequenceData ActionSequence;
    public _CardRarity CardRarity;
    public enum _CardRarity { Basic, Rare, Epic };
    public _CardStyle CardStyle;
    public enum _CardStyle { Basic, Foil, Golden };
    public string CardName;
    public int Cost;
    public Sprite Sprite;
    public AudioClip Sound;
    public Vector2Int MoveSteps;
    public int Value;
    public string TargetRequirement = "";
    public string Description = "";
    public float TimeInUse = 0.5f;
}
