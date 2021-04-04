using UnityEngine;

namespace AlchemyBow.Navigation.Settings
{
    /// <summary>
    /// Describes the settings for a specific layer.
    /// </summary>
    [System.Serializable]
    public sealed class NavigationLayerSettings
    {
        private const float DefaultConnectionRadius = .1f;
        private const float DefaultRaycastLength = 2;
        private const float DefaultMaxEdgeLength = 10;

        private const float MinConnectionRadius = .1f;
        private const float MinRaycastLength = .5f;
        private const float MinMaxEdgeLength = 1f;
        

        [SerializeField]
        private string name;
        [SerializeField, Min(MinConnectionRadius)]
        private float connectionRadius;
        [SerializeField, Min(MinRaycastLength)]
        private float raycastLength;
        [SerializeField, Min(MinMaxEdgeLength)]
        private float maxEdgeLength;

        /// <summary>
        /// Creates a new instance of the NavigationLayerSettings class.
        /// </summary>
        /// <param name="name">A user-friendly name to appear in the Unity Editor</param>
        /// <param name="connectionRadius">The maximum distance between vertices to be counted as one vertex.</param>
        /// <param name="raycastLength">The length of rays that are used to find faces of the navigation mesh. (optimization)</param>
        /// <param name="maxEdgeLength">The maximum length of edges declared by the user to be used. (optimization)</param>
        public NavigationLayerSettings(string name, float connectionRadius,
            float raycastLength, float maxEdgeLength)
        {
            this.name = name;
            this.connectionRadius = connectionRadius;
            this.raycastLength = raycastLength;
            this.maxEdgeLength = maxEdgeLength;
        }

        /// <summary>
        /// A user-friendly name to appear in the Unity Editor
        /// </summary>
        /// <returns>The user-friendly of the layer.</returns>
        public string Name => name;

        /// <summary>
        /// The maximum distance between vertices to be counted as one vertex.
        /// </summary>
        /// <returns>The maximum distance between vertices to be counted as one vertex for the layer.</returns>
        public float ConnectionRadius => connectionRadius;

        /// <summary>
        /// The length of rays that are used to find faces of the navigation mesh. (optimization)
        /// </summary>
        /// <returns>The length of rays that are used to find faces of the navigation mesh for the layer.</returns>
        public float RaycastLength => raycastLength;

        /// <summary>
        /// The maximum length of edges declared by the user to be used. (optimization)
        /// </summary>
        /// <returns>The maximum length of edges declared by the user for the layer.</returns>
        public float MaxEdgeLength => maxEdgeLength;

        /// <summary>
        /// Creates the default instance of the NavigationLayerSettings class.
        /// </summary>
        /// <returns>A new instance of the NavigationLayerSettings class with default values.</returns>
        public static NavigationLayerSettings CreateDefault()
        {
            return new NavigationLayerSettings("default", DefaultConnectionRadius,
                DefaultRaycastLength, DefaultMaxEdgeLength);
        }
    } 
}
