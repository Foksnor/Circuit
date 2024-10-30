using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardReference : MonoBehaviour
{
    public CardScriptableObject CardScriptableObject { private set; get; }

    [SerializeField] TextMeshProUGUI Name, Type, Description;
    [SerializeField] Image Icon;

    public void SetCardInfo(CardScriptableObject cardScriptableObject)
    {
        CardScriptableObject = cardScriptableObject;
        Name.text = CardScriptableObject.CardName;
        Icon.sprite = CardScriptableObject.Sprite;
        Type.text = CardScriptableObject.CardType.ToString();
        Description.text = CardScriptableObject.Description;
    }
}
