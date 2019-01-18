using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Polybrush
{
	/**
	 *	Base class for brush modes that modify the mesh.
	 */
	[System.Serializable]
	public abstract class z_BrushModeMesh : z_BrushMode
	{
		// All meshes that have ever been modified, ever.  Kept around to refresh mesh vertices
		// on Undo/Redo since Unity doesn't.
		private HashSet<Mesh> modifiedMeshes = new HashSet<Mesh>();
		private HashSet<object> modifiedPbMeshes = new HashSet<object>();
		EditorWindow _pbEditor = null;

		public override void OnBrushBeginApply(z_BrushTarget brushTarget, z_BrushSettings brushSettings)
		{
			_pbEditor = z_ReflectionUtil.ProBuilderEditorWindow;

			base.OnBrushBeginApply(brushTarget, brushSettings);
		}

		public override void OnBrushApply(z_BrushTarget brushTarget, z_BrushSettings brushSettings)
		{
			// false means no ToMesh or Refresh, true does.  Optional addl bool runs pb_Object.Optimize()
			brushTarget.editableObject.Apply(true);

			if(_pbEditor != null)
				z_ReflectionUtil.Invoke(_pbEditor, "Internal_UpdateSelectionFast", BindingFlags.Instance | BindingFlags.NonPublic);

			UpdateTempComponent(brushTarget, brushSettings);
		}

		public override void RegisterUndo(z_BrushTarget brushTarget)
		{
			if(z_ReflectionUtil.IsProBuilderObject(brushTarget.gameObject))
			{
				object pb = z_ReflectionUtil.GetComponent(brushTarget.gameObject, "pb_Object");
				Undo.RegisterCompleteObjectUndo(pb as UnityEngine.Object, UndoMessage);
				modifiedPbMeshes.Add(pb);
			}
			else
			{
				Undo.RegisterCompleteObjectUndo(brushTarget.editableObject.graphicsMesh, UndoMessage);
				modifiedMeshes.Add(brushTarget.editableObject.graphicsMesh);
			}

			brushTarget.editableObject.isDirty = true;
		}

		public override void UndoRedoPerformed(List<GameObject> modified)
		{
			modifiedMeshes = new HashSet<Mesh>(modifiedMeshes.Where(x => x != null));

			if(z_ReflectionUtil.ProBuilderExists())
			{
				// delete & undo causes cases where object is not null but the reference to it's pb_Object is
				HashSet<object> remove = new HashSet<object>();

				foreach(object pb in modifiedPbMeshes)
				{
					try
					{
						z_ReflectionUtil.ProBuilder_ToMesh(pb);
						z_ReflectionUtil.ProBuilder_Refresh(pb);
						z_ReflectionUtil.ProBuilder_Optimize(pb);
					}
					catch
					{
						remove.Add(pb);
					}

				}

				if(remove.Count() > 0)
					modifiedPbMeshes.SymmetricExceptWith(remove);
			}

			foreach(Mesh m in modifiedMeshes)
			{
				m.vertices = m.vertices;
				m.UploadMeshData(false);
			}

			base.UndoRedoPerformed(modified);
		}
	}
}
