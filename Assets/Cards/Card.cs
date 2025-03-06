using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Reactor.Animators;
using Unity.VisualScripting;

[RequireComponent(typeof(RectTransform))]
public class Card : MonoBehaviour
{
    public string CardId { get; private set; }
    private CardScriptableObject cardScriptableObject = null;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI targetIconPanelText;
    [SerializeField] private Image cardBackground, cardimage;
    [SerializeField] private Material holographicMat, goldenMat;
    [SerializeField] private AudioClip sound;
    [SerializeField] private Animator feedbackAnimator;
    public Animator cardAnimator;
    public Card_PointerInteraction CardPointerInteraction;

    // Tooltip
    [SerializeField] private CardTooltip cardTooltipPrefab;
    private CardTooltip activeCardTooltip;

    private RectTransform rectTransform;
    private Vector2 targetSelfDestructDestination;
    public float MaxTimeInUse = 0.5f;
    //public float MaxTimeInUse { get; private set; } = 0.5f;
    private bool isCardActivated { get; set; } = false;

    public bool IsCardVisible { get; private set; } = false;
    public CardSocket ConnectedSocket { get; private set; }
    public CircuitBoard ConnectedCircuitboard { get; private set; }

    public void SetCardInfo(CardScriptableObject scriptableObject, CircuitBoard owner, bool isVisible)
    {
        CardId = Guid.NewGuid().ToString();
        cardScriptableObject = scriptableObject;
        ConnectedCircuitboard = owner;
        IsCardVisible = isVisible;
        rectTransform = GetComponent<RectTransform>();

        if (isVisible)
        {
            nameText.text = cardScriptableObject.CardName;
            cardimage.sprite = cardScriptableObject.Sprite;
            PopulateCardActionIcons();

            // Sets the material for various images based on card parameters
            switch (scriptableObject.CardRarity)
            {
                case _CardRarity.Rare:
                    break;
                case _CardRarity.Epic:
                    break;
            }
            switch (scriptableObject.CardFoiling)
            {
                case _CardFoiling.Holographic:
                    cardBackground.material = holographicMat;
                    break;
                case _CardFoiling.Golden:
                    cardBackground.material = goldenMat;
                    break;
            }
        }
    }

    private void PopulateCardActionIcons()
    {
        // Empty placeholder string
        targetIconPanelText.text = string.Empty;

        if (cardScriptableObject.ActionSequence == null || cardScriptableObject.ActionSequence.Actions == null)
            return;

        // Populate icons for each Card Action
        foreach (CardActionData actionData in cardScriptableObject.ActionSequence.Actions)
        {
            // Ignore Card Actions that don't have a description
            if (HelperFunctions.actionDescriptions.ContainsKey(actionData.CardAction))
            {
                // Populate icon on text
                targetIconPanelText.text += HelperFunctions.GetCardActionIcon(actionData.CardAction);

                // Add spacing between each icons
                targetIconPanelText.text += " ";
            }
        }
    }

    public void ToggleCardTooltip(bool showTooltip)
    {
        if (showTooltip)
        {
            if (activeCardTooltip == null)
            {
                activeCardTooltip = Instantiate(cardTooltipPrefab, transform);
                activeCardTooltip.SetTooltip(cardScriptableObject, rectTransform);
            }
        }
        else
        {
            if (activeCardTooltip != null)
            {
                activeCardTooltip.RemoveToolTip();
            }
        }
    }

    public CardScriptableObject GetCardInfo()
    {
        return cardScriptableObject;
    }

    private void Update()
    {
        SetCardHighlight();
        CheckForSelfDestruct();
    }

    public void SetCardHighlight()
    {
        // Sets the card highlight on the circuitboard when card is played
        MaxTimeInUse -= Time.deltaTime;
        if (MaxTimeInUse < 0)
            cardAnimator.SetBool("isHighlighted", false);
        else
            cardAnimator.SetBool("isHighlighted", true);
    }

    public void ConnectToSocket(CardSocket socket)
    {
        ConnectedSocket = socket;
        socket.SlotCard(this);
    }

    public void RemoveFromSocket()
    {
        ConnectedSocket.SlotCard(null);
        ConnectedSocket = null;
    }

    public void ActivateCard(Character instigator)
    {
        isCardActivated = true;
        MaxTimeInUse = cardScriptableObject.TimeInUse;

        if (instigator != null)
        {
            List<CardActionData> actions = cardScriptableObject.ActionSequence.Actions;
            GridSelector targets = cardScriptableObject.ActionSequence.TargetRequirement;

            // Process all actions, including nested ActionSequences
            ProcessActions(instigator, this, actions, targets);
        }
    }

    public void ProcessActions(Character instigator, Card card, List<CardActionData> actions, GridSelector targets)
    {
        ExecuteActionSequence(instigator, card, actions, targets);
    }

    private void ExecuteActionSequence(Character instigator, Card card, List<CardActionData> actions, GridSelector targets)
    {
        foreach (CardActionData action in actions)
        {
            if (action.CardAction == _CardAction.ActionSequence && action.ActionSequence != null)
            {
                // Recursively process nested ActionSequences
                ExecuteActionSequence(instigator, card, action.ActionSequence.Actions, action.ActionSequence.TargetRequirement);
            }
            else
            {
                // Context based value as parameter
                object value;
                switch (action.CardAction)
                {
                    default:
                        value = action.Value;
                        break;
                    case _CardAction.SpawnParticleOnTarget:
                    case _CardAction.SpawnParticleOnSelf:
                        value = action.Particle;
                        break;
                }

                // Trigger the actual action
                CardActions.Instance.CallAction(instigator, card, action.CardAction, value, targets);
            }
        }
    }

    public void DeactivateCard()
    {
        isCardActivated = false;
    }

    public void SetSelfDestructWhenReachingTargetPosition(Vector2 targetPosition)
    {
        targetSelfDestructDestination = targetPosition;
    }

    private void CheckForSelfDestruct()
    {
        if (targetSelfDestructDestination == Vector2.zero)
        {
            return;
        }

        float distance = Vector2.Distance(targetSelfDestructDestination, rectTransform.anchoredPosition);

        if (distance <= 0.5f)
        {
            Destroy(gameObject, 0.1f);
        }
    }
}