using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.IO;

namespace Polybrush
{
	public static class z_EditorUtility
	{
		/**
		 *	True if this gameObject is in the Selection.gameObjects array or it is a
		 *	child of a gameObject in that array.
		 */
		public static bool InSelection(GameObject gameObject)
		{
			return InSelection(gameObject.transform);
		}

		public static bool InSelection(Transform trs)
		{
			Transform[] selection = Selection.transforms;
			Transform node = trs;

			do
			{
				if( selection.Contains(node) )	
					return true;

				node = node.parent;

			}
			while( node != null );

			return false;
		}

		/**
		 *	Return the mesh source, and the guid if applicable (scene instances don't get GUIDs).
		 */
		public static z_ModelSource GetMeshGUID(Mesh mesh, ref string guid)
		{
			string path = AssetDatabase.GetAssetPath(mesh);

			if(path != "")
			{
				AssetImporter assetImporter = AssetImporter.GetAtPath(path);

				if( assetImporter != null )
				{
					// Only imported model (e.g. FBX) assets use the ModelImporter,
					// where a saved asset will have an AssetImporter but *not* ModelImporter.
					// A procedural mesh (one only existing in a scene) will not have any.
					if (assetImporter is ModelImporter)
					{
						guid = AssetDatabase.AssetPathToGUID(path);
						return z_ModelSource.Imported;
					}
					else
					{
						guid = AssetDatabase.AssetPathToGUID(path);
						return z_ModelSource.Asset;
					}
				}
				else
				{
					return z_ModelSource.Scene;
				}
			}

			return z_ModelSource.Scene;
		}

		const int DIALOG_OK = 0;
		const int DIALOG_CANCEL = 1;
		const int DIALOG_ALT = 2;
		const string DO_NOT_SAVE = "DO_NOT_SAVE";

		/**
		 *	Save any modifications to the z_EditableObject.  If the mesh is a scene mesh or imported mesh, it
		 *	will be saved to a new asset.  If the mesh was originally an asset mesh, the asset is overwritten.
		 * 	\return true if save was successful, false if user-canceled or otherwise failed.
		 */
		public static bool SaveMeshAsset(Mesh mesh, MeshFilter meshFilter = null, SkinnedMeshRenderer skinnedMeshRenderer = null)
		{
			string save_path = DO_NOT_SAVE;

			string guid = null;
			z_ModelSource source = GetMeshGUID(mesh, ref guid);

			switch( source )
			{
				case z_ModelSource.Asset:

					int saveChanges = EditorUtility.DisplayDialogComplex(
						"Save Changes",
						"Save changes to edited mesh?",
						"Save",				// DIALOG_OK
						"Cancel",			// DIALOG_CANCEL
						"Save As");			// DIALOG_ALT

					if( saveChanges == DIALOG_OK )
						save_path = AssetDatabase.GetAssetPath(mesh);
					else if( saveChanges == DIALOG_ALT )
						save_path = EditorUtility.SaveFilePanelInProject("Save Mesh As", mesh.name + ".asset", "asset", "Save edited mesh to");
					else
						return false;

					break;

				case z_ModelSource.Imported:
				case z_ModelSource.Scene:
				default:
					// @todo make sure path is in Assets/
					save_path = EditorUtility.SaveFilePanelInProject("Save Mesh As", mesh.name + ".asset", "asset", "Save edited mesh to");
				break;
			}

			if( !save_path.Equals(DO_NOT_SAVE) && !string.IsNullOrEmpty(save_path) )
			{
				Object existing = AssetDatabase.LoadMainAssetAtPath(save_path);

				if( existing != null && existing is Mesh )
				{
					// save over an existing mesh asset
					z_MeshUtility.Copy((Mesh)existing, mesh);
					GameObject.DestroyImmediate(mesh);
				}
				else
				{
					AssetDatabase.CreateAsset(mesh, save_path );
				}

				AssetDatabase.Refresh();

				if(meshFilter != null)
					meshFilter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(save_path, typeof(Mesh));
				else if(skinnedMeshRenderer != null)
					skinnedMeshRenderer.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(save_path, typeof(Mesh));

				return true;
			}

			// Save was canceled
			return false;
		}

		const string SHADER_ATTRIB_FILE_EXTENSION = "pbs.json";

		/**
		 *	Store user-set shader attribute information.
		 *	Returns the path written to on success, null otherwise.
		 */
		public static string SaveMeshAttributesData(z_AttributeLayoutContainer container, bool overwrite = false)
		{
			return SaveMeshAttributesData(container.shader, container.attributes, overwrite);
		}
		
		public static string SaveMeshAttributesData(Shader shader, z_AttributeLayout[] attributes, bool overwrite = false)
		{
			if(shader == null || attributes == null)
			{
				Debug.LogError("Cannot save null attributes for shader.");
				return null;
			}

			string path = FindPolybrushMetaDataForShader(shader);
			string shader_path = AssetDatabase.GetAssetPath(shader);
			string shader_directory = Path.GetDirectoryName(shader_path);
			string shader_filename = Path.GetFileNameWithoutExtension(path);

			// metadata didn't exist before
			if(string.IsNullOrEmpty(path))
			{
				if( string.IsNullOrEmpty(shader_path) )
				{
					// how!?
					path = EditorUtility.SaveFilePanelInProject(
						"Save Polybrush Shader Attributes",
						shader_filename,
						SHADER_ATTRIB_FILE_EXTENSION,
						"Please enter a file name to save Polybrush shader metadata to.");

					if(string.IsNullOrEmpty(path))
					{
						Debug.LogWarning(string.Format("Could not save Polybrush shader metadata.  Please try again, possibly with a different file name or folder path."));
						return null;
					}
				}
				else
				{
					shader_filename = Path.GetFileNameWithoutExtension(shader_path);
					path = string.Format("{0}/{1}.{2}", shader_directory, shader_filename, SHADER_ATTRIB_FILE_EXTENSION);
				}
			}

			if(!overwrite && File.Exists(path))
			{
				// @todo
				Debug.LogWarning("shader metadata exists. calling function refuses to overwrite and lazy developer didn't add a save dialog here.");
				return null;
			}

			try
			{
				z_AttributeLayoutContainer container = z_AttributeLayoutContainer.Create(shader, attributes);
				string json = JsonUtility.ToJson(container, true);
				File.WriteAllText(path, json);
				return path;
			}
			catch(System.Exception e)
			{
				Debug.LogError("Failed saving Polybrush Shader MetaData\n" + e.ToString());
				return path;
			}
		}

		private static bool TryReadAttributeLayoutsFromJson(string path, out z_AttributeLayoutContainer container)
		{
			container = null;

			if(!File.Exists(path))
				return false;

			string json = File.ReadAllText(path);

			if( string.IsNullOrEmpty(json) )
				return false;

			container = ScriptableObject.CreateInstance<z_AttributeLayoutContainer>();
			JsonUtility.FromJsonOverwrite(json, container);

			return true;
		}

		/**
		 *	Find a path to the Polybrush metadata for a shader.  Returns null if not found.
		 */
		public static string FindPolybrushMetaDataForShader(Shader shader)
		{
			if(shader == null)
				return null;

			string path = AssetDatabase.GetAssetPath(shader);

			if(string.IsNullOrEmpty(path))
				return null;

			string filename = Path.GetFileNameWithoutExtension(path);
			string directory = Path.GetDirectoryName(path);

			string[] paths = new string[]
			{
				string.Format("{0}/{1}.{2}", directory, z_ShaderUtil.GetMetaDataPath(shader), SHADER_ATTRIB_FILE_EXTENSION),
				string.Format("{0}/{1}.{2}", directory, filename, SHADER_ATTRIB_FILE_EXTENSION)
			};

			// @todo verify that the json is actually valid
			foreach(string str in paths)
			{
				if(File.Exists(str))
				{
					// remove `..` from path since `AssetDatabase.LoadAssetAtPath` doesn't like 'em
					string full = Path.GetFullPath(str).Replace("\\", "/");
					string resolved = full.Replace(Application.dataPath, "Assets");
					return resolved;
				}
			}

			return null;
		}

		/**
		 *	Searches only by looking for a compatibly named file in the same directory.
		 */
		public static bool FindMeshAttributesForShader(Shader shader, out z_AttributeLayoutContainer attributes)
		{
			attributes = null;

			string path = AssetDatabase.GetAssetPath(shader);
			string filename = Path.GetFileNameWithoutExtension(path);
			string directory = Path.GetDirectoryName(path);

			string[] paths = new string[]
			{
				string.Format("{0}/{1}.{2}", directory, filename, SHADER_ATTRIB_FILE_EXTENSION),
				string.Format("{0}/{1}.{2}", directory, z_ShaderUtil.GetMetaDataPath(shader), SHADER_ATTRIB_FILE_EXTENSION)
			};

			foreach(string str in paths)
			{
				if( TryReadAttributeLayoutsFromJson(str, out attributes) )
					return true;
			}

			return false;
		}

		/**
		 *	Load a Unity internal icon.
		 */
		internal static Texture2D LoadIcon(string icon)
		{
			MethodInfo loadIconMethod = typeof(EditorGUIUtility).GetMethod("LoadIcon", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			Texture2D img = (Texture2D) loadIconMethod.Invoke(null, new object[] { icon } );
			return img;
		}

		public static string RootFolder
		{
			get
			{
				if( Directory.Exists("Assets/ProCore/" + z_Pref.ProductName) )
				{
					return "Assets/ProCore/" + z_Pref.ProductName + "/";
				}
				else
				{
					string[] matches = Directory.GetDirectories("Assets/", z_Pref.ProductName, SearchOption.AllDirectories);

					if(matches != null && matches.Length == 1)
					{
						return matches[0];
					}
					else
					{
						Debug.LogError("Cannot find " + z_Pref.ProductName + " folder!  Please re-install the package.");
						return "";
					}
				}
			}
		}

		public static string FindFolder(string folder)
		{
			string single = folder.Replace("\\", "/").Substring(folder.LastIndexOf('/') + 1);

			string[] matches = Directory.GetDirectories("Assets/", single, SearchOption.AllDirectories);

			foreach(string str in matches)
			{
				if(str.Replace("\\", "/").Contains(folder))
					return str;
			}
			return null;
		}

		/**
		 *	Fetch a default asset from path relative to the product folder.  If not found, a new one is created.
		 */
		public static T GetDefaultAsset<T>(string path) where T : UnityEngine.ScriptableObject, z_IHasDefault
		{
			string full = z_EditorUtility.RootFolder + path;

			T asset = AssetDatabase.LoadAssetAtPath<T>(full);

			if(asset == null)
			{
				asset = ScriptableObject.CreateInstance<T>();
				asset.SetDefaultValues();
				EditorUtility.SetDirty(asset);

				string folder = Path.GetDirectoryName(full);

				if(!Directory.Exists(folder))
					Directory.CreateDirectory(folder);

				AssetDatabase.CreateAsset(asset, full);
			}

			return asset;
		}

		/**
		 *	Set the selected render state for an object.  In Unity 5.4 and lower, this just toggles wireframe 
		 *	on or off.
		 */
		public static void SetSelectionRenderState(Renderer renderer, z_SelectionRenderState state)
		{
			#if UNITY_5_3 || UNITY_5_4
				EditorUtility.SetSelectedWireframeHidden(renderer, state == 0);
			#else
				EditorUtility.SetSelectedRenderState(renderer, (EditorSelectedRenderState) state ); 
			#endif
		}

		public static z_SelectionRenderState GetSelectionRenderState()
		{

			#if UNITY_5_3 || UNITY_5_4

			return z_SelectionRenderState.Wireframe;

			#else

			bool wireframe = false, outline = false;

			try {			
				wireframe = (bool) z_ReflectionUtil.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionWire");
				outline = (bool) z_ReflectionUtil.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionOutline");
			} catch {
				Debug.LogWarning("Looks like Unity changed the AnnotationUtility \"showSelectionOutline\"\nPlease email contact@procore3d.com and let Karl know!");
			}

			z_SelectionRenderState state = z_SelectionRenderState.None;

			if(wireframe) state |= z_SelectionRenderState.Wireframe;
			if(outline) state |= z_SelectionRenderState.Outline;

			return state;

			#endif
		}
	}
}
