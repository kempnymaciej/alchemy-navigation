namespace AlchemyBow.Navigation
{
	/// <summary>
	/// Constant data about the system.
	/// </summary>
	public static class NavigationInfo
	{
		/// <summary>
		/// The number of nodes per one face.
		/// </summary>
		/// <returns>The number of nodes per one face.</returns> 
		public const int NodesCount = 3;
		/// <summary>
		/// The epsilon error offset.
		/// </summary>
		/// <returns>The epsilon error offset</returns> 
		public const float EpsilonOffset = .00001f;
	} 
}
