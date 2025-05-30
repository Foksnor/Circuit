using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Animator))]
public class CardTooltip : MonoBehaviour
{
    private RectTransform tooltipRectTransform;
    private RectTransform cardRectTransform;
    private Animator animator;
    [SerializeField] private TextMeshProUGUI nameTagText;
    [SerializeField] private GameObject targetSelectorPanel;
    [SerializeField] private GridLayoutGroup targetSelectorGridLayoutGroup;
    [SerializeField] private CardTooltipTargetGridCell targetGridCellPrefab;
    [SerializeField] private Transform ContentPanel;
    [SerializeField] private CardActionTooltipDescription CardActionTooltipDescriptionPrefab;
    [SerializeField] private TextMeshProUGUI rarityTagText;
    [SerializeField] private Image[] imagesToColorRarity;
    private const int tooltipOffset = 100;
    private readonly Vector3 cameraFacingDirection = Vector3.left;

    private void Awake()
    {
        tooltipRectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        UpdateRotationOnScreen();
        SetPositionInScreen();
    }

    private void UpdateRotationOnScreen()
    {
        // Always force the tooltip rotation to be straight
        tooltipRectTransform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public void SetTooltip(CardScriptableObject cardInfo, float cardScale, RectTransform cardRect)
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isVisible", true);
        cardRectTransform = cardRect;

        SetNameTag(cardInfo);
        SetTargetSelectorReference(cardInfo);
        PopulateCardActionDescriptions(cardInfo);
        SetRarityTag(cardInfo);
        SetTooltipScale(cardScale);
    }

    public void RemoveToolTip()
    {
        // Animator disabled as destroying the gameobject while animating does nothing
        //animator.SetBool("isVisible", false);
        Destroy(gameObject);
    }

    private void SetPositionInScreen()
    {
        if (HelperFunctions.IsTooltipNearScreenTop(cardRectTransform))
        {            
            tooltipRectTransform.pivot = new Vector2(tooltipRectTransform.pivot.x, 1);
            tooltipRectTransform.anchoredPosition = new Vector2(0, -tooltipOffset);
        }
        else
        {
            tooltipRectTransform.pivot = new Vector2(tooltipRectTransform.pivot.x, 0);
            tooltipRectTransform.anchoredPosition = new Vector2(0, tooltipOffset);
        }
    }

    private void SetNameTag(CardScriptableObject cardInfo)
    {
        nameTagText.text = cardInfo.CardName;
    }

    private void SetTargetSelectorReference(CardScriptableObject cardInfo)
    {
        // Disables the target panel if no target is required for the action
        if (cardInfo.ActionSequence.TargetRequirement == null)
        {
            targetSelectorPanel.SetActive(false);
            return;
        }

        // Adds instigator and target positions
        List<Vector2Int> targetPositions = new(cardInfo.ActionSequence.TargetRequirement.RelativeSelectedPositions);
        if (!cardInfo.ActionSequence.TargetRequirement.InstigatorPosition.IsNullOrEmpty())
            targetPositions.Add(Vector2Int.zero);

        // Rotates the positions to the visual representation of the current camera facing direction
        targetPositions = HelperFunctions.RotatePositionsTowardsTarget(Vector3.zero, cameraFacingDirection, default, targetPositions);

        // Calculate the bounds of the grid
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (Vector2Int pos in targetPositions)
        {
            if (pos.x < minX)
                minX = pos.x;
            if (pos.x > maxX)
                maxX = pos.x;
            if (pos.y < minY)
                minY = pos.y;
            if (pos.y > maxY)
                maxY = pos.y;
        }

        int gridWidth = maxX - minX + 1;

        // Set up grid layout, and centers the grid visually based on the amount of columns
        targetSelectorGridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        targetSelectorGridLayoutGroup.constraintCount = gridWidth;

        // Generate the grid cells
        for (int y = maxY; y >= minY; y--)
        {
            for (int x = minX; x <= maxX; x++)
            {
                CardTooltipTargetGridCell cell = Instantiate(targetGridCellPrefab, targetSelectorGridLayoutGroup.transform);
                Vector2Int currentPos = new(x, y);

                // Instigator position
                if (currentPos == Vector2Int.zero)
                {
                    cell.SetCellToInstigator();
                }

                // Target position
                else if (targetPositions.Contains(currentPos))
                {
                    if (cardInfo.CardType == _CardType.Attack)
                    {
                        cell.SetCellToTarget();
                    }
                    else if (cardInfo.CardType == _CardType.Movement)
                    {
                        Vector3 curPos = new(currentPos.x, currentPos.y); // Convert Vector2Int to Vector3 for the helperfunction
                        float angle = HelperFunctions.GetDirectionAngle(Vector3.zero, curPos);

                        cell.SetCellToMovement(angle);
                    }
                }

                // Turn the other grid cells invisible
                else
                {
                    cell.SetCellInvisible();
                }
            }
        }
    }

    private void PopulateCardActionDescriptions(CardScriptableObject cardInfo)
    {
        if (cardInfo.ActionSequence == null || cardInfo.ActionSequence.Actions == null)
            return;

        // Populate descriptions for each Card Action
        foreach (CardActionData actionData in cardInfo.ActionSequence.Actions)
        {
            // Ignore Card Actions that don't have a description
            if (HelperFunctions.actionDescriptions.ContainsKey(actionData.CardAction))
            {
                CardActionTooltipDescription actionDescription = Instantiate(CardActionTooltipDescriptionPrefab, ContentPanel);
                actionDescription.PopulateDescription(actionData);
            }
        }
    }

    private void SetRarityTag(CardScriptableObject cardInfo)
    {
        rarityTagText.text = cardInfo.CardRarity.ToString();

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

    private void SetTooltipScale(float cardScale)
    {
        // Makes sure that the tooltip of the card remains the same
        // This is because the tooltip is a child of the card and adheres to that scale
        tooltipRectTransform.localScale = tooltipRectTransform.localScale / cardScale;
    }
}
