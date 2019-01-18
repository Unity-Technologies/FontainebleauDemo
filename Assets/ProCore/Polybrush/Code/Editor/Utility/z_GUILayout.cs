using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	GUI field extensions.
	 */
	internal static class z_GUILayout
	{
		/**
		 * Color field control
		 */
		public static z_ColorMask ColorMaskField(string text, z_ColorMask mask)
		{
			return ColorMaskField(string.IsNullOrEmpty(text) ? null : z_GUI.TempContent(text), mask);
		}

		public static z_ColorMask ColorMaskField(GUIContent gc, z_ColorMask mask)
		{
			GUILayout.BeginHorizontal();

			if(gc != null)
			{
				int w = (int) EditorGUIUtility.currentViewWidth / 2;
				GUILayout.Label(gc, GUILayout.MinWidth(w), GUILayout.MaxWidth(w));
			}

			if( GUILayout.Button("R", mask.r ? "Commandleft" : z_GUI.GetStyleOn("Commandleft")) )
				mask.r = !mask.r;

			if( GUILayout.Button("G", mask.g ? "Commandmid" : z_GUI.GetStyleOn("Commandmid")) )
				mask.g = !mask.g;

			if( GUILayout.Button("B", mask.b ? "Commandmid" : z_GUI.GetStyleOn("Commandmid")) )
				mask.b = !mask.b;

			if( GUILayout.Button("A", mask.a ? "Commandright" : z_GUI.GetStyleOn("Commandright")) )
				mask.a = !mask.a;

			GUILayout.EndHorizontal();

			return mask;
		}

		public static uint BitMaskField(uint value, string[] descriptions, string tooltip)
		{
			GUIContent gc = z_GUI.TempContent("", tooltip);

			GUILayout.BeginHorizontal();

			int l = descriptions.Length;

			for(int i = 1; i < l; i++)
			{
				int s = i - 1;

				GUIStyle style = GUI.skin.GetStyle(i < 2 ? "Commandleft" : (i >= l - 1 ? "Commandright" : "Commandmid"));
				Texture2D nrmBackground = style.normal.background;
				Color nrmColor = style.normal.textColor;

				if((value & (1u << s)) > 0)
				{
					style.normal.background = style.onNormal.background;
					style.normal.textColor 	= style.onNormal.textColor;
				}

				gc.text = descriptions[i];

				if(GUILayout.Button(gc, style))
					value ^= (1u << s);

				style.normal.background = nrmBackground;
				style.normal.textColor = nrmColor;
			}

			GUILayout.EndHorizontal();

			return value;
		}

		public static int CycleButton(int index, GUIContent[] content, GUIStyle style = null)
		{
			if(style != null)
			{
				if( GUILayout.Button(content[index], style) )
					return (index + 1) % content.Length;
				else
					return index;
			}
			else
			{
				if( GUILayout.Button(content[index]) )
					return (index + 1) % content.Length;
				else
					return index;
			}
		}

		/**
		 * Similar to EditorGUILayoutUtility.Slider, except this allows for values outside of the min/max bounds via the float field.
		 */
		public static float FreeSlider(string content, float value, float min, float max)
		{
			return FreeSlider(z_GUI.TempContent(content), value, min, max);
		}

		/**
		 * Similar to EditorGUILayoutUtility.Slider, except this allows for values outside of the min/max bounds via the float field.
		 */
		public static float FreeSlider(GUIContent content, float value, float min, float max)
		{
			const float PAD = 4f;
			const float SLIDER_HEIGHT = 16f;
			const float MIN_LABEL_WIDTH = 0f;
			const float MAX_LABEL_WIDTH = 128f;
			const float MIN_FIELD_WIDTH = 48f;

			GUILayoutUtility.GetRect(Screen.width, 18);

			Rect previousRect = GUILayoutUtility.GetLastRect();
			float y = previousRect.y;

			float labelWidth = content != null ? Mathf.Max(MIN_LABEL_WIDTH, Mathf.Min(GUI.skin.label.CalcSize(content).x + PAD, MAX_LABEL_WIDTH)) : 0f;
			float remaining = (Screen.width - (PAD * 2f)) - labelWidth;
			float sliderWidth = remaining - (MIN_FIELD_WIDTH + PAD);
			float floatWidth = MIN_FIELD_WIDTH;

			Rect labelRect = new Rect(PAD, y + 2f, labelWidth, SLIDER_HEIGHT);
			Rect sliderRect = new Rect(labelRect.x + labelWidth, y + 1f, sliderWidth, SLIDER_HEIGHT);
			Rect floatRect = new Rect(sliderRect.x + sliderRect.width + PAD, y + 1f, floatWidth, SLIDER_HEIGHT);

			if(content != null)
				GUI.Label(labelRect, content);

			EditorGUI.BeginChangeCheck();

				int controlID = GUIUtility.GetControlID(FocusType.Passive, sliderRect);
				float tmp = value;
				tmp = GUI.Slider(sliderRect, tmp, 0f, min, max, GUI.skin.horizontalSlider, (!EditorGUI.showMixedValue) ? GUI.skin.horizontalSliderThumb : "SliderMixed", true, controlID);

			if(EditorGUI.EndChangeCheck())
				value = Event.current.control ? 1f * Mathf.Round(tmp / 1f) : tmp;

			value = EditorGUI.FloatField(floatRect, value);

			return value;
		}

		public static int ChannelField(int index, z_AttributeLayout[] channels, int thumbSize, int yPos)
		{
			int mIndex = index;
			int attribsLength = channels != null ? channels.Length : 0;

			const int margin = 4; 					// group pad
			const int pad = 2; 						// texture pad
			const int selected_rect_height = 10;	// the little green bar and height padding

			int actual_width = (int) Mathf.Ceil(thumbSize + pad/2);
			int container_width = (int) Mathf.Floor(EditorGUIUtility.currentViewWidth) - margin * 2 - 4;
			int columns = (int) Mathf.Floor(container_width / actual_width);
			int fill = (int) Mathf.Floor(((container_width % actual_width) - 1) / columns);
			int size = thumbSize + fill;
			int rows = attribsLength / columns + (attribsLength % columns == 0 ? 0 : 1);
			int height = rows * (size + selected_rect_height);// + margin * 2;

			Rect r = new Rect(margin + pad, yPos + margin, size, size);

			Rect border = new Rect( margin, yPos + margin, container_width + margin, height + margin );
			GUI.color = EditorGUIUtility.isProSkin ? z_GUI.BOX_OUTLINE_DARK : z_GUI.BOX_OUTLINE_LIGHT;
			EditorGUI.DrawPreviewTexture(border, EditorGUIUtility.whiteTexture);
			border.x += 1;
			border.y += 1;
			border.width -= 2;
			border.height -= 2;
			GUI.color = EditorGUIUtility.isProSkin ? z_GUI.BOX_BACKGROUND_DARK : z_GUI.BOX_BACKGROUND_LIGHT;
			EditorGUI.DrawPreviewTexture(border, EditorGUIUtility.whiteTexture);
			GUI.color = Color.white;

			for(int i = 0; i < attribsLength; i++)
			{
				if(i > 0 && i % columns == 0)
				{
					r.x = pad + margin;
					r.y += r.height + selected_rect_height;
				}

				string summary = channels[i].propertyTarget;

				if(string.IsNullOrEmpty(summary))
					summary = "channel\n" + (i+1);

				if( AttributeComponentButton(r, summary, channels[i].previewTexture, i == mIndex) )
				{
					mIndex = i;
					GUI.changed = true;
				}

				r.x += r.width + pad;
			}

			GUILayoutUtility.GetRect(container_width - 8, height);

			return mIndex;
		}

		static readonly Color texture_button_border = new Color(.1f, .1f, .1f, 1f);
		static readonly Color texture_button_fill = new Color(.18f, .18f, .18f, 1f);

		static bool AttributeComponentButton(Rect rect, string text, Texture2D img, bool selected)
		{
			bool clicked = false;

			Rect r = rect;

			Rect border = new Rect(r.x + 2, r.y + 6, r.width - 4, r.height - 4);

			GUI.color = texture_button_border;
			EditorGUI.DrawPreviewTexture(border, EditorGUIUtility.whiteTexture, null, ScaleMode.ScaleToFit, 0f);
			GUI.color = Color.white;

			border.x += 2;
			border.y += 2;
			border.width -= 4;
			border.height -= 4;

			if(img != null)
			{
				EditorGUI.DrawPreviewTexture(border, img, null, ScaleMode.ScaleToFit, 0f);
			}
			else
			{
				GUI.color = texture_button_fill;
				EditorGUI.DrawPreviewTexture(border, EditorGUIUtility.whiteTexture, null, ScaleMode.ScaleToFit, 0f);
				GUI.color = Color.white;
				GUI.Label(border, text, z_GUI.centeredStyle);
			}

			if(selected)
			{
				r.y += r.height + 4;
				r.x += 2;
				r.width -= 5;
				r.height = 6;
				GUI.color = Color.green;
				EditorGUI.DrawPreviewTexture(r, EditorGUIUtility.whiteTexture, null, ScaleMode.StretchToFill, 0);
				GUI.color = Color.white;
			}

			clicked = GUI.Button(border, "", GUIStyle.none);

			return clicked;
		}

		public static bool AssetPreviewButton(Rect rect, Object obj, bool selected)
		{
			bool clicked = false;
			Rect r = rect;
			Rect border = new Rect(r.x + 2, r.y + 6, r.width - 4, r.height - 4);

			GUI.color = texture_button_border;
			EditorGUI.DrawPreviewTexture(border, EditorGUIUtility.whiteTexture, null, ScaleMode.ScaleToFit, 0f);
			GUI.color = Color.white;

			border.x += 2;
			border.y += 2;
			border.width -= 4;
			border.height -= 4;

			Texture2D preview = z_AssetPreview.GetAssetPreview(obj);

			if(preview != null)
			{
				EditorGUI.DrawPreviewTexture(border, preview, null, ScaleMode.ScaleToFit, 0f);
			}
			else
			{
				string text = obj != null ? obj.name : "null";
				GUI.color = texture_button_fill;
				EditorGUI.DrawPreviewTexture(border, EditorGUIUtility.whiteTexture, null, ScaleMode.ScaleToFit, 0f);
				GUI.color = Color.white;
				GUI.Label(border, text, z_GUI.centeredStyle);
			}

			if(selected)
			{
				r.y += r.height + 4;
				r.x += 2;
				r.width -= 5;
				r.height = 6;
				GUI.color = Color.green;
				EditorGUI.DrawPreviewTexture(r, EditorGUIUtility.whiteTexture, null, ScaleMode.StretchToFill, 0);
				GUI.color = Color.white;
			}

			clicked = GUI.Button(border, "", GUIStyle.none);

			return clicked;
		}

		public static bool Foldout(bool state, GUIContent content)
		{
			GUILayout.BeginHorizontal();

			if( GUILayout.Button(z_GUI.TempContent(""), state ? "FoldoutOpen" : "FoldoutClosed") )
				state = !state;

			GUILayout.Label(content);

			GUILayout.EndHorizontal();

			return state;
		}

		public static bool Toggle(GUIContent gc, bool isToggled)
		{
			GUILayout.BeginHorizontal();
			GUIStyle toggleStyle = GUI.skin.GetStyle("Toggle");

			Vector2 s = toggleStyle.CalcSize(z_GUI.TempContent("", gc.tooltip));

			// 18px accounts for window margins
			GUILayout.Label(gc, GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - Mathf.Ceil(s.x) - 18));

			GUILayout.FlexibleSpace();

			isToggled = GUILayout.Toggle(isToggled, z_GUI.TempContent("", gc.tooltip), toggleStyle);

			GUILayout.EndHorizontal();

			return isToggled;
		}

		public static System.Enum EnumPopup(GUIContent gc, System.Enum value)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label(gc);
			GUILayout.FlexibleSpace();
			var ret = EditorGUILayout.EnumPopup(value, "Popup", GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth / 2));

			GUILayout.EndHorizontal();

			return ret;
		}

		public static float FloatField(GUIContent gc, float value, params GUILayoutOption[] options)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label(gc);
			GUILayout.FlexibleSpace();
			float ret = EditorGUILayout.FloatField(value, "textfield", GUILayout.MaxWidth(System.Math.Min(64, EditorGUIUtility.currentViewWidth / 2)));

			GUILayout.EndHorizontal();

			return ret;
		}

		public static Color ColorField(GUIContent gc, Color color)
		{
			if(gc != null && !string.IsNullOrEmpty(gc.text))
				GUILayout.Label(gc);

			var ret = EditorGUILayout.ColorField(z_GUI.TempContent("", gc.tooltip), color);
			return ret;
		}

		public static Gradient GradientField(GUIContent gc, Gradient value)
		{
			GUILayout.Label(gc);

			object out_gradient = z_ReflectionUtil.Invoke(	null,
												typeof(EditorGUILayout),
												"GradientField",
												new System.Type[] { typeof(GUIContent), typeof(Gradient), typeof(GUILayoutOption[]) },
												BindingFlags.NonPublic | BindingFlags.Static,
												new object[] { z_GUI.TempContent("", gc.tooltip), value, null });

			return (Gradient) out_gradient;
		}

		public static bool HeaderWithDocsLink(GUIContent gc)
		{
			GUILayout.Label(gc, "HeaderLabel");
			Rect last = GUILayoutUtility.GetLastRect();
			const int HELP_ICON_SIZE = 18;
			const int HELP_ICON_PAD = 2;
			last.x = (last.x + last.width) - HELP_ICON_SIZE - HELP_ICON_PAD;
			last.width = HELP_ICON_SIZE;
			return GUI.Button(last, "", "HelpIcon");
		}
	}
}
