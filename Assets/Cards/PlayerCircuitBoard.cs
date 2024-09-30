using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCircuitBoard : CircuitBoard
{
    [SerializeField] private List<CardScriptableObject> startingCardsInDeck = new List<CardScriptableObject>();
    [SerializeField] private int cardDrawPerTurn = 3;
    [SerializeField] private GameObject cardDrawPanel = null;
    [HideInInspector] public List<Card> drawnCards = new List<Card>();

    private bool canPlayerDrawNewHand = true;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFill;

    protected override void Awake()
    {
        base.Awake();
        SetUpPlayerDeck();
        Teams.CharacterTeams.SetKing(transform.GetComponent<Character>());
    }

    private void SetUpPlayerDeck()
    {
        PlayerDeck.CardDrawPerTurn = cardDrawPerTurn;

        // Add starting cards to the player deck
        for (int i = 0; i < startingCardsInDeck.Count; i++)
        {
            PlayerDeck.TotalCardsInDeck.Add(startingCardsInDeck[i]);
        }
        PlayerDeck.CurrentCardsInDeck = PlayerDeck.TotalCardsInDeck;
    }

    public override void PlayerDrawPhase()
    {
        if (canPlayerDrawNewHand)
            for (int i = 0; i < PlayerDeck.CardDrawPerTurn; i++)
            {
                // Shuffle discard pile into the draw pile if the draw pile is empty
                if (PlayerDeck.CurrentCardsInDeck.Count == 0)
                {
                    // QQQ TODO add cool animation for this where discard goes to draw pile
                    PlayerDeck.CurrentCardsInDeck.AddRange(PlayerDeck.CurrentCardsInDiscard);
                    PlayerDeck.CurrentCardsInDiscard.Clear();
                }

                // Add a card to the draw panel
                drawnCards.Add(Instantiate(cardSmall, cardDrawPanel.transform));
                drawnCards[i].CardPointerInteraction.ToggleInteractableState(true);

                // Pick a random cardscriptable object from the player deck
                int rng = UnityEngine.Random.Range(0, PlayerDeck.CurrentCardsInDeck.Count);
                CardScriptableObject newCardScriptableObject = PlayerDeck.CurrentCardsInDeck[rng];
                PlayerDeck.CurrentCardsInDeck.RemoveAt(rng);

                // Places the picked cardscriptable object into the recently instantiated card
                // Also adds the card to current drawn hand to be used later for discard phase
                drawnCards[i].SetCardInfo(newCardScriptableObject, this, true);

                // Sets the desired target position of the recently instantiated card
                // QQQ TODO fix position calculation or use sockets
                float xPos = cardDrawPanel.GetComponent<RectTransform>().rect.size.x / (1 + PlayerDeck.CardDrawPerTurn) * (1 + i);
                Vector2 newCardPosition = new Vector2(xPos, cardDrawPanel.transform.position.y);
                drawnCards[i].CardPointerInteraction.AssignPosition(newCardPosition);
            }
        canPlayerDrawNewHand = false;
    }

    public override void ReplaceCardInCircuit(Card newCard, Card cardToReplace)
    {
        // New card takes the position in the active cards list of the old card
        int posInList = activeCards.IndexOf(cardToReplace);
        activeCards[posInList] = newCard;
        newCard.transform.SetParent(cardPanel.transform);
        CardSelect.SelectedSocket.SlotCard(newCard);

        // Remove the new card from the players hand
        newCard.SetCardInfo(newCard.GetCardInfo(), this, false);
        drawnCards.Remove(newCard);

        // Put the old card into the discard deck
        PlayerDeck.CurrentCardsInDiscard.Add(cardToReplace.GetCardInfo());
        Destroy(cardToReplace.gameObject);

        // Enables reset of the card simulation
        needsNewCardCalculation = true;

        RemoveCardsFromHand();
    }

    private void RemoveCardsFromHand()
    {
        // Clear the drawn cards and add them to your discard pile
        for (int remainingDrawnCardNumber = 0; remainingDrawnCardNumber < drawnCards.Count; remainingDrawnCardNumber++)
        {
            PlayerDeck.CurrentCardsInDiscard.Add(drawnCards[remainingDrawnCardNumber].GetCardInfo());

            // QQQ TODO this destroy function is temp. In future, needs the card to go towards discard pile visually before destroying here.
            Destroy(drawnCards[remainingDrawnCardNumber].gameObject);
        }
        // QQQ TODO this destroy function is temp. In future, needs the card to go towards discard pile visually before destroying here.
        drawnCards.Clear();
    }

    public override bool IsProcessingCards(Character targetCharacter)
    {
        bool isProcessing = base.IsProcessingCards(targetCharacter);

        // Player discards their hand.
        // Sets value to true so players can draw a single hand during their next draw phase.
        if (!canPlayerDrawNewHand)
            RemoveCardsFromHand();
        canPlayerDrawNewHand = true;

        return isProcessing;
    }

    protected override void UpdateCardOrder()
    {
        // Sort card order by comparing the x position, which can happen if the player drags on of their cards in the circuit board
        List<Card> CardOrderBeforeSort = new List<Card>();
        CardOrderBeforeSort.AddRange(activeCards);
        activeCards.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // If any of the active cards has their position changed. A new card calculation is in order. This can happen during setup phase
        // Bool needsNewCardCalculation will set itself to false once the calculation action has ended
        if (!needsNewCardCalculation)
        {
            needsNewCardCalculation = HelperFunctions.AreCardListsDifferent(CardOrderBeforeSort, activeCards);
            TurnSequence.TransitionTurns.NeedsNewCardCalculation = needsNewCardCalculation;
        }
    }

    protected override void AllignCardOnCircuitBoard(int cardNumber)
    {
        // Sets the new socket reference to the card
        activeCards[cardNumber].ConnectToSocket(activeSockets[cardNumber]);

        // Sets the new position of the card to the connected socket
        Vector2 newCardPosition = activeSockets[cardNumber].transform.position;
        activeCards[cardNumber].CardPointerInteraction.AssignPosition(newCardPosition);
    }

    protected override void ToggleInteractableCardStateOnCircuitBoard(int cardNumber, bool isInteractable)
    {
        activeCards[cardNumber].CardPointerInteraction.ToggleInteractableState(isInteractable);
        activeSockets[cardNumber].ToggleSocketLock(isInteractable);
    }

    public override void SetCircuitDisplayTimer(string timerDisplayText, float timerDisplayFillAmount)
    {
        timerText.text = timerDisplayText;
        timerFill.fillAmount = timerDisplayFillAmount;
    }
}