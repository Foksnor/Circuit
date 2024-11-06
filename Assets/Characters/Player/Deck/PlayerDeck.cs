using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public List<CardScriptableObject> TotalCardsInDeck { set; get; } = new();
    public List<CardScriptableObject> CurrentCardsInHand { set; get; } = new();
    public List<CardScriptableObject> CurrentCardsInDeck { set; get; } = new();
    public List<CardScriptableObject> CurrentCardsInDiscard { set; get; } = new();
    public List<CardScriptableObject> AllPossibleAvailableCards { set; get; } = new();
    public PlayerHandPanel HandPanel { set; get; }
    public int CardDrawPerTurn {set; get; } = 5;

    private void Awake()
    {
        Decks.Playerdeck = this;
    }
}

public static class Decks
{
    public static PlayerDeck Playerdeck;
}
