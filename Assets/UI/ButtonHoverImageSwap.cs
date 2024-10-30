using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHoverImageSwap : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite hoverSprite;

    public void OnHoverEnter()
    {
        if (targetImage != null && hoverSprite != null)
        {
            targetImage.sprite = hoverSprite;
        }
    }

    public void OnHoverExit()
    {
        if (targetImage != null && defaultSprite != null)
        {
            targetImage.sprite = defaultSprite;
        }
    }
}
