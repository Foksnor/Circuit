using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum _CardAction
{
    Damage, Heal,
    DrawCard, DiscardThisCard, DiscardOtherCard, DestroyThisCard, DestroyOtherCard,
    EnhanceSlotFire, EnhanceSlotShock, AddLife, SubtractLife, ConsumeCorpse,
    SpawnParticleOnField,
    ActionSequence
};

[System.Serializable]
public class CardActionData
{
    public _CardAction CardAction;

    [ShowIf("CardAction", _CardAction.SpawnParticleOnField)]
    public GameObject Particle;

    [HideIf("CardAction", _CardAction.ActionSequence)]
    public int Value;

    [ShowIf("CardAction", _CardAction.ActionSequence)]
    public CardSequenceData ActionSequence;
}