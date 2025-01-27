using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSocket : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject enhancementTag;
    [SerializeField] private TextMeshProUGUI enhancementTagDuration;
    private int enhancementCharges = 0;
    private _CardAction currentSlotEnhancement;
    private Card slottedCard = null;

    // Slot enhancement materials
    [SerializeField] private Image enhancementImageTarget = null;
    [SerializeField] private Material enhancementFireMat, enhancementShockMat, enhancementRetriggerMat;

    public Card SlottedCard
    {
        get => slottedCard;
        set => slottedCard = value;
    }

    private void Awake()
    {
        RemoveSlotEnhancement();
    }

    public void SlotCard(Card card)
    {
        SlottedCard = card;
    }

    public void ToggleSocketLock(bool isOpen)
    {
        animator.SetBool("isOpen", isOpen);
    }

    public _CardAction UseSlotEnhancement()
    {
        if (enhancementCharges > 0)
        {
            // Use a charge
            enhancementCharges--;
            enhancementTagDuration.text = enhancementCharges.ToString();

            // If no charges are left, remove the slot enhancement
            if (enhancementCharges <= 0)
                RemoveSlotEnhancement();

            return currentSlotEnhancement;
        }
        return default;
    }

    public void SetSlotEnhancement(_CardAction action, int amount)
    {
        if (currentSlotEnhancement == action)
        {
            // Add the charges if you re-apply the same slot enhancement
            enhancementCharges += amount;
        }
        else
        {
            // Overwrite current slot enhancement
            currentSlotEnhancement = action;
            enhancementCharges = amount;
        }

        // Set slot materials
        switch (action)
        {
            case _CardAction.EnhanceSlotFire:
                enhancementImageTarget.material = enhancementFireMat;
                break;
            case _CardAction.EnhanceSlotShock:
                enhancementImageTarget.material = enhancementShockMat;
                break;
            case _CardAction.EnhanceSlotRetrigger:
                enhancementImageTarget.material = enhancementRetriggerMat;
                break;
        }

        // Updates the visual tag that is on the top side of a card socket
        enhancementTag.SetActive(true);
        enhancementTagDuration.text = enhancementCharges.ToString();
    }

    private void RemoveSlotEnhancement()
    {
        enhancementTag.SetActive(false);
        enhancementImageTarget.material = null;
    }
}
