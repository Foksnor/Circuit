using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;

public class CardActionTooltipDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;

    public void PopulateDescription(CardActionData actionData)
    {
        // Gets the text associated with the card action
        textComponent.text = HelperFunctions.FormatDescription(actionData.CardAction, actionData.Value);
    }
}
