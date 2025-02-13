using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardTooltip : MonoBehaviour
{
    [SerializeField] Image[] imagesToColorRarity;

    public void SetTooltip(CardScriptableObject cardInfo)
    {
        SetRarityColor(cardInfo);
    }

    private void SetRarityColor(CardScriptableObject cardInfo)
    {
        Color color = Color.white;
        switch (cardInfo.CardRarity)
        {
            case _CardRarity.Rare:
                color = GlobalSettings.C_RarityRare;
                break;
            case _CardRarity.Epic:
                color = GlobalSettings.C_RarityEpic;
                break;
        }

        // Sets the rarity color on each image but preserves the original alpha value
        foreach (Image image in imagesToColorRarity)
        {
            float imageColorAlpha = image.color.a;
            image.color = new Color(color.r, color.g, color.b, imageColorAlpha);
        }
    }
}
