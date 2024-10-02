using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Button_PreviewTurn : MonoBehaviour, ITurnSequenceTriggerable
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

        // Adds the preview actions to the turn triggerables
        if (!TurnSequence.TransitionTurns.TurnSequenceTriggerables.Contains(this))
            TurnSequence.TransitionTurns.TurnSequenceTriggerables.Add(this);
    }

    // ITurnSequenceTriggerable interface
    public void OnEndTurn()
    {
        GameData.Loader.RestartScene();
        TurnSequence.TransitionTurns.TurnSequenceTriggerables.Remove(this);
    }

    public void OnStartEnemyTurn()
    {
        return;
    }

    public void OnStartPlayerTurn()
    {
        GameData.Loader.SaveGameState();
    }
}
