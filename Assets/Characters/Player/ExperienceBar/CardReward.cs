using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent (typeof(Button), typeof(Animator))]
public class CardReward : MonoBehaviour
{
    private CardScriptableObject cardScriptableObject = null;
    private Button button;
    private Animator animator;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonPressed);
        animator = GetComponent<Animator>();
    }

    public void SetCardInfo(CardScriptableObject cardInfo)
    {
        cardScriptableObject = cardInfo;
    }

    public CardScriptableObject GetCardInfo()
    {
        return cardScriptableObject;
    }

    public void OnButtonPressed()
    {
        // Add card to deck
        Decks.Playerdeck.CurrentCardsInDeck.Add(cardScriptableObject);

        // Remove options from screen
        PlayerUI.RewardScreen.RemoveCardRewardOptions(cardScriptableObject);
    }

    public void RemoveFromRewardScreen(bool isClaimed)
    {
        animator.SetBool("isRemovedFromRewardScreen", true);
        animator.SetBool("isClaimed", isClaimed);
        if (isClaimed)
        {
            /*
            // Add a card to the draw panel
            Card shuffleCard = Instantiate(cardToSpawn, targetDiscardCardsTo);
            CardScriptableObject discardCardScriptableObject = Decks.Playerdeck.CurrentCardsInDiscard[0];
            shuffleCard.SetCardInfo(discardCardScriptableObject, PlayerUI.PlayerCircuitboard, true);
            Vector2 localDrawPos = HelperFunctions.ConvertWorldToAnchoredPosition(targetDrawCardsFrom.position, targetDiscardCardsTo);
            shuffleCard.CardPointerInteraction.AssignAnchoredPosition(localDrawPos, targetDrawCardsFrom.position, 0.25f);
            shuffleCard.SetSelfDestructWhenReachingTargetPosition(localDrawPos);

            Decks.Playerdeck.CurrentCardsInDeck.Add(Decks.Playerdeck.CurrentCardsInDiscard[0]);
            Decks.Playerdeck.CurrentCardsInDiscard.RemoveAt(0);
            */
        }
        else
        {
            Invoke(nameof(Destroy), 1);
        }
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
