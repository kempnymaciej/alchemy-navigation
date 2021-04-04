using UnityEngine;

namespace AlchemyBow.Navigation.HighLevel
{
    /// <summary>
    /// Describes the settings of a baking process.
    /// </summary>
    [System.Serializable]
    public sealed class MeshBasedBakeSettings
    {
        /// <summary>
        /// The maximum distance between vertices to be counted as one vertex.
        /// </summary>
        /// <returns>The maximum distance between vertices to be counted as one vertex.</returns>
        [Range(NavigationInfo.EpsilonOffset, 1)]
        public float connectionRadius = .1f;

        /// <summary>
        /// The upwards direction.
        /// </summary>
        /// <returns>The upwards direction.</returns>
        public Vector3 upwards = Vector3.up;

        /// <summary>
        /// Maximum angle between Upwards and the normal of face.
        /// </summary>
        /// <returns>Maximum angle between Upwards and the normal of face.</returns>
        [Range(0, 180)]
        public float maxSlope = 45f;
    } 
}
