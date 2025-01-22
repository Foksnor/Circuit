using UnityEngine;
using System.Collections.Generic;
using Sirenix.Utilities;

public enum _TargetRequirementType
{
    None, ClosestSelfTeam, ClosestAllyNoSelf, ClosestEnemyTeam
};

[CreateAssetMenu(fileName = "GridSelector", menuName = "ScriptableObject/Grid Selector")]
public class GridSelector : ScriptableObject
{
    public List<Vector2Int> SelectedPositions = new();
    public List<Vector2Int> RelativeSelectedPositions = new();
    public List<Vector2Int> InstigatorPosition = new(); // List, because Unity doesn't like nullable vectorInt
    public _TargetRequirementType AutoTargetType;
    public int MaxRange;
    public int RepeatCountForDifferentCharacters;

    // Update the relative positions based on the player position
    public void UpdateRelativePositions()
    {
        RelativeSelectedPositions.Clear();
        if (!InstigatorPosition.IsNullOrEmpty())
        {
            foreach (var pos in SelectedPositions)
            {
                RelativeSelectedPositions.Add(pos - InstigatorPosition[0]);
            }
        }
        else
        {
            // If no player position, treat absolute positions as relative
            RelativeSelectedPositions.AddRange(SelectedPositions);
        }
    }
}