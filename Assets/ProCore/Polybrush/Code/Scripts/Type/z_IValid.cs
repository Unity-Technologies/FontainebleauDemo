namespace Polybrush
{
	/**
	 *	Interface for objects that may be null or otherwise invalid for use.
	 *	\sa z_EditableObject, z_BrushTarget, z_Util.IsValid
	 */
	public interface z_IValid
	{
		bool IsValid { get; }
	}
}
