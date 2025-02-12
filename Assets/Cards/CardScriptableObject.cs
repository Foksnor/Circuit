using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum _CardType { Movement, Attack };
public enum _CardRarity { Basic, Rare, Epic };
public enum _CardFoiling { Basic, Holographic, Golden, Polychrome, Ghost, Galaxy, Factured };
public enum _AutoTargetType { None, ClosestSelfTeam, ClosestAllyNoSelf, ClosestEnemyTeam, ClosestCorpse };

[CreateAssetMenu(fileName = "CardScriptableObject", menuName = "ScriptableObject/New Card")]
public class CardScriptableObject : ScriptableObject
{
    public _CardType CardType;
    public _CardRarity CardRarity;
    public _CardFoiling CardFoiling;
    public _AutoTargetType AutoTargetType;
    [ShowIf("ShouldShowMaxRange")]
    public int MaxRange = 1;
    public CardSequenceData ActionSequence;
    public string CardName;
    public Sprite Sprite;
    public AudioClip Sound;
    public string Description = "";
    public float TimeInUse = 0.5f;

    private bool ShouldShowMaxRange()
    {
        return AutoTargetType != _AutoTargetType.None;
    }
}
