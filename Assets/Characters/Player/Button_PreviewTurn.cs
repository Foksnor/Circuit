using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Button_PreviewTurn : MonoBehaviour, ITurnSequenceTriggerable
{
    private Button button;
    private TransitionTurns tT = null;
    [SerializeField] private GameObject overlayToSpawn = null;
    private GameObject overlayFromSpawn = null;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => TransitionTurn());
        tT = TurnSequence.TransitionTurns;
    }

    private void TransitionTurn()
    {
        tT.TransitionTurn();

        // Spawn overlay when previw is starting, preventing duplicates
        if (overlayFromSpawn == null)
            overlayFromSpawn = Instantiate(overlayToSpawn);

        // Adds the preview actions to the turn triggerables
        HelperFunctions.AddToTurnTrigger(this);
    }

    // ITurnSequenceTriggerable interface
    public void OnEndTurn()
    {
        GameData.Loader.RestartScene();
        HelperFunctions.RemoveFromTurnTrigger(this);
    }

    public void OnEndstep()
    {
        return;
    }

    public void OnStartEnemyTurn()
    {
        return;
    }

    public void OnStartPlayerTurn()
    {
        return;
    }
}
