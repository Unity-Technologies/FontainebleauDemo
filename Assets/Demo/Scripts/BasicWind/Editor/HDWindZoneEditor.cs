using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace Fontainebleau
{
    [CustomEditorForRenderPipeline(typeof(WindZone), typeof(HDRenderPipelineAsset))]
    sealed partial class HDWindZoneEditor : Editor
    {
        SerializedProperty windMain;
        SerializedProperty windTurbulence;

        void OnEnable()
        {
            windMain = serializedObject.FindProperty("m_WindMain");
            windTurbulence = serializedObject.FindProperty("m_WindTurbulence");
            WindZone windZone = target as WindZone;
            if (!windZone.GetComponent<AdditionalWindData>())
                windZone.gameObject.AddComponent<AdditionalWindData>();
            if (GameObject.FindObjectsOfType<AdditionalWindData>().Length > 1)
                Debug.LogWarning("Your setup has more than one WindZone. Only one is supported by Basic Wind.");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("Only directional mode is supported at the moment", MessageType.Info);
            EditorGUILayout.PropertyField(windMain);
            EditorGUILayout.Slider(windTurbulence, 0, 1);
            serializedObject.ApplyModifiedProperties();
        }
    }
}