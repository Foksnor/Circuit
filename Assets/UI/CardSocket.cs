using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSocket : MonoBehaviour
{
    [SerializeField] private Animator animator;
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
}
