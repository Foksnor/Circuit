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
    [SerializeField] private RectTransform targetDrawCardsFrom;
    [SerializeField] private RectTransform targetDiscardCardsTo;
    [SerializeField] private float drawDelay = 0.1f;
    [SerializeField] private Canvas targetReferenceScalingCanvas;
    [SerializeField] private float maxSpacing = 50;
    private const float maxTimeForShufflingToComplete = 1;
    private const float maxCardFanRotation = 7;
    private const float maxCardFanHeightoffset = 10f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        PlayerUI.HandPanel = this;
    }

    private void Update()
    {
        // Only update the position of the cards if the amount of cards in your hand changed
        if (cardsInPanel.Count != previousCardCount)
        {
            previousCardCount = cardsInPanel.Count;
            FanCardsInPanel();
        }
    }

    public void FanCardsInPanel()
    {
        if (cardsInPanel.Count == 0)
            return;

        // Get panel width and scale it with screen resolution
        float panelWidth = rectTransform.rect.width / targetReferenceScalingCanvas.scaleFactor;

        // Calculate spacing, ensuring it doesn't spread too far
        float spacing = Mathf.Min(maxSpacing, panelWidth / Mathf.Max(1, cardsInPanel.Count));

        // Calculate the total width occupied by cards
        float totalWidth = (cardsInPanel.Count - 1) * spacing;

        // Normalize startX so it remains consistent across resolutions
        float startX = -totalWidth / 2f;

        // Calculate center index
        float centerIndex = Mathf.Max((cardsInPanel.Count - 1) / 2f, 0.5f);

        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            // Adjust the card's local position within the hand panel
            Vector2 cardPosition = new(startX + (i * spacing), 0);

            // Offset from the center
            float offset = i - centerIndex;

            // Apply card fan rotation
            float zRotation = -offset * (maxCardFanRotation / centerIndex);

            // Apply rotation
            Vector3 cardRotation = new(
                cardsInPanel[i].transform.eulerAngles.x,
                cardsInPanel[i].transform.eulerAngles.y,
                zRotation
            );
            cardsInPanel[i].CardPointerInteraction.AssignRotation(cardRotation);

            // Adjust height (cards with more rotation are positioned lower)
            float heightOffset = -maxCardFanHeightoffset * Mathf.Pow(offset / centerIndex, 2);
            cardPosition.y += heightOffset;

            // Assign the corrected anchored position
            cardsInPanel[i].CardPointerInteraction.AssignAnchoredPosition(cardPosition, cardPosition);
        }
    }

    public void AssignCardToPanel(Card card)
    {
        card.transform.SetParent(transform);
        card.GetComponent<RectTransform>().localScale = Vector3.one; // Reset scale to prevent flipping or resolution scaling
        cardsInPanel.Insert(0, card); // This inserts the card at the beginning
    }

    public void RemoveCardFromPanel(Card card, bool isSendToDiscard)
    {
        // Only play discard animations when actually removing this card to discard
        // E.g. this bool can be false if cards are removed due to replacing them with a card in your circuit
        if (isSendToDiscard)
            SentCardToDiscard(card, rectTransform, 0);

        // Remove the card from the panel list
        cardsInPanel.Remove(card);

        // Reset card rotation
        card.CardPointerInteraction.AssignRotation(Vector3.zero);
    }

    public void SentCardToDiscard(Card card, RectTransform parentRectTransform, float travelTime)
    {
        // Add card to discard list
        Decks.Playerdeck.CurrentCardsInDiscard.Add(card.GetCardInfo());

        // Convert discard pile world position to UI-anchored position using the canvas as reference
        Vector2 localDiscardPos = HelperFunctions.ConvertWorldToAnchoredPosition(targetDiscardCardsTo.position, parentRectTransform);

        // Assign the correctly converted position
        card.CardPointerInteraction.AssignAnchoredPosition(localDiscardPos, targetDiscardCardsTo.position, travelTime);

        // Set the card to self-destruct when reaching the discard pile
        card.SetSelfDestructWhenReachingTargetPosition(localDiscardPos);
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
                    Vector2 localDrawPos = HelperFunctions.ConvertWorldToAnchoredPosition(targetDrawCardsFrom.position, targetDiscardCardsTo);
                    shuffleCard.CardPointerInteraction.AssignAnchoredPosition(localDrawPos, targetDrawCardsFrom.position, 0.25f);
                    shuffleCard.SetSelfDestructWhenReachingTargetPosition(localDrawPos);

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
