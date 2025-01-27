using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "StatusEffectScriptableObject", menuName = "ScriptableObject/New Status Effect")]

public class StatusEffect_ScriptableObject : ScriptableObject
{
    public Sprite Icon { get { return icon; } private set { icon = value; } }
    [SerializeField] private Sprite icon = null;
    public int Damage { get { return damage; } private set { damage = value; } }
    [SerializeField] private int damage = 2;
    public int TurnDuration { get { return turnDuration; } private set { turnDuration = value; } }
    [SerializeField] private int turnDuration = 1;
    public GameObject IdleEffectParticle { get { return idleEffectParticle; } private set { idleEffectParticle = value; } }
    [SerializeField] private GameObject idleEffectParticle = null;
    public GameObject TriggeredEffectParticle { get { return triggeredEffectParticle; } private set { triggeredEffectParticle = value; } }
    [SerializeField] private GameObject triggeredEffectParticle = null;
}
