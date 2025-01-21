using UnityEngine;
using System.Collections.Generic;

public enum _TargetRequirementType
{
    None, ClosestSelfTeam, ClosestAllyNoSelf, ClosestEnemyTeam
};

[CreateAssetMenu(fileName = "GridSelector", menuName = "ScriptableObject/Grid Selector")]
public class GridSelector : ScriptableObject
{
    public List<Vector2Int> SelectedPositions = new(); // Absolute positions
    public List<Vector2Int> RelativeSelectedPositions = new(); // Relative positions
    public Vector2Int? PlayerPosition = null; // Player position
    public _TargetRequirementType AutoTargetType;
    public int MaxRange;
    public int RepeatCountForDifferentCharacters;

    // Update the relative positions based on the player position
    public void UpdateRelativePositions()
    {
        RelativeSelectedPositions.Clear();
        if (PlayerPosition.HasValue)
        {
            foreach (var pos in SelectedPositions)
            {
                RelativeSelectedPositions.Add(pos - PlayerPosition.Value);
            }
        }
        else
        {
            // If no player position, treat absolute positions as relative
            RelativeSelectedPositions.AddRange(SelectedPositions);
        }
    }
}