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

    protected virtual void Awake()
    {
        SetUpCircuitBoard();
        SetCardsInCircuit();
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

        // Sets the new socket reference to the card
        for (int i = 0; i < ActiveCards.Count; i++)
            ActiveCards[i].ConnectToSocket(ActiveSockets[i]);
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
            ActiveCards[activeCardNumber].CalculateGridCubeDestination(targetCharacter, false);
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

    public virtual void PlaceCardInSocket(Card newCard, CardSocket socket)
    {
        newCard.transform.SetParent(cardPanel.transform);
        newCard.ConnectToSocket(socket);
        newCard.CardPointerInteraction.AssignPosition(socket.transform.position);
    }

    public virtual void RemoveFromSocket(Card card)
    {
        card.RemoveFromSocket();
    }

    public virtual void SetCircuitDisplayTimer(string timerDisplayText, float timerDisplayFillAmount)
    {
    }
}