using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	/**
	 *	Static helper functions for working with meshes.
	 */
	public static class z_MeshUtility
	{
		/**
		 * Duplicate @src and return the copy.
		 */
		public static Mesh DeepCopy(Mesh src)
		{
			Mesh dest = new Mesh();
			Copy(dest, src);
			return dest;
		}

		/**
		 *	Copy @src mesh values to @dest
		 */
		public static void Copy(Mesh dest, Mesh src)
		{
			dest.Clear();
			dest.vertices = src.vertices;

			List<Vector4> uvs = new List<Vector4>();

			src.GetUVs(0, uvs); dest.SetUVs(0, uvs);
			src.GetUVs(1, uvs); dest.SetUVs(1, uvs);
			src.GetUVs(2, uvs); dest.SetUVs(2, uvs);
			src.GetUVs(3, uvs); dest.SetUVs(3, uvs);

			dest.normals = src.normals;
			dest.tangents = src.tangents;
			dest.boneWeights = src.boneWeights;
			dest.colors = src.colors;
			dest.colors32 = src.colors32;
			dest.bindposes = src.bindposes;

			dest.subMeshCount = src.subMeshCount;

			for(int i = 0; i < src.subMeshCount; i++)
				dest.SetIndices(src.GetIndices(i), src.GetTopology(i), i);

			dest.name = z_Util.IncrementPrefix("z", src.name);
		}

		/**
		 *	Creates a new mesh using only the @src positions, normals, and a new color array.
		 */
		public static Mesh CreateOverlayMesh(z_Mesh src)
		{
			Mesh m = new Mesh();
			m.name = "Overlay Mesh: " + src.name;
			m.vertices = src.vertices;
			m.normals = src.normals;
			m.colors = z_Util.Fill<Color>(new Color(0f, 0f, 0f, 0f), m.vertexCount);
			m.subMeshCount = src.subMeshCount;

			for(int i = 0; i < src.subMeshCount; i++)
			{
				if(src.GetTopology(i) == MeshTopology.Triangles)
				{
					int[] tris = src.GetIndices(i);
					int[] lines = new int[tris.Length * 2];
					int index = 0;
					for(int n = 0; n < tris.Length; n+=3)
					{
						lines[index++] = tris[n+0];
						lines[index++] = tris[n+1];
						lines[index++] = tris[n+1];
						lines[index++] = tris[n+2];
						lines[index++] = tris[n+2];
						lines[index++] = tris[n+0];
					}
					m.SetIndices(lines, MeshTopology.Lines, i);
				}
				else
				{
					m.SetIndices(src.GetIndices(i), src.GetTopology(i), i);
				}
			}
			return m;
		}

		private static readonly Color clear = new Color(0,0f,0,0f);

		public static Mesh CreateVertexBillboardMesh(z_Mesh src, List<List<int>> common)
		{
			int vertexCount = System.Math.Min( ushort.MaxValue / 4, common.Count() );

			Vector3[] positions = new Vector3[vertexCount * 4];
			Vector2[] uv0 		= new Vector2[vertexCount * 4];
			Vector2[] uv2 		= new Vector2[vertexCount * 4];
			Color[] colors 		= new Color[vertexCount * 4];
			int[] 	  tris 		= new int[vertexCount * 6];

			int n = 0;
			int t = 0;

			Vector3 up = Vector3.up;// * .1f;
			Vector3 right = Vector3.right;// * .1f;

			Vector3[] v = src.vertices;

			for(int i = 0; i < vertexCount; i++)
			{
				int tri = common[i][0];

				positions[t+0] = v[tri];
				positions[t+1] = v[tri];
				positions[t+2] = v[tri];
				positions[t+3] = v[tri];

				uv0[t+0] = Vector3.zero;
				uv0[t+1] = Vector3.right;
				uv0[t+2] = Vector3.up;
				uv0[t+3] = Vector3.one;

				uv2[t+0] = -up-right;
				uv2[t+1] = -up+right;
				uv2[t+2] =  up-right;
				uv2[t+3] =  up+right;

				tris[n+0] = t + 0;
				tris[n+1] = t + 1;
				tris[n+2] = t + 2;
				tris[n+3] = t + 1;
				tris[n+4] = t + 3;
				tris[n+5] = t + 2;

				colors[t+0] = clear;
				colors[t+1] = clear;
				colors[t+2] = clear;
				colors[t+3] = clear;

				t += 4;
				n += 6;
			}

			Mesh m = new Mesh();

			m.vertices = positions;
			m.uv = uv0;
			m.uv2 = uv2;
			m.colors = colors;
			m.triangles = tris;

			return m;
		}

		/**
		 *	Builds a lookup table for each vertex index and it's average normal with other vertices sharing a position.
		 */
		public static Dictionary<int, Vector3> GetSmoothNormalLookup(z_Mesh mesh)
		{
			Vector3[] n = mesh.normals;
			Dictionary<int, Vector3> normals = new Dictionary<int, Vector3>();

			if(n == null || n.Length != mesh.vertexCount)
				return normals;

			List<List<int>> groups = GetCommonVertices(mesh);

			Vector3 avg = Vector3.zero;
			Vector3 a = Vector3.zero;
			foreach(var group in groups)
			{
				avg.x = 0f;
				avg.y = 0f;
				avg.z = 0f;

				foreach(int i in group)
				{
					a = n[i];

					avg.x += a.x;
					avg.y += a.y;
					avg.z += a.z;
				}

				avg /= (float) group.Count();

				foreach(int i in group)
					normals.Add(i, avg);
			}

			return normals;
		}

		/// Store a temporary cache of common vertex indices.
		public static Dictionary<z_Mesh, List<List<int>>> commonVerticesCache = new Dictionary<z_Mesh, List<List<int>>>();

		/**
		 *	Builds a list<group> with each vertex index and a list of all other vertices sharing a position.
		 * 	key: Index in vertices array
		 *	value: List of other indices in positions array that share a point with this index.
		 */
		public static List<List<int>> GetCommonVertices(z_Mesh mesh)
		{
			List<List<int>> indices;

			if( commonVerticesCache.TryGetValue(mesh, out indices) )
			{
				// int min = mesh.vertexCount, max = 0;

				// for(int x = 0; x < indices.Count; x++)
				// {
				// 	for(int y = 0; y < indices[x].Count; y++)
				// 	{
				// 		int index = indices[x][y];
				// 		if(index < min) min = index;
				// 		if(index > max) max = index;
				// 	}
				// }

				// if(max - min + 1 == mesh.vertexCount)
					return indices;
			}

			Vector3[] v = mesh.vertices;
			int[] t = z_Util.Fill<int>((x) => { return x; }, v.Length);
			indices = t.ToLookup( x => (z_RndVec3)v[x] ).Select(y => y.ToList()).ToList();

			if(!commonVerticesCache.ContainsKey(mesh))
				commonVerticesCache.Add(mesh, indices);
			else
				commonVerticesCache[mesh] = indices;

			return indices;
		}

		public static List<z_CommonEdge> GetEdges(z_Mesh m)
		{
			Dictionary<int, int> lookup = GetCommonVertices(m).GetCommonLookup<int>();
			return GetEdges(m, lookup);
		}

		public static List<z_CommonEdge> GetEdges(z_Mesh m, Dictionary<int, int> lookup)
		{
			int[] tris = m.GetTriangles();
			int count = tris.Length;

			List<z_CommonEdge> edges = new List<z_CommonEdge>(count);

			for(int i = 0; i < count; i += 3)
			{
				edges.Add( new z_CommonEdge(tris[i+0], tris[i+1], lookup[tris[i+0]], lookup[tris[i+1]]) );
				edges.Add( new z_CommonEdge(tris[i+1], tris[i+2], lookup[tris[i+1]], lookup[tris[i+2]]) );
				edges.Add( new z_CommonEdge(tris[i+2], tris[i+0], lookup[tris[i+2]], lookup[tris[i+0]]) );
			}

			return edges;
		}

		public static HashSet<z_CommonEdge> GetEdgesDistinct(z_Mesh mesh, out List<z_CommonEdge> duplicates)
		{
			Dictionary<int, int> lookup = GetCommonVertices(mesh).GetCommonLookup<int>();
			return GetEdgesDistinct(mesh, lookup, out duplicates);
		}

		private static HashSet<z_CommonEdge> GetEdgesDistinct(z_Mesh m, Dictionary<int, int> lookup, out List<z_CommonEdge> duplicates)
		{
			int[] tris = m.GetTriangles();
			int count = tris.Length;

			HashSet<z_CommonEdge> edges = new HashSet<z_CommonEdge>();
			duplicates = new List<z_CommonEdge>();

			for(int i = 0; i < count; i += 3)
			{
				z_CommonEdge a = new z_CommonEdge(tris[i+0], tris[i+1], lookup[tris[i+0]], lookup[tris[i+1]]);
				z_CommonEdge b = new z_CommonEdge(tris[i+1], tris[i+2], lookup[tris[i+1]], lookup[tris[i+2]]);
				z_CommonEdge c = new z_CommonEdge(tris[i+2], tris[i+0], lookup[tris[i+2]], lookup[tris[i+0]]);

				if(!edges.Add(a))
					duplicates.Add(a);

				if(!edges.Add(b))
					duplicates.Add(b);

				if(!edges.Add(c))
					duplicates.Add(c);
			}

			return edges;
		}

		/**
		 *	Returns all vertex indices that are on an open edge.
		 */
		public static HashSet<int> GetNonManifoldIndices(z_Mesh mesh)
		{
			List<z_CommonEdge> duplicates;
			HashSet<z_CommonEdge> edges = GetEdgesDistinct(mesh, out duplicates);
			edges.ExceptWith(duplicates);
			HashSet<int> hash = z_CommonEdge.ToHashSet( edges );

			return hash;
		}

		/**
		 *	Builds a lookup with each vertex index and a list of all neighboring indices.
		 */
		public static Dictionary<int, List<int>> GetAdjacentVertices(z_Mesh mesh)
		{
			List<List<int>> common = GetCommonVertices(mesh);
			Dictionary<int, int> lookup = common.GetCommonLookup<int>();

			List<z_CommonEdge> edges = GetEdges(mesh, lookup).ToList();
			List<List<int>> map = new List<List<int>>();

			for(int i = 0; i < common.Count(); i++)
				map.Add(new List<int>());

			for(int i = 0; i < edges.Count; i++)
			{
				map[edges[i].cx].Add(edges[i].y);
				map[edges[i].cy].Add(edges[i].x);
			}

			Dictionary<int, List<int>> adjacent = new Dictionary<int, List<int>>();
			IEnumerable<int> distinctTriangles = mesh.GetTriangles().Distinct();

			foreach(int i in distinctTriangles)
				adjacent.Add(i, map[lookup[i]]);

			return adjacent;
		}

		static Dictionary<z_Mesh, Dictionary<z_Edge, List<int>>> adjacentTrianglesCache = new Dictionary<z_Mesh, Dictionary<z_Edge, List<int>>>();

		/**
		 *	Returns a dictionary where each z_Edge is mapped to a list of triangle indices that share that edge.
		 *	To translate triangle list to vertex indices, multiply by 3 and take those indices (ex, triangles[index+{0,1,2}])
		 */
		public static Dictionary<z_Edge, List<int>> GetAdjacentTriangles(z_Mesh m)
		{
			int len = m.GetTriangles().Length;

			if(len % 3 !=0 || len / 3 == m.vertexCount)
				return new Dictionary<z_Edge, List<int>>();

			Dictionary<z_Edge, List<int>> lookup = null;

			// @todo - should add some checks to make sure triangle structure hasn't changed
			if(adjacentTrianglesCache.TryGetValue(m, out lookup))
				return lookup;

			int smc = m.subMeshCount;

			lookup = new Dictionary<z_Edge, List<int>>();
			List<int> connections;

			for(int n = 0; n < smc; n++)
			{
				int[] tris = m.GetIndices(n);

				for(int i = 0; i < tris.Length; i+=3)
				{
					int index = i/3;

					z_Edge a = new z_Edge(tris[i  ], tris[i+1]);
					z_Edge b = new z_Edge(tris[i+1], tris[i+2]);
					z_Edge c = new z_Edge(tris[i+2], tris[i  ]);

					if(lookup.TryGetValue(a, out connections))
						connections.Add(index);
					else
						lookup.Add(a, new List<int>(){index});

					if(lookup.TryGetValue(b, out connections))
						connections.Add(index);
					else
						lookup.Add(b, new List<int>(){index});

					if(lookup.TryGetValue(c, out connections))
						connections.Add(index);
					else
						lookup.Add(c, new List<int>(){index});
				}
			}

			adjacentTrianglesCache.Add(m, lookup);

			return lookup;
		}

		private static Dictionary<z_Mesh, List<List<int>>> commonNormalsCache = new Dictionary<z_Mesh, List<List<int>>>();

		/**
		 *	Vertices that are common, form a seam, and should be smoothed.
		 */
		public static List<List<int>> GetSmoothSeamLookup(z_Mesh m)
		{
			Vector3[] normals = m.normals;

			if(normals == null)
				return null;

			List<List<int>> lookup = null;

			if(commonNormalsCache.TryGetValue(m, out lookup))
				return lookup;

			List<List<int>> common = GetCommonVertices(m);

			var z = common
				.SelectMany(x => x.GroupBy( i => (z_RndVec3)normals[i] ))
					.Where(n => n.Count() > 1)
						.Select(t => t.ToList())
							.ToList();

			commonNormalsCache.Add(m, z);

			return z;
		}

		/**
		 *	Recalculates a mesh's normals while retaining smoothed common vertices.
		 */
		public static void RecalculateNormals(z_Mesh m)
		{
			List<List<int>> smooth = GetSmoothSeamLookup(m);

			m.RecalculateNormals();

			if(smooth != null)
			{
				Vector3[] normals = m.normals;

				foreach(List<int> l in smooth)
				{
					Vector3 n = z_Math.Average(normals, l);

					foreach(int i in l)
						normals[i] = n;
				}

				m.normals = normals;
			}
		}

		/**
		 *	Get a string of the values in mesh.
		 */
		public static string Print(Mesh m, int maxAttributesToList = 8)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.AppendLine(string.Format("{0,-28}{1,-28}{2,-28}{3,-28}{4,-28}{5,-28}{6,-28}",
				"Positions",
				"Colors",
				"Tangents",
				"UV0",
				"UV2",
				"UV3",
				"UV4"));

			sb.AppendLine(string.Format("vertices: {0}   triangles: {1}", m.vertexCount, m.triangles.Length));

			Vector3[] positions = m.vertices;
			Color[] colors 		= m.colors;
			Vector4[] tangents 	= m.tangents;

			List<Vector4> uv0 	= new List<Vector4>();
			Vector2[] uv2 		= m.uv2;
			List<Vector4> uv3 	= new List<Vector4>();
			List<Vector4> uv4 	= new List<Vector4>();

#if !UNITY_4_7 && !UNITY_5_0
			m.GetUVs(0, uv0);
			m.GetUVs(2, uv3);
			m.GetUVs(3, uv4);
#else
			uv0 = m.uv.Cast<Vector4>().ToList();
#endif

			if( positions != null && positions.Count() != m.vertexCount)
				positions = null;
			if( colors != null && colors.Count() != m.vertexCount)
				colors = null;
			if( tangents != null && tangents.Count() != m.vertexCount)
				tangents = null;
			if( uv0 != null && uv0.Count() != m.vertexCount)
				uv0 = null;
			if( uv2 != null && uv2.Count() != m.vertexCount)
				uv2 = null;
			if( uv3 != null && uv3.Count() != m.vertexCount)
				uv3 = null;
			if( uv4 != null && uv4.Count() != m.vertexCount)
				uv4 = null;

			int vc = m.vertexCount;

			if(maxAttributesToList > -1 && maxAttributesToList < vc)
				vc = maxAttributesToList;

			for(int i = 0; i < vc; i ++)
			{
				sb.AppendLine(string.Format("{0,-28}{1,-28}{2,-28}{3,-28}{4,-28}{5,-28}{6,-28}",
					positions == null 	? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}", positions[i].x, positions[i].y, positions[i].z),
					colors == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", colors[i].r, colors[i].g, colors[i].b, colors[i].a),
					tangents == null 	? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", tangents[i].x, tangents[i].y, tangents[i].z, tangents[i].w),
					uv0 == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", uv0[i].x, uv0[i].y, uv0[i].z, uv0[i].w),
					uv2 == null 		? "null" : string.Format("{0:F2}, {1:F2}", uv2[i].x, uv2[i].y),
					uv3 == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", uv3[i].x, uv3[i].y, uv3[i].z, uv3[i].w),
					uv4 == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", uv4[i].x, uv4[i].y, uv4[i].z, uv4[i].w)));
			}

			int tc = m.triangles.Length;

			if(maxAttributesToList > -1 && maxAttributesToList * 3 < tc)
				tc = maxAttributesToList * 3;

			for(int i = 0; i < tc; i += 3)
				sb.AppendLine(string.Format("{0}, {1}, {2}", m.triangles[i], m.triangles[i+1], m.triangles[i+2]));

			return sb.ToString();
		}
	}
}
