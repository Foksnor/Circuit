using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum _CardAction
{
    Damage, Heal,
    DrawCard, DiscardThisCard, DiscardOtherCard, DestroyThisCard, DestroyOtherCard,
    EnhanceSlotFire, EnhanceSlotShock, EnhanceSlotRetrigger,
    AddLife, SubtractLife, ConsumeCorpse,
    SpawnParticleOnTarget, SpawnParticleOnSelf,
    ActionSequence
};

[System.Serializable]
public class CardActionData
{
    public _CardAction CardAction;

    [HideIf("ShouldShowValue")]
    public int Value;

    [ShowIf("ShouldShowParticle")]
    public GameObject Particle;

    [ShowIf("CardAction", _CardAction.ActionSequence)]
    public CardSequenceData ActionSequence;

    private bool ShouldShowValue()
    {
        return CardAction == _CardAction.SpawnParticleOnSelf ||
            CardAction == _CardAction.SpawnParticleOnTarget ||
            CardAction == _CardAction.ActionSequence;
    }

    private bool ShouldShowParticle()
    {
        return CardAction == _CardAction.SpawnParticleOnSelf ||
            CardAction == _CardAction.SpawnParticleOnTarget;
    }
}