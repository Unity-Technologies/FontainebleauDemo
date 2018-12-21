using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JBooth.VertexPainterPro
{
   public class PaintJob
   {
      public MeshFilter meshFilter;
      public Renderer renderer;
      public VertexInstanceStream _stream;
      // cache of data we often need so we don't have to cross the c#->cpp bridge often 
      public Vector3[] verts;
      public Vector3[] normals;
      public Vector4[] tangents;

      // getters which take stream into account
      public Vector3 GetPosition(int i)
      {
         if (stream.positions != null && stream.positions.Length == verts.Length)
            return stream.positions[i];
         return verts[i];
      }

      public Vector3 GetNormal(int i)
      {
         if (stream.normals != null && stream.normals.Length == verts.Length)
            return stream.normals[i];
         return normals[i];
      }

      public Vector4 GetTangent(int i)
      {
         if (stream.tangents != null && stream.tangents.Length == verts.Length)
            return stream.tangents[i];
         return tangents[i];
      }

      public bool HasStream() { return _stream != null; }
      public bool HasData()
      {
         if (_stream == null)
            return false;

         int vertexCount = verts.Length;
         bool hasColors = (stream.colors != null && stream.colors.Length == vertexCount);
         bool hasUV0 = (stream.uv0 != null && stream.uv0.Count == vertexCount);
         bool hasUV1 = (stream.uv1 != null && stream.uv1.Count == vertexCount);
         bool hasUV2 = (stream.uv2 != null && stream.uv2.Count == vertexCount);
         bool hasUV3 = (stream.uv3 != null && stream.uv3.Count == vertexCount);
         bool hasPositions = (stream.positions != null && stream.positions.Length == vertexCount);
         bool hasNormals = (stream.normals != null && stream.normals.Length == vertexCount);

         return (hasColors || hasUV0 || hasUV1 || hasUV2 || hasUV3 || hasPositions || hasNormals);
      }

      public void EnforceStream()
      {
         if (_stream == null && renderer != null && meshFilter != null)
         {
            _stream = meshFilter.gameObject.AddComponent<VertexInstanceStream>();
         }
      }

      public VertexInstanceStream stream
      {
         get
         {
            if (_stream == null)
            {
               if (meshFilter == null)
               { // object has been deleted
                  return null;
               }
               _stream = meshFilter.gameObject.GetComponent<VertexInstanceStream>();
               if (_stream == null)
               {
                  _stream = meshFilter.gameObject.AddComponent<VertexInstanceStream>();
               }
               else
               {
                  _stream.Apply();
               }
            }
            return _stream;
         }

      }

      public void InitMeshConnections()
      {
         // a half edge representation would be nice, but really just care about adjacentcy for now.. 
         int vertCount = meshFilter.sharedMesh.vertexCount;
         vertexConnections = new List<int>[vertCount];
         for (int i = 0; i < vertexConnections.Length; ++i)
         {
            vertexConnections[i] = new List<int>();
         }
         int[] tris = meshFilter.sharedMesh.triangles;
         for (int i = 0; i < tris.Length; i=i+3)
         {
            int c0 = tris[i];
            int c1 = tris[i + 1];
            int c2 = tris[i + 2];

            List<int> l = vertexConnections[c0];
            if (!l.Contains(c1))
            {
               l.Add(c1);
            }
            if (!l.Contains(c2))
            {
               l.Add(c2);
            }

            l = vertexConnections[c1];
            if (!l.Contains(c2))
            {
               l.Add(c2);
            }
            if (!l.Contains(c0))
            {
               l.Add(c0);
            }

            l = vertexConnections[c2];
            if (!l.Contains(c1))
            {
               l.Add(c1);
            }
            if (!l.Contains(c0))
            {
               l.Add(c0);
            }
         }
      }

      public List<int>[] vertexConnections;

      public PaintJob(MeshFilter mf, Renderer r)
      {
         meshFilter = mf;
         renderer = r;
         _stream = r.gameObject.GetComponent<VertexInstanceStream>();
         verts = mf.sharedMesh.vertices;
         normals = mf.sharedMesh.normals;
         tangents = mf.sharedMesh.tangents;
         // optionally defer this unless the brush is set to position..
         InitMeshConnections();
      }

      public void CaptureMat()
      {
         var r = renderer;
         if (r == null || stream == null)
         {
            return;
         }
         if (r.sharedMaterials != null && r.sharedMaterials.Length > 1)
         {
            stream.originalMaterial = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < r.sharedMaterials.Length; ++i)
            {
               stream.originalMaterial[i] = r.sharedMaterials[i];
            }
         }
         else
         {
            stream.originalMaterial = new Material[1];
            stream.originalMaterial[0] = r.sharedMaterial;
         }
      }
   }
}