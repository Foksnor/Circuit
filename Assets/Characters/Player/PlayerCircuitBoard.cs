using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Runtime.Common.Extensions;
using Unity.VisualScripting;

public static class PlayerUI
{
    public static Canvas CanvasScreenSpace { get; set; } = null;
    public static PlayerHandPanel HandPanel { get; set; } = null;
    public static GameObject CardPanel { get; set; } = null;
    public static GameObject SocketPanel { get; set; } = null;
    public static PlayerCircuitBoard PlayerCircuitboard { get; set; } = null;
    public static ExperienceBar ExperienceBar { set; get; }
    public static RewardScreen RewardScreen { set; get; }
    public static LifePanel LifePanel { set; get; }
}

public class PlayerCircuitBoard : CircuitBoard
{
    [SerializeField] private List<CardScriptableObject> startingCardsInDeck = new List<CardScriptableObject>();
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFill;
    private List<Character> activeListeners = new();

    protected override void Awake()
    {
        PlayerUI.PlayerCircuitboard = this;
        PlayerUI.CardPanel = cardPanel;
        PlayerUI.SocketPanel = socketPanel;

        LinkPlayerboard();
    }

    public void AddSocketFromSaveFile(_CardAction enhancementType, int enhancementCharges, bool wasSlotEmpty)
    {
        CardSocket loadedSocket = AddSocket(enhancementType, enhancementCharges);

        // Fills the empty slot with a dummy card
        // Dummy cards prevent the slot to be filled in with a card when loading a save file
        if (wasSlotEmpty)
            loadedSocket.SkipSlotDuringCardPopulation = true;
    }

    public override void SetUpCircuitBoard(List<CardScriptableObject> cardList)
    {
        // Adds sockets, if save data didn't provide them, or when starting a new game
        if (ActiveSockets.Count == 0)
            for (int i = 0; i < cardList.Count; i++)
                AddSocket(default, 0);

        // Only use sockets can be populated when setting up the board
        List<CardSocket> availableSockets = ActiveSockets
            .Where(socket => socket.SkipSlotDuringCardPopulation == false)
            .ToList();

        // Adds card
        for (int i = 0; i < cardList.Count; i++)
        {
            Card newCard = Instantiate(cardPrefab, cardPanel.transform);
            ActiveCards.Add(newCard);


            // Sets the new socket reference to the card
            newCard.ConnectToSocket(availableSockets[i]);

            // Sets the card info per card in the circuit board
            newCard.SetCardInfo(cardList[i], this, true);
        }
    }

    public override bool IsProcessingCards(Character targetCharacter)
    {
        bool isProcessing = base.IsProcessingCards(targetCharacter);

        // Cards should not be able to be interacted with when processing cards
        ToggleInteractableCardState(!isProcessing);
        return isProcessing;
    }

    protected override void ActivateSelectedCard(Character targetCharacter)
    {
        activeListeners = Teams.CharacterTeams.PlayerTeamCharacters;

        // Add all current player listeners to a list of characters
        for (int c = 0; c < activeListeners.Count; c++)
        {
            if (activeListeners[c].BrainType == Character._BrainType.Listener)
            {
                ActiveCards[activeCardNumber].ActivateCard(activeListeners[c]);
            }
        }
    }

    private CardSocket AddSocket(_CardAction enhancementType, int enhancementChargeCount)
    {
        CardSocket newSocket = Instantiate(socketPrefab, socketPanel.transform);

        // Prohibit a socket to set an enchantment when no valid type is given
        // This can happen when loading a default socket type from a save file
        if (enhancementType != default)
            newSocket.SetSlotEnhancement(enhancementType, enhancementChargeCount);

        ActiveSockets.Add(newSocket);
        return newSocket;
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
        SetUpCircuitBoard(StartingCardsInPlay);
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

    public List<CardSocket> GetActiveSockets()
    {
        return ActiveSockets;
    }
}