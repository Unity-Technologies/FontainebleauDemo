using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	public class z_SplatSet
	{
		const float WEIGHT_EPSILON = 0.0001f;

		// how many vertices are stored in this splatset
		private int weightCount;

		// channel to index in weights array
		private Dictionary<z_MeshChannel, int> channelMap;

		// splatset doesn't store an array of splatweight because it's too slow
		// to reconstruct mesh arrays from selecting each component of a splatweight.
		private Vector4[][] weights;

		// Assigns where each weight is applied on the mesh.
		public z_AttributeLayout[] attributeLayout;

		// The number of values being passed to the mesh (ex, color.rgba = 4)
		public int attributeCount { get { return attributeLayout.Length; } }

		/**
		 *	Initialize a new SplatSet with vertex count and attribute layout.  Attributes should
		 *	match the length of weights applied (one attribute per value).
		 *	Weight values are initialized to zero (unless preAlloc is false, then only the channel
		 *	container array is initialized and arrays aren't allocated)
		 */
		public z_SplatSet(int vertexCount, z_AttributeLayout[] attributes, bool preAlloc = true)
		{
			this.channelMap = z_SplatWeight.GetChannelMap(attributes);
			int channels = channelMap.Count;
			this.attributeLayout = attributes;
			this.weights = new Vector4[channels][];
			this.weightCount = vertexCount;

			if(preAlloc)
			{
				for(int i = 0; i < channels; i++)
					this.weights[i] = new Vector4[vertexCount];
			}
		}

		/**
		 *	Copy constructor.
		 */
		public z_SplatSet(z_SplatSet other)
		{
			int attribCount = other.attributeCount;
			this.attributeLayout = new z_AttributeLayout[attribCount];
			System.Array.Copy(other.attributeLayout, 0, this.attributeLayout, 0, attribCount);

			this.channelMap = new Dictionary<z_MeshChannel, int>();

			foreach(var kvp in other.channelMap)
				this.channelMap.Add(kvp.Key, kvp.Value);

			int channelCount = other.channelMap.Count;
			this.weightCount = other.weightCount;
			this.weights = new Vector4[channelCount][];

			for(int i = 0; i < channelCount; i++)
			{
				this.weights[i] = new Vector4[weightCount];
				System.Array.Copy(other.weights[i], this.weights[i], weightCount);
			}
		}

		private static Vector4 Color32ToVec4(Color32 color)
		{
			return new Vector4( color.r / 255f,
								color.g / 255f,
								color.b / 255f,
								color.a / 255f );
		}

		private static Color32 Vec4ToColor32(Vector4 vec)
		{
			return new Color32( (byte) (255 * vec.x),
								(byte) (255 * vec.y),
								(byte) (255 * vec.z),
								(byte) (255 * vec.w) );
		}

		/**
		 *	Initialize a SplatSet with mesh and attribute layout.
		 */
		public z_SplatSet(z_Mesh mesh, z_AttributeLayout[] attributes) : this(mesh.vertexCount, attributes, false)
		{
			foreach(var kvp in channelMap)
			{
				switch(kvp.Key)
				{
					case z_MeshChannel.UV0:
					case z_MeshChannel.UV2:
					case z_MeshChannel.UV3:
					case z_MeshChannel.UV4:
					{
						List<Vector4> uv = mesh.GetUVs( z_MeshChannelUtility.UVChannelToIndex(kvp.Key) );
						weights[kvp.Value] = uv.Count == weightCount ? uv.ToArray() : new Vector4[weightCount];
					}
					break;

					case z_MeshChannel.Color:
					{
						Color32[] color = mesh.colors;
						weights[kvp.Value] = color != null && color.Length == weightCount ? System.Array.ConvertAll(color, x => Color32ToVec4(x) ) : new Vector4[weightCount];
					}
					break;

					case z_MeshChannel.Tangent:
					{
						Vector4[] tangent = mesh.tangents;
						weights[kvp.Value] = tangent != null && tangent.Length == weightCount ? tangent : new Vector4[weightCount];
					}
					break;
				}
			}
		}

		public z_SplatWeight GetMinWeights()
		{
			z_SplatWeight min = new z_SplatWeight(channelMap);

			foreach(z_AttributeLayout al in attributeLayout)
			{
				Vector4 v = min[al.channel];
				v[(int)al.index] = al.min;
				min[al.channel] = v;
			}

			return min;
		}

		public z_SplatWeight GetMaxWeights()
		{
			z_SplatWeight max = new z_SplatWeight(channelMap);

			foreach(z_AttributeLayout al in attributeLayout)
			{
				Vector4 v = max[al.channel];
				v[(int)al.index] = al.max;
				max[al.channel] = v;
			}

			return max;
		}

		/**
		 *	Lerp each attribute value with matching `mask` to `rhs`.
		 *	weights, lhs, and rhs must have matching layout attributes.
		 */
		public void LerpWeights(z_SplatSet lhs, z_SplatSet rhs, int mask, float[] alpha)
		{
			Dictionary<int, uint> affected = new Dictionary<int, uint>();

			foreach(z_AttributeLayout al in attributeLayout)
			{
				int mapIndex = channelMap[al.channel];

				if(al.mask == mask)
				{
					if(!affected.ContainsKey(mapIndex))
						affected.Add(mapIndex, al.index.ToFlag());
					else
						affected[mapIndex] |= al.index.ToFlag();
				}
			}

			foreach(var v in affected)
			{
				Vector4[] a = lhs.weights[v.Key];
				Vector4[] b = rhs.weights[v.Key];
				Vector4[] c = weights[v.Key];

				for(int i = 0; i < weightCount; i++)
				{
					if((v.Value & 1) != 0) c[i].x = Mathf.Lerp(a[i].x, b[i].x, alpha[i]);
					if((v.Value & 2) != 0) c[i].y = Mathf.Lerp(a[i].y, b[i].y, alpha[i]);
					if((v.Value & 4) != 0) c[i].z = Mathf.Lerp(a[i].z, b[i].z, alpha[i]);
					if((v.Value & 8) != 0) c[i].w = Mathf.Lerp(a[i].w, b[i].w, alpha[i]);
				}
			}
		}

		public void LerpWeights(z_SplatSet lhs, z_SplatWeight rhs, float alpha)
		{
			for(int i = 0; i < weightCount; i++)
			{
				foreach(var cm in channelMap)
					this.weights[cm.Value][i] = Vector4.LerpUnclamped(lhs.weights[cm.Value][i], rhs[cm.Key], alpha);
			}
		}

		public void CopyTo(z_SplatSet other)
		{
			if(other.weightCount != weightCount)
			{
				Debug.LogError("Copying splat set to mis-matched container length");
				return;
			}

			for(int i = 0; i < channelMap.Count; i++)
				System.Array.Copy(this.weights[i], other.weights[i], weightCount);
		}

		public void Apply(z_Mesh mesh)
		{
			foreach(z_AttributeLayout al in attributeLayout)
			{
				switch(al.channel)
				{
					case z_MeshChannel.UV0:
					case z_MeshChannel.UV2:
					case z_MeshChannel.UV3:
					case z_MeshChannel.UV4:
					{
						List<Vector4> uv = new List<Vector4>(weights[channelMap[al.channel]]);
						mesh.SetUVs(z_MeshChannelUtility.UVChannelToIndex(al.channel), uv);
					}
					break;

					case z_MeshChannel.Color:
					{
						// @todo consider storing Color array separate from Vec4 since this cast costs ~5ms
						mesh.colors = System.Array.ConvertAll(weights[channelMap[al.channel]], x => Vec4ToColor32(x));
						break;
					}

					case z_MeshChannel.Tangent:
					{
						mesh.tangents = weights[channelMap[z_MeshChannel.Tangent]];
						break;
					}
				}
			}
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			foreach(z_AttributeLayout al in attributeLayout)
				sb.AppendLine(al.ToString());

			sb.AppendLine("--");

			for(int i = 0; i < weightCount; i++)
				sb.AppendLine(weights[i].ToString());

			return sb.ToString();
		}
	}
}
