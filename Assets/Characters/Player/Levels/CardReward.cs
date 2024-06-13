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

    public void SetCardRewardInfo(CardScriptableObject cardScriptableObject)
    {
        CardScriptableObject = cardScriptableObject;
        Name.text = CardScriptableObject.CardName;
        Icon.sprite = CardScriptableObject.Sprite;
        Type.text = CardScriptableObject.CardType.ToString();
        Description.text = CardScriptableObject.Description;
    }
}
