using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_TransitionTurn : MonoBehaviour
{
    private Button button;
    private TransitionTurns tT = null;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => TransitionTurn());
        tT = TurnSequence.TransitionTurns;
    }

    private void TransitionTurn()
    {
        tT.TransitionTurn();
    }
}
