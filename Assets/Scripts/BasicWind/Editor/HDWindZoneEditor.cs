using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.HighDefinition;

namespace HDRPSamples
{
    [CustomEditorForRenderPipeline(typeof(WindZone), typeof(HDRenderPipelineAsset))]
    sealed partial class BasicWindZoneEditor : Editor
    {
        SerializedProperty windMain;
        SerializedProperty windTurbulence;

        void OnEnable()
        {
            windMain = serializedObject.FindProperty("m_WindMain");
            windTurbulence = serializedObject.FindProperty("m_WindTurbulence");
            WindZone windZone = target as WindZone;
            if (!windZone.GetComponent<BasicWindData>())
                windZone.gameObject.AddComponent<BasicWindData>();
            if (GameObject.FindObjectsOfType<BasicWindData>().Length > 1)
                Debug.LogWarning("Your setup has more than one WindZone. Only one is supported by Basic Wind.");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("Only one directional wind zone is supported by Basic Wind.", MessageType.Info);
            EditorGUILayout.PropertyField(windMain);
            EditorGUILayout.Slider(windTurbulence, 0, 1);
            serializedObject.ApplyModifiedProperties();
        }
    }
}