using UnityEngine;
using System.Collections;
using UnityEditor;

namespace JBooth.VertexPainterPro
{
   // interface class for utilities that plug into the vertex painter window
   public interface IVertexPainterUtility 
   {
      string GetName();
      void OnGUI(PaintJob[] jobs);

   }
}