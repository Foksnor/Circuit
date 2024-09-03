using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Card : MonoBehaviour
{
    private CardScriptableObject cardScriptableObject = null;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image cardBackground, cardimage;
    [SerializeField] private Material fireMat, electricityMat, foilMat, goldenMat;
    [SerializeField] private AudioClip sound;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Animator cardAnimator;
    [SerializeField] private Animator feedbackAnimator;
    public Card_PointerInteraction CardPointerInteraction;
    private List<GridCube> attackedGridTargets = new();
    public List<Character> potentialKillTargets { private set; get; } = new();
    private GridCube targetedGridForMovement;
    public float MaxTimeInUse { get; private set; } = 0;
    private bool isCardActivated = false;
    private bool hasParticleSpawnedOnSelf = false;

    public bool isInHand { get; private set; }
    public CardSocket connectedSocket { get; private set; }
    public CircuitBoard ConnectedCircuitboard { get; private set; }

    public void SetCardInfo(CardScriptableObject scriptableObject, CircuitBoard owner, bool setInHand)
    {
        cardScriptableObject = scriptableObject;
        ConnectedCircuitboard = owner;
        isInHand = setInHand;
        //nameText.text = cardScriptableObject.name;
        //costText.text = cardScriptableObject.cost.ToString();
        cardimage.sprite = cardScriptableObject.Sprite;
        valueText.text = cardScriptableObject.Value.ToString();
        descriptionText.text = cardScriptableObject.Description;

        // Sets the material for various images based on card parameters
        switch (scriptableObject.CardRarity)
        {
            case CardScriptableObject._CardRarity.Rare:
                break;
            case CardScriptableObject._CardRarity.Epic:
                break;
        }
        switch (scriptableObject.CardType)
        {
            case CardScriptableObject._CardType.ElementFire:
                cardimage.material = fireMat;
                break;
            case CardScriptableObject._CardType.ElementElectricity:
                cardimage.material = electricityMat;
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

    public CardScriptableObject GetCardInfo()
    {
        return cardScriptableObject;
    }

    private void Update()
    {
        SetCardHighlight();
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
        // Sets the card feedback on the circuitboard if state based actions could give feedback to why certain things work or won't work
        if (!feedbackAnimator.isActiveAndEnabled)
            return;
        feedbackAnimator.SetBool(animationParameter, parameterState);
    }

    public void ConnectToSocket(CardSocket socket)
    {
        connectedSocket = socket;
        socket.SlotCard(this);
    }

    public void ActivateCard(Character instigator)
    {
        if (isCardActivated)
            return;

        isCardActivated = true;
        MaxTimeInUse = cardScriptableObject.MaxTimeInUse;

        // Attack
        if (cardScriptableObject.CardType == CardScriptableObject._CardType.Attack)
            HandleAttack(instigator);
        // Movement
        else if (cardScriptableObject.CardType == CardScriptableObject._CardType.Movement)
            HandleMovement(instigator);
        else
            HandleBuffs(instigator);
    }

    public void DeactivateCard()
    {
        MaxTimeInUse = 0;
        isCardActivated = false;
        hasParticleSpawnedOnSelf = false;
    }

    private GridCube GetGridOfClosestTarget(GridCube savedGridUsedByPreviousCard)
    {
        float closestTargetDistance = 99;
        GridCube pCGridNumber = savedGridUsedByPreviousCard;

        // QQQ TODO make it so it only targets the opposing team instead of only player team, in case the player gets auto target cards
        // Cycles through all characters and checks which one is closer, then return that as reference if possible
        for (int pC = 0; pC < CharacterTeams._PlayerTeamCharacters.Count; pC++)
        {
            pCGridNumber = CharacterTeams._PlayerTeamCharacters[pC].AssignedGridCube;
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

    public GridCube CalculateGridCubeDestination(Character instigator, GridCube savedGridUsedByPreviousCard, int cardNumber, bool isSetupPhase)
    {
        GridCube targetedGrid = savedGridUsedByPreviousCard;
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
                GridCube newTargetGrid = GetGridOfClosestTarget(savedGridUsedByPreviousCard);
                attackSteps = RotateCardTowardsTarget(newTargetGrid, savedGridUsedByPreviousCard, attackSteps);
                moveSteps = RotateCardTowardsTarget(newTargetGrid, savedGridUsedByPreviousCard, moveSteps);
                break;
        }

        // Attack
        if (cardScriptableObject.CardType == CardScriptableObject._CardType.Attack)
        {
            // Empty the list before adding attack positions
            // List gets cleared everytime because the character can be updating their position before commiting to the attack
            attackedGridTargets.Clear();

            for (int attackStepY = 1; attackStepY < attackSteps.y + 1; attackStepY++)
            {
                // Handles the Y steps in the attack
                Vector2 attackPosAfterStepY = new Vector2 (savedGridUsedByPreviousCard.Position.x, savedGridUsedByPreviousCard.Position.y + (attackStepY * directionMultiplier));

                // Offset the attack width so the attack is always centered
                int attackWidthOffset = 1 + Mathf.CeilToInt(attackSteps.x / 2);

                for (int attackStepX = 1; attackStepX < attackSteps.x + 1; attackStepX++)
                {
                    Vector2 attackPosAfterStepX = new Vector2 (attackPosAfterStepY.x - attackWidthOffset + attackStepX, attackPosAfterStepY.y);
                    GridCube result = GridPositions.GetGridByPosition(attackPosAfterStepX);
                    if (result != null)
                    {
                        // Handles X steps in the attack
                        if (ValidateGridPosition.CanAttack(savedGridUsedByPreviousCard, result, attackStepY))
                        {
                            targetedGrid = result;
                            // Tile based previs
                            GameObject tilevisual = targetedGrid.GetIndicatorVisual(instigator, cardScriptableObject);
                            instigator.ToggleCardPrevis(isSetupPhase, cardNumber, tilevisual, 0);
                            attackedGridTargets.Add(targetedGrid);
                        }
                    }
                }
            }

            // Save target location for movement during set up phase
            if (isSetupPhase)
            {
                targetedGridForMovement = savedGridUsedByPreviousCard;
                CheckIfCharactersWouldDie(instigator, cardScriptableObject.Value);
            }

            return savedGridUsedByPreviousCard;
        }
        else if (cardScriptableObject.CardType == CardScriptableObject._CardType.Movement)
        {
            for (int stepLength = 0; stepLength < Mathf.Abs(moveSteps.y); stepLength++)
            {
                // Handles the Y steps in the movement
                targetedGrid = GetMovementGrid(instigator, targetedGrid, moveSteps, cardNumber, isSetupPhase);
            }
            for (int stepWidth = 0; stepWidth < Mathf.Abs(moveSteps.x); stepWidth++)
            {
                // Handles X steps in the movement
                targetedGrid = GetMovementGrid(instigator, targetedGrid, moveSteps, cardNumber, isSetupPhase);
            }

            // Save target location for movement during set up phase
            if (isSetupPhase)
                targetedGridForMovement = targetedGrid;
        }

        // Reset feedback animation if needed
        if (!isSetupPhase)
            SetCardFeedback("isInvalid", false);

        // Returns the integer of the current used gridnumber, so it's location can be used for the other cards in the sequence
        return targetedGrid;
    }

    private GridCube GetMovementGrid(Character instigator, GridCube startingGrid, Vector2Int moveSteps, int cardNumber, bool isSetupPhase)
    {
        GridCube destinationGrid = startingGrid;
        SetCardFeedback("isInvalid", false);
        Vector2Int moveIncrement = moveSteps.ConvertVector2IntToIncrement();

        // Move X
        if (moveSteps.x != 0)
        {
            int moveX = moveIncrement.x;
            Vector2 targetPos = new Vector2(startingGrid.Position.x + moveX, startingGrid.Position.y);
            GridCube result = GridPositions.GetGridByPosition(targetPos);

            if (ValidateGridPosition.CanStep(instigator, startingGrid, result, cardNumber, isSetupPhase))
            {
                destinationGrid = result;
                ConnectedCircuitboard.SaveMovementGridCube(result);
            }
            else
                SetCardFeedback("isInvalid", true);
        }

        // Move Y
        if (moveSteps.y != 0)
        {
            int moveY = moveIncrement.y;
            Vector2 targetPos = new Vector2(startingGrid.Position.x, startingGrid.Position.y + moveY);
            GridCube result = GridPositions.GetGridByPosition(targetPos);

            if (ValidateGridPosition.CanStep(instigator, startingGrid, result, cardNumber, isSetupPhase))
            {
                destinationGrid = result;
                ConnectedCircuitboard.SaveMovementGridCube(result);
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
            instigator.ToggleCardPrevis(isSetupPhase, cardNumber, tilevisual, dirAngle);
        }

        return destinationGrid;
    }

    private float GetDirectionAngleBetweenGrids(GridCube grid1, GridCube grid2)
    {
        // Calculates the angle between two grids, used for changing the angle of the previs if needed
        Vector3 pos1 = grid1.Position;
        Vector3 pos2 = grid2.Position;
        float angle = Vector3.Angle(pos1 - pos2, transform.up);

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
        bool electricityAvailable = ConnectedCircuitboard.UseAvailableBuff(CardScriptableObject._CardType.ElementElectricity);

        for (int i = 0; i < attackedGridTargets.Count; i++)
        {
            SpawnParticleEffectAtGridCube(instigator, attackedGridTargets[i]);

            if (fireAvailable)
                attackedGridTargets[i].ToggleSurfaceStatus(_StatusEffect.Fire);
            if (electricityAvailable)
                attackedGridTargets[i].ToggleSurfaceStatus(_StatusEffect.Shocked);


            if (instigator.isSimulation)
            {
                Character simOnThisGrid = null;
                if (attackedGridTargets[i].SimulationOnThisGrid != null)
                    simOnThisGrid = attackedGridTargets[i].SimulationOnThisGrid;

                // Stop if no simulations are hit
                if (simOnThisGrid != null)
                {
                    // No friendly fire allowed, stop damage function when this happens
                    if (simOnThisGrid.TeamType != instigator.TeamType)
                    {
                        simOnThisGrid.ChangeHealth(cardScriptableObject.Value, instigator);
                    }
                }
            }
            else
            {
                Character charOnThisGrid = null;
                if (attackedGridTargets[i].CharacterOnThisGrid != null)
                    charOnThisGrid = attackedGridTargets[i].CharacterOnThisGrid;

                // Stop if no characters are hit
                if (charOnThisGrid != null)
                {
                    // No friendly fire allowed, stop damage function when this happens
                    if (charOnThisGrid.TeamType != instigator.TeamType)
                    {
                        charOnThisGrid.ChangeHealth(cardScriptableObject.Value, instigator);
                    }
                }
            }
        }
    }

    private void HandleBuffs(Character instigator)
    {
        switch (cardScriptableObject.CardType)
        {
            case CardScriptableObject._CardType.ElementFire:
            case CardScriptableObject._CardType.ElementElectricity:
                ConnectedCircuitboard.AddBuff(instigator, cardScriptableObject.CardType, cardScriptableObject.Value);
                break;
        }
    }

    private void CheckIfCharactersWouldDie(Character instigator, int damageValue)
    {
        for (int i = 0; i < attackedGridTargets.Count; i++)
        {
            Character charOnThisGrid = null;
            if (attackedGridTargets[i].CharacterOnThisGrid != null)
                charOnThisGrid = attackedGridTargets[i].CharacterOnThisGrid;

            // Return if no characters are hit
            if (charOnThisGrid == null)
                return;

            // No friendly fire allowed, stop damage function when this happens
            if (charOnThisGrid.TeamType == instigator.TeamType)
                return;

            // Check if target would die to damage before actually dealing damage
            // If that's true, then mark it as a potental kill and add them to the list
            // In case of a card swap; the list will be used to remove potential kill references
            if (charOnThisGrid.MarkPotentialKillIfDamageWouldKill(damageValue))
                if (!potentialKillTargets.Contains(charOnThisGrid))
                    potentialKillTargets.Add(charOnThisGrid);
        }
    }


    private void SpawnParticleEffectAtGridCube(Character instigator, GridCube cube)
    {
        if (cardScriptableObject.Particle == null)
            return;

        GameObject particle = null;

        switch (cardScriptableObject.ParticleLocation)
        {
            case CardScriptableObject._ParticleLocation.OnSelf:
                if (!hasParticleSpawnedOnSelf)
                {
                    particle = Instantiate(cardScriptableObject.Particle, instigator.transform.position, transform.rotation);
                    hasParticleSpawnedOnSelf = true;

                    // Mirrors the particle transform when the particle is playing backward of character
                    if (cardScriptableObject.TargetType == CardScriptableObject._TargetType.BackwardOfCharacter)
                        particle.transform.localScale = new Vector2(-1, 1);
                }
                break;
            case CardScriptableObject._ParticleLocation.OnDamageTiles:
            case CardScriptableObject._ParticleLocation.OnMovementTiles:
                particle = Instantiate(cardScriptableObject.Particle, cube.transform.position, transform.rotation);
                break;
        }

        // Sets the spawned particle in the "Simulation" layer
        // This is needed because the simulation camera renders all simulation based events different than the main camera
        if (particle != null && instigator.CharacterSimulation == null)
        {
            int layerNumber = LayerMask.NameToLayer("Simulation");
            HelperFunctions.SetGameLayerRecursive(particle, layerNumber);
        }
    }
}
