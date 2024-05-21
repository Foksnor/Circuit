using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPrevis : MonoBehaviour
{
    // Reference set through inspector prefab window
    [SerializeField] private SpriteRenderer characterVisual = null;

    public void SetCharacterVisual(Sprite characterSprite)
    {
        characterVisual.sprite = characterSprite;
    }
}
