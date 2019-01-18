using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Polybrush
{
	[CustomEditor(typeof(z_ColorPalette))]
	public class z_ColorPaletteEditor : Editor
	{
		public class DragState
		{
			public enum Status
			{
				Ready,
				Dragging,
				DragInvalid
			}

			private Status _status = Status.Ready;
			private Status queued_status = Status.Ready;
			public SerializedProperty swatch;
			private int _sourceIndex;
			private int _destinationIndex;
			private int queued_destinationIndex, queued_sourceIndex;
			public Vector2 offset;

			public Status status {
				get {
					return _status;
				}

				set {
					queued_status = value;
					wantsUpdate = true;
				}
			}

			public int sourceIndex {
				get {
					return _sourceIndex;
				}

				set {
					queued_sourceIndex = value;
					wantsUpdate = true;
				}
			}

			public int destinationIndex {
				get {
					return _destinationIndex;
				}

				set {
					queued_destinationIndex = value;
					wantsUpdate = true;
				}
			}

			private bool wantsUpdate = false;
			private bool isBetweenRepaint = true;

			public void Init(int index, SerializedProperty swatch, Vector2 mouseOffset)
			{
				this.sourceIndex = index;
				this.destinationIndex = index;
				this.swatch = swatch;
				this.status = Status.Dragging;
				this.offset = mouseOffset;
				this.isBetweenRepaint = true;
				this.wantsUpdate = true;
			}

			public void Reset()
			{
				this.status = DragState.Status.Ready;
				this.swatch = null;
				this.sourceIndex = -1;
				this.destinationIndex = -1;
			}

			public void Update(Event e)
			{
				if(e.type == EventType.Layout)
					isBetweenRepaint = true;
				else if(e.type == EventType.Repaint)
					isBetweenRepaint = false;

				if(!wantsUpdate || isBetweenRepaint)
					return;

				wantsUpdate = false;

				_status = queued_status;
				_sourceIndex = queued_sourceIndex;
				_destinationIndex = queued_destinationIndex;

				z_Editor.DoRepaint();
			}

			public override string ToString()
			{
				return string.Format("{0}: {1} -> {2}", status, sourceIndex, destinationIndex);
			}
		}

		private SerializedProperty currentProperty;
		private SerializedProperty colorsProperty;

		public Delegate<Color> onSelectIndex = null;
		public Delegate<z_ColorPalette> onSaveAs = null;

		DragState drag = new DragState();
		const int DRAG_OVER_NULL = -1;
		const int DRAG_OVER_TRASH = -42;

		GUIContent gc_AddColorSwatch = new GUIContent( (Texture2D) null, "Add Selected Color to Palette");

		private void OnEnable()
		{
			currentProperty = serializedObject.FindProperty("current");
			colorsProperty = serializedObject.FindProperty("colors");
			gc_AddColorSwatch.image = z_IconUtility.GetIcon("Icon/AddColor");
		}

		private void SetCurrent(Color color)
		{
			if(onSelectIndex != null)
				onSelectIndex(color);

			currentProperty.colorValue = color;
		}

		int IncrementIndex(int index, int rowSize)
		{
			index++;

			if(index % rowSize == 0)
			{
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}

			return index;
		}

		public override void OnInspectorGUI()
		{
			z_GUI.PushGUISkin(z_GUI.PolybrushSkin);

			Event e = Event.current;

			serializedObject.Update();

			Color current = currentProperty.colorValue;

			z_GUI.PushUnitySkin();
			EditorGUI.BeginChangeCheck();
			current = EditorGUILayout.ColorField(current);
			if(EditorGUI.EndChangeCheck())
				SetCurrent(current);
			z_GUI.PopGUISkin();

			int swatchSize = 18;
			int viewWidth = (int) EditorGUIUtility.currentViewWidth - 12;
			int swatchesPerRow = viewWidth / (swatchSize + 4);
			swatchSize += (viewWidth % (swatchSize + 4)) / swatchesPerRow;

			GUILayout.BeginHorizontal();

			int mouseOverIndex = DRAG_OVER_NULL;
			int index = 0;
			int arraySize = colorsProperty.arraySize;
			int arraySizeWithAdd = colorsProperty.arraySize + 1;

			for(int i = 0; i < arraySizeWithAdd; i++)
			{
				bool isColorSwatch = i < arraySize;
				bool isActiveDrag = drag.status == DragState.Status.Dragging && drag.destinationIndex == i && i != arraySizeWithAdd - 1;
				SerializedProperty swatch = isColorSwatch ? colorsProperty.GetArrayElementAtIndex(i) : null;
				Rect swatchRect = new Rect(-1f, -1f, 0f, 0f);

				if(isActiveDrag)
				{
					GUILayout.Space(swatchSize + 4);
					index = IncrementIndex(index, swatchesPerRow);
				}

				if(isColorSwatch)
				{
					GUI.backgroundColor = swatch.colorValue;

					if(drag.status != DragState.Status.Dragging || i != drag.sourceIndex)
					{
						GUILayout.Label("", "ColorSwatch",
								GUILayout.MinWidth(swatchSize),
								GUILayout.MaxWidth(swatchSize),
								GUILayout.MinHeight(swatchSize),
								GUILayout.MaxHeight(swatchSize) );

						swatchRect = GUILayoutUtility.GetLastRect();

						index = IncrementIndex(index, swatchesPerRow);
					}
				}
				else
				{
					if( drag.status != DragState.Status.Dragging )
					{
						GUI.backgroundColor = current;

						if( GUILayout.Button(gc_AddColorSwatch, "ColorSwatch",
								GUILayout.MinWidth(swatchSize),
								GUILayout.MaxWidth(swatchSize),
								GUILayout.MinHeight(swatchSize),
								GUILayout.MaxHeight(swatchSize) ))
						{
							colorsProperty.arraySize++;
							SerializedProperty added = colorsProperty.GetArrayElementAtIndex(colorsProperty.arraySize - 1);
							added.colorValue = current;
						}
					}
					else
					{
						GUILayout.FlexibleSpace();
						GUILayout.Label(z_IconUtility.GetIcon("Icon/Trashcan"));
					}

					swatchRect = GUILayoutUtility.GetLastRect();
					index = IncrementIndex(index, swatchesPerRow);
				}

				GUI.backgroundColor = Color.white;

				if( swatchRect.Contains(e.mousePosition) )
				{
					if(drag.status == DragState.Status.Dragging)
						mouseOverIndex = i >= drag.destinationIndex ? i + 1 : i;
					else
						mouseOverIndex = i;

					if(i == arraySize)
						mouseOverIndex = DRAG_OVER_TRASH;

					if(e.type == EventType.MouseDrag)
					{
						if( drag.status == DragState.Status.Ready && isColorSwatch )
						{
							e.Use();
							drag.Init(mouseOverIndex, colorsProperty.GetArrayElementAtIndex(mouseOverIndex), swatchRect.position - e.mousePosition);
						}
						else if(drag.status == DragState.Status.Dragging)
						{
							drag.destinationIndex = mouseOverIndex;
						}
					}
					else if(e.type == EventType.MouseUp && drag.status != DragState.Status.Dragging &&  isColorSwatch)
					{
						if( onSelectIndex != null )
						{
							SetCurrent(swatch.colorValue);
						}
					}
				}
			}

			GUILayout.EndHorizontal();

			// If drag was previously over the trash bin but has moved, reset the index to be over the last array entry
			// instead.
			if( e.type == EventType.MouseDrag &&
				drag.status == DragState.Status.Dragging &&
				mouseOverIndex == DRAG_OVER_NULL &&
				drag.destinationIndex == DRAG_OVER_TRASH)
			{
				drag.destinationIndex = arraySize;
			}

			bool dragIsOverTrash = drag.destinationIndex == DRAG_OVER_TRASH;

			if(drag.status == DragState.Status.Dragging && drag.swatch != null)
			{
				Rect r = new Rect(e.mousePosition.x + drag.offset.x, e.mousePosition.y + drag.offset.y, swatchSize, swatchSize);
				GUI.backgroundColor = drag.swatch.colorValue;
				GUI.Label(r, "", dragIsOverTrash ? "ColorSwatchGhost" : "ColorSwatch");
				GUI.backgroundColor = Color.white;

				z_Editor.DoRepaint();
				Repaint();
			}

			switch( e.type )
			{
				case EventType.MouseUp:
				{
					if(drag.status == DragState.Status.Dragging)
					{
						if(drag.destinationIndex != DRAG_OVER_NULL)
						{
							if(dragIsOverTrash)
								colorsProperty.DeleteArrayElementAtIndex(drag.sourceIndex);
							else
								colorsProperty.MoveArrayElement(drag.sourceIndex, drag.destinationIndex > drag.sourceIndex ? drag.destinationIndex - 1 : drag.destinationIndex);
						}
					}

					drag.Reset();

					z_Editor.DoRepaint();
					Repaint();
				}
				break;
			}

			serializedObject.ApplyModifiedProperties();
			drag.Update(e);

			z_GUI.PopGUISkin();
		}

		private void DrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, serializedObject.targetObject.name);
		}

		private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			SerializedProperty col = colorsProperty.GetArrayElementAtIndex(index);
			Rect r = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 5);
			EditorGUI.PropertyField(r, col);
		}

		private void OnAddItem(ReorderableList list)
		{
			ReorderableList.defaultBehaviours.DoAddButton(list);

			SerializedProperty col = colorsProperty.GetArrayElementAtIndex(list.index);
			col.colorValue = Color.white;
		}

		public static z_ColorPalette AddNew()
		{
			string path = z_EditorUtility.FindFolder(z_Pref.ProductName + "/" + "Color Palettes");

			if(string.IsNullOrEmpty(path))
				path = "Assets";

			path = AssetDatabase.GenerateUniqueAssetPath(path + "/New Color Palette.asset");

			if(!string.IsNullOrEmpty(path))
			{
				z_ColorPalette palette = ScriptableObject.CreateInstance<z_ColorPalette>();
				palette.SetDefaultValues();

				AssetDatabase.CreateAsset(palette, path);
				AssetDatabase.Refresh();

				EditorGUIUtility.PingObject(palette);

				return palette;
			}

			return null;
		}
	}
}
