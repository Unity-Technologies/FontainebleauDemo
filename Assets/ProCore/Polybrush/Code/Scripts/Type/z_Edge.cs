using UnityEngine;
using System.Collections.Generic;

namespace Polybrush
{
	public struct z_Edge : System.IEquatable<z_Edge>
	{
		// tri indices
		public int x;
		public int y;

		public z_Edge(int _x, int _y)
		{
			this.x = _x;
			this.y = _y;
		}

		public bool Equals(z_Edge p)
		{
			return (p.x == x && p.y == y) || (p.x == y && p.y == x);
		}

		public override bool Equals(System.Object b)
		{
			return b is z_Edge && this.Equals((z_Edge)b);
		}

		public static bool operator ==(z_Edge a, z_Edge b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(z_Edge a, z_Edge b)
		{
			return !a.Equals(b);
		}

		public override int GetHashCode()
		{
			// http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
			int a, b, hash = 17;

			a = (x < y ? x : y).GetHashCode();
			b = (x < y ? y : x).GetHashCode();
	
			unchecked
			{
				hash = hash * 29 + a.GetHashCode();
				hash = hash * 29 + b.GetHashCode();
			}

			return hash;
		}

		public override string ToString()
		{
			return string.Format("{{{{{0},{1}}}}}", x, y);
		}

		public static List<int> ToList(IEnumerable<z_Edge> edges)
		{
			List<int> list = new List<int>();

			foreach(z_Edge e in edges)
			{
				list.Add(e.x);
				list.Add(e.y);
			}

			return list;
		}

		public static HashSet<int> ToHashSet(IEnumerable<z_Edge> edges)
		{
			HashSet<int> hash = new HashSet<int>();

			foreach(z_Edge e in edges)
			{
				hash.Add(e.x);
				hash.Add(e.y);
			}

			return hash;
		}
	}
}
