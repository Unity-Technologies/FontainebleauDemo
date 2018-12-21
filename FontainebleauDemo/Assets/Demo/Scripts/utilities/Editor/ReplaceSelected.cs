/* This wizard will replace a selection with an object or prefab.
 * Scene objects will be cloned (destroying their prefab links).
 * Original coding by 'yesfish', nabbed from Unity Forums
 * 'keep parent' added by Dave A (also removed 'rotation' option, using localRotation
 * Updated with new APIs (prefabutility and undo system)
 */
using UnityEngine;
using UnityEditor;
using System.Collections;
using JBooth.VertexPainterPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class ReplaceSelected : ScriptableWizard
{
    static GameObject replacement = null;
    static bool keep = false;

    public GameObject ReplacementObject = null;
    public bool KeepOriginals = false;

    [MenuItem("Tools/RemoveMissingScript")]
    static void RemoveMissingScript()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in allObjects)
        {
            // We must use the GetComponents array to actually detect missing components
            var components = gameObject.GetComponents<Component>();

            // Create a serialized object so that we can edit the component list
            var serializedObject = new SerializedObject(gameObject);
            // Find the component list property
            var prop = serializedObject.FindProperty("m_Component");

            // Track how many components we've removed
            int r = 0;

            // Iterate over all components
            for (int j = 0; j < components.Length; j++)
            {
                // Check if the ref is null
                if (components[j] == null)
                {
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);

                    // If so, remove from the serialized component array
                    prop.DeleteArrayElementAtIndex(j - r);
                    // Increment removed count
                    r++;
                }
            }

            // Apply our changes to the game object
            serializedObject.ApplyModifiedProperties();
        }
    }

    [MenuItem("Tools/Disable StaticBatching when VertexColor stream is present")]
    static void FixStaticBatchingAndVertexColor()
    {
        VertexInstanceStream[] components = FindObjectsOfType<VertexInstanceStream>();
        foreach (JBooth.VertexPainterPro.VertexInstanceStream item in components)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(item.gameObject);
            flags = flags & ~(StaticEditorFlags.BatchingStatic);
            GameObjectUtility.SetStaticEditorFlags(item.gameObject, flags);
            EditorUtility.SetDirty(item.gameObject);
        }

        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
    }

    [MenuItem("Tools/Replace Selection... _%#R")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Replace Selection", typeof(ReplaceSelected), "Replace");
    }

    public ReplaceSelected()
    {
        ReplacementObject = replacement;
        KeepOriginals = keep;
    }

    void OnWizardUpdate()
    {
        replacement = ReplacementObject;
        keep = KeepOriginals;
    }

    void OnWizardCreate()
    {
        if (replacement == null)
            return;

        //Undo.RegisterSceneUndo("Replace Selection");
        //Undo.RegisterCreatedObjectUndo(global, "Undo Replacement");

        Transform[] transforms = Selection.GetTransforms(
            SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);

        foreach (Transform t in transforms)
        {
            GameObject g;
            PrefabType pref = PrefabUtility.GetPrefabType(replacement);

            if (pref == PrefabType.Prefab || pref == PrefabType.ModelPrefab)
            {
                g = (GameObject)PrefabUtility.InstantiatePrefab(replacement);
            }
            else
            {
                g = (GameObject)Editor.Instantiate(replacement);
            }

            Transform gTransform = g.transform;
            gTransform.parent = t.parent;
            g.name = replacement.name;
            gTransform.localPosition = t.localPosition;
            gTransform.localScale = t.localScale;
            gTransform.localRotation = t.localRotation;

            Undo.RegisterCreatedObjectUndo(g, "Undo Replacement");
        }

        if (!keep)
        {
            foreach (GameObject g in Selection.gameObjects)
            {
                Undo.DestroyObjectImmediate(g);
            }
        }
    }
}