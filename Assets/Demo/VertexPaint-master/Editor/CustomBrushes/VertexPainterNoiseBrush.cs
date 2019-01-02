using UnityEngine;
using System.Collections;
using UnityEditor;


// This is an example of how to create a custom brush for the vertex painting system. The use case driving this
// feature was needing to paint into more channels than just RGBA at once, but you can create fairly arbitrary
// brushes with it, that can affect as many channels as you want in any way you want. The example here is
// a brush that uses the vertex position to generate noise for the color channels. 
// 
// Brushes are scriptable objects, and can provide a gui via the scriptable object interface as well as an
// in-vertex editor GUI. You can use this to only expose some values to the editor, so if you want the user
// to make separate scriptable objects for the brushes, but still be able to adjust some of the parameters easily,
// you can do that. 

namespace JBooth.VertexPainterPro
{
   // Allow the user to create brush objects as assets in an editor folder
   [CreateAssetMenu(menuName = "Vertex Painter Brush/Noise Brush", fileName="noise_brush")]
   public class VertexPainterNoiseBrush : VertexPainterCustomBrush 
   {
      // I like a little object to hold the brush settings..
      [System.Serializable]
      public class BrushData
      {
         public float frequency = 10;
         public float amplitude = 1;
      }
      public BrushData brushData = new BrushData();

      // return a bitmask of channels in use, so Channels.Colors | Channels.UV0 if you affect those channels with your brush..
      // This will force the channels to be initialized before your brush is applied..
      public override Channels GetChannels()
      {
         return Channels.Colors;
      }

      // preview color for the brush, if we care to provide one
      public override Color GetPreviewColor()
      {
         return Color.yellow;
      }

      // return the data that will be provided to our stamping function, in this case the brush data above..
      public override object GetBrushObject()
      {
         return brushData;
      }

      // draw any custom GUI we want for this brush in the editor
      public override void DrawGUI()
      {
         brushData.frequency = EditorGUILayout.Slider("frequency", brushData.frequency, 0.01f, 100.0f);
         brushData.amplitude = EditorGUILayout.Slider("amplitude", brushData.amplitude, 0.1f, 10.0f);
      }
      // this is the delegate we're going to return to apply a brush stamp. Your passed a paint job,
      // which contains important cached information to make painting fast, and hold the actual stream data. 
      // the index is the index into the vertex array, the val is the brush data we supplied in GetBrushObject,
      // and r is a 0 to 1 with how far to tween the value towards the brush (brush pressure * deltatime)
      void LerpFunc(PaintJob j, int idx, ref object val, float r)
      {
         // retrieve our brush data and get the stream we're painting into
         BrushData bd = val as BrushData;
         var s = j.stream;
         // use our vertex position to generate noise. We use the get position function because it will
         // return a vert position from the original meshes cached verticies or modified verticies if
         // we've modified them. This makes it compatible with deformations, etc. 
         Vector3 pos = j.GetPosition(idx);
         // convert into world space
         pos = j.renderer.localToWorldMatrix.MultiplyPoint(pos);
         // scale by frequency
         pos.x *= bd.frequency;
         pos.y *= bd.frequency;
         pos.z *= bd.frequency;
         float noise = 0.5f * (0.5f * JBooth.VertexPainterPro.SimplexNoise.Noise.Generate(pos.x, pos.y, pos.z) + 0.5f);
         noise += 0.25f * (0.5f * JBooth.VertexPainterPro.SimplexNoise.Noise.Generate(pos.y * 2.031f, pos.z * 2.031f, pos.x * 2.031f) + 0.5f);
         noise += 0.25f * (0.5f * JBooth.VertexPainterPro.SimplexNoise.Noise.Generate(pos.z * 4.01f, pos.x * 4.01f, pos.y * 4.01f) + 0.5f);
         noise *= bd.amplitude;
         // lerp the noise in
         Color c = s.colors[idx];
         c.r = Mathf.Lerp(c.r, noise, r);
         c.g = Mathf.Lerp(c.g, noise, r);
         c.b = Mathf.Lerp(c.b, noise, r);

         s.colors[idx] = c;
      }

      // this one is what's actually called by the system, it returns the delegate function above
      // You want to make the delegate into a proper function, otherwise an anonymous one will allocate
      // temporary memory..
      public override VertexPainterWindow.Lerper GetLerper()
      {
         return LerpFunc;
      }

   }
}
