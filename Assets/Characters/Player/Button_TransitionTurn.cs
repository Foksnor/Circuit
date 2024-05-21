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
        
        tT = GameObject.FindWithTag("GameManager")?.GetComponent<TransitionTurns>();
        if (tT == null)
            Debug.LogError("Cannot find gameobject with tag GameManager and with a TransitionTurns script component on it.");
    }

    private void TransitionTurn()
    {
        tT.TransitionTurn();
    }
}
