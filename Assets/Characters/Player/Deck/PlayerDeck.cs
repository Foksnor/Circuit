using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerDeck
{
    // QQQ TODO change these lists into non-static
    public static List<CardScriptableObject> TotalCardsInDeck { set; get; } = new List<CardScriptableObject>();
    public static List<CardScriptableObject> CurrentCardsInDeck { set; get; } = new List<CardScriptableObject>();
    public static List<CardScriptableObject> CurrentCardsInDiscard { set; get; } = new List<CardScriptableObject>();
    public static int CardDrawPerTurn = 0;
}
