using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class UI_PopulateCardsInContainer : MonoBehaviour
{
    [SerializeField] _CardPlacement placementType;
    [SerializeField] RectTransform targetContainer;
    [SerializeField] Card card;

    public void PopulateContainer()
    {
        // Destroy ALL existing child objects immediately
        while (targetContainer.childCount > 0)
        {
            DestroyImmediate(targetContainer.GetChild(0).gameObject);
        }

        // Add new cards to populate the targetcontainer
        List<CardScriptableObject> cardsInfoFromPlacementType = new();
        switch (placementType)
        {
            case _CardPlacement.Play:
                cardsInfoFromPlacementType.AddRange(Decks.Playerdeck.CurrentCardsInPlay);
                break;
            case _CardPlacement.Deck:
                cardsInfoFromPlacementType.AddRange(Decks.Playerdeck.CurrentCardsInDeck);
                break;
            case _CardPlacement.Discard:
                cardsInfoFromPlacementType.AddRange(Decks.Playerdeck.CurrentCardsInDiscard);
                break;
        }

        // Spawn the reference cards inside the container
        List<Card> spawnedCards = new();
        foreach (CardScriptableObject cardInfo in cardsInfoFromPlacementType)
        {
            Card referenceCard = Instantiate(card, targetContainer.transform.position, transform.rotation, targetContainer.transform);
            referenceCard.SetCardInfo(cardInfo, PlayerUI.PlayerCircuitboard, true);
            spawnedCards.Add(referenceCard);
        }

        // Update the layout, that way the cards are positioned correctly inside the Grid Layout Group
        // Code can then read the information about their position after the UI has been updated
        LayoutRebuilder.ForceRebuildLayoutImmediate(targetContainer);
        Canvas.ForceUpdateCanvases();

        // Now get the final positions AFTER GridLayout updates
        foreach (Card referenceCard in spawnedCards)
        {
            RectTransform rect = referenceCard.GetComponent<RectTransform>();
            Vector2 finalAnchoredPosition = rect.anchoredPosition;

            // Only assign position if it's different from the Grid Layout Group's auto positioning
            referenceCard.CardPointerInteraction.AssignAnchoredPosition(finalAnchoredPosition, targetContainer.position);
        }
    }
}
