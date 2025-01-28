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
    public int EnhancementCharges { private set; get; } = 0;
    public _CardAction CurrentSlotEnhancement { private set; get; } = default;
    private Card slottedCard = null;

    // This is used when populating a card from loading a save file
    public bool SkipSlotDuringCardPopulation = false;

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
        _CardAction action = CurrentSlotEnhancement;

        if (EnhancementCharges > 0)
        {
            // Use a charge
            EnhancementCharges--;

            // Display how many charges are left
            enhancementTagDuration.text = EnhancementCharges.ToString();

            // If no charges are left, remove the slot enhancement
            if (EnhancementCharges <= 0)
                RemoveSlotEnhancement();
        }
        return action;
    }

    public void SetSlotEnhancement(_CardAction action, int amount)
    {
        if (CurrentSlotEnhancement == action)
        {
            // Add the charges if you re-apply the same slot enhancement
            EnhancementCharges += amount;
        }
        else
        {
            // Overwrite current slot enhancement
            CurrentSlotEnhancement = action;
            EnhancementCharges = amount;
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
        enhancementTagDuration.text = EnhancementCharges.ToString();
    }

    private void RemoveSlotEnhancement()
    {
        CurrentSlotEnhancement = default;
        enhancementTag.SetActive(false);
        enhancementImageTarget.material = null;
    }
}
