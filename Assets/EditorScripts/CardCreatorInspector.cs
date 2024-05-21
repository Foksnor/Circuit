using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CardCreator))]
public class CardCreatorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Click on a square to assign it as an attack");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("1"))
        {

        }
        if (GUILayout.Button("2"))
        {

        }
        if (GUILayout.Button("3"))
        {

        }
        if (GUILayout.Button("4"))
        {

        }
        if (GUILayout.Button("5"))
        {

        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("1"))
        {

        }
        if (GUILayout.Button("2"))
        {

        }
        if (GUILayout.Button("3"))
        {

        }
        if (GUILayout.Button("4"))
        {

        }
        if (GUILayout.Button("5"))
        {

        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("1"))
        {

        }
        if (GUILayout.Button("2"))
        {

        }
        if (GUILayout.Button("3"))
        {

        }
        if (GUILayout.Button("4"))
        {

        }
        if (GUILayout.Button("5"))
        {

        }
        EditorGUILayout.EndHorizontal();

        CardCreator cardCreator = (CardCreator)target;
        for (int gY = 0; gY < cardCreator.GridWindow.y; gY++)
        {
            if (GUILayout.Button("4"))
            {

            }
            for (int gX = 0; gX < cardCreator.GridWindow.x; gX++)
            {
                if (GUILayout.Button("testknop"))
                {
                
                }
            }
        }
    }
}
