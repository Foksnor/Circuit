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
    [SerializeField] private Image image;
    [SerializeField] private AudioClip sound;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Animator animator;
    public Card_PointerInteraction CardPointerInteraction;
    private List<GridCube> attackedGridTargets = new List<GridCube>();
    private int targetedGridForMovement = 0;
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
        image.sprite = cardScriptableObject.Sprite;
        valueText.text = cardScriptableObject.Value.ToString();
        descriptionText.text = cardScriptableObject.Description;
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
        // Sets the card highlight on the circuitboard
        MaxTimeInUse -= Time.deltaTime;
        if (MaxTimeInUse < 0)
            animator.SetBool("isHighlighted", false);
        else
            animator.SetBool("isHighlighted", true);
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
    }

    public void DeactivateCard()
    {
        MaxTimeInUse = 0;
        isCardActivated = false;
        hasParticleSpawnedOnSelf = false;
    }

    private int GetGridOfClosestTarget(int savedGridUsedByPreviousCard)
    {
        float closestTargetDistance = 99;
        int pCGridNumber = savedGridUsedByPreviousCard;

        // QQQ TODO make it so it only targets the opposing team instead of only player team, in case the player gets auto target cards
        // Cycles through all characters and checks which one is closer, then return that as reference if possible
        for (int pC = 0; pC < CharacterTeams._PlayerTeamCharacters.Count; pC++)
        {
            pCGridNumber = CharacterTeams._PlayerTeamCharacters[pC].PositionInGrid;
            Vector3 curPos = GridPositions._GridCubes[savedGridUsedByPreviousCard].transform.position;
            Vector3 tarPos = GridPositions._GridCubes[pCGridNumber].transform.position;

            if (Vector3.Distance(curPos, tarPos) < closestTargetDistance)
                closestTargetDistance = Vector3.Distance(curPos, tarPos);
        }
        return pCGridNumber;
    }

    public Vector2Int RotateCardTowardsTarget(int targetGrid, int savedGridUsedByPreviousCard, Vector2Int steps)
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

    public int CalculateCard(Character targetCharacter, int savedGridUsedByPreviousCard, bool isSetupPhase)
    {
        int targetedGrid = savedGridUsedByPreviousCard;
        Vector2Int attackSteps = cardScriptableObject.AttackSteps;
        Vector2Int moveSteps = cardScriptableObject.MoveSteps;

        if (cardScriptableObject.AutoTargetNearest)
        {
            int newTargetGrid = GetGridOfClosestTarget(savedGridUsedByPreviousCard);
            attackSteps = RotateCardTowardsTarget(newTargetGrid, savedGridUsedByPreviousCard, attackSteps);
            moveSteps = RotateCardTowardsTarget(newTargetGrid, savedGridUsedByPreviousCard, moveSteps);
        }

        // Attack
        if (cardScriptableObject.CardType == CardScriptableObject._CardType.Attack)
        {
            for (int attackStepY = 1; attackStepY < attackSteps.y + 1; attackStepY++)
            {
                // Handles the Y steps in the attack
                int attackLength = (int)GridPositions._GridSize.x;
                attackLength *= attackStepY;
                targetedGrid = savedGridUsedByPreviousCard + attackLength;

                // Offset the attack width so the attack is always centered
                int attackWidth = Mathf.FloorToInt(attackSteps.x / 2);
                targetedGrid -= attackWidth;

                if (isSetupPhase)
                {
                    // Empty the list before adding attack positions
                    // List gets cleared everytime because the character can be updating their position before commiting to the attack
                    attackedGridTargets.Clear();

                    for (int attackStepX = 1; attackStepX < attackSteps.x + 1; attackStepX++)
                    {
                        // Handles X steps in the attack
                        if (ValidateGridPosition.CanAttack(savedGridUsedByPreviousCard, targetedGrid, attackStepY))
                        {
                            // Tile based previs disabled for now
                            // GridPositions._GridCubes[targetedGrid].SetIndicatorVisual(isSetupPhase, cardScriptableObject, 0);
                            if (isSetupPhase)
                                attackedGridTargets.Add(GridPositions._GridCubes[targetedGrid]);
                        }
                        targetedGrid++;
                    }
                }
            }

            // Save target location for movement during set up phase
            if (isSetupPhase)
                targetedGridForMovement = savedGridUsedByPreviousCard;

            return savedGridUsedByPreviousCard;
        }
        else if (cardScriptableObject.CardType == CardScriptableObject._CardType.Movement)
        {
            for (int stepLength = 0; stepLength < Mathf.Abs(moveSteps.y); stepLength++)
            {
                // Handles the Y steps in the movement
                targetedGrid = GetMovementGridNumber(targetCharacter, targetedGrid, moveSteps);
                float dirAngle = GetDirectionAngleBetweenGrids(targetedGrid, savedGridUsedByPreviousCard);
                // Tile based previs disabled for now
                // GridPositions._GridCubes[targetedGrid].SetIndicatorVisual(isSetupPhase, cardScriptableObject, dirAngle);
            }
            for (int stepWidth = 0; stepWidth < Mathf.Abs(moveSteps.x); stepWidth++)
            {
                // Handles X steps in the movement
                targetedGrid = GetMovementGridNumber(targetCharacter, targetedGrid, moveSteps);
                float dirAngle = GetDirectionAngleBetweenGrids(targetedGrid, savedGridUsedByPreviousCard);
                // Tile based previs disabled for now
                //GridPositions._GridCubes[targetedGrid].SetIndicatorVisual(isSetupPhase, cardScriptableObject, dirAngle);
            }

            // Save target location for movement during set up phase
            if (isSetupPhase)
                targetedGridForMovement = targetedGrid;
        }

        // Returns the integer of the current used gridnumber, so it's location can be used for the other cards in the sequence
        return targetedGrid;
    }

    private int GetMovementGridNumber(Character targetCharacter, int startingGridNumber, Vector2Int moveSteps)
    {
        int offsetFromInstigator = 0;

        // Move X
        Vector2Int moveIncrement = moveSteps.ConvertVector2IntToIncrement();
        int moveX = moveIncrement.x;
        if (moveX != 0)
        {
            if (ValidateGridPosition.CanStepX(targetCharacter, startingGridNumber, startingGridNumber + moveX))
                offsetFromInstigator += moveX;
        }

        // Move Y
        // Since the grid is generated horizontally; a step in the Y axis needs to take the entire grid with into account
        if (moveSteps.y != 0)
        {
            int moveY = (int)GridPositions._GridSize.x;
            if (moveSteps.y < 0)
                moveY = -moveY;

            if (startingGridNumber + moveY <= GridPositions._GridCubes.Count)
            {
                if (ValidateGridPosition.CanStepY(targetCharacter, startingGridNumber, startingGridNumber + moveY))
                    offsetFromInstigator += moveY;
            }
            else // If the targeted grid number does not exist, fall back to starting grid
                return startingGridNumber;
        }

        return startingGridNumber + offsetFromInstigator;
    }

    private float GetDirectionAngleBetweenGrids(int grid1, int grid2)
    {
        // Calculates the angle between two grids, used for changing the angle of the previs if needed
        Vector3 pos1 = GridPositions._GridCubes[grid1].transform.position;
        Vector3 pos2 = GridPositions._GridCubes[grid2].transform.position;
        float angle = Vector3.Angle(pos1 - pos2, transform.up);

        // Vector3.Angle always returns an absolute number, so checking whether pos1 is left or right of pos2 to see if the angle needs to be set to a negative        
        Vector3 cross = pos1 - pos2;
        if (cross.x > 0)
            angle = -angle;        
        return angle;
    }

    private void HandleMovement(Character instigator)
    {
        instigator.ChangeDestinationGridNumber(targetedGridForMovement);
    }

    private void HandleAttack(Character instigator)
    {
        for (int i = 0; i < attackedGridTargets.Count; i++)
        {
            if (attackedGridTargets[i].CharacterOnThisGrid != null)
                attackedGridTargets[i].CharacterOnThisGrid.ChangeHealth(1, instigator);
            SpawnParticleEffectAtGridCube(instigator, attackedGridTargets[i]);
        }
        attackedGridTargets.Clear();
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
                }
                break;
            case CardScriptableObject._ParticleLocation.OnDamageTiles:
            case CardScriptableObject._ParticleLocation.OnMovementTiles:
                particle = Instantiate(cardScriptableObject.Particle, cube.transform.position, transform.rotation);
                break;
        }

        // Sets the spawned particle in the "Simulation" layer
        // This is needed because the simulation camera renders all simulation based events different than the main camera
        if (particle != null && instigator.InstancedCharacterSimulation == null)
        {
            int layerNumber = LayerMask.NameToLayer("Simulation");
            SetGameLayerRecursive(particle, layerNumber);
        }
    }

    // Cycles through every transform in a gameobject and sets the same layer throughout
    private void SetGameLayerRecursive(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
        {
            SetGameLayerRecursive(child.gameObject, layer);
        }
    }
}
