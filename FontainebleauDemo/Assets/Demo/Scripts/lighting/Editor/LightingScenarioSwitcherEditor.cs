using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightingScenarioSwitcher))]
public class LightingScenarioSwitcherEditor : Editor
{
    public int index;

    public void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        LightingScenarioSwitcher switcher = (LightingScenarioSwitcher)target;

        index = EditorGUILayout.IntField(new GUIContent("scenario index"),index);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        if (GUILayout.Button("Load lighting scenario in editor"))
        {
            switcher.loadLightingScenario(index);
        }

    }
}
