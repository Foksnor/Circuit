using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCircuitBoardListener : CircuitBoard
{
    protected override void Awake()
    {
        cardPanel = PlayerUI.CardPanel;
        socketPanel = PlayerUI.SocketPanel;
        base.Awake();
    }

    public override bool IsProcessingCards(Character targetCharacter)
    {
        // Get the current active cards from the player circuit board and insert them in the listener
        ActiveCards.Clear();
        ActiveCards.AddRange(PlayerUI.PlayerCircuitboard.GetActiveCardsList());
                
        return base.IsProcessingCards(targetCharacter);
    }
}