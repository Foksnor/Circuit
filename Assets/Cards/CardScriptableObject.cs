using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CardScriptableObject", menuName = "ScriptableObject/New Card")]
public class CardScriptableObject : ScriptableObject
{
    public _CardType CardType;
    public enum _CardType { Movement, Attack};
    public string CardName;
    public int Cost;
    public Sprite Sprite;
    public AudioClip Sound;
    public GameObject Particle;
    public _ParticleLocation ParticleLocation; 
    public enum _ParticleLocation { OnSelf, OnDamageTiles, OnMovementTiles };
    public _TargetType TargetType;
    public enum _TargetType { InFrontOfPlayer, NearestEnemy };
    public Vector2Int MoveSteps;
    public Vector2Int AttackSteps;
    public bool AutoTargetNearest;
    public int AttackOffset;
    public int Value;
    public string Description;
    public float MaxTimeInUse = 0.5f;
}
