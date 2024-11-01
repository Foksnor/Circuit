using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCircuitBoard : CircuitBoard
{
    [SerializeField] private List<CardScriptableObject> startingCardsInDeck = new List<CardScriptableObject>();
    [SerializeField] private int cardDrawPerTurn = 3;
    [SerializeField] private float drawDelay = 0.1f;
    [SerializeField] private GameObject cardDrawPanel = null;
    [HideInInspector] public List<Card> drawnCards = new List<Card>();

    private bool canPlayerDrawNewHand = true;

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
        Decks.Playerdeck.CurrentCardsInDeck = Decks.Playerdeck.TotalCardsInDeck;
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
                Decks.Playerdeck.CurrentCardsInHand.Add(cardScriptableObject);
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
        StartCoroutine(DrawWithDelay());
    }

    IEnumerator DrawWithDelay()
    {
        if (canPlayerDrawNewHand)
        {
            int cardsDrawn = 0;

            while (cardsDrawn < cardDrawPerTurn)
            {
                // Shuffle discard pile into the draw pile if the draw pile is empty
                if (Decks.Playerdeck.CurrentCardsInDeck.Count == 0)
                {
                    // QQQ TODO add cool animation for this where discard goes to draw pile
                    Decks.Playerdeck.CurrentCardsInDeck.AddRange(Decks.Playerdeck.CurrentCardsInDiscard);
                    Decks.Playerdeck.CurrentCardsInDiscard.Clear();
                }

                // Add a card to the draw panel
                drawnCards.Add(Instantiate(cardSmall, cardDrawPanel.transform));
                drawnCards[cardsDrawn].CardPointerInteraction.ToggleInteractableState(true);

                // Pick a random cardscriptable object from the player deck
                int rng = UnityEngine.Random.Range(0, Decks.Playerdeck.CurrentCardsInDeck.Count);
                CardScriptableObject newCardScriptableObject = Decks.Playerdeck.CurrentCardsInDeck[rng];
                Decks.Playerdeck.CurrentCardsInDeck.RemoveAt(rng);

                // Places the picked cardscriptable object into the recently instantiated card
                // Also adds the card to current drawn hand to be used later for discard phase
                drawnCards[cardsDrawn].SetCardInfo(newCardScriptableObject, this, true);

                // Sets the desired target position of the recently instantiated card
                // QQQ TODO fix position calculation or use sockets
                float xPos = cardDrawPanel.GetComponent<RectTransform>().rect.size.x / (1 + cardDrawPerTurn) * (1 + cardsDrawn);
                Vector2 newCardPosition = new Vector2(xPos, cardDrawPanel.transform.position.y);
                drawnCards[cardsDrawn].CardPointerInteraction.AssignPosition(newCardPosition);

                // Add current hand to this reference which is used for save/load game states
                Decks.Playerdeck.CurrentCardsDrawn.Add(newCardScriptableObject);

                // Wait for the specified delay before the next card
                cardsDrawn++;
                yield return new WaitForSeconds(drawDelay);
            }
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
        Decks.Playerdeck.CurrentCardsInDiscard.Add(cardToReplace.GetCardInfo());
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
            Decks.Playerdeck.CurrentCardsInDiscard.Add(drawnCards[remainingDrawnCardNumber].GetCardInfo());

            // QQQ TODO this destroy function is temp. In future, needs the card to go towards discard pile visually before destroying here.
            Destroy(drawnCards[remainingDrawnCardNumber].gameObject);
        }
        // QQQ TODO this destroy function is temp. In future, needs the card to go towards discard pile visually before destroying here.
        drawnCards.Clear();
        // Clear current hand reference which is used for save/load game states
        Decks.Playerdeck.CurrentCardsDrawn.Clear();
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

            // Sets the hand in the playerdeck equal to the active cards in the circuitboard
            // Used for save/loading you current circuitboard cards
            Decks.Playerdeck.CurrentCardsInHand.Clear();
            for (int i = 0; i < activeCards.Count; i++)
            {
                Decks.Playerdeck.CurrentCardsInHand.Add(activeCards[i].GetCardInfo());
            }
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