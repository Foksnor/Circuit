using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Runtime.Common.Extensions;

public static class PlayerUI
{
    public static PlayerHandPanel HandPanel = null;
    public static GameObject CardPanel = null;
    public static GameObject SocketPanel = null;
    public static PlayerCircuitBoard PlayerCircuitboard { get; set; } = null;
}

public class PlayerCircuitBoard : CircuitBoard
{
    [SerializeField] private List<CardScriptableObject> startingCardsInDeck = new List<CardScriptableObject>();
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFill;

    protected override void Awake()
    {
        PlayerUI.PlayerCircuitboard = this;
        PlayerUI.CardPanel = cardPanel;
        PlayerUI.SocketPanel = socketPanel;

        base.Awake();
        LinkPlayerboard();
    }

    protected override void SetUpCircuitBoard(List<CardScriptableObject> cardList)
    {
        // Adds card slots
        for (int i = 0; i < cardList.Count; i++)
        {
            Card newCard = Instantiate(card, cardPanel.transform);
            ActiveCards.Add(newCard);

            AddSocket();

            // Sets the new socket reference to the card
            newCard.ConnectToSocket(ActiveSockets[i]);

            // Sets the card info per card in the circuit board
            newCard.SetCardInfo(cardList[i], this, false);
        }
    }

    private void AddSocket()
    {
        CardSocket newSocket = Instantiate(socket, socketPanel.transform);
        ActiveSockets.Add(newSocket);
    }

    public List<Card> GetActiveCardsList()
    {
        return ActiveCards;
    }

    private void Update()
    {
        UpdateCardPositionsInCircuitBoard();
    }

    private void LinkPlayerboard()
    {
        cardPanel = PlayerUI.CardPanel;
        socketPanel = PlayerUI.SocketPanel;
    }

    public void SetUpNewPlayerDeck()
    {
        // Add starting cards to the player deck
        Decks.Playerdeck.TotalCardsInDeck.AddRange(StartingCardsInPlay);
        Decks.Playerdeck.TotalCardsInDeck.AddRange(startingCardsInDeck);
        Decks.Playerdeck.CurrentCardsInPlay.AddRange(StartingCardsInPlay);
        Decks.Playerdeck.CurrentCardsInDeck.AddRange(startingCardsInDeck);
        Decks.Playerdeck.CurrentCardsInDeck.Shuffle();
        UpdateCardsInPlay();
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
            case _CardPlacement.Play:
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
        base.PlaceCardInSocket(newCard, socket);

        // Reset of the card calculation
        UpdateCardsInPlay();
    }

    public override void RemoveFromSocket(Card card)
    {
        base.RemoveFromSocket(card);

        // Reset of the card calculation
        UpdateCardsInPlay();
    }

    public override bool IsProcessingCards(Character targetCharacter)
    {
        bool isProcessing = true;

        if (timeBetweenCardsPlayed > 0)
            return true;

        if (activeCardNumber < ActiveCards.Count)
        {
            ActiveCards[activeCardNumber].ActivateCard(targetCharacter);
            timeBetweenCardsPlayed = ActiveCards[activeCardNumber].MaxTimeInUse;
            activeCardNumber++;
            isProcessing = true;
            print("activate card" + activeCardNumber);
        }

        // After all cards have been processed, deactivate them
        if (activeCardNumber == ActiveCards.Count)
        {
            for (int i = 0; i < ActiveCards.Count; i++)
            {
                ActiveCards[i].DeactivateCard();
            }
            activeCardNumber = 0;
            isProcessing = false;
            print("turn off all cards");
        }

        // Lock the interactable state of cards based on: if the cards are still processing or not
        ToggleInteractableCardState(!isProcessing);
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

    private void UpdateCardPositionsInCircuitBoard()
    {
        for (int i = 0; i < ActiveSockets.Count; i++)
        {
            if (ActiveSockets[i].SlottedCard != null)
            {
                // Sets the position of the card to the connected socket
                Vector2 newCardPosition = ActiveSockets[i].transform.position;
                ActiveSockets[i].SlottedCard.CardPointerInteraction.AssignPosition(newCardPosition);
            }
        }
    }

    private void ToggleInteractableCardState(bool isInteractable)
    {
        for (int i = 0; i < ActiveSockets.Count; i++)
            ActiveSockets[i].ToggleSocketLock(isInteractable);
        for (int i = 0; i < ActiveCards.Count; i++)
            ActiveCards[i].CardPointerInteraction.SetInteractableState(isInteractable);
    }

    public override void SetCircuitDisplayTimer(string timerDisplayText, float timerDisplayFillAmount)
    {
        timerText.text = timerDisplayText;
        timerFill.fillAmount = timerDisplayFillAmount;
    }
}