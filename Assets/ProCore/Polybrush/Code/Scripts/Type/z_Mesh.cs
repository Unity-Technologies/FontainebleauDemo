using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Polybrush
{

	/**
	 * Caches the attributes of UnityEngine.Mesh class that Polybrush can edit.
	 *
	 * Necessary because accessing attributes on UnityEngine.Mesh always invokes
	 * an expensive C# <-> C++ trip, plus it copies data, which 99% of the time
	 * we don't need.
	 */
	public class z_Mesh
	{
		public string name = "";
		public Vector3[] vertices = null;
		public Vector3[] normals = null;
		public Color32[] colors = null;
		public Vector4[] tangents = null;
		public List<Vector4> uv0 = null;
		public List<Vector4> uv1 = null;
		public List<Vector4> uv2 = null;
		public List<Vector4> uv3 = null;
		private int _subMeshCount;
		private int[] triangles = null;
		private int[][] indices;
		private MeshTopology[] meshTopology = new MeshTopology[1] { MeshTopology.Triangles };
		public MeshTopology GetTopology(int index) { return meshTopology[index]; }

		public int subMeshCount
		{
			get
			{
				return _subMeshCount;
			}

			set
			{
				int[][] copy = new int[value][];
				MeshTopology[] copyTopology = new MeshTopology[value];
				if(indices != null) System.Array.Copy(indices, 0, copy, 0, _subMeshCount);
				System.Array.Copy(meshTopology, 0, copyTopology, 0, _subMeshCount);
				indices = copy;
				meshTopology = copyTopology;
				_subMeshCount = value;
			}
		}

		public int vertexCount { get { return vertices != null ? vertices.Length : 0; } }

		public List<Vector4> GetUVs(int index)
		{
			if(index == 0) return uv0;
			else if(index == 1) return uv1;
			else if(index == 2) return uv2;
			else if(index == 3) return uv3;
			return null;
		}

		public void SetUVs(int index, List<Vector4> uvs)
		{
			if(index == 0) uv0 = uvs;
			else if(index == 1) uv1 = uvs;
			else if(index == 2) uv2 = uvs;
			else if(index == 3) uv3 = uvs;
		}

		public void Clear()
		{
			subMeshCount = 0;
			vertices = null;
			normals = null;
			colors 	= null;
			tangents = null;
			uv0 = null;
			uv1 = null;
			uv2 = null;
			uv3 = null;
		}

		public int[] GetTriangles()
		{
			if(triangles == null)
				triangles = indices.SelectMany(x => x).ToArray();

			return triangles;
		}

		public int[] GetIndices(int index)
		{
			return indices[index];
		}

		public void SetTriangles(int[] triangles, int index)
		{
			indices[index] = triangles;
		}

		public void RecalculateNormals()
		{
			Vector3[] perTriangleNormal = new Vector3[vertexCount];
			int[] perTriangleAvg = new int[vertexCount];
			int[] tris = triangles;

			for(int i = 0; i < tris.Length; i += 3)
			{
				int a = tris[i], b = tris[i + 1], c = tris[i + 2];

				Vector3 cross = z_Math.Normal(vertices[a], vertices[b], vertices[c]);

				perTriangleNormal[a].x += cross.x;
				perTriangleNormal[b].x += cross.x;
				perTriangleNormal[c].x += cross.x;

				perTriangleNormal[a].y += cross.y;
				perTriangleNormal[b].y += cross.y;
				perTriangleNormal[c].y += cross.y;

				perTriangleNormal[a].z += cross.z;
				perTriangleNormal[b].z += cross.z;
				perTriangleNormal[c].z += cross.z;

				perTriangleAvg[a]++;
				perTriangleAvg[b]++;
				perTriangleAvg[c]++;
			}


			for(int i = 0; i < vertexCount; i++)
			{
				normals[i].x = perTriangleNormal[i].x * (float) perTriangleAvg[i];
				normals[i].y = perTriangleNormal[i].y * (float) perTriangleAvg[i];
				normals[i].z = perTriangleNormal[i].z * (float) perTriangleAvg[i];
			}
		}

		/**
		 * Apply the vertex attributes to a UnityEngine mesh (does not set triangles)
		 */
		public void ApplyAttributesToUnityMesh(Mesh m, z_MeshChannel attrib = z_MeshChannel.All)
		{
			// I guess the default value for attrib makes the compiler think that else is never
			// activated?
#pragma warning disable 0162
			if(attrib == z_MeshChannel.All)
			{
				m.vertices = vertices;
				m.normals = normals;
				m.colors32 = colors;
				m.tangents = tangents;

				m.SetUVs(0, uv0);
				m.SetUVs(1, uv1);
				m.SetUVs(2, uv2);
				m.SetUVs(3, uv3);
			}
			else
			{
				if((attrib & z_MeshChannel.Position) > 0) m.vertices = vertices;
				if((attrib & z_MeshChannel.Normal) > 0) m.normals = normals;
				if((attrib & z_MeshChannel.Color) > 0) m.colors32 = colors;
				if((attrib & z_MeshChannel.Tangent) > 0) m.tangents = tangents;
				if((attrib & z_MeshChannel.UV0) > 0) m.SetUVs(0, uv0);
				if((attrib & z_MeshChannel.UV2) > 0) m.SetUVs(1, uv1);
				if((attrib & z_MeshChannel.UV3) > 0) m.SetUVs(2, uv2);
				if((attrib & z_MeshChannel.UV4) > 0) m.SetUVs(3, uv3);
			}
#pragma warning restore 0162
		}
	}
}
