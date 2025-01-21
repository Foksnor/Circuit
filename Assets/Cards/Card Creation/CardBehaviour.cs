using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardActions
{
    public static CardBehaviour Instance = null;
}

public class CardBehaviour
{
    private void Awake()
    {
        CardActions.Instance = this;
    }

    public void CallAction(_CardAction action)
    {
        switch (action)
        {
            case _CardAction.DrawCard:
                PlayerUI.HandPanel.DrawCards(1);
                break;
            case _CardAction.DiscardThisCard:
                break;
            case _CardAction.DiscardOtherCard:
                break;
            case _CardAction.DestroyThisCard:
                break;
            case _CardAction.DestroyOtherCard:
                break;
            case _CardAction.EnhanceSlotFire:
                break;
            case _CardAction.EnhanceSlotShock:
                break;
            case _CardAction.AddLife:
                break;
            case _CardAction.SubtractLife:
                break;
            case _CardAction.ConsumeCorpse:
                break;
        }
    }
}