using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardTooltipTargetGridCell : MonoBehaviour
{
    [SerializeField] private Image ImageComponent;
    [SerializeField] private Sprite cellInstigatorPosition;
    [SerializeField] private Sprite cellTargetPosition;

    public void SetCellToInstigator()
    {
        ImageComponent.sprite = cellInstigatorPosition;
    }

    public void SetCellToTarget()
    {
        ImageComponent.sprite = cellTargetPosition;
    }

    public void SetCellInvisible()
    {
        ImageComponent.color = Color.clear;
    }
}
