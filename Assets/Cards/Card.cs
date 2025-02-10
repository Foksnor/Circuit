using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Reactor.Animators;

public class Card : MonoBehaviour
{
    public string CardId { get; private set; }
    private CardScriptableObject cardScriptableObject = null;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image cardBackground, cardimage;
    [SerializeField] private Material foilMat, goldenMat;
    [SerializeField] private AudioClip sound;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Animator feedbackAnimator;
    public Animator cardAnimator;
    public Card_PointerInteraction CardPointerInteraction;
    private Transform targetSelfDestructDestination;
    public float MaxTimeInUse = 0.5f;
    //public float MaxTimeInUse { get; private set; } = 0.5f;
    private bool isCardActivated { get; set; } = false;
    [SerializeField] private List<Image> rotatableImageMaterial = new();

    public bool IsCardVisible { get; private set; } = false;
    public CardSocket ConnectedSocket { get; private set; }
    public CircuitBoard ConnectedCircuitboard { get; private set; }

    public void SetCardInfo(CardScriptableObject scriptableObject, CircuitBoard owner, bool isVisible)
    {
        CardId = Guid.NewGuid().ToString();
        cardScriptableObject = scriptableObject;
        ConnectedCircuitboard = owner;
        IsCardVisible = isVisible;

        if (isVisible)
        {
            nameText.text = cardScriptableObject.CardName;
            cardimage.sprite = cardScriptableObject.Sprite;
            descriptionText.text = cardScriptableObject.Description;

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
        MaxTimeInUse = cardScriptableObject.TimeInUse;

        if (instigator != null)
        {
            List<CardActionData> actions = cardScriptableObject.ActionSequence.Actions;
            GridSelector targets = cardScriptableObject.ActionSequence.TargetRequirement;

            // Process all actions, including nested ActionSequences
            ProcessActions(instigator, this, actions, targets);
        }
    }

    public void ProcessActions(Character instigator, Card card, List<CardActionData> actions, GridSelector targets)
    {
        ExecuteActionSequence(instigator, card, actions, targets);
    }

    private void ExecuteActionSequence(Character instigator, Card card, List<CardActionData> actions, GridSelector targets)
    {
        foreach (CardActionData action in actions)
        {
            if (action.CardAction == _CardAction.ActionSequence && action.ActionSequence != null)
            {
                // Recursively process nested ActionSequences
                ExecuteActionSequence(instigator, card, action.ActionSequence.Actions, action.ActionSequence.TargetRequirement);
            }
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

                // Trigger the actual action
                CardActions.Instance.CallAction(instigator, card, action.CardAction, value, targets);
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

    public Vector2Int RotateToNearestTarget(GridCube targetGrid, GridCube savedGridUsedByPreviousCard, Vector2Int steps)
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