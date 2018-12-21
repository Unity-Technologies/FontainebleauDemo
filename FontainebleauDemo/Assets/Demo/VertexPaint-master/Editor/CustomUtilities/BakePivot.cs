using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.VertexPainterPro
{
   public class BakePivot : IVertexPainterUtility
   {
      public string GetName() 
      {
         return "Bake Pivot/Rotation";
      }

      public void OnGUI(PaintJob[] jobs)
      {
         pivotTarget = (PivotTarget)EditorGUILayout.EnumPopup("Store in", pivotTarget);
         bakePivotUseLocal = EditorGUILayout.Toggle("Use Local Space", bakePivotUseLocal);

         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Bake Pivot"))
         {
            DoBakePivot(jobs);
         }
         if (GUILayout.Button("Bake Rotation"))
         {
            DoBakeRotation(jobs);
         }

         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();
      }


      enum PivotTarget
      {
         UV0,
         UV1,
         UV2,
         UV3
      }
      public enum BakeChannel
      {
         None,
         Color,
         UV0,
         UV1, 
         UV2,
         UV3
      }

      PivotTarget pivotTarget = PivotTarget.UV2;
      bool bakePivotUseLocal = true;


      void InitBakeChannel(BakeChannel bc, PaintJob[] jobs)
      {
         foreach (PaintJob job in jobs)
         {
            if (bc == BakeChannel.Color)
            {
               if (job.stream.colors == null || job.stream.colors.Length != job.verts.Length)
               {
                  job.stream.SetColor(Color.black, job.verts.Length);
               }
            }
            else if (bc == BakeChannel.UV0)
            {
               if (job.stream.uv0 == null || job.stream.uv0.Count!= job.verts.Length)
               {
                  job.stream.SetUV0(Vector4.zero, job.verts.Length);
               }
            }
            else if (bc == BakeChannel.UV1)
            {
               if (job.stream.uv1 == null || job.stream.uv1.Count != job.verts.Length)
               {
                  job.stream.SetUV1(Vector4.zero, job.verts.Length);
               }
            }
            else if (bc == BakeChannel.UV2)
            {
               if (job.stream.uv2 == null || job.stream.uv2.Count != job.verts.Length)
               {
                  job.stream.SetUV2(Vector4.zero, job.verts.Length);
               }
            }
            else if (bc == BakeChannel.UV3)
            {
               if (job.stream.uv3 == null || job.stream.uv3.Count != job.verts.Length)
               {
                  job.stream.SetUV3(Vector4.zero, job.verts.Length);
               }
            }
            EditorUtility.SetDirty(job.stream);
            EditorUtility.SetDirty(job.stream.gameObject);
         }
      }

      void DoBakeRotation(PaintJob[] jobs)
      {
         switch (pivotTarget)
         {
            case PivotTarget.UV0:
               {
                  InitBakeChannel(BakeChannel.UV0, jobs);
                  foreach (PaintJob job in jobs)
                  {
                     Vector3 lp = bakePivotUseLocal ? job.meshFilter.transform.localRotation.eulerAngles : job.meshFilter.transform.rotation.eulerAngles;
                     job.stream.SetUV0(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
                     EditorUtility.SetDirty(job.stream);
                     EditorUtility.SetDirty(job.stream.gameObject);
                  }
                  break;
               }
            case PivotTarget.UV1:
               {
                  InitBakeChannel(BakeChannel.UV1, jobs);
                  foreach (PaintJob job in jobs)
                  {
                     Vector3 lp = bakePivotUseLocal ? job.meshFilter.transform.localRotation.eulerAngles : job.meshFilter.transform.rotation.eulerAngles;
                     job.stream.SetUV1(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
                     EditorUtility.SetDirty(job.stream);
                     EditorUtility.SetDirty(job.stream.gameObject);
                  }
                  break;
               }
            case PivotTarget.UV2:
               {
                  InitBakeChannel(BakeChannel.UV2, jobs);
                  foreach (PaintJob job in jobs)
                  {
                     Vector3 lp = bakePivotUseLocal ? job.meshFilter.transform.localRotation.eulerAngles : job.meshFilter.transform.rotation.eulerAngles;
                     job.stream.SetUV2(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
                     EditorUtility.SetDirty(job.stream);
                     EditorUtility.SetDirty(job.stream.gameObject);
                  }
                  break;
               }
            case PivotTarget.UV3:
               {
                  InitBakeChannel(BakeChannel.UV3, jobs);
                  foreach (PaintJob job in jobs)
                  {
                     Vector3 lp = bakePivotUseLocal ? job.meshFilter.transform.localRotation.eulerAngles : job.meshFilter.transform.rotation.eulerAngles;
                     job.stream.SetUV3(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
                     EditorUtility.SetDirty(job.stream);
                     EditorUtility.SetDirty(job.stream.gameObject);
                  }
                  break;
               }
         }

      }

      void DoBakePivot(PaintJob[] jobs)
      {
         switch (pivotTarget)
         {
            case PivotTarget.UV0:
               {
                  InitBakeChannel(BakeChannel.UV0, jobs);
                  foreach (PaintJob job in jobs)
                  {
                     Vector3 lp = bakePivotUseLocal ? job.meshFilter.transform.localPosition : job.meshFilter.transform.position;
                     job.stream.SetUV0(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
                     EditorUtility.SetDirty(job.stream);
                     EditorUtility.SetDirty(job.stream.gameObject);
                  }
                  break;
               }
            case PivotTarget.UV1:
               {
                  InitBakeChannel(BakeChannel.UV1, jobs);
                  foreach (PaintJob job in jobs)
                  {
                     Vector3 lp = bakePivotUseLocal ? job.meshFilter.transform.localPosition : job.meshFilter.transform.position;
                     job.stream.SetUV1(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
                     EditorUtility.SetDirty(job.stream);
                     EditorUtility.SetDirty(job.stream.gameObject);
                  }
                  break;
               }
            case PivotTarget.UV2:
               {
                  InitBakeChannel(BakeChannel.UV2, jobs);
                  foreach (PaintJob job in jobs)
                  {
                     Vector3 lp = bakePivotUseLocal ? job.meshFilter.transform.localPosition : job.meshFilter.transform.position;
                     job.stream.SetUV2(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
                     EditorUtility.SetDirty(job.stream);
                     EditorUtility.SetDirty(job.stream.gameObject);
                  }
                  break;
               }
            case PivotTarget.UV3:
               {
                  InitBakeChannel(BakeChannel.UV3, jobs);
                  foreach (PaintJob job in jobs)
                  {
                     Vector3 lp = bakePivotUseLocal ? job.meshFilter.transform.localPosition : job.meshFilter.transform.position;
                     job.stream.SetUV3(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
                     EditorUtility.SetDirty(job.stream);
                     EditorUtility.SetDirty(job.stream.gameObject);
                  }
                  break;
               }
         }
      }
   }
}
