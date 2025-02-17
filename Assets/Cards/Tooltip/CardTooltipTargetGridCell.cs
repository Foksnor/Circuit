using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardTooltipTargetGridCell : MonoBehaviour
{
    [SerializeField] private Image ImageComponent;
    [SerializeField] private Sprite cellInstigatorPosition;
    [SerializeField] private Sprite cellTargetPosition;
    [SerializeField] private Sprite cellMovementPosition;

    public void SetCellToInstigator()
    {
        ImageComponent.sprite = cellInstigatorPosition;
    }

    public void SetCellToTarget()
    {
        ImageComponent.sprite = cellTargetPosition;
    }

    public void SetCellToMovement(float angle)
    {
        transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, angle);
        ImageComponent.sprite = cellMovementPosition;
    }

    public void SetCellInvisible()
    {
        ImageComponent.color = Color.clear;
    }
}
