using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CardScriptableObject", menuName = "ScriptableObject/New Card")]
public class CardScriptableObject : ScriptableObject
{
    public _CardType CardType;
    public enum _CardType { Movement, Attack, ElementFire, ElementShock };
    public _CardRarity CardRarity;
    public enum _CardRarity { Basic, Rare, Epic };
    public _CardStyle CardStyle;
    public enum _CardStyle { Basic, Foil, Golden };
    public string CardName;
    public int Cost;
    public Sprite Sprite;
    public AudioClip Sound;
    public GameObject Particle;
    public _ParticleLocation ParticleLocation; 
    public enum _ParticleLocation { OnSelf, OnDamageTiles, OnMovementTiles };
    public _TargetType TargetType;
    public enum _TargetType { ForwardOfCharacter, BackwardOfCharacter, NearestAlly, NearestEnemy, Self };
    public Vector2Int MoveSteps;
    public Vector2Int AttackSteps;
    public int AttackOffset;
    public int Value;
    public string TargetRequirement = "";
    public string Description = "";
    public float MaxTimeInUse = 0.5f;
}
