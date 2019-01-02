using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.VertexPainterPro
{
   public class VertexPainterUtilities 
   {
      public static GameObject MergeMeshes(PaintJob[] jobs)
      {
         if (jobs.Length == 0)
            return null;
         List<CombineInstance> meshes = new List<CombineInstance>();
         for (int i = 0; i < jobs.Length; ++i)
         {
            Mesh m = BakeDownMesh(jobs[i].meshFilter.sharedMesh, jobs[i].stream);
            CombineInstance ci = new CombineInstance();
            ci.mesh = m;
            ci.transform = jobs[i].meshFilter.transform.localToWorldMatrix;
            meshes.Add(ci);
         }

         Mesh mesh = new Mesh();
         mesh.CombineMeshes(meshes.ToArray());
         GameObject go = new GameObject("Combined Mesh");
         go.AddComponent<MeshRenderer>();
         var mf = go.AddComponent<MeshFilter>();
         ;
         mesh.RecalculateBounds();
         mesh.UploadMeshData(false);
         mf.sharedMesh = mesh;
         for (int i = 0; i < meshes.Count; ++i)
         {
            GameObject.DestroyImmediate(meshes[i].mesh);
         }
         return go;
      }

      // copy a mesh, and bake it's vertex stream into the mesh data. 
      public static Mesh BakeDownMesh(Mesh mesh, VertexInstanceStream stream)
      {
         var copy = GameObject.Instantiate(mesh);

         copy.colors = stream.colors;
         if (stream.uv0 != null && stream.uv0.Count > 0) { copy.SetUVs(0, stream.uv0); }
         if (stream.uv1 != null && stream.uv1.Count > 0) { copy.SetUVs(1, stream.uv1); }
         if (stream.uv2 != null && stream.uv2.Count > 0) { copy.SetUVs(2, stream.uv2); }
         if (stream.uv3 != null && stream.uv3.Count > 0) { copy.SetUVs(3, stream.uv3); }

         if (stream.positions != null && stream.positions.Length == copy.vertexCount)
         {
            copy.vertices = stream.positions;
         }
         if (stream.normals != null && stream.normals.Length == copy.vertexCount)
         {
            copy.normals = stream.normals;
         }
         if (stream.tangents != null && stream.tangents.Length == copy.vertexCount)
         {
            copy.tangents = stream.tangents;
         }
         ;
         copy.RecalculateBounds();
         copy.UploadMeshData(false);

         return copy;
      }


      public static void SaveMesh(PaintJob[] jobs)
      {
         if (jobs.Length != 0)
         {
            string path = EditorUtility.SaveFilePanel("Save Asset", Application.dataPath, "models", "asset");
            if (!string.IsNullOrEmpty(path))
            {
               path = FileUtil.GetProjectRelativePath(path);
               Mesh firstMesh = BakeDownMesh(jobs[0].meshFilter.sharedMesh, jobs[0].stream);

               AssetDatabase.CreateAsset(firstMesh, path);

               for (int i = 1; i < jobs.Length; ++i)
               {
                  Mesh m = BakeDownMesh(jobs[i].meshFilter.sharedMesh, jobs[i].stream);
                  AssetDatabase.AddObjectToAsset(m, firstMesh);
               }
               AssetDatabase.SaveAssets();
               AssetDatabase.ImportAsset(path);
            }
         }
      }
   }
}