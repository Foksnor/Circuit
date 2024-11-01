using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PopulateCardsInContainer : MonoBehaviour
{
    [SerializeField] _CardPlacement placementType;
    [SerializeField] GameObject targetContainer;
    [SerializeField] CardReference cardReference;
    private List<CardScriptableObject> cards = new();

    public void PopulateContainer()
    {
        // Remove previous references and populated cards
        cards.Clear();
        foreach (Transform child in targetContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Add new cards to populate the targetcontainer
        switch(placementType)
        {
            case _CardPlacement.Hand:
                cards.AddRange(Decks.Playerdeck.CurrentCardsInHand);
                break;
            case _CardPlacement.Drawn:
                cards.AddRange(Decks.Playerdeck.CurrentCardsDrawn);
                break;
            case _CardPlacement.Deck:
                cards.AddRange(Decks.Playerdeck.CurrentCardsInDeck);
                break;
            case _CardPlacement.Discard:
                cards.AddRange(Decks.Playerdeck.CurrentCardsInDiscard);
                break;
        }
        for (int i = 0; i < cards.Count; i++)
        {
            CardReference card = Instantiate(cardReference, targetContainer.transform.position, transform.rotation, targetContainer.transform);
            card.SetCardInfo(cards[i]);
        }
    }
}
