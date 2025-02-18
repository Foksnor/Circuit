using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CardActionTooltipDescription : MonoBehaviour
{
    public void PopulateDescription(CardActionData actionData)
    {
        TextMeshProUGUI textField = GetComponent<TextMeshProUGUI>();

        // Gets the text associated with the card action
        textField.text = HelperFunctions.GetDescription(actionData.CardAction, actionData.Value);
    }
}
