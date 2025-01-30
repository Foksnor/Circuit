using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.Common.Extensions;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PlayerHandPanel : MonoBehaviour
{
    private RectTransform rectTransform;
    private List<Card> cardsInPanel = new();
    private int previousCardCount = 0;
    [SerializeField] private Card cardToSpawn;
    [SerializeField] private Transform targetDrawCardsFrom;
    [SerializeField] private Transform targetDiscardCardsTo;
    [SerializeField] private float drawDelay = 0.1f;
    [SerializeField] private Canvas targetReferenceScalingCanvas;
    [SerializeField] private float maxSpacing = 50;
    private float maxTimeForShufflingToComplete = 1;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        PlayerUI.HandPanel = this;
    }

    private void Update()
    {
        // Only update the position of the cards if the amount of cards in your hand changed
        if (cardsInPanel.Count != previousCardCount || HelperFunctions.HasResolutionChanged())
        {
            previousCardCount = cardsInPanel.Count;
            UpdateCardPositions();
        }
    }

    public void UpdateCardPositions()
    {
        // Calculate dynamic spacing based on available width and number of objects
        float spacing = Mathf.Min(maxSpacing, rectTransform.rect.width / Mathf.Max(1, cardsInPanel.Count - 1));

        // Get the scale factor from targetReferenceScalingCanvas
        spacing *= targetReferenceScalingCanvas.scaleFactor;

        // Calculate the starting X position to center the objects around
        float totalWidth = (cardsInPanel.Count - 1) * spacing;
        float startX = rectTransform.transform.position.x + (totalWidth / 2);

        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            // Calculate position for each GameObject
            Vector2 personalVector2 = new Vector2(startX - (i * spacing), rectTransform.transform.position.y);
            cardsInPanel[i].CardPointerInteraction.AssignPosition(personalVector2);
        }
    }

    public void AssignCardToPanel(Card card)
    {
        card.transform.SetParent(transform);
        cardsInPanel.Add(card);
    }

    public void RemoveCardFromPanel(Card card, bool isSendToDiscard)
    {
        // Only play discard animations when actually removing this card to discard
        // E.g. this bool can be false if cards are removed due to replacing them with a card in your circuit
        if (isSendToDiscard)
            SentCardToDiscard(card);
        cardsInPanel.Remove(card);
    }

    public void SentCardToDiscard(Card card)
    {
        Decks.Playerdeck.CurrentCardsInDiscard.Add(card.GetCardInfo());
        card.CardPointerInteraction.AssignPosition(targetDiscardCardsTo.position);
        card.SetSelfDestructWhenReachingTargetTransform(targetDiscardCardsTo);
    }

    public void DrawCards(int drawAmount)
    {
        StartCoroutine(DrawWithDelay(drawAmount));
    }

    IEnumerator DrawWithDelay(int drawAmount)
    {
        // First loop: Draw cards and set up their info
        for (int i = 0; i < drawAmount; i++)
        {
            // Shuffle if the draw pile is empty
            if (Decks.Playerdeck.CurrentCardsInDeck.Count == 0)
            {
                // Calculate how fast shuffling should be
                float stepDelay = maxTimeForShufflingToComplete / Decks.Playerdeck.CurrentCardsInDiscard.Count;

                // Shuffle discard pile into the draw pile
                while (Decks.Playerdeck.CurrentCardsInDiscard.Count > 0)
                {
                    // Add a card to the draw panel
                    Card shuffleCard = Instantiate(cardToSpawn, targetDiscardCardsTo);
                    CardScriptableObject discardCardScriptableObject = Decks.Playerdeck.CurrentCardsInDiscard[0];
                    shuffleCard.SetCardInfo(discardCardScriptableObject, PlayerUI.PlayerCircuitboard, true);
                    shuffleCard.CardPointerInteraction.AssignPosition(targetDrawCardsFrom.position);
                    shuffleCard.SetSelfDestructWhenReachingTargetTransform(targetDrawCardsFrom);

                    Decks.Playerdeck.CurrentCardsInDeck.Add(Decks.Playerdeck.CurrentCardsInDiscard[0]);
                    Decks.Playerdeck.CurrentCardsInDiscard.RemoveAt(0);

                    // Wait for the specified delay before processing the next card
                    yield return new WaitForSeconds(stepDelay);
                }

                Decks.Playerdeck.CurrentCardsInDeck.Shuffle();
            }

            // Add a card to the draw panel
            Card newCard = Instantiate(cardToSpawn, targetDrawCardsFrom);

            // Pick the top card(s) of the draw pile
            CardScriptableObject newCardScriptableObject = Decks.Playerdeck.CurrentCardsInDeck[0];

            // Places the picked cardscriptable object into the recently instantiated card
            // Also adds the card to current drawn hand to be used later for discard phase
            newCard.SetCardInfo(newCardScriptableObject, PlayerUI.PlayerCircuitboard, true);

            // Start the second loop immediately after shuffle
            AssignCardToPanel(newCard);

            // Remove the top card from the deck after adding it into the game
            Decks.Playerdeck.CurrentCardsInDeck.RemoveAt(0);

            // Wait for the specified delay before processing the next card
            yield return new WaitForSeconds(drawDelay);
        }
    }

    public void RemoveAllCardsFromPanel(bool isSendToDiscard)
    {
        // Clear the drawn cards and add them to your discard pile
        while (cardsInPanel.Count > 0)
        {
            RemoveCardFromPanel(cardsInPanel[0], isSendToDiscard);
        }
    }
}
