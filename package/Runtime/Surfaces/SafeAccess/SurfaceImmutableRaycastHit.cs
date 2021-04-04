using UnityEngine;

namespace AlchemyBow.Navigation.Surfaces.SafeAccess
{
    /// <summary>
    /// Threadsafe structure used to get information back from a navigation raycast.
    /// </summary>
    public sealed class SurfaceImmutableRaycastHit
    {
        /// <summary>
        /// The intersection point in world space.
        /// </summary>
        /// <returns>The intersection point in world space.</returns>
        public Vector3 position;

        /// <summary>
        /// The face that was hit.
        /// </summary>
        /// <returns>The face that was hit.</returns>
        public IImmutableFace face;

        /// <summary>
        /// Create the threadsafe copy of the <c>SurfaceRaycastHit</c> class instance.
        /// </summary>
        /// <param name="surfaceRaycastHit">The source object.</param>
        public SurfaceImmutableRaycastHit(SurfaceRaycastHit surfaceRaycastHit)
        {
            this.position = surfaceRaycastHit.position;
            this.face = surfaceRaycastHit.face;
        }
    } 
}
