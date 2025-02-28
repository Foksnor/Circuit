using Doozy.Runtime.Reactor.Animators;
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
    public _CardAction LastSlotEnhancement { private set; get; } = default;
    private Card slottedCard = null;

    // This is used when populating a card from loading a save file
    public bool SkipSlotDuringCardPopulation = false;

    // Slot enhancement materials
    [SerializeField] private Image enhancementImageTarget = null;
    [SerializeField] private Material enhancementFireMat, enhancementShockMat, enhancementRetriggerMat;

    public Card SlottedCard
    {
        get => slottedCard;
        private set => slottedCard = value;
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

    public _CardAction UseSlotEnhancement(_CardAction action)
    {
        // If the used action is the same as the slot enhancement, don't consume a slot charge
        if (action == CurrentSlotEnhancement)
            return action;

        if (EnhancementCharges > 0)
        {
            // Save the slot
            action = CurrentSlotEnhancement;

            // Use a charge
            EnhancementCharges--;

            // Display how many charges are left
            enhancementTagDuration.text = EnhancementCharges.ToString();

            // If no charges are left, remove the slot enhancement
            if (EnhancementCharges <= 0)
                RemoveSlotEnhancement();

            // Return enhancement value
            return action;
        }

        return default;
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
        }

        // Plays an animation on the socket when it gets enhanced
        animator.Play("GetsEnhanced");

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

    public int GetSlotTriggers()
    {
        int bonusTriggers = 0;

        // Add slot unique buffs here

        return bonusTriggers;
    }
}
