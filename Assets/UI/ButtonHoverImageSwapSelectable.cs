using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHoverImageSwapSelectable : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite hoverSprite;
    private bool isButtonSelected = false;

    public void OnHoverEnter()
    {
        if (targetImage != null && hoverSprite != null)
        {
            targetImage.sprite = hoverSprite;
        }
    }

    public void OnHoverExit()
    {
        // Only swap back to the default sprite if the button is not selected
        if (targetImage != null && defaultSprite != null && !isButtonSelected)
        {
            targetImage.sprite = defaultSprite;
        }
    }

    public void OnButtonPressed()
    {
        isButtonSelected = !isButtonSelected;
    }
}