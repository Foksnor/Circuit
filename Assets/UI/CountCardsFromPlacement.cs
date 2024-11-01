using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountCardsFromPlacement : MonoBehaviour
{
    [SerializeField] private _CardPlacement cardPlacementType;
    [SerializeField] private TextMeshProUGUI targetTextMeshPro;
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string animationName;
    private int cardCount = 0;

    void Update()
    {
        int previousNumber = cardCount;
        switch (cardPlacementType)
        {
            case _CardPlacement.Hand:
                cardCount = Decks.Playerdeck.CurrentCardsInHand.Count;
                break;
            case _CardPlacement.Drawn:
                cardCount = Decks.Playerdeck.CurrentCardsDrawn.Count;
                break;
            case _CardPlacement.Deck:
                cardCount = Decks.Playerdeck.CurrentCardsInDeck.Count;
                break;
            case _CardPlacement.Discard:
                cardCount = Decks.Playerdeck.CurrentCardsInDiscard.Count;
                break;
        }
        targetTextMeshPro.text = cardCount.ToString();

        // Animate the counter if the number has changed since last frame
        if (previousNumber != cardCount)
            targetAnimator.Play(animationName, 0, 0);
    }
}
