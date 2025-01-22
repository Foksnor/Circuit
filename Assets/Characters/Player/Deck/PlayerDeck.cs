using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public List<CardScriptableObject> TotalCardsInDeck { set; get; } = new();
    public List<CardScriptableObject> CurrentCardsInPlay { set; get; } = new();
    public List<CardScriptableObject> CurrentCardsInDeck { set; get; } = new();
    public List<CardScriptableObject> CurrentCardsInDiscard { set; get; } = new();
    public List<CardScriptableObject> AllPossibleAvailableCards { set; get; } = new();
    public int CardDrawPerTurn {set; get; } = 5;

    private void Awake()
    {
        Decks.Playerdeck = this;
        UpdatePossibleAvailableCards();
    }

    private void UpdatePossibleAvailableCards()
    {
        PlayerLevelScriptableObject[] playerlevels = PlayerUI.ExperienceBar.Playerlevels;
        for (int i = 0; i < playerlevels.Length; i++)
        {
            AllPossibleAvailableCards.AddRange(playerlevels[i].possibleNewCardRewards);
        }
    }
}

public static class Decks
{
    public static PlayerDeck Playerdeck;
}
