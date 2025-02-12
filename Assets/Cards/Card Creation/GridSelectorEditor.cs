using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridSelector))]
public class GridSelectorEditor : Editor
{
    private const int GridSize = 5; // 5x5 grid
    private const int ButtonSize = 30; // Size of each button

    public override void OnInspectorGUI()
    {
        // Get the target ScriptableObject
        GridSelector gridSelector = (GridSelector)target;

        // SerializedObject to handle properties
        SerializedObject serializedObject = new SerializedObject(gridSelector);
        serializedObject.Update();

        // Update the relative positions
        gridSelector.UpdateRelativePositions();

        // Serialized properties for the additional variables
        SerializedProperty autoTargetType = serializedObject.FindProperty("AutoTargetType");
        SerializedProperty maxRange = serializedObject.FindProperty("MaxRange");
        SerializedProperty repeatCount = serializedObject.FindProperty("RepeatCountForDifferentCharacters");

        // Display the grid buttons (fixed layout with absolute positions)
        GUILayout.Label("Grid Selector", EditorStyles.boldLabel);
        for (int y = 2; y >= -2; y--) // Iterate rows (top to bottom)
        {
            GUILayout.BeginHorizontal();
            for (int x = -2; x <= 2; x++) // Iterate columns (left to right)
            {
                // Calculate the absolute position of this button
                Vector2Int absolutePosition = new Vector2Int(x, y);

                // Check if this position is selected or the instigator position
                bool isSelected = gridSelector.SelectedPositions.Contains(absolutePosition);
                bool isInstigatorPosition = !gridSelector.InstigatorPosition.IsNullOrEmpty() && absolutePosition == gridSelector.InstigatorPosition[0];

                // Set button color
                if (isInstigatorPosition)
                {
                    GUI.backgroundColor = Color.blue; // Instigator position
                }
                else if (isSelected)
                {
                    GUI.backgroundColor = Color.red; // Selected position
                }
                else
                {
                    GUI.backgroundColor = Color.white; // Default
                }

                // Render the button
                if (GUILayout.Button("", GUILayout.Width(ButtonSize), GUILayout.Height(ButtonSize)))
                {
                    if (isInstigatorPosition)
                    {
                        // Change instigator position to a selected position (red)
                        gridSelector.InstigatorPosition.Clear();
                        if (!gridSelector.SelectedPositions.Contains(absolutePosition))
                        {
                            gridSelector.SelectedPositions.Add(absolutePosition);
                        }
                    }
                    else if (isSelected)
                    {
                        // Remove from selected positions
                        gridSelector.SelectedPositions.Remove(absolutePosition);
                    }
                    else
                    {
                        // Add to selected positions
                        gridSelector.SelectedPositions.Add(absolutePosition);

                        // Set as instigator position if none exists
                        if (gridSelector.InstigatorPosition.IsNullOrEmpty())
                        {
                            gridSelector.InstigatorPosition.Add(absolutePosition);
                            gridSelector.SelectedPositions.Remove(absolutePosition); // Ensure it's not in the selected list
                        }
                    }

                    // Mark the object as dirty for saving changes
                    EditorUtility.SetDirty(gridSelector);
                }
            }
            GUILayout.EndHorizontal();
        }

        // Reset button color to default
        GUI.backgroundColor = Color.white;

        // Display the additional variables
        GUILayout.Space(10);
        GUILayout.Label("Additional Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(maxRange, new GUIContent("Max Range"));
        EditorGUILayout.PropertyField(repeatCount, new GUIContent("Repeat Count for Different Characters"));

        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();

        // Display the current selected positions (absolute)
        GUILayout.Space(10);
        GUILayout.Label("Selected Positions (Absolute):");
        foreach (var pos in gridSelector.SelectedPositions)
        {
            GUILayout.Label($"(x = {pos.x}, y = {pos.y})");
        }

        // Display the current selected positions (relative)
        GUILayout.Space(10);
        GUILayout.Label("Selected Positions (Relative to Instigator Position):");
        foreach (var pos in gridSelector.RelativeSelectedPositions)
        {
            GUILayout.Label($"(x = {pos.x}, y = {pos.y})");
        }

        // Display the current instigator position
        GUILayout.Space(10);
        GUILayout.Label("Instigator Position:");
        if (!gridSelector.InstigatorPosition.IsNullOrEmpty())
        {
            GUILayout.Label($"(x = {gridSelector.InstigatorPosition[0].x}, y = {gridSelector.InstigatorPosition[0].y})");
        }
        else
        {
            GUILayout.Label("None");
        }
    }
}