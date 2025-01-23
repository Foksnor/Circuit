using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSocket : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private int enhancementCharges = 0;
    private _CardAction currentSlotEnhancement;
    private Card slottedCard = null;

    public Card SlottedCard
    {
        get => slottedCard;
        set => slottedCard = value;
    }


    public void SlotCard(Card card)
    {
        SlottedCard = card;
    }

    public void ToggleSocketLock(bool isOpen)
    {
        animator.SetBool("isOpen", isOpen);
    }

    public _CardAction GetSlotEnhancement()
    {
        if (enhancementCharges > 0)
        {
            enhancementCharges--;
            return currentSlotEnhancement;
        }
        return _CardAction.Damage;
    }

    public void SetSlotEnhancement(_CardAction action, int amount)
    {
        // Overwrite current slot enhancement
        currentSlotEnhancement = action;
        enhancementCharges = amount;

        switch (action)
        {
            // Overwrite current charge count
            case _CardAction.EnhanceSlotFire:
                break;
            case _CardAction.EnhanceSlotShock:
                break;
        }
    }
}
