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
        bool cardExists = Decks.Playerdeck.AllPossibleAvailableCards.Any(card => card.name == cardData.GetName());

        if (!cardExists)
        {
            print($"forgot to add {cardData.GetName()} in the possible level rewards. Add it otherwise a save file cannot load the card in");
            return;
        }

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

    public override void PlaceCardInSocket(Card newCard, CardSocket socket)
    {
        int index = ActiveSockets.IndexOf(socket);
        print($"{socket.SlottedCard.GetCardInfo().name} got replaced by {newCard.GetCardInfo().name} at index {index}");
        newCard.transform.SetParent(cardPanel.transform);
        socket.SlotCard(newCard);
        newCard.connectedSocket = socket;
        newCard.CardPointerInteraction.AssignPosition(socket.transform.position);

        // Reset of the card calculation
        UpdateCardsInPlay();
    }

    public override void RemoveFromSocket(Card card)
    {
        card.connectedSocket = null;

        // Reset of the card calculation
        UpdateCardsInPlay();
    }

    public override bool IsProcessingCards(Character targetCharacter)
    {
        bool isProcessing = base.IsProcessingCards(targetCharacter);
        return isProcessing;
    }

    public void UpdateCardsInPlay()
    {
        // Sets CurrentCardsInPlay in the playerdeck equal to the active cards in the circuitboard
        // Used for save/loading you current circuitboard cards
        ActiveCards.Clear();
        Decks.Playerdeck.CurrentCardsInPlay.Clear();
        for (int i = 0; i < ActiveSockets.Count; i++)
        {
            if (ActiveSockets[i].SlottedCard != null)
            {
                Card card = ActiveSockets[i].SlottedCard;
                ActiveCards.Add(card);
                Decks.Playerdeck.CurrentCardsInPlay.Add(card.GetCardInfo());
            }
        }
    }

    protected override void AllignCardOnCircuitBoard(int cardNumber)
    {
        // Sets the new position of the card to the connected socket
        Vector2 newCardPosition = ActiveCards[cardNumber].connectedSocket.transform.position;
        ActiveCards[cardNumber].CardPointerInteraction.AssignPosition(newCardPosition);
    }

    protected override void ToggleInteractableCardStateOnCircuitBoard(int cardNumber, bool isInteractable)
    {
        ActiveCards[cardNumber].CardPointerInteraction.SetInteractableState(isInteractable);
        ActiveSockets[cardNumber].ToggleSocketLock(isInteractable);
    }

    public override void SetCircuitDisplayTimer(string timerDisplayText, float timerDisplayFillAmount)
    {
        timerText.text = timerDisplayText;
        timerFill.fillAmount = timerDisplayFillAmount;
    }
}