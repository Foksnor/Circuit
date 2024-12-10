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
    private float maxTimeForCounterToComplete = 0.5f;

    private void Update()
    {
        // Set value equal to what cardNumber was previous frame
        int previousNumber = cardNumber;

        // Change cardNumber value to what it should be current frame
        switch (cardPlacementType)
        {
            case _CardPlacement.Play:
                cardNumber = Decks.Playerdeck.CurrentCardsInPlay.Count;
                break;
            case _CardPlacement.Deck:
                cardNumber = Decks.Playerdeck.CurrentCardsInDeck.Count;
                break;
            case _CardPlacement.Discard:
                cardNumber = Decks.Playerdeck.CurrentCardsInDiscard.Count;
                break;
        }

        // Check if cardNumber values are different between current and last frame
        if (previousNumber != cardNumber)
        {
            int difference = Mathf.Abs(previousNumber - cardNumber);
            float stepDelay = maxTimeForCounterToComplete / difference;
            StopAllCoroutines(); // Prevent duplicate coroutine of the number counter
            StartCoroutine(ChangeValue(previousNumber, cardNumber, stepDelay));
        }
    }

    private IEnumerator ChangeValue(int startValue, int targetValue, float stepDelay)
    {
        currentValue = startValue;
        int step = (targetValue > startValue) ? 1 : -1; // Determine increment or decrement

        // Animate and increase/decrease the counter
        while (currentValue != targetValue)
        {
            currentValue += step;
            targetTextMeshPro.text = currentValue.ToString();
            targetAnimator.Play(animationName, 0, 0);
            yield return new WaitForSeconds(stepDelay);
        }
    }
}