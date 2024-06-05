using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerDeck
{
    private static List<CardScriptableObject> totalCardsInDeck = new List<CardScriptableObject>();
    private static List<CardScriptableObject> currentCardsInDeck = new List<CardScriptableObject>();
    private static List<CardScriptableObject> currentCardsInDiscard = new List<CardScriptableObject>();
    public static int CardDrawPerTurn = 0;

    public static List<CardScriptableObject> TotalCardsInDeck
    {
        get => totalCardsInDeck;
        set => totalCardsInDeck = value;
    }

    public static List<CardScriptableObject> CurrentCardsInDeck
    {
        get => currentCardsInDeck;
        set => currentCardsInDeck = value;
    }

    public static List<CardScriptableObject> CurrentCardsInDiscard
    {
        get => currentCardsInDiscard;
        set => currentCardsInDiscard = value;
    }
}
