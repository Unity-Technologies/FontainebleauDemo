
namespace Polybrush
{
	/**
	 *	Mesh property map.
	 */
	[System.Flags]
	public enum z_MeshChannel
	{
		Null		= 0,
		Position	= 0x0,
		Normal		= 0x1,
		Color 		= 0x2,
		Tangent 	= 0x4,
		UV0 		= 0x8,
		UV2 		= 0x10,
		UV3 		= 0x20,
		UV4 		= 0x40,
		All			= 0xFF
	};

	public static class z_MeshChannelUtility
	{
		public static z_MeshChannel StringToEnum(string str)
		{
			string upper = str.ToUpper();

			foreach(var v in System.Enum.GetValues(typeof(z_MeshChannel)))
			{
				if( upper.Equals( ((z_MeshChannel)v).ToString().ToUpper() ) )
					return (z_MeshChannel) v;
			}

			return z_MeshChannel.Null;
		}

		public static int UVChannelToIndex(z_MeshChannel channel)
		{
			if(channel == z_MeshChannel.UV0)
				return 0;
			else if(channel == z_MeshChannel.UV2)
				return 1;
			else if(channel == z_MeshChannel.UV3)
				return 2;
			else if(channel == z_MeshChannel.UV4)
				return 3;

			return -1;
		}
	}
}
