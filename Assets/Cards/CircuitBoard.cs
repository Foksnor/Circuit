using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CircuitBoard : MonoBehaviour
{
    [SerializeField] protected GameObject cardPanel = null;
    [SerializeField] private GameObject socketPanel = null;
    [SerializeField] protected Card card = null;
    [SerializeField] private CardSocket socket = null;
    protected List<Card> ActiveCards { set; get; } = new ();
    protected List<CardSocket> ActiveSockets { set; get; } = new();
    public List<CardScriptableObject> StartingCards = new();
    private int activeCardNumber = 0;
    private float timeBetweenCardsPlayed = 0;
    public int FireAttacksAvailable { get; private set; }
    public int ShockAttacksAvailable { get; private set; }
    [SerializeField] private List<GridCube> savedMovementGridCubes = new();
    protected bool NeedsNewCardCalculation { set; get; } = false;

    protected virtual void Awake()
    {
        SetUpCircuitBoard();
        SetCardsInCircuit();

        // Sets the new socket reference to the card
        for (int i = 0; i < ActiveCards.Count; i++)
            ActiveCards[i].ConnectToSocket(ActiveSockets[i]);
    }

    private void Update()
    {
        timeBetweenCardsPlayed -= Time.deltaTime;
    }

    private void SetUpCircuitBoard()
    {
        // Adds card slots
        for (int i = 0; i < StartingCards.Count; i++)
        {
            ActiveCards.Add(Instantiate(card, cardPanel.transform));
            ActiveSockets.Add(Instantiate(socket, socketPanel.transform));
        }
    }

    private void SetCardsInCircuit()
    {
        // Sets the card info per card in the circuit board
        for (int i = 0; i < ActiveCards.Count; i++)
        {
            ActiveCards[i].SetCardInfo(StartingCards[i], this, false);
        }
    }

    public virtual bool IsProcessingCards(Character targetCharacter)
    {
        if (timeBetweenCardsPlayed > 0)
            return true;

        if (activeCardNumber < ActiveCards.Count)
        {
            ActiveCards[activeCardNumber].ActivateCard(targetCharacter);
            timeBetweenCardsPlayed = ActiveCards[activeCardNumber].MaxTimeInUse;
            activeCardNumber++;
            return true;
        }

        // After all cards have been processed, deactivate them
        if (activeCardNumber == ActiveCards.Count)
        {
            for (int i = 0; i < ActiveCards.Count; i++)
            {
                ActiveCards[i].DeactivateCard();
            }
            activeCardNumber = 0;
            return false;
        }
        return true;
    }

    public void ResetCardProcessing()
    {
        timeBetweenCardsPlayed = 0;
        activeCardNumber = 0;
        for (int i = 0; i < ActiveCards.Count; i++)
        {
            ActiveCards[i].DeactivateCard();
        }

        // Reset movement priority references
        for (int i = 0; i < savedMovementGridCubes.Count; i++)
        {
            savedMovementGridCubes[i].ResetCharacterMovementPriority();
        }
    }

    public void CalculateAllCards(Character targetCharacter, bool isSetupPhase)
    {
        GridCube previsGrid = targetCharacter.AssignedGridCube;
        for (int cardNumber = 0; cardNumber < ActiveCards.Count; cardNumber++)
        {
            if (isSetupPhase)
            {
                AllignCardOnCircuitBoard(cardNumber);
                previsGrid = ActiveCards[cardNumber].CalculateGridCubeDestination(targetCharacter, previsGrid, cardNumber, isSetupPhase);
            }
            ToggleInteractableCardStateOnCircuitBoard(cardNumber, isSetupPhase);
        }

        // Remove temporary references after calculation
        for (int cardNumber = 0; cardNumber < ActiveCards.Count; cardNumber++)
        {
            // Remove potental kill references, used by movement calculation
            for (int i = 0; i < ActiveCards[cardNumber].potentialKillTargets.Count; i++)
                ActiveCards[cardNumber].potentialKillTargets[i].RemovePotentialKillMark();
            ActiveCards[cardNumber].potentialKillTargets.Clear();
        }
    }

    public void AddBuff(Character targetCharacter, CardScriptableObject._CardType cardType, int buffAmount)
    {
        switch (cardType)
        {
            case CardScriptableObject._CardType.ElementFire:
                FireAttacksAvailable += buffAmount;
                break;
            case CardScriptableObject._CardType.ElementShock:
                ShockAttacksAvailable += buffAmount;
                break;
        }
    }

    public bool UseAvailableBuff(CardScriptableObject._CardType cardType)
    {
        switch (cardType)
        {
            case CardScriptableObject._CardType.ElementFire:
                if (FireAttacksAvailable > 0)
                {
                    FireAttacksAvailable -= 1;
                    return true;
                }
                break;
            case CardScriptableObject._CardType.ElementShock:
                if (ShockAttacksAvailable > 0)
                {
                    ShockAttacksAvailable -= 1;
                    return true;
                }
                break;
        }
        return false;
    }

    public void SaveMovementGridCube(GridCube gridCube)
    {
        if (savedMovementGridCubes.Contains(gridCube))
            return;

        // Save grid cube reference so the priority of movement can be removed later after restarting simulations
        savedMovementGridCubes.Add(gridCube);
    }

    public virtual void PlayerDrawPhase()
    {
    }

    public virtual void PlaceCardInSocket(Card newCard, CardSocket socket)
    {
    }

    public virtual void RemoveFromSocket(Card card)
    {
    }

    public virtual void SetCircuitDisplayTimer(string timerDisplayText, float timerDisplayFillAmount)
    {
    }

    protected virtual void AllignCardOnCircuitBoard(int cardNumber)
    {
    }

    protected virtual void ToggleInteractableCardStateOnCircuitBoard(int cardNumber, bool isInteractable)
    {
    }
}