using UnityEngine;
using UnityEditor;

namespace LightingTools.LightProbesVolumes
{
    [CustomEditor(typeof(LightProbesVolumeSettings))]
    public class LightProbesVolumeEditor : Editor
    {
        SerializedProperty horizontalSpacing;
        SerializedProperty verticalSpacing;
        SerializedProperty offsetFromFloor;
        SerializedProperty numberOfLayers;
        SerializedProperty fillVolume;
        SerializedProperty followFloor;
        SerializedProperty discardInsideGeometry;
        SerializedProperty drawDebug;

        void OnEnable()
        {
            horizontalSpacing = serializedObject.FindProperty("horizontalSpacing");
            verticalSpacing = serializedObject.FindProperty("verticalSpacing");
            offsetFromFloor = serializedObject.FindProperty("offsetFromFloor");
            numberOfLayers = serializedObject.FindProperty("numberOfLayers");
            fillVolume = serializedObject.FindProperty("fillVolume");
            followFloor = serializedObject.FindProperty("followFloor");
            discardInsideGeometry = serializedObject.FindProperty("discardInsideGeometry");
            drawDebug = serializedObject.FindProperty("drawDebug");
        }

        public override void OnInspectorGUI()
        {
            var volume = (LightProbesVolumeSettings)target;

            serializedObject.Update();
            EditorGUILayout.DelayedFloatField(horizontalSpacing);
            EditorGUILayout.DelayedFloatField(verticalSpacing);
            EditorGUILayout.PropertyField(offsetFromFloor);
            EditorGUILayout.PropertyField(fillVolume);
            serializedObject.ApplyModifiedProperties();
            EditorGUI.BeginDisabledGroup(fillVolume.boolValue);
            EditorGUILayout.PropertyField(numberOfLayers);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(followFloor);
            EditorGUILayout.PropertyField(discardInsideGeometry);
            EditorGUILayout.PropertyField(drawDebug);

            if (GUILayout.Button("Create Light Probes in Selected Volume"))
            {
                volume.Populate();
            }

            serializedObject.ApplyModifiedProperties(); 
        }

        [MenuItem("GameObject/Light/Lightprobes Volume", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject volume = new GameObject("LightprobeVolume");
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(volume, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(volume, "Create " + volume.name);
            Selection.activeObject = volume;
            volume.AddComponent<LightProbesVolumeSettings>();
            volume.GetComponent<BoxCollider>().size = new Vector3(5, 2, 5);
            volume.GetComponent<BoxCollider>().isTrigger = true;
        }
    }
}