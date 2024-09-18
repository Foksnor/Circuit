using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardReward : MonoBehaviour
{
    public CardScriptableObject CardScriptableObject { private set; get; }

    [SerializeField] TextMeshProUGUI Name, Type, Description;
    [SerializeField] Image Icon;
    [SerializeField] Animator animator;

    public void SetCardRewardInfo(CardScriptableObject cardScriptableObject)
    {
        CardScriptableObject = cardScriptableObject;
        Name.text = CardScriptableObject.CardName;
        Icon.sprite = CardScriptableObject.Sprite;
        Type.text = CardScriptableObject.CardType.ToString();
        Description.text = CardScriptableObject.Description;
    }

    public void AddCardToDeck()
    {
        PlayerDeck.CurrentCardsInDeck.Add(CardScriptableObject);
        PlayerStats.RewardScreen.RemoveCardRewardOptions(CardScriptableObject);
    }

    public void RemoveFromRewardScreen(bool isClaimed)
    {
        animator.SetBool("isRemovedFromRewardScreen", true);
        animator.SetBool("isClaimed", isClaimed);
        Invoke(nameof(Destroy), 1);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
