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
    private const float maxTimeForShufflingToComplete = 1;
    private const float maxCardFanRotation = 7;
    private const float maxCardFanHeightoffset = 3f;

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
            FanCardsInPanel();
        }
    }

    public void FanCardsInPanel()
    {
        // Calculate dynamic spacing based on available width and number of objects
        float spacing = Mathf.Min(maxSpacing, rectTransform.rect.width / Mathf.Max(1, cardsInPanel.Count - 1));

        // Get the scale factor from targetReferenceScalingCanvas
        spacing *= targetReferenceScalingCanvas.scaleFactor;

        // Calculate the starting X position to center the objects around
        float totalWidth = (cardsInPanel.Count - 1) * spacing;
        float startX = rectTransform.transform.position.x + (totalWidth / 2);

        // Calculate the "center index"
        // Avoiding zero calculation by using Mathf.Max
        float centerIndex = Mathf.Max((cardsInPanel.Count - 1) / 2f, 0.5f);

        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            // Calculate position for each GameObject
            Vector2 cardPosition = new(startX - (i * spacing), rectTransform.transform.position.y);

            // Offset from the center (e.g., -2, -1, 0, 1, 2 for 5 cards)
            float offset = i - centerIndex;

            // This scales the offset so that the leftmost card is -maxCardFanRotation
            // and the rightmost is +maxCardFanRotation
            float zRotation = offset * (maxCardFanRotation / centerIndex);

            // Apply the rotation
            Vector3 cardRotation = new(
                cardsInPanel[i].transform.eulerAngles.x,
                cardsInPanel[i].transform.eulerAngles.y,
                zRotation
            );
            cardsInPanel[i].CardPointerInteraction.AssignRotation(cardRotation);

            // Apply height adjustment - cards with higher rotation go down
            float heightOffset = Mathf.Abs(zRotation) * maxCardFanHeightoffset;
            cardPosition.y -= heightOffset;
            cardsInPanel[i].CardPointerInteraction.AssignPosition(cardPosition);
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
            SentCardToDiscard(card, 0);

        // Remove the card from the panel list
        cardsInPanel.Remove(card);

        // Reset card rotation
        card.CardPointerInteraction.AssignRotation(Vector3.zero);
    }

    public void SentCardToDiscard(Card card, float travelTime)
    {
        Decks.Playerdeck.CurrentCardsInDiscard.Add(card.GetCardInfo());
        card.CardPointerInteraction.AssignPosition(targetDiscardCardsTo.position, travelTime);
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
                    shuffleCard.CardPointerInteraction.AssignPosition(targetDrawCardsFrom.position, 0.25f);
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
