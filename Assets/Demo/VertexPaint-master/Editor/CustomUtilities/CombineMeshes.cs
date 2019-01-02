using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.VertexPainterPro
{
   [System.Serializable]
   public class CombineMeshes : IVertexPainterUtility
   {
      public string GetName() 
      {
         return "Combine Meshes";
      }

      public void OnGUI(PaintJob[] jobs)
      {
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Combine Meshes"))
         {
            VertexPainterUtilities.MergeMeshes(jobs);
         }
         if (GUILayout.Button("Combine and Save"))
         {
            if (jobs.Length != 0)
            {
               string path = EditorUtility.SaveFilePanel("Save Asset", Application.dataPath, "models", "asset");
               if (!string.IsNullOrEmpty(path))
               {
                  path = FileUtil.GetProjectRelativePath(path);
                  GameObject go = VertexPainterUtilities.MergeMeshes(jobs);
                  Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
                  AssetDatabase.CreateAsset(m, path);
                  AssetDatabase.SaveAssets();
                  AssetDatabase.ImportAsset(path);
                  GameObject.DestroyImmediate(go);
               }
            }
         }
         EditorGUILayout.EndHorizontal();
      }



   }
}
