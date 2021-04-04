namespace AlchemyBow.Navigation
{
    /// <summary>
    /// A unique object that allows you to unregister faces from the system.
    /// </summary>
    public sealed class NavigationFaceWrapper
    {
        /// <summary>
        /// The index of the layer that contains the face.
        /// </summary>
        /// <returns>The index of the layer that contains the face</returns>
        public readonly int layerIndex;
        /// <summary>
        /// The weight of the face.
        /// </summary>
        /// <returns>The weight of the layer that contains the face</returns>
        public readonly float weight;
        /// <summary>
        /// The index of the area to which the face belongs in the form of a mask.
        /// </summary>
        /// <returns>The index of the area to which the face belongs in the form of a mask.</returns>
        public readonly int areaMask;

        /// <summary>
        /// Creates a new instance of the NavigationFaceWrapper class.
        /// </summary>
        /// <param name="layerIndex">The index of the layer that contains the face.</param>
        /// <param name="weight">The weight of the face.</param>
        /// <param name="areaMask">The index of the area to which the face belongs in the form of a mask.</param>
        public NavigationFaceWrapper(int layerIndex, float weight, int areaMask)
        {
            this.layerIndex = layerIndex;
            this.weight = weight;
            this.areaMask = areaMask;
        }
    } 
}
