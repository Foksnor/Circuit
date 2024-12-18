using UnityEngine;

[CreateAssetMenu(fileName = "FeedbackPanelScriptableObject", menuName = "ScriptableObject/New Interaction feedback for panel")]
public class FeedbackPanelScriptableObject : ScriptableObject
{
    public _FeedbackType FeedbackType;
    public enum _FeedbackType { CannotInteractCardDuringEnemyTurn, LifeLost };
    public string FeedbackText = null;
    public float FeedbackDuration = 2;
}
