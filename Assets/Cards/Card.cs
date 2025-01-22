using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Doozy.Runtime.Common.Extensions;
using System.Net.Sockets;

public class Card : MonoBehaviour
{
    private CardScriptableObject cardScriptableObject = null;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image cardBackground, cardimage;
    [SerializeField] private Material foilMat, goldenMat;
    [SerializeField] private AudioClip sound;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI TargetRequirementText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Animator feedbackAnimator;
    public Animator cardAnimator;
    public Card_PointerInteraction CardPointerInteraction;
    private Transform targetSelfDestructDestination;
    private List<GridCube> attackedGridTargets = new();
    private GridCube targetedGridForMovement;
    public float MaxTimeInUse = 0.5f;
    //public float MaxTimeInUse { get; private set; } = 0.5f;
    private bool isCardActivated { get; set; } = false;
    [SerializeField] private List<Image> rotatableImageMaterial = new();

    public bool isCardVisible { get; private set; } = false;
    public CardSocket ConnectedSocket { get; private set; }
    public CircuitBoard ConnectedCircuitboard { get; private set; }

    public void SetCardInfo(CardScriptableObject scriptableObject, CircuitBoard owner, bool isVisible)
    {
        cardScriptableObject = scriptableObject;
        ConnectedCircuitboard = owner;
        isCardVisible = isVisible;

        if (isVisible)
        {
            nameText.text = cardScriptableObject.CardName;
            //costText.text = cardScriptableObject.cost.ToString();
            cardimage.sprite = cardScriptableObject.Sprite;
            valueText.text = cardScriptableObject.Value.ToString();
            descriptionText.text = cardScriptableObject.Description;

            // Only show the requirement when necessary
            if (cardScriptableObject.TargetRequirement.IsNullOrEmpty())
                TargetRequirementText.transform.gameObject.SetActive(false);
            else
                TargetRequirementText.text = cardScriptableObject.TargetRequirement;

            // Set random rotation of the card material so that every card in hand looks a bit different
            SetRandomMaterialRotation();

            // Sets the material for various images based on card parameters
            switch (scriptableObject.CardRarity)
            {
                case CardScriptableObject._CardRarity.Rare:
                    break;
                case CardScriptableObject._CardRarity.Epic:
                    break;
            }
            switch (scriptableObject.CardStyle)
            {
                case CardScriptableObject._CardStyle.Foil:
                    cardBackground.material = foilMat;
                    break;
                case CardScriptableObject._CardStyle.Golden:
                    cardBackground.material = goldenMat;
                    break;
            }
        }
    }

    private void SetRandomMaterialRotation()
    {
        if (rotatableImageMaterial.Count > 0)
        {
            for (int i = 0; i < rotatableImageMaterial.Count; i++)
            {
                float rngAngle = UnityEngine.Random.Range(0, 360);
                rotatableImageMaterial[i].material.SetFloat("TextureRotation", rngAngle);
            }
        }
    }

    public CardScriptableObject GetCardInfo()
    {
        return cardScriptableObject;
    }

    private void Update()
    {
        SetCardHighlight();
        CheckForSelfDestruct();
    }

    public void SetCardHighlight()
    {
        // Sets the card highlight on the circuitboard when card is played
        MaxTimeInUse -= Time.deltaTime;
        if (MaxTimeInUse < 0)
            cardAnimator.SetBool("isHighlighted", false);
        else
            cardAnimator.SetBool("isHighlighted", true);
    }

    private void SetCardFeedback(string animationParameter, bool parameterState)
    {
        // Disabled card feedback for now until further testing without it.

        /*
        // Sets the card feedback on the circuitboard if state based actions could give feedback to why certain things work or won't work
        if (!feedbackAnimator.isActiveAndEnabled)
            return;
        feedbackAnimator.SetBool(animationParameter, parameterState);
        */
    }

    public void ConnectToSocket(CardSocket socket)
    {
        ConnectedSocket = socket;
        socket.SlotCard(this);
    }

    public void RemoveFromSocket()
    {
        ConnectedSocket.SlotCard(null);
        ConnectedSocket = null;
    }

    public void ActivateCard(Character instigator)
    {
        isCardActivated = true;
        MaxTimeInUse = cardScriptableObject.MaxTimeInUse;

        if (instigator != null)
        {
            List<CardActionData> actions = cardScriptableObject.ActionSequence.Actions;
            GridSelector targets = cardScriptableObject.ActionSequence.TargetRequirement;

            // Process all actions, including nested ActionSequences
            ProcessActions(instigator, actions, targets);

            // Attack
            //if (cardScriptableObject.CardType == CardScriptableObject._CardType.Attack)
            //    HandleAttack(instigator);
            // Movement
            /*
            else if (cardScriptableObject.CardType == CardScriptableObject._CardType.Movement)
                HandleMovement(instigator);
            else
                HandleBuffs(instigator);
            */
        }
    }

    public void ProcessActions(Character instigator, List<CardActionData> actions, GridSelector targets)
    {
        foreach (CardActionData action in actions)
        {
            // Recursively process the nested ActionSequence
            if (action.CardAction == _CardAction.ActionSequence && action.ActionSequence != null)
            {
                ProcessActions(instigator, action.ActionSequence.Actions, action.ActionSequence.TargetRequirement);
            }
            // Call the action if it's not an ActionSequence
            else
            {
                // Context based value as parameter
                object value;
                switch (action.CardAction)
                {
                    default:
                        value = action.Value;
                        break;
                    case _CardAction.SpawnParticleOnTarget:
                    case _CardAction.SpawnParticleOnSelf:
                        value = action.Particle;
                        break;
                }
                CardActions.Instance.CallAction(instigator, action.CardAction, value, targets);
            }
        }
    }

    public void DeactivateCard()
    {
        isCardActivated = false;
    }

    private GridCube GetGridOfClosestTarget(GridCube savedGridUsedByPreviousCard)
    {
        float closestTargetDistance = 99;
        GridCube pCGridNumber = savedGridUsedByPreviousCard;

        // QQQ TODO make it so it only targets the opposing team instead of only player team, in case the player gets auto target cards
        // Cycles through all characters and checks which one is closer, then return that as reference if possible
        for (int pC = 0; pC < Teams.CharacterTeams.PlayerTeamCharacters.Count; pC++)
        {
            pCGridNumber = Teams.CharacterTeams.PlayerTeamCharacters[pC].AssignedGridCube;
            Vector3 curPos = savedGridUsedByPreviousCard.Position;
            Vector3 tarPos = pCGridNumber.Position;

            if (Vector3.Distance(curPos, tarPos) < closestTargetDistance)
                closestTargetDistance = Vector3.Distance(curPos, tarPos);
        }
        return pCGridNumber;
    }

    public Vector2Int RotateCardTowardsTarget(GridCube targetGrid, GridCube savedGridUsedByPreviousCard, Vector2Int steps)
    {
        float targetAngle = GetDirectionAngleBetweenGrids(targetGrid, savedGridUsedByPreviousCard);
        if (Mathf.Abs(targetAngle) < 172)
        {
            steps = new Vector2Int(steps.y, steps.x);
            if (targetAngle < 0)
                steps = new Vector2Int(-steps.x, steps.y);
        }
        return steps;
    }

    public GridCube CalculateGridCubeDestination(Character instigator, bool isSetupPhase)
    {
        GridCube targetGrid = instigator.AssignedGridCube;
        Vector2Int attackSteps = cardScriptableObject.AttackSteps;
        Vector2Int moveSteps = cardScriptableObject.MoveSteps;

        int directionMultiplier = 1;
        switch (cardScriptableObject.TargetType)
        {
            case CardScriptableObject._TargetType.Self:
            case CardScriptableObject._TargetType.ForwardOfCharacter:
                break;
            case CardScriptableObject._TargetType.BackwardOfCharacter:
                directionMultiplier = -1;
                break;
            case CardScriptableObject._TargetType.NearestAlly:
            case CardScriptableObject._TargetType.NearestEnemy:
                GridCube newTargetGrid = GetGridOfClosestTarget(targetGrid);
                attackSteps = RotateCardTowardsTarget(newTargetGrid, targetGrid, attackSteps);
                moveSteps = RotateCardTowardsTarget(newTargetGrid, targetGrid, moveSteps);
                break;
        }

        // Movement
        if (cardScriptableObject.CardType == CardScriptableObject._CardType.Movement)
        {
            for (int stepLength = 0; stepLength < Mathf.Abs(moveSteps.y); stepLength++)
            {
                // Handles the Y steps in the movement
                targetGrid = GetMovementGrid(instigator, targetGrid, moveSteps, isSetupPhase);
            }
            for (int stepWidth = 0; stepWidth < Mathf.Abs(moveSteps.x); stepWidth++)
            {
                // Handles X steps in the movement
                targetGrid = GetMovementGrid(instigator, targetGrid, moveSteps, isSetupPhase);
            }

            // Save target location for movement during set up phase
            targetedGridForMovement = targetGrid;
        }

        // Reset feedback animation if needed
        if (!isSetupPhase)
            SetCardFeedback("isInvalid", false);

        // Returns the integer of the current used gridnumber, so it's location can be used for the other cards in the sequence
        return targetGrid;
    }

    private GridCube GetMovementGrid(Character instigator, GridCube startingGrid, Vector2Int moveSteps, bool isSetupPhase)
    {
        GridCube destinationGrid = startingGrid;
        SetCardFeedback("isInvalid", false);
        Vector2Int moveIncrement = moveSteps.ConvertVector2IntToIncrement();

        // Move X
        if (moveSteps.x != 0)
        {
            int moveX = moveIncrement.x;
            Vector2 targetPos = new Vector2(startingGrid.Position.x + moveX, startingGrid.Position.y);
            GridCube targetGrid = Grid.GridPositions.GetGridByPosition(targetPos);

            if (ValidateGridPosition.CanStep(instigator, startingGrid, targetGrid))
            {
                destinationGrid = targetGrid;
            }
            else
                SetCardFeedback("isInvalid", true);
        }

        // Move Y
        if (moveSteps.y != 0)
        {
            int moveY = moveIncrement.y;
            Vector2 targetPos = new Vector2(startingGrid.Position.x, startingGrid.Position.y + moveY);
            GridCube targetGrid = Grid.GridPositions.GetGridByPosition(targetPos);

            if (ValidateGridPosition.CanStep(instigator, startingGrid, targetGrid))
            {
                destinationGrid = targetGrid;
            }
            else
                SetCardFeedback("isInvalid", true);
        }

        // Only execute tile previs if there is movement
        if (destinationGrid != startingGrid)
        {
            // Tile based previs
            GameObject tilevisual = destinationGrid.GetIndicatorVisual(instigator, cardScriptableObject);
            float dirAngle = GetDirectionAngleBetweenGrids(destinationGrid, startingGrid);
            instigator.ToggleTilePrevis(isSetupPhase, tilevisual, dirAngle);
        }

        return destinationGrid;
    }

    private float GetDirectionAngleBetweenGrids(GridCube grid1, GridCube grid2)
    {
        // Calculates the angle between two grids, used for changing the angle of the previs if needed
        Vector3 pos1 = grid1.Position;
        Vector3 pos2 = grid2.Position;
        float angle = Vector3.Angle(pos1 - pos2, Vector3.up);

        // Vector3.Angle always returns an absolute number, so checking whether pos1 is left or right of pos2 to see if the angle needs to be set to a negative        
        Vector3 cross = pos1 - pos2;
        if (cross.x > 0)
            angle = -angle;        
        return angle;
    }

    private void HandleMovement(Character instigator)
    {
        instigator.ChangeDestinationGrid(targetedGridForMovement, MaxTimeInUse);
    }

    private void HandleAttack(Character instigator)
    {
        // Use elemental attack if available
        bool fireAvailable = ConnectedCircuitboard.UseAvailableBuff(CardScriptableObject._CardType.ElementFire);
        bool shockAvailable = ConnectedCircuitboard.UseAvailableBuff(CardScriptableObject._CardType.ElementShock);

        for (int i = 0; i < attackedGridTargets.Count; i++)
        {
            if (fireAvailable)
                attackedGridTargets[i].ToggleStatus(instigator, _StatusType.Fire, true);
            if (shockAvailable)
                attackedGridTargets[i].ToggleStatus(instigator, _StatusType.Shocked, true);

            Character charOnThisGrid = null;
            if (attackedGridTargets[i].CharacterOnThisGrid != null)
                charOnThisGrid = attackedGridTargets[i].CharacterOnThisGrid;

            // Stop if no characters are hit
            if (charOnThisGrid != null)
            {
                // No friendly fire allowed, stop damage function when this happens
                if (charOnThisGrid.TeamType != instigator.TeamType)
                {
                    charOnThisGrid.SubtractHealth(cardScriptableObject.Value, instigator);
                }
            }
        }
    }

    private void HandleBuffs(Character instigator)
    {
        switch (cardScriptableObject.CardType)
        {
            case CardScriptableObject._CardType.ElementFire:
            case CardScriptableObject._CardType.ElementShock:
                ConnectedCircuitboard.AddBuff(instigator, cardScriptableObject.CardType, cardScriptableObject.Value);
                break;
        }
    }

    public void SetSelfDestructWhenReachingTargetTransform(Transform target)
    {
        targetSelfDestructDestination = target;
    }

    private void CheckForSelfDestruct()
    {
        // If the card transform is close to a self destruct destination
        // E.g. used when discard this card to the discard pile
        if (targetSelfDestructDestination != null)
            if (Vector2.Distance(transform.position, targetSelfDestructDestination.position) <= 2.5f)
                Destroy(gameObject);
    }
}