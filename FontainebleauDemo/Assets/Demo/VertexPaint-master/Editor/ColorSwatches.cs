using UnityEngine;
using System.Collections;

namespace JBooth.VertexPainterPro
{
   [System.Serializable]
   class ColorSwatches : ScriptableObject
   {
      public Color[] colors = new Color[] { Color.white, Color.black, Color.red, Color.green, Color.blue, Color.cyan,
         Color.magenta, Color.yellow, Color.gray, Color.gray, Color.gray, Color.gray, Color.gray, Color.gray, Color.gray, Color.gray
      };
   }
}