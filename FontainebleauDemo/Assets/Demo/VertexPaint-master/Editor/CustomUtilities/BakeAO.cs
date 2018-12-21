using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.VertexPainterPro
{
   [System.Serializable]
   public class BakeAO : IVertexPainterUtility
   {
      public string GetName() 
      {
         return "Ambient Occlusion";
      }

      public void OnGUI(PaintJob[] jobs)
      {
         var window = VertexPainterWindow.GetWindow<VertexPainterWindow>();
         window.brushMode = (VertexPainterWindow.BrushTarget)EditorGUILayout.EnumPopup("Target Channel", window.brushMode);
         aoSamples = EditorGUILayout.IntSlider("Samples", aoSamples, 64, 1024);
         EditorGUILayout.BeginHorizontal();
         aoRange = EditorGUILayout.Vector2Field("Range (Min, Max)", aoRange);
         aoRange.x = Mathf.Max(aoRange.x, 0.0001f);
         EditorGUILayout.EndHorizontal();
         aoIntensity = EditorGUILayout.Slider("Intensity", aoIntensity, 0.25f, 4.0f);
         bakeLighting = EditorGUILayout.Toggle("Bake Lighting", bakeLighting);
         if (bakeLighting)
         {
            aoLightAmbient = EditorGUILayout.ColorField("Light Ambient", aoLightAmbient);
         }
         aoBakeMode = (AOBakeMode)EditorGUILayout.EnumPopup("Mode", aoBakeMode);

         EditorGUILayout.Space();
         if (GUILayout.Button("Bake"))
         {
            DoBakeAO(jobs, window);
         }
      }

      public int     aoSamples = 512;
      public Vector2 aoRange = new Vector2(0.0001f, 1.5f);
      public float   aoIntensity = 2.0f;
      public bool    bakeLighting;
      public Color   aoLightAmbient = new Color(0.05f, 0.05f, 0.05f, 1);

      public enum AOBakeMode
      {
         Replace = 0,
         Multiply
      }
      public AOBakeMode aoBakeMode = AOBakeMode.Replace;


      RaycastHit hit = new RaycastHit();

      void ApplyAOLight(ref Color c, Light l, Vector3 pos, Vector3 n)
      {
         if (!l.isActiveAndEnabled)
            return;
         Vector3 dir;
         float intensity = l.intensity;
         if (l.type == LightType.Directional)
         {
            dir = -l.transform.forward;
         }
         else if (l.type == LightType.Point)
         {
            dir = (l.transform.position - pos).normalized;
            intensity *= Mathf.Clamp01((l.range - Vector3.Distance(l.transform.position, pos)) / l.range);
         }
         else
         {
            return;
         }

         intensity *= Mathf.Clamp01(Vector3.Dot(n, dir));

         c.r += l.color.r * intensity;
         c.g += l.color.g * intensity;
         c.b += l.color.b * intensity;
      }

      void DoBakeAO(PaintJob[] jobs, VertexPainterWindow window)
      {
         Light[] aoLights = null;
         if (bakeLighting)
         {
            aoLights = GameObject.FindObjectsOfType<Light>();
         }
         int sample = 0;
         int numVerts = 0;
         for (int i = 0; i < jobs.Length; ++i)
         {
            numVerts += jobs[i].verts.Length;
         }
         int numSamples = numVerts * aoSamples;

         float oldFloat = window.floatBrushValue;
         Color oldColor = window.brushColor;
         int oldVal = window.brushValue;

         // add temp colliders if needed
         bool[] tempCollider = new bool[jobs.Length];
         for (int jIdx = 0; jIdx < jobs.Length; ++jIdx)
         {
            PaintJob job = jobs[jIdx];

            if (job.meshFilter.GetComponent<Collider>() == null)
            {
               job.meshFilter.gameObject.AddComponent<MeshCollider>();
               tempCollider[jIdx] = true;
            }
         }

         // do AO
         for (int jIdx = 0; jIdx < jobs.Length; ++jIdx)
         {
            PaintJob job = jobs[jIdx];

            window.PrepBrushMode(job);
            // bake down the mesh so we take instance positions into account..
            Mesh mesh = VertexPainterUtilities.BakeDownMesh(job.meshFilter.sharedMesh, job.stream);
            Vector3[] verts = mesh.vertices;
            if (mesh.normals == null || mesh.normals.Length == 0)
            {
               mesh.RecalculateNormals();
            }
            Vector3[] normals = mesh.normals;

            window.brushValue = 255;
            window.floatBrushValue = 1.0f;
            window.brushColor = Color.white;
            var val = window.GetBrushValue();
            VertexPainterWindow.Lerper lerper = null;
            VertexPainterWindow.Multiplier mult = null;

            if (aoBakeMode == AOBakeMode.Replace)
            {
               lerper = window.GetLerper();
               for (int i = 0; i < job.verts.Length; ++i)
               {
                  lerper.Invoke(job, i, ref val, 1);
               }
            }
            else
            {
               mult = window.GetMultiplier();
            }

            for (int i = 0; i<verts.Length; i++) 
            {
               Vector3 norm = normals[i];
               // to world space!
               Vector3 v = job.meshFilter.transform.TransformPoint(verts[i]);
               Vector3 n = job.meshFilter.transform.TransformPoint(verts[i] + norm);
               Vector3 worldSpaceNormal = (n-v).normalized;

               float totalOcclusion = 0;

               // the slow part..
               for (int j = 0; j < aoSamples; j++) 
               {
                  // random rotate around hemisphere
                  float rot = 180.0f;
                  float rot2 = rot / 2.0f;
                  float rotx = (( rot * Random.value ) - rot2);
                  float roty = (( rot * Random.value ) - rot2);
                  float rotz = (( rot * Random.value ) - rot2);

                  Vector3 dir = Quaternion.Euler( rotx, roty, rotz ) * Vector3.up;
                  Quaternion dirq = Quaternion.FromToRotation(Vector3.up, worldSpaceNormal);
                  Vector3 ray = dirq * dir;
                  Vector3 offset = Vector3.Reflect( ray, worldSpaceNormal );

                  // raycast
                  ray = ray * (aoRange.y/ray.magnitude);
                  if ( Physics.Linecast( v-(offset*0.1f), v + ray, out hit ) ) 
                  {
                     if ( hit.distance > aoRange.x ) 
                     {
                        totalOcclusion += Mathf.Clamp01( 1 - ( hit.distance / aoRange.y ) );
                     }
                  }

                  sample++;
                  if (sample % 500 == 0) 
                  {
                     EditorUtility.DisplayProgressBar("Baking AO...", "Baking...", (float)sample / (float)numSamples);
                  }
               }

               totalOcclusion = Mathf.Clamp01( 1 - ((totalOcclusion*aoIntensity)/aoSamples) );

               if (aoLights != null && aoLights.Length > 0)
               {
                  Color c = aoLightAmbient;
                  for (int l = 0; l < aoLights.Length; ++l)
                  {
                     Light light = aoLights[l];
                     ApplyAOLight(ref c, light, v, n);
                  }
                  c.r *= totalOcclusion;
                  c.g *= totalOcclusion;
                  c.b *= totalOcclusion;
                  c.a = totalOcclusion;
                  window.brushColor = c;

                  // if we're lit and targeting a channel other than color, bake max intensity..
                  window.floatBrushValue = Mathf.Max(Mathf.Max(c.r, c.g), c.b) * totalOcclusion;
                  window.brushValue = (int)(window.floatBrushValue * 255);
               }
               else
               {
                  window.brushColor.r = totalOcclusion;
                  window.brushColor.g = totalOcclusion;
                  window.brushColor.b = totalOcclusion;
                  window.brushColor.a = totalOcclusion;

                  window.floatBrushValue = totalOcclusion;
                  window.brushValue = (int)(totalOcclusion * 255);
               }
               val = window.GetBrushValue();
               if (aoBakeMode == AOBakeMode.Replace)
               {
                  lerper.Invoke(job, i, ref val, 1);
               }
               else
               {
                  mult.Invoke(job.stream, i, ref val);
               }
            }
            job.stream.Apply();
            EditorUtility.SetDirty(job.stream);
            EditorUtility.SetDirty(job.stream.gameObject);
            GameObject.DestroyImmediate(mesh);

            window.brushValue = oldVal;
            window.floatBrushValue = oldFloat;
            window.brushColor = oldColor;
         }
         // remove temp colliders
         for (int jIdx = 0; jIdx < jobs.Length; ++jIdx)
         {
            if (tempCollider[jIdx] == true)
            {
               Collider c = jobs[jIdx].meshFilter.GetComponent<Collider>();
               if (c != null)
               {
                  GameObject.DestroyImmediate(c);
               }
            }
         }

         EditorUtility.ClearProgressBar();
         SceneView.RepaintAll();

      }
   }
}
