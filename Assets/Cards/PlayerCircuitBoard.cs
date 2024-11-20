using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Runtime.Common.Extensions;

public class PlayerCircuitBoard : CircuitBoard
{
    [SerializeField] private List<CardScriptableObject> startingCardsInDeck = new List<CardScriptableObject>();
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFill;

    protected override void Awake()
    {
        base.Awake();
        Teams.CharacterTeams.SetPlayerKing(transform.GetComponent<Character>());
        Teams.CharacterTeams.SetPlayerCircuitboard(this);
    }

    public void SetUpPlayerDeck()
    {
        // Add starting cards to the player deck
        Decks.Playerdeck.TotalCardsInDeck.AddRange(startingCardsInDeck);
        Decks.Playerdeck.CurrentCardsInDeck.AddRange(startingCardsInDeck);
        Decks.Playerdeck.CurrentCardsInDeck.Shuffle();
    }

    public void AddCardFromSavefile(CardData cardData)
    {
        // Get the card scriptable object by the name of the card data
        CardScriptableObject cardScriptableObject = Decks.Playerdeck.AllPossibleAvailableCards.FirstOrDefault(card => card.name == cardData.GetName());
        
        // Add the card scriptable object to the correct zone of the card data
        Decks.Playerdeck.TotalCardsInDeck.Add(cardScriptableObject);
        switch (cardData.GetCardPlacement())
        {
            case _CardPlacement.Hand:
                Decks.Playerdeck.CurrentCardsInPlay.Add(cardScriptableObject);
                break;
            case _CardPlacement.Deck:
                Decks.Playerdeck.CurrentCardsInDeck.Add(cardScriptableObject);
                break;
            case _CardPlacement.Discard:
                Decks.Playerdeck.CurrentCardsInDiscard.Add(cardScriptableObject);
                break;
        }
    }

    public override void PlayerDrawPhase()
    {
    }

    public override void PlaceCardInSocket(Card newCard, CardSocket socket)
    {
        activeCards.Add(newCard);
        newCard.transform.SetParent(cardPanel.transform);
        socket.SlotCard(newCard);
        Decks.Playerdeck.CurrentCardsInPlay.Add(newCard.GetCardInfo());
        newCard.connectedSocket = socket;
        newCard.CardPointerInteraction.AssignPosition(socket.transform.position);

        // Enables reset of the card simulation
        needsNewCardCalculation = true;
    }

    public override void RemoveFromSocket(Card card)
    {
        // Make sure the correct card of the selected socket gets called (In case you have duplicates of the card reference slotted in other sockets)
        int index = activeSockets.IndexOf(card.connectedSocket);
        card.connectedSocket = null;
        Decks.Playerdeck.CurrentCardsInPlay.RemoveAt(index);
        activeCards.RemoveAt(index);
    }

    public override bool IsProcessingCards(Character targetCharacter)
    {
        bool isProcessing = base.IsProcessingCards(targetCharacter);
        return isProcessing;
    }

    public void UpdateCardOrder()
    {
        // Sort card order by comparing the x position, which can happen if the player drags on of their cards in the circuit board
        List<Card> CardOrderBeforeSort = new();
        CardOrderBeforeSort.AddRange(activeCards);
        activeCards.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // If any of the active cards has their position changed. A new card calculation is in order. This can happen during setup phase
        // Bool needsNewCardCalculation will set itself to false once the calculation action has ended
        if (!needsNewCardCalculation)
        {
            needsNewCardCalculation = HelperFunctions.AreCardListsDifferent(CardOrderBeforeSort, activeCards);
            TurnSequence.TransitionTurns.NeedsNewCardCalculation = needsNewCardCalculation;

            // Sets the hand in the playerdeck equal to the active cards in the circuitboard
            // Used for save/loading you current circuitboard cards
            Decks.Playerdeck.CurrentCardsInPlay.Clear();
            for (int i = 0; i < activeCards.Count; i++)
            {
                Decks.Playerdeck.CurrentCardsInPlay.Add(activeCards[i].GetCardInfo());
            }
        }
    }

    protected override void AllignCardOnCircuitBoard(int cardNumber)
    {
        // Sets the new position of the card to the connected socket
        Vector2 newCardPosition = activeCards[cardNumber].connectedSocket.transform.position;
        activeCards[cardNumber].CardPointerInteraction.AssignPosition(newCardPosition);
    }

    protected override void ToggleInteractableCardStateOnCircuitBoard(int cardNumber, bool isInteractable)
    {
        activeCards[cardNumber].CardPointerInteraction.SetInteractableState(isInteractable);
        activeSockets[cardNumber].ToggleSocketLock(isInteractable);
    }

    public override void SetCircuitDisplayTimer(string timerDisplayText, float timerDisplayFillAmount)
    {
        timerText.text = timerDisplayText;
        timerFill.fillAmount = timerDisplayFillAmount;
    }
}