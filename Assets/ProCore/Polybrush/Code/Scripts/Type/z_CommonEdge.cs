using UnityEngine;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	Contains z_Edge with it's accompanying common lookup edge.
	 */
	public struct z_CommonEdge : System.IEquatable<z_CommonEdge>
	{
		public z_Edge edge, common;

		public int x { get { return edge.x; } }
		public int y { get { return edge.y; } }

		public int cx { get { return common.x; } }
		public int cy { get { return common.y; } }

		public z_CommonEdge(int _x, int _y, int _cx, int _cy)
		{
			this.edge = new z_Edge(_x, _y);
			this.common = new z_Edge(_cx, _cy);
		}

		public bool Equals(z_CommonEdge b)
		{
			return common.Equals(b.common);
		}

		public override bool Equals(System.Object b)
		{
			return b is z_CommonEdge && common.Equals(((z_CommonEdge)b).common);
		}

		public static bool operator ==(z_CommonEdge a, z_CommonEdge b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(z_CommonEdge a, z_CommonEdge b)
		{
			return !a.Equals(b);
		}

		public override int GetHashCode()
		{
			// http://stackoverflow.com/questions/5221396/what-is-an-appropriate-gethashcode-algorithm-for-a-2d-point-struct-avoiding
			return common.GetHashCode();
		}
		
		public override string ToString()
		{
			return string.Format("{{ {{{0}:{1}}}, {{{2}:{3}}} }}", edge.x, common.x, edge.y, common.y);
		}

		/**
		 *	Returns a new list of indices by selecting the x,y of each edge (discards common).
		 */
		public static List<int> ToList(IEnumerable<z_CommonEdge> edges)
		{
			List<int> list = new List<int>();

			foreach(z_CommonEdge e in edges)
			{
				list.Add(e.edge.x);
				list.Add(e.edge.y);
			}

			return list;
		}

		/**
		 *	Returns a new hashset of indices by selecting the x,y of each edge (discards common).
		 */
		public static HashSet<int> ToHashSet(IEnumerable<z_CommonEdge> edges)
		{
			HashSet<int> hash = new HashSet<int>();

			foreach(z_CommonEdge e in edges)
			{
				hash.Add(e.edge.x);
				hash.Add(e.edge.y);
			}

			return hash;
		}
	}
}
