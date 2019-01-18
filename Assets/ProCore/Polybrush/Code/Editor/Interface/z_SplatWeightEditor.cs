using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

namespace Polybrush
{
	/**
	 *	The default editor for z_SplatWeight.
	 */
	[CustomEditor(typeof(z_SplatWeight))]
	public class z_SplatWeightEditor : Editor
	{
		static int thumbSize = 64;

		/**
		 *	Editor for blend.  Returns true if blend has been modified.
		 */
		public static int OnInspectorGUI(int index, ref z_SplatWeight blend, z_AttributeLayout[] attribs)
		{
			// if(blend == null && attribs != null)
			// 	blend = new z_SplatWeight( z_SplatWeight.GetChannelMap(attribs) );

			// bool mismatchedOrNullAttributes = blend == null || !blend.MatchesAttributes(attribs);

			Rect r = GUILayoutUtility.GetLastRect();
			int yPos = (int) Mathf.Ceil(r.y + r.height);

			index = z_GUILayout.ChannelField(index, attribs, thumbSize, yPos);

			return index;
		}
	}
}
