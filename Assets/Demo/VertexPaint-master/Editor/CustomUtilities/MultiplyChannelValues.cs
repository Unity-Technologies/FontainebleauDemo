using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.VertexPainterPro
{
   public class MultiplyChannelValues : IVertexPainterUtility 
   {
      public string GetName()
      {
         return "Multiply Channel Values";
      }

      public void OnGUI(PaintJob[] jobs)
      {
         EditorGUILayout.HelpBox("Multiply a channel by a value. Useful for scaling UVs, objects, etc", MessageType.Info);
         bakeChannel = (BakeChannel)EditorGUILayout.EnumPopup("Channel", bakeChannel);
         multValue = EditorGUILayout.FloatField("Multiply By", multValue);
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Multiply It"))
         {
            Bake(jobs);
         }
         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();
      }
         

      public enum BakeChannel
      {
         Position = 0,
         Color,
         UV0,
         UV1, 
         UV2,
         UV3
      }



      float multValue;
      BakeChannel bakeChannel = BakeChannel.Color;


      void InitBakeChannel(BakeChannel bc, PaintJob[] jobs)
      {
         foreach (PaintJob job in jobs)
         {
            if (bc == BakeChannel.Position)
            {
               if (job.stream.positions == null || job.stream.positions.Length != job.verts.Length)
               {
                  job.stream.positions = job.verts;
               }
            }
            if (bc == BakeChannel.Color)
            {
               if (job.stream.colors == null || job.stream.colors.Length != job.verts.Length)
               {
                  job.stream.colors = job.meshFilter.sharedMesh.colors;
               }
            }
            else if (bc == BakeChannel.UV0)
            {
               if (job.stream.uv0 == null || job.stream.uv0.Count!= job.verts.Length)
               {
                  job.stream.uv0 = new List<Vector4>(job.verts.Length);
                  job.meshFilter.sharedMesh.GetUVs(0, job.stream.uv0);
               }
            }
            else if (bc == BakeChannel.UV1)
            {
               if (job.stream.uv1 == null || job.stream.uv1.Count != job.verts.Length)
               {
                  job.stream.uv1 = new List<Vector4>(job.verts.Length);
                  job.meshFilter.sharedMesh.GetUVs(0, job.stream.uv1);
               }
            }
            else if (bc == BakeChannel.UV2)
            {
               if (job.stream.uv2 == null || job.stream.uv2.Count != job.verts.Length)
               {
                  job.stream.uv2 = new List<Vector4>(job.verts.Length);
                  job.meshFilter.sharedMesh.GetUVs(0, job.stream.uv2);
               }
            }
            else if (bc == BakeChannel.UV3)
            {
               if (job.stream.uv3 == null || job.stream.uv3.Count != job.verts.Length)
               {
                  job.stream.uv3 = new List<Vector4>(job.verts.Length);
                  job.meshFilter.sharedMesh.GetUVs(0, job.stream.uv3);
               }
            }
            EditorUtility.SetDirty(job.stream);
            EditorUtility.SetDirty(job.stream.gameObject);
         }
      }

      void Bake(PaintJob[] jobs)
      {
         // make sure we have the channels we're baking to..
         InitBakeChannel(bakeChannel, jobs);

         foreach (PaintJob job in jobs)
         {

            switch (bakeChannel)
            {
               case BakeChannel.Position:
                  {
                     for (int i = 0; i < job.verts.Length; ++i)
                     {
                        Vector3 newPos = job.GetPosition(i);
                        newPos *= multValue;
                        job.stream.positions[i] = newPos;
                     }
                     break;
                  }
               case BakeChannel.Color:
                  {
                     var colors = job.stream.colors;
                     for (int i = 0; i < job.verts.Length; ++i)
                     {
                        var c = colors[i];
                        c.r *= multValue;
                        c.g *= multValue;
                        c.b *= multValue;
                        c.a *= multValue;
                        colors[i] = c;
                     }
                     job.stream.colors = colors;
                     break;
                  }
               case BakeChannel.UV0:
                  {
                     var uv = job.stream.uv0;
                     for (int i = 0; i < job.verts.Length; ++i)
                     {
                        uv[i] *= multValue;
                     }
                     job.stream.uv0 = uv;
                     break;
                  }
               case BakeChannel.UV1:
                  {
                     var uv = job.stream.uv1;
                     for (int i = 0; i < job.verts.Length; ++i)
                     {
                        uv[i] *= multValue;
                     }
                     job.stream.uv1 = uv;
                     break;
                  }
               case BakeChannel.UV2:
                  {
                     var uv = job.stream.uv2;
                     for (int i = 0; i < job.verts.Length; ++i)
                     {
                        uv[i] *= multValue;
                     }
                     job.stream.uv2 = uv;
                     break;
                  }
               case BakeChannel.UV3:
                  {
                     var uv = job.stream.uv3;
                     for (int i = 0; i < job.verts.Length; ++i)
                     {
                        uv[i] *= multValue;
                     }
                     job.stream.uv3 = uv;
                     break;
                  }
            }
            job.stream.Apply();
         }
      }
   }
}

