using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelLightmapData))]
public class LightinScenariosInspector : Editor
{
	public SerializedProperty lightingScenariosScenes;
    public SerializedProperty clearCache;

    public void OnEnable()
    {
		lightingScenariosScenes = serializedObject.FindProperty("lightingScenariosScenes");
        clearCache = serializedObject.FindProperty("clearCache");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

		LevelLightmapData lightmapData = (LevelLightmapData)target;

        EditorGUILayout.PropertyField(clearCache, new GUIContent("Clear cache before bake"));
        EditorGUILayout.PropertyField(lightingScenariosScenes, new GUIContent("Lighting Scenarios Scenes"), includeChildren:true);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        var ScenariosCount = new int();

        if ( lightmapData.lightingScenariosScenes != null )
        {
            ScenariosCount = lightmapData.lightingScenariosScenes.Count;
        }
        else
        {
            ScenariosCount = 0;
        }

        for (int i = 0; i < ScenariosCount; i++)
        {
            if ( lightmapData.lightingScenariosScenes[i] != null )
            {
                if (GUILayout.Button("Build " + lightmapData.lightingScenariosScenes[i]))
                {
                    lightmapData.BuildLightingScenario(lightmapData.lightingScenariosScenes[i]);
                    //lightmapData.StoreLightmapInfos(i);
                }
            }
            if (lightmapData.lightingScenariosScenes[i] != null)
            {
                if (GUILayout.Button("Store " + lightmapData.lightingScenariosScenes[i]))
                {
                    lightmapData.StoreLightmapInfos(i);
                }
            }

        }

        if (GUILayout.Button("Build all"))
        {
            lightmapData.BuildLightingScenario(lightmapData.lightingScenariosScenes[0]);
            lightmapData.StoreLightmapInfos(0);
            lightmapData.BuildLightingScenario(lightmapData.lightingScenariosScenes[1]);
            lightmapData.StoreLightmapInfos(1);
            lightmapData.BuildLightingScenario(lightmapData.lightingScenariosScenes[2]);
            lightmapData.StoreLightmapInfos(2);
        }
    }
}
