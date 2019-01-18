using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	GUI field extensions.
	 */
	internal static class z_GUI
	{
		public static Color BOX_BACKGROUND_LIGHT = new Color(.8f, .8f, .8f, 1f);
		public static Color BOX_BACKGROUND_DARK = new Color(.24f, .24f, .24f, 1f);
		public static Color BOX_OUTLINE_LIGHT = new Color(0.6745f, 0.6745f, 0.6745f, 1f);
		public static Color BOX_OUTLINE_DARK = new Color(.29f, .29f, .29f, 1f);

		/// Used as a container to pass text to GUI functions requiring a GUIContent without allocating
		/// a new GUIContent isntance.
		static GUIContent tmp_content = new GUIContent("", "");

		public static GUIContent TempContent(string text, string tooltip = null)
		{
			tmp_content.text = text;
			tmp_content.tooltip = tooltip;
			return tmp_content;
		}

		/// Maintain GUI.backgroundColor history.
		private static Stack<Color> backgroundColor = new Stack<Color>();

		public static void PushBackgroundColor(Color bg)
		{
			backgroundColor.Push(GUI.backgroundColor);
			GUI.backgroundColor = bg;
		}

		public static void PopBackgroundColor()
		{
			GUI.backgroundColor = backgroundColor.Pop();
		}

		private static Stack<GUISkin> guiSkin = new Stack<GUISkin>();

		public static void PushGUISkin(GUISkin skin)
		{
			guiSkin.Push(GUI.skin);
			GUI.skin = skin;
		}

		public static void PushUnitySkin()
		{
			if(guiSkin.Count < 1)
				PushGUISkin(GUI.skin);
			else
				PushGUISkin(guiSkin.Last());
		}

		public static void PopGUISkin()
		{
			GUI.skin = guiSkin.Pop();
		}

		private static GUIStyle _headerTextStyle = null;

		/**
		 *	Large bold slightly transparent font.
		 */
		public static GUIStyle headerTextStyle
		{
			get
			{
				const int PAD = 2, MARGIN_HORIZONTAL = 4, MARGIN_VERTICAL = 0;

				if(_headerTextStyle == null)
				{
					_headerTextStyle = new GUIStyle();
					_headerTextStyle.margin = new RectOffset(MARGIN_HORIZONTAL, MARGIN_HORIZONTAL, MARGIN_VERTICAL, MARGIN_VERTICAL);
					_headerTextStyle.padding = new RectOffset(PAD, PAD, 4, PAD);
					_headerTextStyle.fontSize = 14;
					_headerTextStyle.fontStyle = FontStyle.Bold;
					_headerTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.gray : Color.gray;
				}

				return _headerTextStyle;
			}
		}

		private static GUISkin _polybrushSkin = null;

		public static GUISkin PolybrushSkin
		{
			get
			{
				if(_polybrushSkin == null)
					_polybrushSkin = Resources.Load<GUISkin>( EditorGUIUtility.isProSkin ? "PolybrushDark" : "PolybrushLight");

				return _polybrushSkin;
			}
		}

		private static Dictionary<string, GUIStyle> activeStyles = new Dictionary<string, GUIStyle>();

		public static GUIStyle GetStyleOn(string name)
		{
			GUIStyle active = null;

			if(activeStyles.TryGetValue(name, out active))
				return active;

			active = new GUIStyle(GUI.skin.GetStyle(name));
			active.normal = active.onNormal;
			activeStyles.Add(name, active);

			return active;
		}

		static GUIStyle _backgroundColorStyle = null;

		public static GUIStyle backgroundColorStyle
		{
			get
			{
				if(_backgroundColorStyle == null)
				{
					_backgroundColorStyle = new GUIStyle();
					_backgroundColorStyle.margin = new RectOffset(4,4,4,4);
					_backgroundColorStyle.padding = new RectOffset(4,4,4,4);
					_backgroundColorStyle.normal.background = EditorGUIUtility.whiteTexture;
				}

				return _backgroundColorStyle;
			}
		}

		static GUIStyle _centeredStyle = null;

		public static GUIStyle centeredStyle
		{
			get
			{
				if(_centeredStyle == null)
				{
					_centeredStyle = new GUIStyle();
					_centeredStyle.alignment = TextAnchor.MiddleCenter;
					_centeredStyle.normal.textColor = new Color(.85f, .85f, .85f, 1f);
					_centeredStyle.wordWrap = true;
				}
				return _centeredStyle;
			}
		}
	}
}
