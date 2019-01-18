using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	public static class z_SceneUtility
	{
		public static Ray InverseTransformRay(this Transform transform, Ray InWorldRay)
		{
			Vector3 o = InWorldRay.origin;
			o -= transform.position;
			o = transform.worldToLocalMatrix * o;
			Vector3 d = transform.worldToLocalMatrix.MultiplyVector(InWorldRay.direction);
			return new Ray(o, d);
		}

		/**
		 * Find the nearest triangle intersected by InWorldRay on this mesh.  InWorldRay is in world space.
		 * @hit contains information about the hit point.  @distance limits how far from @InWorldRay.origin the hit
		 * point may be.  @cullingMode determines what face orientations are tested (Culling.Front only tests front
		 * faces, Culling.Back only tests back faces, and Culling.FrontBack tests both).
		 * Ray origin and position values are in local space.
		 */
		public static bool WorldRaycast(Ray InWorldRay, Transform transform, Vector3[] vertices, int[] triangles, out z_RaycastHit hit, float distance = Mathf.Infinity, Culling cullingMode = Culling.Front)
		{
			Ray ray = transform.InverseTransformRay(InWorldRay);
			return MeshRaycast(ray, vertices, triangles, out hit, distance, cullingMode);
		}

		/**
		 *	Cast a ray (in model space) against a mesh.
		 */
		public static bool MeshRaycast(Ray InRay, Vector3[] vertices, int[] triangles, out z_RaycastHit hit, float distance = Mathf.Infinity, Culling cullingMode = Culling.Front)
		{
			// float dot; 		// vars used in loop
			float hitDistance = Mathf.Infinity;
			Vector3 hitNormal = new Vector3(0f, 0f, 0f);	// vars used in loop
			Vector3 a, b, c;
			int hitFace = -1;
			Vector3 o = InRay.origin, d = InRay.direction;

			/**
			 * Iterate faces, testing for nearest hit to ray origin.
			 */
			for(int CurTri = 0; CurTri < triangles.Length; CurTri += 3)
			{
				a = vertices[triangles[CurTri+0]];
				b = vertices[triangles[CurTri+1]];
				c = vertices[triangles[CurTri+2]];

				if(z_Math.RayIntersectsTriangle2(o, d, a, b, c, ref distance, ref hitNormal))
				{
					hitFace = CurTri / 3;
					hitDistance = distance;
					break;
				}
			}

			hit = new z_RaycastHit( hitDistance,
									InRay.GetPoint(hitDistance),
									hitNormal,
									hitFace);

			return hitFace > -1;
		}

		/**
		 *	Returns true if the event is one that should consume the mouse or keyboard.
		 */
		public static bool SceneViewInUse(Event e)
		{
			return 	e.alt
					|| Tools.current == Tool.View
					|| GUIUtility.hotControl > 0
					|| (e.isMouse ? e.button > 1 : false)
					|| Tools.viewTool == ViewTool.FPS
					|| Tools.viewTool == ViewTool.Orbit;
		}

		/**
		 *	Calculates the per-vertex weight for each raycast hit and fills in brush target weights.
		 */
		public static void CalculateWeightedVertices(z_BrushTarget target, z_BrushSettings settings)
		{
			if( target.editableObject == null )
				return;

			bool uniformScale = z_Math.VectorIsUniform(target.transform.lossyScale);
			float scale = uniformScale ? 1f / target.transform.lossyScale.x : 1f;

			z_Mesh mesh = target.editableObject.editMesh;

			List<List<int>> common = z_MeshUtility.GetCommonVertices(mesh);

			Transform transform = target.transform;
			int vertexCount = mesh.vertexCount;
			Vector3[] vertices = mesh.vertices;

			if(!uniformScale)
			{
				Vector3[] world = new Vector3[vertexCount];
				for(int i = 0; i < vertexCount; i++)
					world[i] = transform.TransformPoint(vertices[i]);
				vertices = world;
			}

			AnimationCurve curve = settings.falloffCurve;
			float radius = settings.radius * scale, falloff_mag = Mathf.Max((radius - radius * settings.falloff), 0.00001f);

			Vector3 hitPosition = Vector3.zero;
			z_RaycastHit hit;

			for(int n = 0; n < target.raycastHits.Count; n++)
			{
				hit = target.raycastHits[n];
				hit.SetVertexCount(vertexCount);

				hitPosition = uniformScale ? hit.position : transform.TransformPoint(hit.position);
				int c = common.Count;

				for(int i = 0; i < c; i++)
				{
					int commonArrayCount = common[i].Count;
					Vector3 a = hitPosition, b = vertices[common[i][0]];
					float x = a.x - b.x, y = a.y - b.y, z = a.z - b.z;
					float dist = Mathf.Sqrt(x*x + y*y + z*z);

					if(dist > radius)
					{
						for(int j = 0; j < commonArrayCount; j++)
							hit.weights[common[i][j]] = 0f;
					}
					else
					{
						float weight = Mathf.Clamp(curve.Evaluate(1f - Mathf.Clamp((radius - dist) / falloff_mag, 0f, 1f)), 0f, 1f);

						for(int j = 0; j < commonArrayCount; j++)
							hit.weights[common[i][j]] = weight;
					}
				}
			}

			target.GetAllWeights(true);
		}

		public static IEnumerable<GameObject> FindInstancesInScene(IEnumerable<GameObject> match, System.Func<GameObject, string> instanceNamingFunc)
		{
			IEnumerable<string> matches = match.Where(x => x != null).Select(y => instanceNamingFunc(y));

			return Object.FindObjectsOfType<GameObject>().Where(x => {
				return matches.Contains( x.name );
				});
		}

		/**
		 * Store the previous GIWorkflowMode and set the current value to OnDemand (or leave it Legacy).
		 */
		internal static void PushGIWorkflowMode()
		{
			z_Pref.SetInt("z_GIWorkflowMode", (int)Lightmapping.giWorkflowMode);

			if(Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.Legacy)
				Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
		}

		/**
		 * Return GIWorkflowMode to it's prior state.
		 */
		internal static void PopGIWorkflowMode()
		{
			// if no key found (?), don't do anything.
			if(!z_Pref.HasKey("z_GIWorkflowMode"))
				return;

			 Lightmapping.giWorkflowMode = (Lightmapping.GIWorkflowMode)z_Pref.GetInt("z_GIWorkflowMode");
		}
	}
}
