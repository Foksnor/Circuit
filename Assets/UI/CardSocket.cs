using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSocket : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        CardSelect.SelectedSocket = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CardSelect.SelectedSocket == this)
            CardSelect.SelectedSocket = null;
    }
}
