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
    private int cardNumber = 0;
    private int currentValue = 0;

    void Update()
    {
        int previousNumber = cardNumber;
        switch (cardPlacementType)
        {
            case _CardPlacement.Hand:
                cardNumber = Decks.Playerdeck.CurrentCardsInHand.Count;
                break;
            case _CardPlacement.Deck:
                cardNumber = Decks.Playerdeck.CurrentCardsInDeck.Count;
                break;
            case _CardPlacement.Discard:
                cardNumber = Decks.Playerdeck.CurrentCardsInDiscard.Count;
                break;
        }
        targetTextMeshPro.text = cardNumber.ToString();

        // Animate the counter if the number has changed since last frame
        if (previousNumber != cardNumber)
            targetAnimator.Play(animationName, 0, 0);
    }
}
