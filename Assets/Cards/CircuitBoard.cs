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
    [SerializeField] protected Card cardSmall = null;
    [SerializeField] private CardSocket socket = null;
    [HideInInspector] public List<Card> activeCards = new List<Card>();
    [HideInInspector] public List<CardSocket> activeSockets = new List<CardSocket>();
    public List<CardScriptableObject> startingCards = new List<CardScriptableObject>();
    private int activeCardNumber = 0;
    private float timeBetweenCardsPlayed = 0;
    private bool isReadyToProcessSimulation = true;

    protected bool needsNewCardCalculation { set; get; } = false;

    protected virtual void Awake()
    {
        SetUpCircuitBoard();
        SetCardsInCircuit();
        UpdateCardOrder();
    }

    private void Update()
    {
        timeBetweenCardsPlayed -= Time.deltaTime;
    }

    private void SetUpCircuitBoard()
    {
        // Adds card slots
        for (int i = 0; i < startingCards.Count; i++)
        {
            activeCards.Add(Instantiate(cardSmall, cardPanel.transform));
            activeSockets.Add(Instantiate(socket, socketPanel.transform));
        }
    }

    private void SetCardsInCircuit()
    {
        // Sets the card info per card in the circuit board
        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].SetCardInfo(startingCards[i], this, false);
        }
    }

    public virtual bool IsProcessingCards(Character targetCharacter)
    {
        if (timeBetweenCardsPlayed > 0)
            return true;

        if (activeCardNumber < activeCards.Count)
        {
            activeCards[activeCardNumber].ActivateCard(targetCharacter);
            timeBetweenCardsPlayed = activeCards[activeCardNumber].MaxTimeInUse;
            activeCardNumber++;
            return true;
        }

        // After all cards have been processed, deactivate them
        if (activeCardNumber == activeCards.Count)
        {
            for (int i = 0; i < activeCards.Count; i++)
            {
                activeCards[i].DeactivateCard();
            }
            activeCardNumber = 0;
            return false;
        }
        return true;
    }

    public bool IsProcessingSimulation(Character targetCharacter)
    {
        // Only process simulation when it's ready, and wait when there are enemy simulations happening
        if (isReadyToProcessSimulation)
        {
            isReadyToProcessSimulation = IsProcessingCards(targetCharacter.InstancedCharacterSimulation);
            // Sets highlight to false once the function "IsProcessingCards" finished
            targetCharacter.InstancedCharacterSimulation.ToggleCharacterSimHighlight(isReadyToProcessSimulation);
        }

        if (needsNewCardCalculation)
        {
            ResetCardProcessing();
            targetCharacter.RefreshCharacterSimulation();
            needsNewCardCalculation = false;
        }
        return isReadyToProcessSimulation;
    }

    public void ResetCardProcessing()
    {
        timeBetweenCardsPlayed = 0;
        activeCardNumber = 0;
        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].DeactivateCard();
        }
    }

    public void ResetSimulationProcessing(Character targetCharacter)
    {
        targetCharacter.RefreshCharacterSimulation();
        isReadyToProcessSimulation = true;
    }

    public void CalculateAllCards(Character targetCharacter, bool isSetupPhase)
    {
        targetCharacter.ToggleCharacterSimulation(isSetupPhase);
        int previsGridNumber = targetCharacter.PositionInGrid;
        for (int cardNumber = 0; cardNumber < activeCards.Count; cardNumber++)
        {
            if (isSetupPhase)
            {
                UpdateCardOrder();
                AllignCardOnCircuitBoard(cardNumber);
            }
            // TODO: voeg toe dat spelers aan het begin van hun beurt een nieuwe hand van kaarten uit hun deck trekt, en dan er een mogen kiezen, daarna moet deze onderstaande functie aangeroepen worden. :)
            previsGridNumber = activeCards[cardNumber].CalculateCard(targetCharacter, previsGridNumber, isSetupPhase);
            ToggleInteractableCardStateOnCircuitBoard(cardNumber, isSetupPhase);
        }
    }

    public virtual void PlayerDrawPhase()
    {
    }

    public virtual void ReplaceCardInCircuit(Card newCard, Card cardToReplace)
    {
    }

    public virtual void SetCircuitDisplayTimer(string timerDisplayText, float timerDisplayFillAmount)
    {
    }

    protected virtual void UpdateCardOrder()
    {
    }

    protected virtual void AllignCardOnCircuitBoard(int cardNumber)
    {
    }

    protected virtual void ToggleInteractableCardStateOnCircuitBoard(int cardNumber, bool isInteractable)
    {
    }
}