using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Polybrush
{
	/**
	 *	Stores a cache of the unmodified mesh and meshrenderer
	 *	so that the z_Editor can work non-destructively.  Also
	 * 	handles ProBuilder compatibility so that brush modes don't
	 * 	have to deal with it.
	 */
	public class z_EditableObject : IEquatable<z_EditableObject>, z_IValid
	{
		const string INSTANCE_MESH_GUID = null;

		private static HashSet<string> UnityPrimitiveMeshNames = new HashSet<string>()
		{
			"Sphere",
			"Capsule",
			"Cylinder",
			"Cube",
			"Plane",
			"Quad"
		};

		// The GameObject being modified.
		public GameObject gameObject = null;

		// The mesh that is
		private Mesh _graphicsMesh = null;

		public Mesh graphicsMesh { get { return _graphicsMesh; } }

		[System.Obsolete("Use graphicsMesh or editMesh instead")]
		public Mesh mesh { get { return _graphicsMesh; } }

		private z_Mesh _editMesh = null;

		public z_Mesh editMesh { get { return _editMesh; } }

		// The original mesh.  Can be the same as mesh.
		public Mesh originalMesh { get; private set; }

		// Where this mesh originated.
		public z_ModelSource source { get; private set; }

		// If mesh was an asset or model, save the original GUID
		// public string sourceGUID { get; private set; }

		// Marks this object as having been modified.
		public bool modified = false;

		private T GetAttribute<T>(System.Func<Mesh, T> getter) where T : IList
		{
			if(usingVertexStreams)
			{
				int vertexCount = originalMesh.vertexCount;
				T arr = getter(this.graphicsMesh);
				if(arr != null && arr.Count == vertexCount)
					return arr;
			}
			return getter(originalMesh);
		}

		/**
		 *	Return a mesh that is the combination of both additionalVertexStreams and the originalMesh.
		 *		- Position
		 *		- UV0
		 *		- UV2
		 *		- UV3
		 *		- UV4
		 *		- Color
		 *		- Tangent
		 *	If usingVertexStreams is false, null is returned.
		 */
		private z_Mesh GetCompositeMesh()
		{
			if(_editMesh == null)
				_editMesh = new z_Mesh();

			_editMesh.Clear();
			_editMesh.name = originalMesh.name;
			_editMesh.vertices	= GetAttribute<Vector3[]>(x => { return x.vertices; } );
			_editMesh.normals	= GetAttribute<Vector3[]>(x => { return x.normals; } );
			_editMesh.colors 	= GetAttribute<Color32[]>(x => { return x.colors32; } );
			_editMesh.tangents	= GetAttribute<Vector4[]>(x => { return x.tangents; } );
			_editMesh.uv0 = GetAttribute<List<Vector4>>(x => { List<Vector4> l = new List<Vector4>(); x.GetUVs(0, l); return l; } );
			_editMesh.uv1 = GetAttribute<List<Vector4>>(x => { List<Vector4> l = new List<Vector4>(); x.GetUVs(1, l); return l; } );
			_editMesh.uv2 = GetAttribute<List<Vector4>>(x => { List<Vector4> l = new List<Vector4>(); x.GetUVs(2, l); return l; } );
			_editMesh.uv3 = GetAttribute<List<Vector4>>(x => { List<Vector4> l = new List<Vector4>(); x.GetUVs(3, l); return l; } );

			_editMesh.subMeshCount = originalMesh.subMeshCount;

			for(int i = 0; i < _editMesh.subMeshCount; i++)
				_editMesh.SetTriangles(originalMesh.GetTriangles(i), i);

			return _editMesh;
		}

		public int vertexCount { get { return originalMesh.vertexCount; } }

		// Convenience getter for gameObject.GetComponent<MeshFilter>().
		public MeshFilter meshFilter { get; private set; }

		// Convenience getter for gameObject.transform
		public Transform transform { get { return gameObject.transform; } }

		// Convenience getter for gameObject.renderer
		public Renderer renderer { get { return gameObject.GetComponent<MeshRenderer>(); } }

		// If this object's mesh has been edited, isDirty will be flagged meaning that the mesh should not be
		// cleaned up when finished editing.
		public bool isDirty = false;

		// Is the mesh owned by ProBuilder?
		public bool isProBuilderObject { get; private set; }

		// Reference to the pb_Object component (if it exists)
		[SerializeField] private object probuilderMesh = null;

		// Is the mesh using additional vertex streams?
		public bool usingVertexStreams { get; private set; }

		// Container for additionalVertexStreams. @todo remove when Unity fixes
		public z_AdditionalVertexStreams additionalVertexStreams;

		// Did this mesh already have an additionalVertexStreams mesh?
		private bool hadVertexStreams = true;

		/// <summary>
		/// Shorthand for checking if object and mesh are non-null.
		/// </summary>
		public bool IsValid
		{
			get
			{
				if(gameObject == null || graphicsMesh == null)
					return false;

				if(isProBuilderObject && probuilderMesh != null)
				{
					object vertexCount = z_ReflectionUtil.GetValue(probuilderMesh, z_ReflectionUtil.ProBuilderObjectType, "vertexCount");

					if(vertexCount != null)
					{
						if(_editMesh != null && _editMesh.vertexCount != (int)vertexCount)
						{
							return false;
						}
					}
				}

				return true;
			}
		}

		/**
		 *	Public constructor for editable objects.  Guarantees that a mesh
		 *	is editable and takes care of managing the asset.
		 */
		public static z_EditableObject Create(GameObject go)
		{
			if(go == null)
				return null;

			MeshFilter mf = go.GetComponent<MeshFilter>();
			SkinnedMeshRenderer sf = go.GetComponent<SkinnedMeshRenderer>();

			if(!mf && !sf)
			{
				mf = go.GetComponentsInChildren<MeshFilter>().FirstOrDefault();
				sf = go.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault();
			}

			if((mf == null || mf.sharedMesh == null) && (sf == null || sf.sharedMesh == null))
				return null;

			return new z_EditableObject(go);
		}

		/**
		 *	Internal constructor.
		 *	\sa Create
		 */
		private z_EditableObject(GameObject go)
		{
			this.gameObject = go;
			isProBuilderObject = z_ReflectionUtil.IsProBuilderObject(go);

			Mesh advsMesh = null;
			MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
			meshFilter = this.gameObject.GetComponent<MeshFilter>();
			SkinnedMeshRenderer skinFilter = this.gameObject.GetComponent<SkinnedMeshRenderer>();
			usingVertexStreams = false;

			this.originalMesh = meshFilter.sharedMesh;

			if(originalMesh == null && skinFilter != null)
				this.originalMesh = skinFilter.sharedMesh;

			if( z_Pref.GetBool(z_Pref.additionalVertexStreams, false) && !isProBuilderObject)
			{
				if(meshRenderer != null || skinFilter != null)
				{
					additionalVertexStreams = gameObject.GetComponent<z_AdditionalVertexStreams>();

					if(additionalVertexStreams == null)
						additionalVertexStreams = gameObject.AddComponent<z_AdditionalVertexStreams>();

					advsMesh = additionalVertexStreams.m_AdditionalVertexStreamMesh;

					if(advsMesh == null)
					{
						advsMesh = new Mesh();
						advsMesh.vertices = originalMesh.vertices;
						advsMesh.name = string.Format("{0}({1})", originalMesh.name, additionalVertexStreams.GetInstanceID());
						hadVertexStreams = false;
					}

					usingVertexStreams = true;
				}
			}

			if(!usingVertexStreams)
			{
				// if editing a non-scene instance mesh, make it an instance
				// (unity primitives are a special case - they *are* scene instances but they also aren't)
				string guid = INSTANCE_MESH_GUID;
				this.source = z_EditorUtility.GetMeshGUID(originalMesh, ref guid);

				if(source != z_ModelSource.Scene || UnityPrimitiveMeshNames.Contains(originalMesh.name))
					this._graphicsMesh = z_MeshUtility.DeepCopy(meshFilter.sharedMesh);
				else
					this._graphicsMesh = originalMesh;
			}
			else
			{
				this._graphicsMesh = advsMesh;
				this.source = z_ModelSource.AdditionalVertexStreams;
			}

			// if it's a probuilder object rebuild the mesh without optimization
			if( isProBuilderObject )
			{
				object pb = probuilderMesh = go.GetComponent("pb_Object");

				if(pb != null)
				{
					z_ReflectionUtil.ProBuilder_ToMesh(pb);
					z_ReflectionUtil.ProBuilder_Refresh(pb, (ushort) 0xFF);
				}

				if(setVerticesMethod == null)
				{
					setVerticesMethod = pb.GetType().GetMethod(
						"SetVertices",
						BindingFlags.Public | BindingFlags.Instance,
						null,
						SetVerticesArguments,
						null);
				}

				if(setUvsMethod == null)
				{
					setUvsMethod = pb.GetType().GetMethod(
						"SetUVs",
						BindingFlags.Public | BindingFlags.Instance,
						null,
						SetUVsArguments,
						null);
				}

				if(setTangentsMethod == null)
				{
					setTangentsMethod = pb.GetType().GetMethod(
						"SetTangents",
						BindingFlags.Public | BindingFlags.Instance,
						null,
						new Type[] { typeof(Vector4[]) },
						null);
				}

				if(setColorsMethod == null)
				{
					setColorsMethod = pb.GetType().GetMethod(
						"SetColors",
						BindingFlags.Public | BindingFlags.Instance,
						null,
						new Type[] { typeof(Color[]) },
						null);
				}
			}

			_editMesh = GetCompositeMesh();

			if(!isProBuilderObject)
				SetMesh(graphicsMesh);
		}

		~z_EditableObject()
		{
			// clean up the composite mesh (if required)
			// delayCall ensures Destroy is called on main thread
			// if(editMesh != null)
			// 	EditorApplication.delayCall += () => { GameObject.DestroyImmediate(editMesh); };
		}

		/**
		 * Sets the MeshFilter.sharedMesh or SkinnedMeshRenderer.sharedMesh to @mesh.
		 */
		private void SetMesh(Mesh m)
		{
			if( gameObject != null )
			{
				if(usingVertexStreams)
				{
					additionalVertexStreams.SetAdditionalVertexStreamsMesh(m);
				}
				else
				{
					MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

					if(meshFilter != null)
					{
						meshFilter.sharedMesh = m;
					}
					else
					{
						SkinnedMeshRenderer mr = gameObject.GetComponent<SkinnedMeshRenderer>();

						if(mr != null)
							mr.sharedMesh = m;
					}
				}
			}
		}

		private static readonly Type[] SetVerticesArguments = new Type[] { typeof(Vector3[]) };
		private static readonly Type[] SetUVsArguments = new Type[] { typeof(int), typeof(List<Vector4>) };

		private static MethodInfo setVerticesMethod;
		private static MethodInfo setColorsMethod;
		private static MethodInfo setTangentsMethod;
		private static MethodInfo setUvsMethod;

		/**
		 *	Applies mesh changes back to the pb_Object (if necessary).  Optionally does a
		 *	mesh rebuild.
		 *	@rebuildMesh only applies to ProBuilder meshes.
		 *  @optimize determines if the mesh collisions are rebuilt (if that option is enabled) or if
		 *	the mehs is a probuilder object, the mesh is optimized (condensed to share verts, other
		 *	otpimziations etc)
		 */
		public void Apply(bool rebuildMesh, bool optimize = false)
		{
			if(usingVertexStreams)
			{
				if( z_Pref.GetBool(z_Pref.rebuildNormals) )
					z_MeshUtility.RecalculateNormals(editMesh);

				editMesh.ApplyAttributesToUnityMesh(graphicsMesh);
				graphicsMesh.UploadMeshData(false);
				EditorUtility.SetDirty(gameObject.GetComponent<MeshRenderer>());
				return;
			}

			// if it's a probuilder object rebuild the mesh without optimization
			if( isProBuilderObject )
			{
				object pb = probuilderMesh;

				if(pb != null)
				{
					// Set the pb_Object.vertices array so that pb_Editor.UpdateSelection
					// can draw the wireframes correctly.

					if(setVerticesMethod != null)
						setVerticesMethod.Invoke(pb, new object[] { editMesh.vertices } );

					if(!optimize)
						goto NonProbuilderMeshRebuild;

					Color[] colors = System.Array.ConvertAll(editMesh.colors, x => (Color) x);
					if(setColorsMethod != null && colors != null && colors.Length == vertexCount)
						setColorsMethod.Invoke(pb, new object[] { colors } );

					Vector4[] tangents = editMesh.tangents;
					if(setTangentsMethod != null && tangents != null && tangents.Length == vertexCount)
						setTangentsMethod.Invoke(pb, new object[] { tangents } );

					// Check if UV3/4 have been modified
					List<Vector4> uv3 = editMesh.GetUVs(2);
					List<Vector4> uv4 = editMesh.GetUVs(3);

					if(setUvsMethod != null && uv3.Count == vertexCount)
						setUvsMethod.Invoke(pb, new object[] { 2, uv3 } );

					if(setUvsMethod != null && uv4.Count == vertexCount)
						setUvsMethod.Invoke(pb, new object[] { 3, uv4 } );

					if(rebuildMesh)
					{
						z_ReflectionUtil.ProBuilder_ToMesh(pb);
						z_ReflectionUtil.ProBuilder_Refresh(pb, (ushort) (optimize ? 0xFF : (0x2 | 0x4 | 0x8)));
					}

					return;
				}
			}

NonProbuilderMeshRebuild:

			if( z_Pref.GetBool(z_Pref.rebuildNormals) )
				z_MeshUtility.RecalculateNormals(editMesh);

			editMesh.ApplyAttributesToUnityMesh(graphicsMesh);

			graphicsMesh.RecalculateBounds();

			// expensive call, delay til optimize is enabled.
			if(z_Pref.GetBool(z_Pref.rebuildCollisions) && optimize)
			{
				MeshCollider mc = gameObject.GetComponent<MeshCollider>();

				if(mc != null)
				{
					mc.sharedMesh = null;
					mc.sharedMesh = graphicsMesh;
				}
			}

		}

		/**
		 * Apply the mesh channel attributes to the graphics mesh.
		 */
		public void ApplyMeshAttributes(z_MeshChannel channel = z_MeshChannel.All)
		{
			editMesh.ApplyAttributesToUnityMesh(_graphicsMesh, channel);

			if(usingVertexStreams)
				_graphicsMesh.UploadMeshData(false);
		}

		/**
		 *	Set the MeshFilter or SkinnedMeshRenderer back to originalMesh.
		 */
		public void Revert()
		{
			if(usingVertexStreams)
			{
				if(!hadVertexStreams)
				{
					GameObject.DestroyImmediate(graphicsMesh);
					MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
					mr.additionalVertexStreams = null;
				}
				return;
			}

			if(	originalMesh == null ||
				(source == z_ModelSource.Scene && !UnityPrimitiveMeshNames.Contains(originalMesh.name)) ||
				isProBuilderObject
			)
			{
				if( isProBuilderObject )
					Apply(true, true);
				return;
			}

			if(graphicsMesh != null)
				GameObject.DestroyImmediate(graphicsMesh);

			SetMesh(originalMesh);
		}

		public bool Equals(z_EditableObject rhs)
		{
			return rhs.GetHashCode() == this.GetHashCode();
		}

		public override bool Equals(object rhs)
		{
			if(rhs == null)
				return this.gameObject == null ? true : false;
			else if(this.gameObject == null)
				return false;

			if(rhs is z_EditableObject)
				return rhs.Equals(this);
			else if(rhs is GameObject)
				return ((GameObject)rhs).GetHashCode() == gameObject.GetHashCode();

			return false;
		}

		public override int GetHashCode()
		{
			return gameObject != null ? gameObject.GetHashCode() : base.GetHashCode();
		}
	}
}
