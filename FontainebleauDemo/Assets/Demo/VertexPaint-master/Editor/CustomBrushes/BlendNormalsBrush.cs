using UnityEngine;
using System.Collections;
using UnityEditor;


namespace JBooth.VertexPainterPro
{
   // Allow the user to create brush objects as assets in an editor folder
   [CreateAssetMenu(menuName = "Vertex Painter Brush/Blend Normal Brush", fileName="VertexNormalBlend_brush")]
   public class BlendNormalsBrush : VertexPainterCustomBrush
   {
      public GameObject target;

      // return a bitmask of channels in use, so Channels.Colors | Channels.UV0 if you affect those channels with your brush..
      // This will force the channels to be initialized before your brush is applied..
      public override Channels GetChannels()
      {
         return Channels.Normals;
      }

      // preview color for the brush, if we care to provide one
      public override Color GetPreviewColor()
      {
         return Color.yellow;
      }

      // return the data that will be provided to our stamping function, in this case the brush data above..
      public override object GetBrushObject()
      {
         return target;
      }

      // draw any custom GUI we want for this brush in the editor
      public override void DrawGUI()
      {
         EditorGUI.BeginChangeCheck();
         target = (GameObject)EditorGUILayout.ObjectField("Blend With", target, typeof(GameObject),true);
         if (EditorGUI.EndChangeCheck())
         {
            if (target == null)
            {
               streams = null;
            }
            else
            {
               streams = target.GetComponentsInChildren<VertexInstanceStream>();
            }
         }
      }

      VertexInstanceStream[] streams;

      bool didHit;
      Vector3 normal;
      Vector4 tangent;
      public override void BeginApplyStroke(Ray ray)
      {
         Vector3 bary = Vector3.zero;
         VertexInstanceStream stream = null;
         didHit = false;
         Mesh best = null;
         int triangle = 0;
         float distance = float.MaxValue;
         if (streams != null)
         {
            for (int i = 0; i < streams.Length; ++i)
            {
               Matrix4x4 mtx = streams[i].transform.localToWorldMatrix;
               Mesh msh = streams[i].GetComponent<MeshFilter>().sharedMesh;

               RaycastHit hit;
               RXLookingGlass.IntersectRayMesh(ray, msh, mtx, out hit);
               if (hit.distance < distance)
               {
                  distance = hit.distance;
                  bary = hit.barycentricCoordinate;
                  best = msh;
                  triangle = hit.triangleIndex;
                  stream = streams[i];
                  didHit = true;
               }
            }
         }
         if (didHit && best != null)
         {
            var triangles = best.triangles;
            int i0 = triangles[triangle];
            int i1 = triangles[triangle+1];
            int i2 = triangles[triangle+2];

            normal = stream.GetSafeNormal(i0) * bary.x + 
               stream.GetSafeNormal(i1) * bary.y +
               stream.GetSafeNormal(i2) * bary.z;

            tangent = stream.GetSafeTangent(i0) * bary.x +
               stream.GetSafeTangent(i1) * bary.y +
               stream.GetSafeTangent(i2) * bary.z;
         }
      }

      void LerpFunc(PaintJob j, int idx, ref object val, float r)
      {
         if (didHit)
         {
            j.stream.normals[idx] = Vector3.Lerp(j.stream.normals[idx], normal, r);
            j.stream.tangents[idx] = Vector4.Lerp(j.stream.tangents[idx], tangent, r);
         }
      }
         
      public override VertexPainterWindow.Lerper GetLerper()
      {
         return LerpFunc;
      }

   }
}

