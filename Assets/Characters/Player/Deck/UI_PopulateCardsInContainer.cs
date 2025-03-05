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
            case _CardPlacement.Play:
                cards.AddRange(Decks.Playerdeck.CurrentCardsInPlay);
                break;
            case _CardPlacement.Deck:
                cards.AddRange(Decks.Playerdeck.CurrentCardsInDeck);
                break;
            case _CardPlacement.Discard:
                cards.AddRange(Decks.Playerdeck.CurrentCardsInDiscard);
                break;
        }

        foreach (CardScriptableObject cardInfo in cards)
        {
            Card referenceCard = Instantiate(card, targetContainer.transform.position, transform.rotation, targetContainer.transform);

            // Update the layout, that way the card is positioned correctly inside the Grid Layout Group
            // Code can then read the information about their position after the UI has been updated
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetContainer);
            Canvas.ForceUpdateCanvases();

            referenceCard.SetCardInfo(cardInfo, PlayerUI.PlayerCircuitboard, true);
            Vector2 localSocketPosition = HelperFunctions.ConvertWorldToAnchoredPosition(referenceCard.transform.position, targetContainer);
            referenceCard.CardPointerInteraction.AssignAnchoredPosition(localSocketPosition, targetContainer.position);

            Debug.Log(localSocketPosition + " " + referenceCard.GetComponent<RectTransform>().anchoredPosition);
        }
    }
}
