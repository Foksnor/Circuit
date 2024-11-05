using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PlayerHandPanel : MonoBehaviour
{
    private RectTransform rectTransform;
    private List<Card> cardsInPanel = new();
    private int previousCardCount = 0;
    public bool canPlayerDrawNewHand { get; set; }
    [SerializeField] private Card cardToSpawn;
    [SerializeField] private Transform targetDrawCardsFrom;
    [SerializeField] private Transform targetDiscardCardsTo;
    [SerializeField] private float drawDelay = 0.1f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Decks.Playerdeck.HandPanel = this;
    }

    private void Update()
    {
        // Only update the position of the cards if the amount of cards in your hand changed
        if (cardsInPanel.Count != previousCardCount)
        {
            previousCardCount = cardsInPanel.Count;

            // Calculate the starting X position to center the objects around
            float totalWidth = (cardsInPanel.Count - 1) * rectTransform.rect.width;
            float startX = rectTransform.transform.position.x + (totalWidth / 2);

            for (int i = 0; i < cardsInPanel.Count; i++)
            {
                // Calculate position for each GameObject
                Vector2 personalVector2 = new Vector2(startX - (i * rectTransform.rect.width), rectTransform.transform.position.y);
                cardsInPanel[i].CardPointerInteraction.AssignPosition(personalVector2);
            }
        }
    }

    public void AssignCardToPanel(Card card)
    {
        // Add current card to this reference which is used for save/load game states
        Decks.Playerdeck.CurrentCardsDrawn.Add(card.GetCardInfo());

        card.transform.SetParent(transform);
        cardsInPanel.Add(card);
    }

    public void RemoveCardFromPanel(Card card)
    {
        // Add current card to this reference which is used for save/load game states
        Decks.Playerdeck.CurrentCardsDrawn.Remove(card.GetCardInfo());
        Decks.Playerdeck.CurrentCardsInDiscard.Add(card.GetCardInfo());

        card.CardPointerInteraction.AssignPosition(targetDiscardCardsTo.position);
        cardsInPanel.Remove(card);
    }

    public void DrawCards(int drawAmount)
    {
        StartCoroutine(DrawWithDelay(drawAmount));
    }

    IEnumerator DrawWithDelay(int drawAmount)
    {
        int cardsDrawn = 0;

        while (cardsDrawn < drawAmount)
        {
            // Shuffle discard pile into the draw pile if the draw pile is empty
            if (Decks.Playerdeck.CurrentCardsInDeck.Count == 0)
            {
                Decks.Playerdeck.CurrentCardsInDeck.AddRange(Decks.Playerdeck.CurrentCardsInDiscard);
                Decks.Playerdeck.CurrentCardsInDiscard.Clear();
            }

            // Add a card to the draw panel
            Card newCard = Instantiate(cardToSpawn, targetDrawCardsFrom);
            AssignCardToPanel(newCard);
            newCard.CardPointerInteraction.ToggleInteractableState(true);

            // Pick a random cardscriptable object from the player deck
            int rng = UnityEngine.Random.Range(0, Decks.Playerdeck.CurrentCardsInDeck.Count);
            CardScriptableObject newCardScriptableObject = Decks.Playerdeck.CurrentCardsInDeck[rng];
            Decks.Playerdeck.CurrentCardsInDeck.RemoveAt(rng);

            // Places the picked cardscriptable object into the recently instantiated card
            // Also adds the card to current drawn hand to be used later for discard phase
            newCard.SetCardInfo(newCardScriptableObject, Teams.CharacterTeams.PlayerCircuitboard, true);

            // Wait for the specified delay before the next card
            cardsDrawn++;
            yield return new WaitForSeconds(drawDelay);
        }
    }

    public void RemoveAllCardsFromPanel()
    {
        // Clear the drawn cards and add them to your discard pile
        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            RemoveCardFromPanel(cardsInPanel[i]);
        }
    }
}
