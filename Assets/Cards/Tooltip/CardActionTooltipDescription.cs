using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardActionTooltipDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Image backgroundImage;

    public void PopulateDescription(CardActionData actionData)
    {
        // Gets the text associated with the card action
        textComponent.text = HelperFunctions.FormatDescription(actionData.CardAction, actionData.Value);

        // Applies the color of the card action to the background
        // Keep the original alpha value of the background
        Color actionColor = HelperFunctions.GetActionColor(actionData.CardAction);
        backgroundImage.color = new(actionColor.r, actionColor.g, actionColor.b, backgroundImage.color.a);
    }
}
