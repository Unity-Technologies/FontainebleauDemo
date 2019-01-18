using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	public class z_SplatWeight
	{	
		private Dictionary<z_MeshChannel, int> map;
		float[] values;

		public Vector4 this[z_MeshChannel channel]
		{
			get { return GetVec4(map[channel]); }
			set { SetVec4(map[channel], value); }
		}

		public float this[z_AttributeLayout attribute]
		{
			get { return GetAttributeValue(attribute); }
			set { SetAttributeValue(attribute, value); }
		}

		public float this[int valueIndex]
		{
			get { return values[valueIndex]; }
			set { values[valueIndex] = value; }
		}

		public z_SplatWeight(Dictionary<z_MeshChannel, int> map)
		{
			this.map = new Dictionary<z_MeshChannel, int>();
			foreach(var v in map)
				this.map.Add(v.Key, v.Value);
			this.values = new float[map.Count * 4];
		}

		/**
		 *	Deep copy constructor.
		 */
		public z_SplatWeight(z_SplatWeight rhs)
		{
			this.map = new Dictionary<z_MeshChannel, int>();

			foreach(var kvp in rhs.map)
				this.map.Add(kvp.Key, kvp.Value);

			int len = rhs.values.Length;
			this.values = new float[len];
			System.Array.Copy(rhs.values, this.values, len);
		}

		public static Dictionary<z_MeshChannel, int> GetChannelMap(z_AttributeLayout[] attributes)
		{
			int index = 0;

			Dictionary<z_MeshChannel, int> channelMap = new Dictionary<z_MeshChannel, int>();

			foreach(z_MeshChannel ch in attributes.Select(x => x.channel).Distinct())
				channelMap.Add(ch, index++);

			return channelMap;
		}

		public List<int> GetAffectedIndicesWithMask(z_AttributeLayout[] attributes, int mask)
		{
			List<int> affected = new List<int>();

			foreach(z_AttributeLayout attrib in attributes)
			{
				if(attrib.mask == mask)
					affected.Add( map[attrib.channel] * 4 + (int) attrib.index );
			}

			return affected;
		}

		public bool MatchesAttributes(z_AttributeLayout[] attributes)
		{
			for(int i = 0; i < attributes.Length; i++)
				if(!map.ContainsKey(attributes[i].channel))
					return false;
			return true;
		}

		private Vector4 GetVec4(int index)
		{
			return new Vector4(
				values[index * 4    ],
				values[index * 4 + 1],
				values[index * 4 + 2],
				values[index * 4 + 3]);
		}

		private void SetVec4(int index, Vector4 value)
		{
			values[index * 4    ] = value.x;
			values[index * 4 + 1] = value.y;
			values[index * 4 + 2] = value.z;
			values[index * 4 + 3] = value.w;
		}

		public float GetAttributeValue(z_AttributeLayout attrib)
		{
			return values[map[attrib.channel] * 4 + (int) attrib.index];
		}

		public void SetAttributeValue(z_AttributeLayout attrib, float value)
		{
			values[map[attrib.channel] * 4 + (int) attrib.index] = value;
		}

		/**
		 *	Copy values array to another splatweight.  This function doesn't check
		 *	that attribute layouts are matching; musht do this yourself.
		 */
		public void CopyTo(z_SplatWeight other)
		{
			for(int i = 0; i < values.Length; i++)
				other.values[i] = this.values[i];
		}

		public void Lerp(z_SplatWeight lhs, z_SplatWeight rhs, float alpha)
		{
			int len = values.Length;

			if(len == 4)
			{
				values[0] = Mathf.LerpUnclamped(lhs.values[0], rhs.values[0], alpha);
				values[1] = Mathf.LerpUnclamped(lhs.values[1], rhs.values[1], alpha);
				values[2] = Mathf.LerpUnclamped(lhs.values[2], rhs.values[2], alpha);
				values[3] = Mathf.LerpUnclamped(lhs.values[3], rhs.values[3], alpha);
			}
			else if(len == 8)
			{
				values[0] = Mathf.LerpUnclamped(lhs.values[0], rhs.values[0], alpha);
				values[1] = Mathf.LerpUnclamped(lhs.values[1], rhs.values[1], alpha);
				values[2] = Mathf.LerpUnclamped(lhs.values[2], rhs.values[2], alpha);
				values[3] = Mathf.LerpUnclamped(lhs.values[3], rhs.values[3], alpha);
				values[4] = Mathf.LerpUnclamped(lhs.values[4], rhs.values[4], alpha);
				values[5] = Mathf.LerpUnclamped(lhs.values[5], rhs.values[5], alpha);
				values[6] = Mathf.LerpUnclamped(lhs.values[6], rhs.values[6], alpha);
				values[7] = Mathf.LerpUnclamped(lhs.values[7], rhs.values[7], alpha);				
			}
			else if(len == 16)
			{
				values[0] = Mathf.LerpUnclamped(lhs.values[0], rhs.values[0], alpha);
				values[1] = Mathf.LerpUnclamped(lhs.values[1], rhs.values[1], alpha);
				values[2] = Mathf.LerpUnclamped(lhs.values[2], rhs.values[2], alpha);
				values[3] = Mathf.LerpUnclamped(lhs.values[3], rhs.values[3], alpha);
				values[4] = Mathf.LerpUnclamped(lhs.values[4], rhs.values[4], alpha);
				values[5] = Mathf.LerpUnclamped(lhs.values[5], rhs.values[5], alpha);
				values[6] = Mathf.LerpUnclamped(lhs.values[6], rhs.values[6], alpha);
				values[7] = Mathf.LerpUnclamped(lhs.values[7], rhs.values[7], alpha);	
				values[8] = Mathf.LerpUnclamped(lhs.values[8], rhs.values[8], alpha);
				values[9] = Mathf.LerpUnclamped(lhs.values[9], rhs.values[9], alpha);
				values[10] = Mathf.LerpUnclamped(lhs.values[10], rhs.values[10], alpha);
				values[11] = Mathf.LerpUnclamped(lhs.values[11], rhs.values[11], alpha);
				values[12] = Mathf.LerpUnclamped(lhs.values[12], rhs.values[12], alpha);
				values[13] = Mathf.LerpUnclamped(lhs.values[13], rhs.values[13], alpha);
				values[14] = Mathf.LerpUnclamped(lhs.values[14], rhs.values[14], alpha);
				values[15] = Mathf.LerpUnclamped(lhs.values[15], rhs.values[15], alpha);				
			}
			else
			{
				for(int i = 0; i < lhs.values.Length; i++)
					values[i] = Mathf.LerpUnclamped(lhs.values[i], rhs.values[i], alpha);
			}
		}

		public void Lerp(z_SplatWeight lhs, z_SplatWeight rhs, float alpha, List<int> mask)
		{
			// optimize for some common values
			// unrolling the loop in these smaller cases can improve performances by ~33%
			if(mask.Count == 4)
			{
				values[mask[0]] = Mathf.LerpUnclamped(lhs.values[mask[0]], rhs.values[mask[0]], alpha);
				values[mask[1]] = Mathf.LerpUnclamped(lhs.values[mask[1]], rhs.values[mask[1]], alpha);
				values[mask[2]] = Mathf.LerpUnclamped(lhs.values[mask[2]], rhs.values[mask[2]], alpha);
				values[mask[3]] = Mathf.LerpUnclamped(lhs.values[mask[3]], rhs.values[mask[3]], alpha);
			}
			else if(mask.Count == 8)
			{
				values[mask[0]] = Mathf.LerpUnclamped(lhs.values[mask[0]], rhs.values[mask[0]], alpha);
				values[mask[1]] = Mathf.LerpUnclamped(lhs.values[mask[1]], rhs.values[mask[1]], alpha);
				values[mask[2]] = Mathf.LerpUnclamped(lhs.values[mask[2]], rhs.values[mask[2]], alpha);
				values[mask[3]] = Mathf.LerpUnclamped(lhs.values[mask[3]], rhs.values[mask[3]], alpha);
				values[mask[4]] = Mathf.LerpUnclamped(lhs.values[mask[4]], rhs.values[mask[4]], alpha);
				values[mask[5]] = Mathf.LerpUnclamped(lhs.values[mask[5]], rhs.values[mask[5]], alpha);
				values[mask[6]] = Mathf.LerpUnclamped(lhs.values[mask[6]], rhs.values[mask[6]], alpha);
				values[mask[7]] = Mathf.LerpUnclamped(lhs.values[mask[7]], rhs.values[mask[7]], alpha);				
			}
			else
			{
				for(int i = 0; i < mask.Count; i++)
					values[mask[i]] = Mathf.LerpUnclamped(lhs.values[mask[i]], rhs.values[mask[i]], alpha);
			}
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			foreach(var v in map)
			{
				sb.Append(v.Key.ToString());
				sb.Append(": ");
				sb.AppendLine(GetVec4(v.Value).ToString("F2"));
			}

			return sb.ToString();
		}
	}
}
