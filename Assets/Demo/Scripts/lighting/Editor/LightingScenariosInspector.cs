using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelLightmapData))]
public class LightinScenariosInspector : Editor
{
	public SerializedProperty lightingScenariosScenes;

    public void OnEnable()
    {
		lightingScenariosScenes = serializedObject.FindProperty("lightingScenariosScenes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

		LevelLightmapData lightmapData = (LevelLightmapData)target;

        EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(lightingScenariosScenes, new GUIContent("Lighting Scenarios Scenes"), includeChildren:true);
        if(EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            lightmapData.updateNames();
        }
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        for (int i = 0; i < lightmapData.lightingScenariosScenes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if ( lightmapData.lightingScenariosScenes[i] != null )
            {
                EditorGUILayout.LabelField(lightmapData.lightingScenariosScenes[i].name.ToString(), EditorStyles.boldLabel);
                if (GUILayout.Button("Build "))
                {
                    lightmapData.BuildLightingScenario(i);
                }
                if (GUILayout.Button("Store "))
                {
                    lightmapData.StoreLightmapInfos(i);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
