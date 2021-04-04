using AlchemyBow.Navigation.Surfaces.SafeAccess;
using UnityEngine;

namespace AlchemyBow.Navigation.Surfaces
{
    /// <summary>
    /// Structure used to get information back from a navigation raycast.
    /// </summary>
    public class SurfaceRaycastHit 
    {
        /// <summary>
        /// The distance from the ray's origin to the intersection point.
        /// </summary>
        /// <returns>The distance from the ray's origin to the intersection point.</returns>
        public float distance;

        /// <summary>
        /// The intersection point in world space.
        /// </summary>
        /// <returns>The intersection point in world space.</returns>
        public Vector3 position;

        /// <summary>
        /// The face that was hit.
        /// </summary>
        /// <returns>The face that was hit.</returns>
        public Face face;

        /// <summary>
        /// Creates an instance of the SurfaceRaycastHit class.
        /// </summary>
        public SurfaceRaycastHit()
        {
            this.face = null;
            this.distance = float.PositiveInfinity;
        }

        /// <summary>
        /// Create the threadsafe copy of the instance.
        /// </summary>
        /// <returns>The threadsafe copy of the instance.</returns>
        public SurfaceImmutableRaycastHit ToImmutable()
        {
            return new SurfaceImmutableRaycastHit(this);
        }
    } 
}
