using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_TransitionTurn : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => TransitionTurn());
    }

    private void TransitionTurn()
    {
        TurnSequence.TransitionTurns.TransitionTurn();
    }
}
