using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	[CustomEditor(typeof(z_PrefabPalette))]
	public class z_PrefabPaletteEditor : Editor
	{
		private SerializedProperty prefabs;
		private HashSet<int> selected = new HashSet<int>();

		public Delegate<IEnumerable<int>> onSelectionChanged = null;

		private void OnEnable()
		{
			prefabs = serializedObject.FindProperty("prefabs");
		}

		public static z_PrefabPalette AddNew()
		{
			string path = z_EditorUtility.FindFolder(z_Pref.ProductName + "/" + "Prefab Palettes");

			if(string.IsNullOrEmpty(path))
				path = "Assets";

			path = AssetDatabase.GenerateUniqueAssetPath(path + "/New Prefab Palette.asset");

			if(!string.IsNullOrEmpty(path))
			{
				z_PrefabPalette palette = ScriptableObject.CreateInstance<z_PrefabPalette>();
				palette.SetDefaultValues();

				AssetDatabase.CreateAsset(palette, path);
				AssetDatabase.Refresh();

				EditorGUIUtility.PingObject(palette);

				return palette;
			}

			return null;
		}

		public override void OnInspectorGUI()
		{
			OnInspectorGUI_Internal(64);
		}

		private bool IsDeleteKey(Event e)
		{
			return e.keyCode == KeyCode.Backspace;
		}

		public void OnInspectorGUI_Internal(int thumbSize)
		{
			serializedObject.Update();

			int count = prefabs != null ? prefabs.arraySize : 0;

			const int margin_x = 8; 				// group pad
			const int margin_y = 4; 				// group pad
			const int pad = 2; 						// texture pad
			const int selected_rect_height = 10;	// the little green bar and height padding

			int actual_width = (int) Mathf.Ceil(thumbSize + pad/2);
			int container_width = (int) Mathf.Floor(EditorGUIUtility.currentViewWidth) - (margin_x * 2);
			int usable_width = container_width - pad * 2;
			int columns = (int) Mathf.Floor(usable_width / actual_width);
			int fill = (int) Mathf.Floor(((usable_width % actual_width)) / columns);
			int size = thumbSize + fill;
			int rows = count / columns + (count % columns == 0 ? 0 : 1);
			if(rows < 1) rows = 1;
			int height = rows * (size + selected_rect_height);

			Rect r = EditorGUILayout.GetControlRect(false, height);

			r.x = margin_x + pad;
			r.y += margin_y;
			r.width = size;
			r.height = size;

			Rect border = new Rect( margin_x, r.y, container_width, height );
//			GUI.color = EditorGUIUtility.isProSkin ? z_GUI.BOX_OUTLINE_DARK : z_GUI.BOX_OUTLINE_LIGHT;
//			EditorGUI.DrawPreviewTexture(border, EditorGUIUtility.whiteTexture);
//			border.x += 1;
//			border.y += 1;
//			border.width -= 2;
//			border.height -= 2;
//			GUI.color = EditorGUIUtility.isProSkin ? z_GUI.BOX_BACKGROUND_DARK : z_GUI.BOX_BACKGROUND_LIGHT;
//			EditorGUI.DrawPreviewTexture(border, EditorGUIUtility.whiteTexture);
//			GUI.color = Color.white;

			GUI.Box(border, "");

			bool listNeedsPruning = false;

			if(count < 1)
			{
				if( GUI.skin.name.Contains("polybrush"))
					GUI.Label(border, "Drag Prefabs Here!", "dragprefablabel");
				else
					GUI.Label(border, "Drag Prefabs Here!", EditorStyles.centeredGreyMiniLabel);
			}

			for(int i = 0; i < count; i++)
			{
				SerializedProperty it = prefabs.GetArrayElementAtIndex(i);
				SerializedProperty prefab = it.FindPropertyRelative("gameObject");

				if( prefab == null || prefab.objectReferenceValue == null )
				{
					listNeedsPruning = true;
					continue;
				}

				if(i > 0 && i % columns == 0)
				{
					r.x = pad + margin_x;
					r.y += r.height + selected_rect_height;
				}

				if( z_GUILayout.AssetPreviewButton(r, prefab.objectReferenceValue, selected.Contains(i)) )
				{
					if(Event.current.shift || Event.current.control)
					{
						if(!selected.Add(i))
							selected.Remove(i);
					}
					else
					{
						selected.Clear();
						selected.Add(i);
					}

					if(onSelectionChanged != null)
						onSelectionChanged( selected );


					GUI.changed = true;
				}

				r.x += r.width + pad;
			}

			if(listNeedsPruning)
			{
				DeleteWhere(prefabs, (index, prop) =>
					{
						if(prop == null) return true;
						SerializedProperty g = prop.FindPropertyRelative("gameObject");
						return g == null || g.objectReferenceValue == null;
					});
			}

			Event e = Event.current;

			if( border.Contains(e.mousePosition) &&
				(e.type == EventType.DragUpdated || e.type == EventType.DragPerform) &&
				DragAndDrop.objectReferences.Length > 0 )
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if(e.type == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();

					IEnumerable<GameObject> dragAndDropReferences = DragAndDrop.objectReferences.Where(x => x is GameObject).Cast<GameObject>();

					foreach(GameObject go in dragAndDropReferences)
					{
						prefabs.InsertArrayElementAtIndex(prefabs.arraySize);
						SerializedProperty last = prefabs.GetArrayElementAtIndex(prefabs.arraySize - 1);
						SerializedProperty gameObject = last.FindPropertyRelative("gameObject");
						gameObject.objectReferenceValue = go;
					}
				}
			}

			if(e.type == EventType.KeyUp)
			{
				if( IsDeleteKey(e) )
				{
					DeleteWhere(prefabs, (i, v) => { return selected.Contains(i); } );
					selected.Clear();
					if(onSelectionChanged != null)
						onSelectionChanged(null);
					z_Editor.DoRepaint();
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void DeleteWhere(SerializedProperty array, System.Func<int, SerializedProperty, bool> lamdba)
		{
			int arraySize = array.arraySize;

			for(int i = arraySize - 1; i > -1; i--)
			{
				if( lamdba(i, array.GetArrayElementAtIndex(i)) )
					array.DeleteArrayElementAtIndex(i);
			}
		}
	}
}
